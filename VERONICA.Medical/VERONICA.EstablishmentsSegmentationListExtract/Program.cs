using Core.Lucas.Utils.Figlet;
using Core.Lucas.Utils.Styles;
using Independentsoft.Office.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;

namespace VERONICA.EstablishmentsSegmentationListExtract
{
    class Program
    {
        //VERONICA.EstablishmentsSegmentationListExtract en un programa que a lee un archivo XML con el nombre de Segmentacion.xlsx ubicado den la carpeta "Temp" en el directorio de la aplicación.
        //Con la información del XML actuliza los campos SEGMENTACION y CADENA de la tabla ESTABLECIMIENTOS en la base de datos.
        private static Stopwatch SW;
        private static Figlet fig;
        private static DateTime ProgramStar;
        private static string SegmentFile = string.Empty;
        static void Main(string[] args)
        {
            SW = new Stopwatch();
            ProgramStar = DateTime.Now;
            fig = new Figlet();


            SegmentFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", "Segmentacion.xlsx");

            DateTime LastExecution = DateTime.Now;
            SW.Restart();
            Console.Clear();
            fig.ToWriteFiglet("Estab.");
            fig.ToWriteFiglet("Segmentation");
            fig.ToWriteFiglet("List Extract");
            LogConsole.Write(string.Format("\nInicio del programa: {0}\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
            LogConsole.Write("\nUltima Ejecución: " + LastExecution + "\n", LogConsoleStyle.Default, LogConsoleType.Info);

            if (File.Exists(SegmentFile))
            {
                try
                {
                    Workbook workbook;
                    int TotalRegisters = 0;
                    using (FileStream FS = new FileStream(SegmentFile, FileMode.Open))
                    {
                        workbook = new Workbook(FS);
                    }
                    //Lee la primera hoja del Excel 
                    Worksheet worksheet = (Worksheet)workbook.Sheets[0];
                    TotalRegisters = worksheet.Rows.Count;

                    List<ESTABLECIMIENTOS> EList = new List<ESTABLECIMIENTOS>();
                    Console.WriteLine("Extrayendo Segmentos y Cadenas del Excel. ");
                    int ConsoleTop = Console.CursorTop;
                    Console.WriteLine("N° de Registros: {0}", worksheet.Rows.Count);
                    //Se recorre todos los registros de la primera hoja del Excel
                    for (int i = 0; i < worksheet.Rows.Count; i++)
                    {
                        if (worksheet.Rows[i] != null)
                        {
                            //La primera columna de la hoja Excel debe ser el código de establecimiento
                            string CODIGO_ESTABLECIMIENTO = worksheet.Rows[i].Cells[0].Value;
                            string segmento = string.Empty;
                            string cadena = string.Empty;
                            //La tercera columna de la hoja de Excel debe ser el segmento.
                            if (worksheet.Rows[i].Cells.Count > 2)
                            {                               
                                segmento = worksheet.Rows[i].Cells[2].Value;
                            }
                            //La Cuarta columna de la hoja de Excel debe ser la cadena.
                            if (worksheet.Rows[i].Cells.Count > 3)
                            {
                                cadena = worksheet.Rows[i].Cells[3].Value;
                            }

                            if (!string.IsNullOrEmpty(CODIGO_ESTABLECIMIENTO))
                            {
                                //Se verifica que el codigo de establecimiento leido en el excel sea un establecimiento registrado en la tabla ESTABLECIMIENTOS de base de datos.
                                ESTABLECIMIENTOS E = new MedicalManager().GetESTABLECIMIENTOSByCod_Estab(CODIGO_ESTABLECIMIENTO);
                                if (E is ESTABLECIMIENTOS)
                                {
                                    if (!string.IsNullOrEmpty(segmento))
                                    {
                                        E.SEGMENTACION = segmento;
                                        E.CADENA = cadena.ToLower();
                                        EList.Add(E);
                                        //Se actualiza los campos SEGMENTACION y CADENA de la tabla ESTABLECIMIENTOS.
                                        new MedicalManager().UpdateESTABLECIMIENTOSBySEGMENTACION(E);
                                    }
                                }
                            }
                            int percent = (int)((double)100 * ((double)i / TotalRegisters));
                            Console.SetCursorPosition(0, ConsoleTop);
                            Console.WriteLine("Progreso: {0}", percent);
                        }
                    }

                    Console.WriteLine("\nNúmero de Segmentos y Cadenas extraidos: {0}", EList.Count);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: {0}", e.Message);
                }
            }


            Console.ReadKey();
        }
    }
}
