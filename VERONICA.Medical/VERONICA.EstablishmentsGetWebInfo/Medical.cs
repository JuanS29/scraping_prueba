using Core.Lucas.Utils.Styles;
using RestSharp;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Xml;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;

namespace VERONICA.EstablishmentsGetWebInfo
{
    internal class Medical
    {
        private EventViewer log;
        private DateTime StartProcess;
        private Stopwatch SW;
        private SemaphoreSlim TaskSemaphore;

        /// <summary>
        /// contructor de Clase.
        /// </summary>
        public Medical()
        {
            log = new EventViewer();
            StartProcess = DateTime.Now;
            SW = new Stopwatch();
            TaskSemaphore = new SemaphoreSlim(Properties.Settings.Default.TaskNumber);
        }

        /// <summary>
        /// Funcion principal que se encarga de consultar la Web para traer los datos de un establecimiento usando su código.
        /// </summary>
        /// <param name="E">Clase Entidad que representa un establecimiento</param>
        internal void GetEstablishmentsWebInfo(ESTABLECIMIENTOS E)
        {
            SW.Start();
            Console.WriteLine("Inicio del proceso: " + StartProcess.ToString("dd/MM/yyyy HH:mm:ss"));

            //Si el PK_ESTABLECIMIENTO es 0 quiere decir que es un estableciminto que no esa registrado en la Base de Datos.
            if (E.PK_ESTABLECIMIENTO > 0)
            {
                Console.WriteLine("Ya hay un registro con este Codigo: ");
                Console.WriteLine("CODIGO: {0}", E.CODIGO_ESTABLECIMIENTO);
                Console.WriteLine("RUC: {0}", E.RUC);
                Console.WriteLine("NOMBRE COMERCIAL: {0}", E.NOMBRECOMERCIAL);
                Console.WriteLine("RAZON SOCIAL: {0}", E.RAZONSOCIAL);
            }
            else
            {
                Console.WriteLine("CODIGO: {0}", E.CODIGO_ESTABLECIMIENTO);
            }

            //RestClient es una libreria para hacer la consulta a la Web de establecimientos. 
            RestClient client = new RestClient(string.Format(Properties.Settings.Default.URL, E.CODIGO_ESTABLECIMIENTO));
            client.Timeout = 15000;
            RestRequest request = new RestRequest(Method.POST);
            IRestResponse response = client.Execute(request);
            //Si el StatusCode es 0 quiere decir que hubo algun problema de conexión con la web de consulta de establecimientos.
            if (response.StatusCode == 0)
            {
                Console.WriteLine("GetEstablishmentsWebInfo - {0}", response.ErrorMessage);
                log.WriteErrorLog(string.Format("GetEstablishmentsWebInfo - {0}", response.ErrorMessage));
                Thread.Sleep(5000);
            }
            else
            {
                Console.WriteLine("\nConsultando Web de Establecimientos");
                string htmlCode = response.Content;
                if (!htmlCode.Contains("Internal Server Error"))
                {
                    try
                    {
                        //El resultado de la Consulta Web es traido como un XML.
                        Console.WriteLine("Resultados: ");
                        XmlDocument xDoc = new XmlDocument();
                        xDoc.LoadXml(htmlCode);

                        XmlNode Registro = xDoc.DocumentElement.SelectSingleNode("*[@id='p1']");
                        XmlNode Situación = xDoc.DocumentElement.SelectSingleNode("*[@id='p2']");
                        XmlNode LugarRegistro = xDoc.DocumentElement.SelectSingleNode("*[@id='p3']");
                        XmlNode FechaInicio = xDoc.DocumentElement.SelectSingleNode("*[@id='p4']");
                        XmlNode RUC = xDoc.DocumentElement.SelectSingleNode("*[@id='p5']");
                        XmlNode Categoria = xDoc.DocumentElement.SelectSingleNode("*[@id='p6']");
                        XmlNode NombreComercial = xDoc.DocumentElement.SelectSingleNode("*[@id='p7']");
                        XmlNode RazonSocial = xDoc.DocumentElement.SelectSingleNode("*[@id='p8']");
                        XmlNode Direccion = xDoc.DocumentElement.SelectSingleNode("*[@id='p9']");
                        XmlNode DepProvDist = xDoc.DocumentElement.SelectSingleNode("*[@id='p10']");
                        XmlNode HorarioFuncionamiento = xDoc.DocumentElement.SelectSingleNode("*[@id='p11']");

                        ESTABLECIMIENTOS_UPDATE EU = new ESTABLECIMIENTOS_UPDATE();                            

                        EU.CODIGO_ESTABLECIMIENTO = Registro is XmlNode ? Registro.InnerText : "Sin Información";
                        EU.DIRECCION_SALUD = LugarRegistro is XmlNode ? LugarRegistro.InnerText : "Sin Información";
                        EU.NOMBRECOMERCIAL = NombreComercial is XmlNode ? NombreComercial.InnerText : "Sin Información";
                        EU.RAZONSOCIAL = RazonSocial is XmlNode ? RazonSocial.InnerText : "Sin Información";
                        EU.DIRECCION = Direccion is XmlNode ? Direccion.InnerText : "Sin Información";
                        EU.RUC = RUC is XmlNode ? RUC.InnerText : "Sin Información";
                        EU.ESTADO = Situación is XmlNode ? Situación.InnerText : "Sin Información";
                        EU.NOMBRECOMERCIAL = NombreComercial is XmlNode ? NombreComercial.InnerText : "Sin Información";
                        EU.HORARIO = HorarioFuncionamiento is XmlNode ? HorarioFuncionamiento.InnerText : "Sin Información";

                        if (FechaInicio is XmlNode)
                            if (!string.IsNullOrEmpty(FechaInicio.InnerText))
                                EU.FECHAINICIO = DateTime.ParseExact(FechaInicio.InnerText, "yyyy-MM-dd", CultureInfo.InvariantCulture);

                        EU.CATEGORIA = Categoria is XmlNode ? Categoria.InnerText : "Sin Información";
                        EU.DEPPROVDIST = DepProvDist is XmlNode ? DepProvDist.InnerText : "Sin Información";
                        if (EU.DEPPROVDIST != "Sin Información")
                        {
                            string[] DEPPROVDIST = EU.DEPPROVDIST.Split('/');
                            if (DEPPROVDIST.Length > 0)
                                EU.DEPARTAMENTO = DEPPROVDIST[0];
                            if (DEPPROVDIST.Length > 1)
                                EU.PROVINCIA = DEPPROVDIST[1];
                            if (DEPPROVDIST.Length > 2)
                                EU.DISTRITO = DEPPROVDIST[2];
                        }

                        Console.WriteLine("CODIGO: {0}", EU.CODIGO_ESTABLECIMIENTO);
                        Console.WriteLine("RUC: {0}", EU.RUC);
                        Console.WriteLine("NOMBRE COMERCIAL: {0}", EU.NOMBRECOMERCIAL);
                        Console.WriteLine("RAZON SOCIAL: {0}", EU.RAZONSOCIAL);
                        Console.WriteLine("Situación: {0}", EU.ESTADO);
                        Console.WriteLine("Categoria: {0}", EU.CATEGORIA);

                        //Despues de ser leido los datos y llenado la informacion en la clase entidad ESTABLECIMIENTOS_UPDATE se inserta a la base de datos.
                        int PK_ESTABLECIMIENTOS_UPDATE = new MedicalManager().InsertESTABLECIMIENTOS_UPDATE(EU);
                    }
                    catch (Exception e)
                    {
                        log.WriteErrorLog(string.Format("GetEstablishmentsWebInfo: {0}}", e.Message));
                        LogConsole.Write(string.Format("GetEstablishmentsWebInfo: {0}\n\n", e.Message), LogConsoleStyle.Default, LogConsoleType.Warning);
                    }
                }
                else
                {
                    Console.WriteLine("La consulta a la Web no trajo ningun resultado.");
                }
            }

            Console.WriteLine("\nTiempo total del Proceso: {0}", SW.Elapsed.ToString(@"hh\:mm\:ss\.fff"));
            SW.Stop();
        }
    }
}