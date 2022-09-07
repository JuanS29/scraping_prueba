using Core.Lucas.Utils.Figlet;
using Core.Lucas.Utils.Styles;
using System;
using System.Diagnostics;
using VERONICA.EstablishmentsGetWebInfo.Properties;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;

namespace VERONICA.EstablishmentsGetWebInfo
{
    class Program
    {
        //VERONICA.EstablishmentsGetWebInfo es un programa que se encarga de Consultar la Web de Establecimientos y traer información de un estalecimiento a partir del CODIGO_ESTABLECIMIENTO.
        private static Stopwatch SW = new Stopwatch();
        private static EventViewer log;
        private static DateTime ProgramStar;
        private static VERONICA.QueueManager.QueueManager QM;
        private static string QueueMessage = string.Empty;        
        static void Main(string[] args)
        {
            log = new EventViewer();
            log.WriteErrorLog("Modulo Iniciado.");
            ProgramStar = DateTime.Now;

            var fig = new Figlet();
            fig.ToWriteFiglet("Estab.");
            fig.ToWriteFiglet("Get Web Info");
            LogConsole.Write(string.Format("\nInicio del programa: {0}\n\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
            Console.WriteLine("Esperando recibir Colas.");

            //QueueManager: Libreria para conectarse al servidor de colas para recibir y enviar mensajes de cola.
            QM = new VERONICA.QueueManager.QueueManager(Settings.Default.URLQueue);
            while (true)
            {
                int CountOfEstablishments = 0;
                //Se hace una consulta a la base de datos para saber la cantidad de registros y verifiar si hay conexion con el Servidor de base de datos antes de comenzar a procesar.
                try
                {
                    CountOfEstablishments = new MedicalManager().GetCountOfEstablishments();
                }
                catch { }

                try
                {
                    if (CountOfEstablishments > 0)
                    {
                        QM.ErrorMessage += QM_ErrorMessage;
                        QueueMessage = QM.QueueReception(Settings.Default.QueueListen);
                        //Si el QueueMessage es null quiere decir que pudo tener algun problema conectanse al Servidor de colas o que no hay colas pendientes.                                                                                              
                        if (QueueMessage == null)
                        {
                            System.Threading.Thread.Sleep(3000);
                        }
                        else
                        {
                            Console.Clear();
                            fig = new Figlet();
                            fig.ToWriteFiglet("Estab.");
                            fig.ToWriteFiglet("Get Web Info");
                            LogConsole.Write(string.Format("\nInicio del programa: {0}\n\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);

                            //Se trae la informacion mediante el codigo de establecimiento(QueueMessage) de la base de datos.
                            ESTABLECIMIENTOS E = new MedicalManager().GetESTABLECIMIENTOSByCod_Estab(QueueMessage);
                            //Si fuera Null quiere decir que es un establecimiento nuevo, no registrado en la base de datos.
                            if (!(E is ESTABLECIMIENTOS))
                            {
                                E = new ESTABLECIMIENTOS();
                                E.CODIGO_ESTABLECIMIENTO = QueueMessage;
                            }
                            Medical M = new Medical();
                            M.GetEstablishmentsWebInfo(E);
                        }
                    }
                    else
                    {
                        Console.WriteLine("GetAllESTABLECIMIENTOS -  No se pudo traer el regitros. ");
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("GetAllESTABLECIMIENTOS - {0}", e.Message);
                    System.Threading.Thread.Sleep(5000);
                }                
            }
        }

        private static void QM_ErrorMessage(string Message)
        {
            log.WriteErrorLog(string.Format("QM_ErrorMessage: {0} - {1}", Settings.Default.QueueListen, Message));
            LogConsole.Write(string.Format("QM_ErrorMessage: {0} - {1} \n\n", Settings.Default.QueueListen, Message), LogConsoleStyle.Default, LogConsoleType.Warning);
        }
    }
}
