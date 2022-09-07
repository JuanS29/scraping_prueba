using Core.Lucas.Utils.Figlet;
using Core.Lucas.Utils.Styles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;
using VERONICA.ProductEstablishmentGetWebInfo.Properties;

namespace VERONICA.ProductEstablishmentGetWebInfo
{
    class Program
    {
        //VERONICA.ProductEstablishmentGetWebInfo es un programa que se encarga de Consultar la Web de digemid para obtener los datos de venta de un producto en cada estableciento a nivel nacional.
        private static Stopwatch SW = new Stopwatch();
        private static EventViewer log;
        private static DateTime ProgramStar;
        private static VERONICA.QueueManager.QueueManager QM;
        private static string QueueMessage = string.Empty;
        private static int Attempts;
        private static int QueueNumber;
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            irDevelopers.ModifyInMemory.ActivateMemoryPatching();
            log = new EventViewer();
            log.WriteErrorLog("Modulo Iniciado.");
            ProgramStar = DateTime.Now;

            var fig = new Figlet();
            fig.ToWriteFiglet("Product");
            fig.ToWriteFiglet("Establishment");
            fig.ToWriteFiglet("Get Web Info");
            LogConsole.Write(string.Format("\nInicio del programa: {0}\n\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
            Console.WriteLine("Esperando recibir Colas.");

            //QueueManager: Libreria para conectarse al servidor de colas para recibir y enviar mensajes de cola.
            QM = new VERONICA.QueueManager.QueueManager(Settings.Default.URLQueue);
            while (true)
            {
                //Se hace una consulta a la base de datos para saber la cantidad de registros y verifiar si hay conexion con el Servidor de base de datos antes de comenzar a procesar.
                int CountOfCatalogo_Productos = 0;
                try
                {
                    CountOfCatalogo_Productos = new MedicalManager().GetCountOfCatalogo_Productos();
                }
                catch (Exception e)
                {
                    Console.WriteLine("GetCountOfEstablishments - {0}", e.Message);
                    System.Threading.Thread.Sleep(5000);
                }

                try
                {
                    if (CountOfCatalogo_Productos > 0)
                    {
                        Medical M = new Medical();
                        List<Datum> DatumList = M.GetProductGroups("PARACETAMOl");
                        if (DatumList.Count == 0)
                        {
                            Console.WriteLine("Número de colas Procesadas: {0}.", QueueNumber);
                            Console.WriteLine("Verificación API Producto Grupos: {0} resultados.", DatumList.Count);
                            System.Threading.Thread.Sleep(60000);
                            continue;
                        }

                        //Console.WriteLine("Verificación API Producto Grupos: {0} resultados.", DatumList.Count);

                        RootProductos PARACETAMOLTestResult = null;
                        //string jsonSend = "{\"filtro\":{\"codigoProducto\":2926,\"codigoDepartamento\":\"15\",\"codigoProvincia\":\"01\",\"codigoUbigeo\":\"150101\",\"codTipoEstablecimiento\":null,\"catEstablecimiento\":null,\"codGrupoFF\":\"24\",\"concent\":\"100mg/mL\",\"tamanio\":10,\"pagina\":1,\"tokenGoogle\":<0>}}";
                        //PARACETAMOLTestResult = M.APIQueryPrecioVistaCiudadano(new Datum(), 0, jsonSend);
                        //if (!(PARACETAMOLTestResult is RootProductos))
                        //{
                        //    Console.WriteLine("Ocurrio un error en la consulta de medicamentos.");
                        //    System.Threading.Thread.Sleep(20000);
                        //    continue;
                        //}

                        //if (PARACETAMOLTestResult.codigo != "00")
                        //{
                        //    Console.WriteLine("\nRespuesta de API: \nCódigo:{0}\nMensaje:{1}", PARACETAMOLTestResult.codigo, PARACETAMOLTestResult.mensaje);
                        //    System.Threading.Thread.Sleep(30000);
                        //    continue;
                        //}

                        QueueNumber++;
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
                            fig.ToWriteFiglet("Product");
                            fig.ToWriteFiglet("Establishment");
                            fig.ToWriteFiglet("Get Web Info");
                            LogConsole.Write(string.Format("\nInicio del programa: {0}\n\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);

                            if (PARACETAMOLTestResult is RootProductos)
                                Console.WriteLine("Verificación API Producto preciovista: {0}", PARACETAMOLTestResult.mensaje);

                            Console.WriteLine("Verificación API Producto Grupos: {0} resultados.", DatumList.Count);
                            Console.WriteLine("Número de colas Procesadas: {0}.", QueueNumber);
                            int grupo = 0;
                            string codGrupoFF = string.Empty;
                            string concent = string.Empty;
                            string[] MessageArray = QueueMessage.Split(';');
                            int.TryParse(MessageArray[0], out grupo);
                            if (MessageArray.Length > 1)
                                codGrupoFF = MessageArray[1];
                            if (MessageArray.Length > 2)
                                concent = MessageArray[2];
                            if (MessageArray.Length > 3)
                                int.TryParse(MessageArray[3], out Attempts);

                            if (grupo > 0)
                            {
                                Datum D = new Datum()
                                {
                                    grupo = grupo,
                                    codGrupoFF = codGrupoFF,
                                    concent = concent
                                };
                                M = new Medical();
                                M.GetWebInfo(D, Attempts);
                            }
                            else
                                Console.WriteLine("El grupo no es un número. {0}", MessageArray[0]);
                        }
                    }
                    else
                    {
                        Console.WriteLine("GetCountOfEstablishments -  No se pudo traer el regitros. ");
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("GetCountOfEstablishments - {0}", e.Message);
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        private static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("UnhandledExceptionEventHandler - {0}", e.Message);
            log.WriteErrorLog(string.Format("UnhandledExceptionEventHandler - {0}", e.Message));
            System.Threading.Thread.Sleep(60000);
        }

        private static void QM_ErrorMessage(string Message)
        {
            log.WriteErrorLog(string.Format("QM_ErrorMessage: {0} - {1}", Settings.Default.QueueListen, Message));
            LogConsole.Write(string.Format("QM_ErrorMessage: {0} - {1} \n\n", Settings.Default.QueueListen, Message), LogConsoleStyle.Default, LogConsoleType.Warning);
        }
    }
}
