using DotNetBrowser;
using DotNetBrowser.DOM;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;
using VERONICA.ProductEstablishmentGetWebInfo.Properties;
using DotNetBrowser.Events;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VERONICA.ProductEstablishmentGetWebInfo
{
    internal class Medical
    {
        private EventViewer log;
        private DateTime StartProcess;
        private Stopwatch SW;
        private SemaphoreSlim TaskSemaphore;
        private ErrorRoot ER = null;
        private static string Temp = string.Empty;

        /// <summary>
        /// contructor de Clase.
        /// </summary>
        public Medical()
        {
            log = new EventViewer();
            StartProcess = DateTime.Now;
            SW = new Stopwatch();
            TaskSemaphore = new SemaphoreSlim(Properties.Settings.Default.TaskNumber);
            
            Temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            if (!Directory.Exists(Temp))
                Directory.CreateDirectory(Temp);
        }

        /// <summary>
        /// Funcion principal que se encarga de consultar la Web de digemid usando el nombre de un producto para obtener sus datos de venta.        
        /// </summary>
        /// <param name="D">Clase entidad que representa un grupo de búsqueda de productos.</param>
        internal void GetWebInfo(Datum D, int Attempts)
        {
            SW.Start();

            Console.WriteLine("Inicio del proceso: " + StartProcess.ToString("dd/MM/yyyy HH:mm:ss"));
            Console.WriteLine("\nGrupo: {0}", D.grupo);
            Console.WriteLine("codGrupoFF: {0}", D.codGrupoFF);
            Console.WriteLine("Concentracion: {0}", D.concent);
            if (Attempts > 0)
                Console.WriteLine("Intento de cola: {0}\n", Attempts);
            else
                Console.WriteLine("", D.concent);

            List<Product_Establishment> PEList = new List<Product_Establishment>();
            long iTotalRecords = 0;
            int Pag = 1;
            ER = null;
            RootProductos RP = APIQueryPrecioVistaCiudadano(D, Pag);
            if (RP is RootProductos)
            {
                if (!string.IsNullOrEmpty(RP.codigo))
                    Console.WriteLine("Respuesta de API: \nCódigo:{0}\nMensaje:{1}", RP.codigo, RP.mensaje);
                if (RP.codigo == "00")
                {
                    if (RP.cantidad != null)
                    {
                        iTotalRecords = Convert.ToInt32(RP.cantidad);
                        Console.WriteLine("Cantidad Total de Productos: {0}", iTotalRecords);
                    }

                    if (RP.data is List<DatumProducto>)
                        PEList.AddRange(Product_EstablishmentListConvert(RP.data));

                    if (iTotalRecords > Settings.Default.DisplayLength)
                    {
                        int TotalPag = (int)Math.Ceiling((double)iTotalRecords / Settings.Default.DisplayLength);
                        Console.WriteLine("Página Consultada - {0} de {1}", Pag, TotalPag);
                        long TotalQuerys = Settings.Default.DisplayLength;
                        do
                        {
                            Pag++;
                            ER = null;
                            RP = APIQueryPrecioVistaCiudadano(D, Pag);
                            Console.WriteLine("\nRespuesta de API: \nCódigo:{0}\nMensaje:{1}", RP.codigo, RP.mensaje);
                            if (RP.codigo != "00")
                                break;
                            Console.WriteLine("Página {0} de {1}", Pag, TotalPag);

                            if (RP.data is List<DatumProducto>)
                                PEList.AddRange(Product_EstablishmentListConvert(RP.data));

                            TotalQuerys = TotalQuerys + Settings.Default.DisplayLength;
                        } while (TotalQuerys < iTotalRecords);
                    }
                }
            }
            else
            {
                Console.WriteLine("GetCountOfEstablishments -  No se pudo traer el regitros. ");
                System.Threading.Thread.Sleep(20000);
            }
            
            if (RP.codigo != "00")
            {
                Attempts++;
                Console.WriteLine("Reenviando cola a {0}: {1}", Settings.Default.QueueListen, string.Format("{0};{1};{2};{3}", D.grupo, D.codGrupoFF.Trim(), D.concent.Trim(), Attempts));
                VERONICA.QueueManager.QueueManager QM = new QueueManager.QueueManager(Settings.Default.URLQueue);
                QM.SendQueue(string.Format("{0};{1};{2};{3}", D.grupo, D.codGrupoFF.Trim(), D.concent.Trim(), Attempts), Settings.Default.QueueListen);
                

                Console.WriteLine("Verificando conexion a Digemid.");
                string jsonSend = "{\"filtro\":{\"codigoProducto\":2926,\"codigoDepartamento\":\"15\",\"codigoProvincia\":\"01\",\"codigoUbigeo\":\"150101\",\"codTipoEstablecimiento\":null,\"catEstablecimiento\":null,\"codGrupoFF\":\"24\",\"concent\":\"100mg/mL\",\"tamanio\":10,\"pagina\":1,\"tokenGoogle\":<0>}}";
                RootProductos PARACETAMOLTestResult = APIQueryPrecioVistaCiudadano(new Datum(), 0, jsonSend);
                if (PARACETAMOLTestResult.codigo != "00")
                {
                    if (!string.IsNullOrEmpty(PARACETAMOLTestResult.codigo))
                        Console.WriteLine("\nRespuesta de API: \nCódigo:{0}\nMensaje:{1}", PARACETAMOLTestResult.codigo, PARACETAMOLTestResult.mensaje);

                    Console.WriteLine("\nPresione Enter si desea continuar con el programa.");
                    do
                    {
                        ConsoleKeyInfo CKI = Console.ReadKey();
                        if (CKI.Key == ConsoleKey.Enter)
                            break;
                    } while (true);                
                }
                else
                {
                    Console.WriteLine("\nRespuesta de API: \nCódigo:{0}\nMensaje:{1}", PARACETAMOLTestResult.codigo, PARACETAMOLTestResult.mensaje);
                }
                System.Threading.Thread.Sleep(30000);
            }

            //Se procede a insertar todos los resultados obtenidos a la tabla TMP_Product_Establishment de base datos
            List<Product_Establishment> BulkInsertList = new List<Product_Establishment>();
            int currentPosition = 0;
            int Percentage = 0;
            Console.WriteLine("\nTotal a Insertar: - {0}", PEList.Count);
            int CursorTop = Console.CursorTop;
            foreach (Product_Establishment PE in PEList)
            {
                BulkInsertList.Add(PE);
                currentPosition++;
                Percentage = (int)Math.Round(((decimal)currentPosition / (decimal)PEList.Count) * (decimal)100);
                if (BulkInsertList.Count == Properties.Settings.Default.BulkLimit)
                {
                    bool SQLInsert = false;
                    do
                    {
                        try
                        {
                            new MedicalManager().BulkInsertProduct_Establishment(BulkInsertList);
                            BulkInsertList.Clear();
                            SQLInsert = true;
                        }
                        catch (Exception e)
                        {
                            log.WriteErrorLog(string.Format("BulkInsertProductSheet: {0}", e.Message));
                        }
                    } while (!SQLInsert);
                }
                Console.SetCursorPosition(0, CursorTop);
                Console.Write("Porcentaje: {0}%", Percentage);
            }

            if (BulkInsertList.Count > 0)
            {
                bool SQLInsert = false;
                do
                {
                    try
                    {
                        new MedicalManager().BulkInsertProduct_Establishment(BulkInsertList);
                        BulkInsertList.Clear();
                        SQLInsert = true;
                        Console.SetCursorPosition(0, CursorTop);
                        Console.WriteLine("Porcentaje: {0}%", 100);
                    }
                    catch (Exception e)
                    {
                        log.WriteErrorLog(string.Format("BulkInsertProductSheet: {0}", e.Message));
                    }
                } while (!SQLInsert);
            }

            Console.WriteLine("\n\nTiempo total del Proceso: {0}", SW.Elapsed.ToString(@"hh\:mm\:ss\.fff"));
            SW.Stop();
        }

        private List<Product_Establishment> Product_EstablishmentListConvert(List<DatumProducto> data)
        {
            List<Product_Establishment> Result = new List<Product_Establishment>();

            foreach (DatumProducto DP in data)
            {
                Product_Establishment PE = new Product_Establishment();
                PE.EstablishmentType = DP.setcodigo;
                //PE.Product = string.Format("{0} {1} {2} {3}", DP.nombreProducto, DP.concent == null ? "" : DP.concent, DP.nombreFormaFarmaceutica == null ? "" : DP.nombreFormaFarmaceutica, DP.fracciones == 0 ? "" : string.Format("x {0} unid.", DP.fracciones));
                PE.Product = string.Format("{0} {1} {2}", DP.nombreProducto, DP.concent == null ? "" : DP.concent, DP.nombreFormaFarmaceutica == null ? "" : DP.nombreFormaFarmaceutica);
                PE.Laboratory = DP.nombreLaboratorio;
                PE.EstablishmentName = DP.nombreComercial;
                decimal Amount = 0;
                PE.Amount = (decimal)DP.precio1;
                if (DP.precio2 != null)
                {
                    decimal.TryParse(DP.precio2.ToString().Replace(",", "."), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out Amount);
                    PE.Amount = Amount;
                }
                try
                {
                    string UpdateDate = DP.fecha.Replace("a. m.", "AM");
                    UpdateDate = UpdateDate.Replace("p. m.", "PM");
                    PE.UpdateDate = DateTime.ParseExact(UpdateDate, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {

                }
                PE.Cod_Prod = DP.codProdE.ToString();
                PE.Cod_Estab = DP.codEstab;
                PE.UbigeoValue = DP.ubicodigo;
                Result.Add(PE);
            }
            return Result;
        }

        internal RootProductos APIQueryPrecioVistaCiudadano(Datum D, int Pag, string jsonSend = "")
        {
            RootProductos RP = new RootProductos();
            try
            {
                int Attempts = 0;
                do
                {
                    string DatumJsonFile = Path.Combine(Temp, string.Format("{0}.json", Guid.NewGuid().ToString()));
                    string ResultJsonFile = Path.Combine(Path.Combine(Path.GetDirectoryName(Settings.Default.DigemidBrowserApp), "ProcessOut"), Path.GetFileName(DatumJsonFile));
                    Process p = new Process();
                    Task T = Task.Run(() =>
                    {
                        string DatumJson = JsonConvert.SerializeObject(D);
                        File.WriteAllText(DatumJsonFile, DatumJson);
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardOutput = true;
                        p.StartInfo.FileName = Settings.Default.DigemidBrowserApp;
                        p.StartInfo.Arguments = string.Format("\"{0}\" {1}", DatumJsonFile, Pag);
                        p.Start();
                        p.WaitForExit(Settings.Default.BrowserTimeWait * 1000);
                    });
                    T.Wait(Settings.Default.BrowserTimeWait * 1000);
                    try
                    {
                        p.Kill();
                        T.Dispose();
                    }
                    catch { }

                    if (File.Exists(DatumJsonFile))
                        File.Delete(DatumJsonFile);

                    if (File.Exists(ResultJsonFile))
                    {
                        string jsonText = string.Empty;
                        using (StreamReader r = new StreamReader(ResultJsonFile))
                        {
                            jsonText = r.ReadToEnd();
                        }
                        RP = JsonConvert.DeserializeObject<RootProductos>(jsonText);
                        if (string.IsNullOrEmpty(RP.codigo))
                        {
                            try
                            {
                                ER = JsonConvert.DeserializeObject<ErrorRoot>(jsonText);
                                Console.WriteLine("API Error: {0} \nMensaje: {1}", ER.error, ER.message);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Deserialize ErrorRoot - {0}", e.Message);
                                log.WriteErrorLog(string.Format("Deserialize ErrorRoot - {0}", e.Message));
                            }
                        }
                        File.Delete(ResultJsonFile);
                        if (RP.codigo == "00")
                            break;
                    }
                    Attempts++;
                    Console.WriteLine("Consulta a Digemid,Intento: {0}", Attempts);
                } while (Properties.Settings.Default.Attempts > Attempts);                
            }
            catch (Exception e)
            {
                Console.WriteLine("APIQueryPrecioVistaCiudadano - {0}", e.Message);
                log.WriteErrorLog(string.Format("APIQueryPrecioVistaCiudadano - {0}", e.Message));
            }
            return RP;
        }
        
        internal List<Datum> GetProductGroups(string Nom_Prod)
        {
            List<Datum> DatumResult = new List<Datum>();
            try
            {
                RestClient client = null;
                RestRequest request = null;
                IRestResponse response = null;
                HttpStatusCode Result = 0;
                int Attempts = 0;
                while (Result != HttpStatusCode.OK)
                {
                    client = new RestClient(Settings.Default.URLGetGroups);
                    client.Timeout = -1;
                    request = new RestRequest(Method.POST);
                    request.AddHeader("Content-Type", "application/json");

                    string jsonSend = "{\"filtro\":{\"nombreProducto\":\"<0>\",\"pagina\":1,\"tamanio\":10}}";
                    jsonSend = jsonSend.Replace("{", "[");
                    jsonSend = jsonSend.Replace("}", "]");
                    jsonSend = jsonSend.Replace("<", "{");
                    jsonSend = jsonSend.Replace(">", "}");
                    jsonSend = string.Format(jsonSend, Nom_Prod.TrimStart().TrimEnd());
                    jsonSend = jsonSend.Replace("[", "{");
                    jsonSend = jsonSend.Replace("]", "}");

                    request.AddParameter("application/json", jsonSend, ParameterType.RequestBody);
                    response = client.Execute(request);
                    Result = response.StatusCode;
                    Attempts++;
                    if (Attempts > Settings.Default.Attempts)
                        break;
                }

                if (response.StatusCode == 0)
                {
                    Console.WriteLine("GetProductGroups - {0}", response.ErrorMessage);
                    log.WriteErrorLog(string.Format("GetProductGroups - {0}", response.ErrorMessage));
                }
                else
                {
                    Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(response.Content);
                    if (myDeserializedClass.data is List<Datum>)
                    {
                        DatumResult = myDeserializedClass.data;
                        Datum D = new Datum();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("GetProductGroups - {0}", e.Message);
                log.WriteErrorLog(string.Format("GetProductGroups - {0}", e.Message));
            }
            return DatumResult;
        }        
    }
}