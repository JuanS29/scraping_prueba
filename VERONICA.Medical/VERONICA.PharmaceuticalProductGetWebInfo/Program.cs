using Core.Lucas.Utils.Figlet;
using Core.Lucas.Utils.Styles;
using System;
using System.Diagnostics;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;
using VERONICA.PharmaceuticalProductGetWebInfo.Properties;

namespace VERONICA.PharmaceuticalProductGetWebInfo
{
    class Program
    {
        //VERONICA.PharmaceuticalProductGetWebInfo es un programa que se encarga de Consultar la Web de digemid para obtener el principio activo y la clasificación ATC de un producto a partir del código de producto.
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
            fig.ToWriteFiglet("Pharmaceutical");
            fig.ToWriteFiglet("Product");
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
                catch { }

                try
                {
                    if (CountOfCatalogo_Productos > 0)
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
                            fig.ToWriteFiglet("Pharmaceutical");
                            fig.ToWriteFiglet("Product");
                            fig.ToWriteFiglet("Get Web Info");
                            LogConsole.Write(string.Format("\nInicio del programa: {0}\n\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);

                            //Se trae la informacion mediante el codigo de Producto(QueueMessage) de la tabla Catalogo_Productos en la base de datos.
                            Catalogo_Productos CatProd = new MedicalManager().GetCatalogo_ProductosByCod_Prod(QueueMessage);
                            if (CatProd is Catalogo_Productos)
                            {
                                Medical M = new Medical();
                                M.GetWebInfo(CatProd);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("GetCountOfCatalogo_Productos -  No se pudo traer el regitros. ");
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("GetCountOfCatalogo_Productos - {0}", e.Message);
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }

        private static void QM_ErrorMessage(string Message)
        {
            LogConsole.Write(string.Format("QM_ErrorMessage: {0} - {1} \n\n", Settings.Default.QueueListen, Message), LogConsoleStyle.Default, LogConsoleType.Warning);
        }
    }
}
