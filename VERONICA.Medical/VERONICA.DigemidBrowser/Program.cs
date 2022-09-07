using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows.Forms;

namespace VERONICA.DigemidBrowser
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            EventViewer log = new EventViewer();
            try
            {
                int Pag = 0;
                string JsonFile = string.Empty;
                if (args.Length > 0)                
                    JsonFile = args[0];                
                if (args.Length > 1)
                    int.TryParse(args[1],out Pag);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DigemidBrowser(JsonFile, Pag));
            }
            catch (Exception e)
            {
                log.WriteErrorLog(string.Format("DigemidBrowser {0}", e.Message));
            }            
        }
    }
}
