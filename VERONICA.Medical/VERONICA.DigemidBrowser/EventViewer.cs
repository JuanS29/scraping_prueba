using System;
using System.Diagnostics;
using System.IO;

namespace VERONICA.DigemidBrowser
{
    internal class EventViewer
    {
        private string nombreLog;

        public EventViewer()
        {
            nombreLog = "DigemidBrowser";

            if (!EventLog.SourceExists(nombreLog))
                EventLog.CreateEventSource(nombreLog, "Application");
        }

        public void WriteEventViewerError(string nombreMetodo, string Mensaje)
        {
            EventLog.WriteEntry(nombreLog, "An error ocurred in method: " + nombreMetodo + ":  " + Mensaje, EventLogEntryType.Error);
        }

        public void WriteErrorLog(string Message)
        {
            StreamWriter sw = null;
            try
            {
                string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(LogDirectory))
                    Directory.CreateDirectory(LogDirectory);

                sw = new StreamWriter(string.Format("{0}\\LogFile {1}.txt", LogDirectory, DateTime.Now.ToString("dd-MM-yyyy")), true);
                sw.WriteLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + ": " + Message);
                sw.Flush();
                sw.Close();
            }
            catch (Exception) { }
        }
    }
}