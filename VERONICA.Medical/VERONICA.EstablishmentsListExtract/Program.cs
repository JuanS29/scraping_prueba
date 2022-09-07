using Core.Lucas.Utils.Figlet;
using Core.Lucas.Utils.Styles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VERONICA.EstablishmentsListExtract.Properties;
using VERONICA.MedicalProcessing.Core;

namespace VERONICA.EstablishmentsListExtract
{
    class Program
    {
        //VERONICA.EstablishmentsListExtract en un programa que hace una Consulta a la tabla Product_Establishment de la base de datos para traer todos Cod_Estab distintos.
        //con la lista de Cod_Estab se envian en colas para que el programa VERONICA.EstablishmentsGetWebInfo los procese.
        //Este programa se queda esperando hasta que se terminen de procesar todos los Cod_Estab para actulizar la tabla ESTABLECIMIENTOS de la base de datos.
        private static EventViewer log;
        private static DateTime ProgramStar;
        private static Stopwatch SW;
        private static Figlet fig;
        private static SemaphoreSlim TaskSemaphore;
        private static bool Close = false;
        static void Main(string[] args)
        {
            SW = new Stopwatch();
            ProgramStar = DateTime.Now;
            TaskSemaphore = new SemaphoreSlim(Properties.Settings.Default.TaskNumber);
            log = new EventViewer();
            log.WriteErrorLog("Modulo Iniciado.");

            fig = new Figlet();
            fig.ToWriteFiglet("Estab.");
            fig.ToWriteFiglet("List Extract");
            LogConsole.Write(string.Format("\nInicio del programa: {0}\n\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
            CheckTMP_ESTABLECIMIENTOSAndUpdate();

            while (!Close)
            {
                ConsoleKeyInfo CKI = Console.ReadKey();
                if (CKI.Key == ConsoleKey.Escape)
                {
                    Close = true;
                }
            }
        }

        private static void CheckTMP_ESTABLECIMIENTOSAndUpdate()
        {
            while (true)
            {
                try
                {
                    DateTime LastExecution = DateTime.Now;
                    SW.Restart();
                    Console.Clear();
                    fig.ToWriteFiglet("Estab.");
                    fig.ToWriteFiglet("List Extract");
                    LogConsole.Write(string.Format("\nInicio del programa: {0}\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
                    LogConsole.Write("\nUltima Ejecución: " + LastExecution + "\n", LogConsoleStyle.Default, LogConsoleType.Info);

                    //Se traen la lista de todo los Cod_Estab distintos de la tabla Product_Establishment de la base de datos.
                    List<string> Cod_EstabList = new MedicalManager().GetAllCod_Estab();
                    //Se traen la lista de todo los Cod_Estab de la tabla TMP_ESTABLECIMIENTOS de base de datos pendientes a pasarse a la tabla ESTABLECIMIENTOS.
                    List<string> CODIGO_ESTABLECIMIENTOList = new MedicalManager().GetAllTMP_CODIGO_ESTABLECIMIENTO();
                    if (Cod_EstabList is List<string> & CODIGO_ESTABLECIMIENTOList is List<string>)
                    {
                        Console.WriteLine("Número de establecimientos a verificar: {0}", Cod_EstabList.Count);
                        Console.WriteLine("Número de establecimientos ya Registrados: {0}", CODIGO_ESTABLECIMIENTOList.Count);
                        VERONICA.QueueManager.QueueManager QM = new QueueManager.QueueManager(Settings.Default.URLQueue);
                        int TMP_ESTABLECIMIENTOSCount = 0;
                        int PendingMessagesCount = 0;
                        List<string> Result = new List<string>();
                        int Attempts = 0;
                        do
                        {
                            TMP_ESTABLECIMIENTOSCount = CODIGO_ESTABLECIMIENTOList.Count;
                            //Se trae la lista de Cod_Estab pendientes a procesar a partir de la diferencia entre TMP_ESTABLECIMIENTOS y la lista obtenida a partir de Product_Establishment
                            Result = Cod_EstabList.Where(i => !CODIGO_ESTABLECIMIENTOList.Contains(i)).ToList();
                            //Se trae el número de colas enviadas para que lo procese los programas VERONICA.EstablishmentsGetWebInfo que faltan por procesar.
                            PendingMessagesCount = QM.GetPendingMessagesCount(Settings.Default.QueueSend);
                            Console.WriteLine("");
                            //Si no hay colas pendientes a procesar quiere decir que aun no se ha enviado la lista de Cod_Estab
                            if (PendingMessagesCount == 0)
                            {
                                Console.WriteLine("Enviando colas a Procesar.");
                                List<Task> TaskList = new List<Task>();
                                foreach (string item in Result)
                                {
                                    Task T = SendQueue(item);
                                    TaskList.Add(T);
                                }
                                Task.WaitAll(TaskList.ToArray());

                                foreach (var item in TaskList)
                                    item.Dispose();
                                TaskList.Clear();
                            }

                            Console.WriteLine("Esperando Finalizacion de las colas.");
                            DateTime DT = DateTime.Now;
                            //Despues de haber sido enviado todas las colas se espera a que se terminen de procesar 
                            //se verifica que la cantidad de registros en TMP_ESTABLECIMIENTOS sea igual a la cantidad traida desde Product_Establishment 
                            while (DateTime.Now.Subtract(DT).TotalSeconds < Properties.Settings.Default.TimeWait)
                            {
                                Thread.Sleep(10000);
                                CODIGO_ESTABLECIMIENTOList = new MedicalManager().GetAllTMP_CODIGO_ESTABLECIMIENTO();
                                if (TMP_ESTABLECIMIENTOSCount != CODIGO_ESTABLECIMIENTOList.Count)
                                {
                                    TMP_ESTABLECIMIENTOSCount = CODIGO_ESTABLECIMIENTOList.Count;
                                    DT = DateTime.Now;
                                }                                   
                            }                           

                            Result = Cod_EstabList.Where(i => !CODIGO_ESTABLECIMIENTOList.Contains(i)).ToList();
                            PendingMessagesCount = QM.GetPendingMessagesCount(Settings.Default.QueueSend);

                            Console.WriteLine("Establecimientos que no se registraron: {0}", Result.Count);
                            Console.WriteLine("Colas Pendientes: {0}", PendingMessagesCount);
                            if (PendingMessagesCount == 0)
                            {
                                Attempts++;
                                if (Attempts > 3)
                                    break;
                            }
                        } while (Result.Count > 0);

                        //Actuliza la tabla ESTABLECIMIENTOS a partir de la tabla TMP_ESTABLECIMIENTOS
                        new MedicalManager().usp_update_or_insert_ESTABLECIMIENTOS();

                        LogConsole.Write(string.Format("\nTiempo de Proceso: {0}", SW.Elapsed.ToString(@"hh\:mm\:ss\.fff")), LogConsoleStyle.Default, LogConsoleType.Info);
                        SW.Stop();

                        //El programa se queda esperando un intervalo de dias hasta volver ha realizar el proceso.
                        DateTime NextQuery = LastExecution.AddDays(Properties.Settings.Default.DaysInterval);
                        LogConsole.Write(string.Format("\nSiguiente Consulta: {0}", NextQuery.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
                        while (DateTime.Now.Subtract(LastExecution).TotalDays < Properties.Settings.Default.DaysInterval)
                        {
                            Thread.Sleep(10000);
                        }
                    }
                    else
                    {
                        log.WriteErrorLog(string.Format("CheckTMP_ESTABLECIMIENTOSAndUpdate - {0}", "Listas Vacias"));
                        Console.WriteLine("CheckTMP_ESTABLECIMIENTOSAndUpdate - {0}", "Listas Vacias");
                    }
                }
                catch (Exception e)
                {
                    log.WriteErrorLog(string.Format("CheckTMP_ESTABLECIMIENTOSAndUpdate - {0}", e.Message));
                    Console.WriteLine("CheckTMP_ESTABLECIMIENTOSAndUpdate - {0}", e.Message);
                    Thread.Sleep(2000);
                }
            }
        }

        private static Task SendQueue(string QueueMessage)
        {
            return Task.Run(() =>
            {
                TaskSemaphore.Wait();
                VERONICA.QueueManager.QueueManager QM = new QueueManager.QueueManager(Settings.Default.URLQueue);
                QM.SendQueue(QueueMessage, Settings.Default.QueueSend);
                TaskSemaphore.Release();
            });
        }
    }
}
