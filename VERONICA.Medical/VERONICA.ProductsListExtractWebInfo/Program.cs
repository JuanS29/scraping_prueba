using Core.Lucas.Utils.Figlet;
using Core.Lucas.Utils.Styles;
using Independentsoft.Office.Spreadsheet;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;
using VERONICA.ProductsListExtractWebInfo.Properties;

namespace VERONICA.ProductsListExtractWebInfo
{
    class Program
    {
        //VERONICA.ProductsListExtractWebInfo es un programa que se ejecuta automaticamente cada cierto número de dias, configurable en el archivo VERONICA.ProductsListExtractWebInfo.exe.config(DaysInterval). 
        //Consulta la pagina de digemid para descargar el Excel con la lista de Medicamentos e insertarlos en la tabla TMP_Catalogo_Productos de la base de datos
        //Despues toda la informacion de la tabla TMP_Catalogo_Productos es actulizada a la tabla Catalogo_Productos
        //Finalmente cuando se termine de procesar todos los productos por los programas VERONICA.ProductEstablishmentGetWebInfo se ejecuta el procedimiento almacenado de base de datos usp_Product_QuitarDuplicados para actulizar la tabla Product_Establishment a partir de la tabla TMP_Product_Establishment.
        private static EventViewer log;
        private static DateTime ProgramStar;
        private static Stopwatch SW;
        private static Figlet fig;
        private static string Temp;
        private static bool Close = false;
        private static SemaphoreSlim TaskSemaphore;
        static void Main(string[] args)
        {
            TaskSemaphore = new SemaphoreSlim(Properties.Settings.Default.TaskNumber);
            SW = new Stopwatch();
            ProgramStar = DateTime.Now;
            log = new EventViewer();
            Temp = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");           
            if (!Directory.Exists(Temp))
                Directory.CreateDirectory(Temp);

            log.WriteErrorLog("Modulo Iniciado.");

            fig = new Figlet();
            fig.ToWriteFiglet("Products List");
            fig.ToWriteFiglet("Extract");
            fig.ToWriteFiglet("Web Info");
            LogConsole.Write(string.Format("\nInicio del programa: {0}\n\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
            CheckURLAndUpdate();

            while (!Close)
            {
                ConsoleKeyInfo CKI = Console.ReadKey();
                if (CKI.Key == ConsoleKey.Escape)
                {
                    Close = true;
                }
            }
        }

        private static void CheckURLAndUpdate()
        {
            while (true)
            {
                try
                {
                    //Se Eliminan todos los archivos antiguos dentro dela carpeta "Temp" en el direcorio del porgrama.
                    try
                    {
                        foreach (string item in Directory.GetFiles(Temp))
                            File.Delete(item);
                    }
                    catch { }

                    DateTime LastExecution = DateTime.Now;
                    SW.Restart();
                    Console.Clear();
                    fig.ToWriteFiglet("Products List");
                    fig.ToWriteFiglet("Extract");
                    fig.ToWriteFiglet("Web Info");
                    LogConsole.Write(string.Format("\nInicio del programa: {0}\n\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
                    LogConsole.Write("\nUltima Ejecución: " + LastExecution + "\n", LogConsoleStyle.Default, LogConsoleType.Info);

                    int CountOfCatalogo_Productos = 0;
                    //se obtine el número de Medicamentos registrados en Base de datos.
                    CountOfCatalogo_Productos = new MedicalManager().GetCountOfCatalogo_Productos();
                    try
                    {
                        LogConsole.Write(string.Format("Número de Medicamentos Actuales: {0}", CountOfCatalogo_Productos), LogConsoleStyle.Default, LogConsoleType.Info);
                                                
                        RestClient client = null;
                        RestRequest request = null;
                        IRestResponse response = null;
                        HttpStatusCode Result = 0;
                        int Attempts2 = 0;
                        string jsonSend = "{\"filtro\":{\"situacion\":\"ACT\",\"tokenGoogle\":<0>}}";                       

                        while (Result != HttpStatusCode.OK)
                        {
                            //client = new RestClient(Settings.Default.URL);
                            //client.Timeout = -1;
                            //request = new RestRequest(Method.POST);
                            //request.AddHeader("Content-Type", "application/json");                            
                            //request.AddParameter("application/json", jsonSend, ParameterType.RequestBody);
                            //response = client.Execute(request);
                            //Result = response.StatusCode;
                            
                            using (ProductEstablishmentGetWebInfo.DigemidBrowser DB = new ProductEstablishmentGetWebInfo.DigemidBrowser(jsonSend))
                            {
                                DB.ShowDialog();
                                response = DB.response;
                                Result = response.StatusCode;
                            }

                            Attempts2++;
                            if (Attempts2 > 3)
                                break;
                        }

                        if (response.StatusCode == 0)
                        {
                            Console.WriteLine("GetWebInfo - {0}", response.ErrorMessage);
                            log.WriteErrorLog(string.Format("GetWebInfo - {0}", response.ErrorMessage));
                        }
                        else
                        {
                            if (response.RawBytes.Length == 0)
                            {
                                Console.WriteLine("response.RawBytes: {0}", response.RawBytes.Length);
                                System.Threading.Thread.Sleep(10000);
                                continue;
                            }
                            var fileBytes = response.RawBytes;
                            string ExcelFile = Path.Combine(Temp, string.Format("{0}", "catalogoproductos.xlsx"));
                            File.WriteAllBytes(ExcelFile, fileBytes);
                            fileBytes = null;
                            if (File.Exists(ExcelFile))
                            {
                                Workbook workbook;
                                int TotalRegisters = 0;
                                using (FileStream FS = new FileStream(ExcelFile, FileMode.Open))
                                {
                                    workbook = new Workbook(FS);
                                }
                                //Lee la primera hoja del Excel 
                                Worksheet worksheet = (Worksheet)workbook.Sheets[0];
                                TotalRegisters = worksheet.Rows.Count;
                                List<Catalogo_Productos> CPList = new List<Catalogo_Productos>();
                                Console.WriteLine("\nExtrayendo Productos del Excel. ");
                                int ConsoleTop = Console.CursorTop;
                                DateTime? LastUpdateDate = null;
                                //Se recorre todos los registros de la primera hoja del Excel
                                for (int i = 0; i < worksheet.Rows.Count; i++)
                                {
                                    if (worksheet.Rows[i] != null)
                                    {
                                        
                                        string Cod_Prod = worksheet.Rows[i].Cells[0].Value;
                                        if (Cod_Prod.Contains("*En esta lista") | Cod_Prod.Contains("Catálogo de") | Cod_Prod.Contains("Cod_Prod"))
                                        {
                                            continue;
                                        }
                                        else if (Cod_Prod.Contains("Fec. de actualizac.:"))
                                        {
                                            string Fec = worksheet.Rows[i].Cells[1].Value;                                            
                                            LastUpdateDate = DateTime.ParseExact(Fec.Replace("p. m.", "PM"), "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                                            if (LastUpdateDate is DateTime)
                                                Console.WriteLine("\nCatálogo actualizado al: {0}", LastUpdateDate.Value.ToString("dd/MM/yyyy"));
                                            continue;
                                        }

                                        try
                                        {
                                            decimal C = 0;
                                            decimal.TryParse(worksheet.Rows[i].Cells[0].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out C);
                                            //Extrayendo información de Medicamento.
                                            Catalogo_Productos CP = new Catalogo_Productos();
                                            CP.Cod_Prod = ((int)C).ToString();
                                            if (worksheet.Rows[i].Cells.Count > 1)
                                                CP.Nom_Prod = worksheet.Rows[i].Cells[1].Value;

                                            if (worksheet.Rows[i].Cells.Count > 2)
                                                CP.Concent = worksheet.Rows[i].Cells[2].Value;
                                            if (!string.IsNullOrEmpty(CP.Concent))
                                                CP.Concent = CP.Concent.Replace(" ", "");

                                            if (worksheet.Rows[i].Cells.Count > 3)
                                                CP.Nom_Form_Farm = worksheet.Rows[i].Cells[3].Value;

                                            if (worksheet.Rows[i].Cells.Count > 4)
                                                CP.Nom_Form_Farm_Simplif = worksheet.Rows[i].Cells[4].Value;

                                            if (worksheet.Rows[i].Cells.Count > 5)
                                                CP.Presentac = worksheet.Rows[i].Cells[5].Value;

                                            if (worksheet.Rows[i].Cells.Count > 6)
                                            {
                                                decimal Amount = 0;
                                                decimal.TryParse(worksheet.Rows[i].Cells[6].Value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out Amount);
                                                CP.Fracciones = (float)Amount;
                                            }

                                            if (worksheet.Rows[i].Cells.Count > 7)
                                            {
                                                if (!string.IsNullOrEmpty(worksheet.Rows[i].Cells[7].Value))
                                                {
                                                    DateTime Fec_Vcto_Reg_Sanitario = DateTime.ParseExact(worksheet.Rows[i].Cells[7].Value.Replace(".", ""), "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                                    CP.Fec_Vcto_Reg_Sanitario = Fec_Vcto_Reg_Sanitario;
                                                }
                                            }

                                            if (worksheet.Rows[i].Cells.Count > 8)
                                                CP.Num_RegSan = worksheet.Rows[i].Cells[8].Value;
                                            if (worksheet.Rows[i].Cells.Count > 9)
                                                CP.Nom_Titular = worksheet.Rows[i].Cells[9].Value;
                                            if (worksheet.Rows[i].Cells.Count > 10)
                                                CP.Situacion = worksheet.Rows[i].Cells[10].Value;
                                            CP.dateupdate = LastUpdateDate;
                                            //Agregando los Medicamentos leidos del XML a una lista para ser insertado a base de datos.
                                            CPList.Add(CP);
                                        }
                                        catch (Exception e)
                                        {
                                        }

                                        int percent = (int)((double)100 * ((double)i / TotalRegisters));
                                        Console.SetCursorPosition(0, ConsoleTop);
                                        Console.WriteLine("Progreso: {0}", percent);
                                    }
                                }

                                Console.WriteLine("\nNúmero de Productos en Archivo: {0}", CPList.Count);
                                Console.WriteLine("Insertando Registros:");

                                //Limpiando tablas Temporales de Mediamentos y establecimientos de la base de datos. 
                                new MedicalManager().TruncateTMP_Catalogo_Productos();
                                new MedicalManager().TruncateTMP_Product_Establishment();
                                new MedicalManager().TruncateTMP_ESTABLECIMIENTOS();

                                List<Catalogo_Productos> BulkInsertList = new List<Catalogo_Productos>();
                                int currentPosition = 0;
                                int Percentage = 0;
                                int CursorTop = Console.CursorTop;
                                //Recorriendo la lista de Medicamentos e insertandolos en la tabla TMP_Catalogo_Productos.
                                foreach (Catalogo_Productos CP in CPList)
                                {
                                    BulkInsertList.Add(CP);
                                    currentPosition++;
                                    Percentage = (int)Math.Round(((decimal)currentPosition / (decimal)CPList.Count) * (decimal)100);
                                    if (BulkInsertList.Count == Properties.Settings.Default.BulkLimit)
                                    {
                                        bool SQLInsert = false;
                                        do
                                        {
                                            try
                                            {
                                                //Insertando una tanda de la lista de Medicamentos.
                                                new MedicalManager().BulkInsertCatalogo_Productos(BulkInsertList);
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
                                            //Insertando la ultima tanda de la lista de Medicamentos.
                                            new MedicalManager().BulkInsertCatalogo_Productos(BulkInsertList);
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

                                //Actulizando medicamentos desde la tabla TMP_Catalogo_Productos a la tabla Catalogo_Productos de base de datos.
                                new MedicalManager().usp_Catalogo_Productos();

                                Console.WriteLine("Enviando colas: ");
                                List<string> Nom_ProdList = new MedicalManager().GetDistinctNom_Prod();
                                Console.WriteLine("Número de productos a revisar: {0}", Nom_ProdList.Count);
                                List<Task<List<Datum>>> TaskDatumList = new List<Task<List<Datum>>>();
                                foreach (string item in Nom_ProdList)
                                {
                                    Task<List<Datum>> T = GetProductGroups(item);                                   
                                    TaskDatumList.Add(T);
                                }
                                Task.WaitAll(TaskDatumList.ToArray());

                                List<Datum> DatumList = new List<Datum>();
                                foreach (Task<List<Datum>> item in TaskDatumList)
                                {
                                    if (item.Result is List<Datum>)
                                    {
                                        foreach (Datum D in item.Result)
                                        {
                                            //var Dt = from x in DatumList where x.grupo == D.grupo && x.codGrupoFF == D.codGrupoFF select x;
                                            Datum Dt = DatumList.Where(x=> x.grupo == D.grupo && x.codGrupoFF == D.codGrupoFF && x.concent == D.concent).FirstOrDefault();
                                            if (!(Dt is Datum))
                                            {
                                                DatumList.Add(D);
                                            }
                                        }
                                    }
                                }

                                foreach (var item in TaskDatumList)
                                    item.Dispose();
                                TaskDatumList.Clear();

                                List<Task> TaskList = new List<Task>();
                                Console.WriteLine("Número de Grupos a Consultar: {0}", DatumList.Count);
                                foreach (Datum item in DatumList)
                                {
                                    //Enviando colas con el nombre del medicamento para que el programa VERONICA.ProductEstablishmentGetWebInfo procese.
                                    Task T = SendQueue(string.Format("{0};{1};{2}",item.grupo, item.codGrupoFF,item.concent), Settings.Default.QueueSend);
                                    TaskList.Add(T);
                                }
                                Task.WaitAll(TaskList.ToArray());

                                foreach (var item in TaskList)
                                    item.Dispose();
                                TaskList.Clear();

                                List<Catalogo_Productos> Catalogo_ProductosList = new MedicalManager().GetAllCatalogo_Productos();
                                foreach (Catalogo_Productos item in Catalogo_ProductosList)
                                {
                                    //Enviando colas con el codigo de Medicamento para que el programa VERONICA.PharmaceuticalProductGetWebInfo procese.
                                    Task T = SendQueue(item.Cod_Prod, Settings.Default.QueueSend2);
                                    TaskList.Add(T);
                                }
                                Task.WaitAll(TaskList.ToArray());

                                foreach (var item in TaskList)
                                    item.Dispose();
                                TaskList.Clear();

                                Console.WriteLine("Envio de colas Terminado.");
                                LogConsole.Write(string.Format("\nTiempo de Proceso hasta ahora: {0}", SW.Elapsed.ToString(@"hh\:mm\:ss\.fff")), LogConsoleStyle.Default, LogConsoleType.Info);
                                Console.WriteLine("\nEsperando Finalizacion del Proceso de Colas.");

                                Product_Establishment PE = null;
                                Product_Establishment PELast = null;
                                DateTime DT = DateTime.Now;
                                VERONICA.QueueManager.QueueManager QM = new QueueManager.QueueManager(Settings.Default.URLQueue);
                                int Attempts = 0;

                                //El programa se queda en espera mientras los programas VERONICA.ProductEstablishmentGetWebInfo procesan las colas enviadas.
                                while (true)
                                {
                                    int PendingMessagesCount = 0;
                                    PELast = new MedicalManager().GetLastTMP_Product_Establishment();
                                    while (DateTime.Now.Subtract(DT).TotalMinutes < 30)
                                    {
                                        Thread.Sleep(250);
                                    }
                                    DT = DateTime.Now;
                                    PE = new MedicalManager().GetLastTMP_Product_Establishment();
                                    PendingMessagesCount = QM.GetPendingMessagesCount(Settings.Default.QueueSend);
                                    Console.WriteLine("{0}, Número de colas por Procesar: {1}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"), PendingMessagesCount);
                                    if (PELast is Product_Establishment & PE is Product_Establishment)
                                    {
                                        if ((PE.PK_Product_Establishment == PELast.PK_Product_Establishment) & (PendingMessagesCount == 0))
                                            break;
                                    }
                                    else if (PendingMessagesCount == 0)
                                        Attempts++;

                                    if (Attempts > 3)
                                        break;
                                }

                                //Se ejecuta una funcion que inserta los nuevos datos de la tabla TMP_Product_Establishment a Product_Establishment.
                                try
                                {
                                    new MedicalManager().usp_Product_QuitarDuplicados();
                                }
                                catch (Exception e)
                                {
                                    log.WriteErrorLog(string.Format("usp_Product_QuitarDuplicados - {0}", e.Message));
                                    Console.WriteLine("usp_Product_QuitarDuplicados - {0}", e.Message);
                                }
                            }
                            else
                            {
                                Console.WriteLine("No se pudo descargar el Archivo.");
                            }

                            LogConsole.Write(string.Format("\nTiempo Total de Proceso: {0}", SW.Elapsed.ToString(@"hh\:mm\:ss\.fff")), LogConsoleStyle.Default, LogConsoleType.Info);
                            SW.Stop();

                            //Una vez finalizado el programa se queda en espera a cumplir el ciclo de dias programado antes de volver a ejecutar el proceso.
                            DateTime NextQuery = LastExecution.AddDays(Settings.Default.DaysInterval);
                            LogConsole.Write(string.Format("\nSiguiente Consulta: {0}", NextQuery.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
                            while (DateTime.Now.Subtract(LastExecution).TotalDays < Properties.Settings.Default.DaysInterval)
                            {
                                Thread.Sleep(250);
                            }
                        }                      
                    }
                    catch (Exception e)
                    {
                        log.WriteErrorLog(string.Format("CheckURLAndUpdate - {0}", e.Message));
                        Console.WriteLine("CheckURLAndUpdate - {0}", e.Message);
                        Thread.Sleep(5000);
                    }
                }
                catch (Exception e)
                {
                    log.WriteErrorLog(string.Format("CheckURLAndUpdate - {0}", e.Message));
                    Console.WriteLine("CheckURLAndUpdate - {0}", e.Message);
                    Thread.Sleep(2000);
                }
            }
        }

        private static Task<List<Datum>> GetProductGroups(string Nom_Prod)
        {
            return Task.Run(() =>
            {
                List<Datum> DatumResult = new List<Datum>();
                TaskSemaphore.Wait();
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
                        }
                    }
                }
                catch (Exception e)
                {

                }
                TaskSemaphore.Release();
                return DatumResult;
            });
        }

        private static Task SendQueue(string QueueMessage, string QueueName)
        {
            return Task.Run(() =>
            {
                TaskSemaphore.Wait();
                VERONICA.QueueManager.QueueManager QM = new QueueManager.QueueManager(Settings.Default.URLQueue);
                QM.SendQueue(QueueMessage, QueueName);
                TaskSemaphore.Release();
            });
        }
    }
}
