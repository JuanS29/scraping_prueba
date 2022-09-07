using DotNetBrowser;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotNetBrowser.Events;
using VERONICA.DigemidBrowser.Properties;
using System.Net;
using DotNetBrowser.DOM;
using Newtonsoft.Json;

namespace VERONICA.DigemidBrowser
{
    public partial class DigemidBrowser : Form
    {
        private EventViewer log;
        private DotNetBrowser.WinForms.WinFormsBrowserView winFormsBrowserView1;
        private string browserOneUserDataDir;
        private static List<string> ajaxUrls = new List<string>();
        private static ManualResetEvent waitEvent = new ManualResetEvent(false);
        private static ManualResetEvent waitEvent2 = new ManualResetEvent(false);
        private static ManualResetEvent waitFinishLoadingFrameEvent = new ManualResetEvent(false);

        private string ProcessOut = string.Empty;
        private string _JsonFile = string.Empty;
        private int _Pag = 0;
        private static string tokenGoogle = string.Empty;
        public IRestResponse response = null;
        public DigemidBrowser(string JsonFile, int Pag)
        {
            _JsonFile = JsonFile;
            _Pag = Pag;
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            log = new EventViewer();
            irDevelopers.ModifyInMemory.ActivateMemoryPatching();
        }

        private void DigemidBrowser_Load(object sender, EventArgs e)
        {
            try
            {
                ProcessOut = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProcessOut");
                if (!Directory.Exists(ProcessOut))
                    Directory.CreateDirectory(ProcessOut);

                browserOneUserDataDir = Path.GetFullPath("user-data-dir-one");
                if (Directory.Exists(browserOneUserDataDir))
                    Directory.Delete(browserOneUserDataDir, true);
                if (!Directory.Exists(browserOneUserDataDir))
                    Directory.CreateDirectory(browserOneUserDataDir);
                Browser browserTwo = BrowserFactory.Create(new BrowserContext(new BrowserContextParams(browserOneUserDataDir)));
                browserTwo.UserAgent = string.Format("Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 {0}/5.0", Guid.NewGuid().ToString());
                string UserAgent = browserTwo.UserAgent;
                ajaxUrls = new List<string>();
                browserTwo.Context.NetworkService.ResourceHandler = new AjaxResourceHandler();
                browserTwo.Context.NetworkService.NetworkDelegate = new AjaxNetworkDelegate();
                browserTwo.FinishLoadingFrameEvent += Browser_FinishLoadingFrameEvent;

                winFormsBrowserView1 = new DotNetBrowser.WinForms.WinFormsBrowserView(browserTwo);
                winFormsBrowserView1.Dock = DockStyle.Fill;
                panelBrowser.Controls.Add(winFormsBrowserView1);

                timerStart.Start();
            }
            catch (Exception ex)
            {
                log.WriteErrorLog(string.Format("DigemidBrowser - {0}", ex.Message));
                this.Close();
            }
        }

        private void Browser_FinishLoadingFrameEvent(object sender, FinishLoadingEventArgs e)
        {
            if (e.IsMainFrame)
                if (waitFinishLoadingFrameEvent != null)
                    waitFinishLoadingFrameEvent.Set();
        }

        private class AjaxResourceHandler : ResourceHandler
        {
            public bool CanLoadResource(ResourceParams parameters)
            {
                if (parameters.ResourceType == ResourceType.XHR)
                {
                    ajaxUrls.Add(parameters.URL);
                }
                return true;
            }
        }

        private class AjaxNetworkDelegate : DefaultNetworkDelegate
        {
            public override void OnDataReceived(DataReceivedParams parameters)
            {
                if (ajaxUrls.Contains(parameters.Url))
                {
                    if (parameters.Url == Settings.Default.URLtokenGoogle)
                    {
                        PrintResponseData(parameters.Data);
                    }
                }
            }

            private void PrintResponseData(byte[] data)
            {
                var str = Encoding.Default.GetString(data);
                string[] tokenGoogleArray = str.Split(',');
                if (tokenGoogleArray.Length == 1)
                    tokenGoogle = tokenGoogleArray[0];
                else if (tokenGoogleArray.Length >= 10)
                {
                    tokenGoogle = tokenGoogleArray[1];
                    if (waitEvent != null)
                    {
                        waitEvent2 = new ManualResetEvent(false);
                        waitEvent.Set();
                        waitEvent2.WaitOne(15000);
                    }
                }
            }
        }

        private async void timerStart_Tick(object sender, EventArgs e)
        {
            timerStart.Stop();
            try
            {
                Datum D = new Datum();
                using (StreamReader r = new StreamReader(_JsonFile))
                {
                    string json = r.ReadToEnd();
                    D = JsonConvert.DeserializeObject<Datum>(json);
                }

                int BrowserAttempts = 0;
                while (true)
                {
                    RestClient client = null;
                    RestRequest request = null;
                    HttpStatusCode Result = 0;

                    await Task.Run(() =>
                    {
                        waitFinishLoadingFrameEvent = new ManualResetEvent(false);

                        winFormsBrowserView1.Browser.LoadURL(Settings.Default.URLDigemid);

                        waitFinishLoadingFrameEvent.WaitOne(15000);

                        DOMDocument document = winFormsBrowserView1.Browser.GetDocument();
                        List<DOMNode> divs = document.GetElementsByClassName("btn btn-inverse btn-sm btn-s1 margin-gap ");
                        DOMNode CloseButton = divs.Where(x => x.TextContent == "Cerrar").FirstOrDefault();
                        if (CloseButton is DOMNode)
                            CloseButton.Click();

                        waitFinishLoadingFrameEvent = new ManualResetEvent(false);
                        winFormsBrowserView1.Browser.LoadURL(Settings.Default.URLDigemidSource);
                        waitFinishLoadingFrameEvent.WaitOne(15000);

                        document = winFormsBrowserView1.Browser.GetDocument();
                        divs = document.GetElementsByClassName("btn btn-inverse btn-sm btn-s1 margin-gap ");
                        CloseButton = divs.Where(x => x.TextContent == "Cerrar").FirstOrDefault();
                        if (CloseButton is DOMNode)
                            CloseButton.Click();

                        waitFinishLoadingFrameEvent = new ManualResetEvent(false);
                        winFormsBrowserView1.Browser.LoadURL(Settings.Default.URLDigemid);
                        waitFinishLoadingFrameEvent.WaitOne(15000);

                        document = winFormsBrowserView1.Browser.GetDocument();
                        divs = document.GetElementsByClassName("btn btn-inverse btn-sm btn-s1 margin-gap ");
                        CloseButton = divs.Where(x => x.TextContent == "Cerrar").FirstOrDefault();
                        if (CloseButton is DOMNode)
                            CloseButton.Click();

                        waitEvent = new ManualResetEvent(false);

                        document = winFormsBrowserView1.Browser.GetDocument();
                        divs = document.GetElementsByClassName("btn btn-inverse btn-sm btn-s1 margin-gap btn-block");
                        DOMNode a = divs.Where(x => x.TextContent == " Buscar ").FirstOrDefault();
                        if (a is DOMNode)
                            a.Click();
                        else
                        {
                            a = divs.Where(x => x.TextContent == " Search ").FirstOrDefault();
                            if (a is DOMNode)
                                a.Click();
                        }

                        waitEvent.WaitOne(15000);

                        if (waitEvent2 != null)
                            waitEvent2.Set();

                        textBox1.Text = tokenGoogle;

                        string jsonSend = string.Empty;
                        if (D.grupo != 0)
                        {                            
                            jsonSend = "{\"filtro\":{\"codigoProducto\":<0>,\"codigoDepartamento\":null,\"codigoProvincia\":null,\"codigoUbigeo\":null,\"codTipoEstablecimiento\":null,\"catEstablecimiento\":null,\"codGrupoFF\":\"<1>\",\"concent\":\"<2>\",\"tamanio\":<3>,\"pagina\":<4>,\"tokenGoogle\":<5>}}\r\n";
                            jsonSend = jsonSend.Replace("{", "[");
                            jsonSend = jsonSend.Replace("}", "]");
                            jsonSend = jsonSend.Replace("<", "{");
                            jsonSend = jsonSend.Replace(">", "}");
                            jsonSend = string.Format(jsonSend, D.grupo, D.codGrupoFF, D.concent, Settings.Default.DisplayLength, _Pag, tokenGoogle);
                            jsonSend = jsonSend.Replace("[", "{");
                            jsonSend = jsonSend.Replace("]", "}");
                        }
                        else
                        {
                            jsonSend = "{\"filtro\":{\"codigoProducto\":2926,\"codigoDepartamento\":\"15\",\"codigoProvincia\":\"01\",\"codigoUbigeo\":\"150101\",\"codTipoEstablecimiento\":null,\"catEstablecimiento\":null,\"codGrupoFF\":\"24\",\"concent\":\"100mg/mL\",\"tamanio\":10,\"pagina\":1,\"tokenGoogle\":<0>}}";
                            jsonSend = jsonSend.Replace("{", "[");
                            jsonSend = jsonSend.Replace("}", "]");
                            jsonSend = jsonSend.Replace("<", "{");
                            jsonSend = jsonSend.Replace(">", "}");
                            jsonSend = string.Format(jsonSend, tokenGoogle);
                            jsonSend = jsonSend.Replace("[", "{");
                            jsonSend = jsonSend.Replace("]", "}");
                        }

                        client = new RestClient(Settings.Default.URLSearch);
                        client.Timeout = -1;
                        request = new RestRequest(Method.POST);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddParameter("application/json", jsonSend, ParameterType.RequestBody);
                        response = client.Execute(request);
                        Result = response.StatusCode;
                        if (Result == HttpStatusCode.OK)
                        {
                            textBox1.Text = string.Format("Intento: {0} \n{1}", BrowserAttempts, response.Content);
                        }
                    });

                    waitEvent = null;
                    waitEvent2 = null;

                    //if (Result != HttpStatusCode.OK)
                    //    break;

                    if (response.ErrorMessage == null)
                    {
                        RootProductos RP = JsonConvert.DeserializeObject<RootProductos>(response.Content);
                        if (RP is RootProductos)
                        {
                            if (RP.codigo == "00")
                                break;
                        }

                    }
                    BrowserAttempts++;
                    if (BrowserAttempts > Settings.Default.Attempts)
                        break;
                }


                System.IO.File.WriteAllText(Path.Combine(ProcessOut,Path.GetFileName(_JsonFile)), response.Content);

                this.Close();
            }
            catch (Exception ex)
            {
                log.WriteErrorLog(string.Format("DigemidBrowser - {0}", ex.Message));
                this.Close();
            }
        }
    }
}
