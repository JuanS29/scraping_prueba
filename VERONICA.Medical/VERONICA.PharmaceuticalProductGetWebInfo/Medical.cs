using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;
using VERONICA.PharmaceuticalProductGetWebInfo.Properties;

namespace VERONICA.PharmaceuticalProductGetWebInfo
{
    internal class Medical
    {
        private EventViewer log;
        private DateTime StartProcess;
        private Stopwatch SW;
        private string Temp;
        private int Attempts;
        /// <summary>
        /// contructor de Clase.
        /// </summary>
        public Medical()
        {
            log = new EventViewer();
            StartProcess = DateTime.Now;
            SW = new Stopwatch();
            Temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        }

        /// <summary>
        /// Funcion principal que se encarga de consultar la Web de digemid para obtener el Principio Activo y la Clasificación ATC de un producto.
        /// </summary>
        /// <param name="CatProd">Clase entidad de la tabla Catalogo_Productos</param>
        internal void GetWebInfo(Catalogo_Productos CatProd)
        {
            if (!Directory.Exists(Temp))
                Directory.CreateDirectory(Temp);

            SW.Start();
            Console.WriteLine("Inicio del proceso: " + StartProcess.ToString("dd/MM/yyyy HH:mm:ss"));
            Console.WriteLine("PK: {0}", CatProd.PK_Catalogo_Producto);
            Console.WriteLine("Codigo: {0}", CatProd.Cod_Prod);
            Console.WriteLine("Nombre: {0}", CatProd.Nom_Prod);
            Console.WriteLine("N° RegSan: {0}", CatProd.Num_RegSan);
            Console.WriteLine("Titular: {0}", CatProd.Nom_Titular);
            Console.WriteLine("Fecha Vencimiento: {0}", CatProd.Fec_Vcto_Reg_Sanitario == null ? "Sin Información" : CatProd.Fec_Vcto_Reg_Sanitario.Value.ToString("dd/MM/yyyy"));

            //RestClient es una libreria para hacer la consulta a la Web de digemid. 
            RestClient client = null;
            RestRequest request = null;
            IRestResponse response = null;
            HttpStatusCode Result = 0;
            Attempts = 0;
            while (Result != HttpStatusCode.OK)
            {               
                client = new RestClient(Settings.Default.URLSearch);
                client.Timeout = -1;
                request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                string jsonSend = string.Format("{{\"RsNO\":\"{0}\"}}", CatProd.Num_RegSan);
                request.AddParameter("application/json", jsonSend, ParameterType.RequestBody);
                response = client.Execute(request);
                Result = response.StatusCode;
                Attempts++;
                if (Attempts > Settings.Default.Attempts)
                    break;
            }

            //Si el StatusCode es 0 quiere decir que hubo algun problema de conexión con la web.
            if (response.StatusCode == 0)
            {
                Console.WriteLine("GetWebInfo - {0}", response.ErrorMessage);
                log.WriteErrorLog(string.Format("GetWebInfo - {0}", response.ErrorMessage));
                Thread.Sleep(5000);
            }
            else
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //El resultado esta en formato HTML
                    string json = response.Content;
                    Root EstablishmentList = JsonConvert.DeserializeObject<Root>(json);
                    HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                    doc.LoadHtml(EstablishmentList.d);
                    HtmlAgilityPack.HtmlNodeCollection Result2 = doc.DocumentNode.SelectNodes("//div[@id = 'bordes']");

                    HtmlAgilityPack.HtmlNode RS = Result2[0].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode RSRSANT = Result2[0].ChildNodes[3];
                    HtmlAgilityPack.HtmlNode FECHAVENC = Result2[0].ChildNodes[5];

                    HtmlAgilityPack.HtmlNode SITUACION = Result2[1].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode Nombre = Result2[2].ChildNodes[0];
                    HtmlAgilityPack.HtmlNode Descripcion = Result2[2].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode RUBRO = Result2[3].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode TITULAR = Result2[4].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode FABRICANTE = Result2[5].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode PROCEDENCIA = Result2[6].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode CLASIFICACION_FARMACOLOGICA = Result2[7].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode CONDICION_VENTA = Result2[8].ChildNodes[1];
                    HtmlAgilityPack.HtmlNode COMPOSICION = Result2[10].ChildNodes[0];
                    HtmlAgilityPack.HtmlNode ADMINISTRACION = Result2[12].ChildNodes[0];

                    //Se leen todos los datos optenidos de la consulta.
                    Console.WriteLine("\n\nHtml Decode ");
                    if (RS is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("RS: {0}", HttpUtility.HtmlDecode(RS.InnerText));
                    if (RSRSANT is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("RSRSANT: {0}", HttpUtility.HtmlDecode(RSRSANT.InnerText));
                    if (FECHAVENC is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("FECHAVENC: {0}", HttpUtility.HtmlDecode(FECHAVENC.InnerText));
                    if (SITUACION is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("SITUACION: {0}", HttpUtility.HtmlDecode(SITUACION.InnerText));
                    if (Nombre is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("Nombre: {0}", HttpUtility.HtmlDecode(Nombre.InnerText));
                    if (Descripcion is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("Descripcion: {0}", HttpUtility.HtmlDecode(Descripcion.InnerText));
                    if (RUBRO is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("RUBRO: {0}", HttpUtility.HtmlDecode(RUBRO.InnerText));
                    if (TITULAR is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("TITULAR: {0}", HttpUtility.HtmlDecode(TITULAR.InnerText));
                    if (FABRICANTE is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("FABRICANTE: {0}", HttpUtility.HtmlDecode(FABRICANTE.InnerText));
                    if (PROCEDENCIA is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("PROCEDENCIA: {0}", HttpUtility.HtmlDecode(PROCEDENCIA.InnerText));
                    if (CLASIFICACION_FARMACOLOGICA is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("CLASIFICACION_FARMACOLOGICA: {0}", HttpUtility.HtmlDecode(CLASIFICACION_FARMACOLOGICA.InnerText));
                    if (CONDICION_VENTA is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("CONDICION_VENTA: {0}", HttpUtility.HtmlDecode(CONDICION_VENTA.InnerText));
                    if (COMPOSICION is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("COMPOSICION: {0}", HttpUtility.HtmlDecode(COMPOSICION.InnerText));
                    if (RS is HtmlAgilityPack.HtmlNode)
                        Console.WriteLine("ADMINISTRACION: {0}", HttpUtility.HtmlDecode(ADMINISTRACION.InnerText));

                    //Se le asignan los campos de Prin_Activo y Clasi_ATC al producto correspondiente al registro sanitario.
                    CatProd.Prin_Activo = string.Empty;
                    CatProd.Clasi_ATC = string.Empty;
                    if (CLASIFICACION_FARMACOLOGICA is HtmlAgilityPack.HtmlNode & RS is HtmlAgilityPack.HtmlNode)
                    {
                        List<string> ResultExtrac = HttpUtility.HtmlDecode(CLASIFICACION_FARMACOLOGICA.InnerText).Trim().Split(' ').ToList();
                        ResultExtrac = ResultExtrac.Where(x => !string.IsNullOrEmpty(x)).ToList();
                        if (ResultExtrac.Count > 0)
                        {
                            CatProd.Clasi_ATC = ResultExtrac.FirstOrDefault().Trim();
                            if (ResultExtrac.Count > 1)
                                for (int i = 1; i < ResultExtrac.Count; i++)
                                {
                                    CatProd.Prin_Activo += " " + ResultExtrac[i];
                                    CatProd.Prin_Activo = CatProd.Prin_Activo.Trim();
                                }

                            string Num_RegSan = HttpUtility.HtmlDecode(RS.InnerText).TrimStart().TrimEnd();
                            string Num_RegSanANT = string.Empty;
                            if (RSRSANT is HtmlAgilityPack.HtmlNode)
                                Num_RegSanANT = HttpUtility.HtmlDecode(RSRSANT.InnerText).TrimStart().TrimEnd();
                            if (!(CatProd.Num_RegSan.Trim() == Num_RegSan | CatProd.Num_RegSan.Trim() == Num_RegSanANT))
                            {
                                CatProd.Prin_Activo = string.Empty;
                                CatProd.Clasi_ATC = string.Empty;
                                log.WriteErrorLog(string.Format("GetWebInfo - Registro Buscado: {0}; Resgistros Obtenidos: {1},{2}; Cod_Prod: {3}", CatProd.Num_RegSan, Num_RegSan, Num_RegSanANT, CatProd.Cod_Prod));
                                VERONICA.QueueManager.QueueManager QM = new VERONICA.QueueManager.QueueManager(Settings.Default.URLQueue);
                                QM.SendQueue(CatProd.Cod_Prod, Settings.Default.QueueListen);
                            }
                        }                        
                    }
                    else
                    {
                        Console.WriteLine("GetWebInfo - {0}", "No se obtuvo Resultados.");
                    }

                    //Se actuliza los campos Prin_Activo y Clasi_ATC en la tabla Catalogo_Productos de la base de datos.
                    new MedicalManager().UpdateCatalogo_ProductosByID(CatProd);

                    Console.WriteLine("Datos Actulizados.");
                }
                else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    Console.WriteLine("GetWebInfo - {0}", response.StatusCode);
                    log.WriteErrorLog(string.Format("GetWebInfo - {0}", response.StatusCode));
                    VERONICA.QueueManager.QueueManager QM = new QueueManager.QueueManager(Settings.Default.URLQueue);
                    QM.SendQueue(string.Format("{0}", CatProd.Cod_Prod), Settings.Default.QueueListen);
                    Thread.Sleep(5000);
                }
                else
                {
                    Console.WriteLine("GetWebInfo - {0}", response.StatusCode);
                    log.WriteErrorLog(string.Format("GetWebInfo - {0}", response.StatusCode));
                    Thread.Sleep(5000);
                }
            }
            Console.WriteLine("\n\nTiempo total del Proceso: {0}", SW.Elapsed.ToString(@"hh\:mm\:ss\.fff"));
            SW.Stop();
        }
    }
}