using Core.Lucas.Utils.Figlet;
using Core.Lucas.Utils.Styles;
using Independentsoft.Office.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VERONICA.MedicalProcessing.Core;
using VERONICA.MedicalProcessing.Models;

namespace VERONICA.GTINListExtract
{
    class Program
    {
        //VERONICA.GTINListExtract en un programa que a lee un archivo XML con el nombre de GTIN.xlsx ubicado den la carpeta "Temp" en el directorio de la aplicación.
        //Con la información del XML actuliza el campo Cod_GTIN de la tabla Catalogo_Productos en la base de datos.
        private static Stopwatch SW;
        private static Figlet fig;
        private static DateTime ProgramStar;
        private static string GTIN = string.Empty;
        static void Main(string[] args)
        {
            SW = new Stopwatch();
            ProgramStar = DateTime.Now;
            fig = new Figlet();

            GTIN = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp", "GTIN.xlsx");

            DateTime LastExecution = DateTime.Now;
            SW.Restart();
            Console.Clear();
            fig.ToWriteFiglet("GTIN List");
            fig.ToWriteFiglet("Extract");
            fig.ToWriteFiglet("Web Info");
            LogConsole.Write(string.Format("\nInicio del programa: {0}\n", ProgramStar.ToString("dd/MM/yyyy HH:mm:ss")), LogConsoleStyle.Default, LogConsoleType.Info);
            LogConsole.Write("\nUltima Ejecución: " + LastExecution + "\n", LogConsoleStyle.Default, LogConsoleType.Info);

            if (File.Exists(GTIN))
            {
                try
                {
                    Workbook workbook;
                    int TotalRegisters = 0;
                    using (FileStream FS = new FileStream(GTIN, FileMode.Open))
                    {
                        workbook = new Workbook(FS);
                    }
                    //Lee la primera hoja del Excel 
                    Worksheet worksheet = (Worksheet)workbook.Sheets[0];
                    TotalRegisters = worksheet.Rows.Count;
                    
                    List<Catalogo_Productos> CPList = new List<Catalogo_Productos>();
                    Console.WriteLine("Extrayendo Codigos GTIN del Excel. ");
                    int ConsoleTop = Console.CursorTop;
                    //Se recorre todos los registros de la primera hoja del Excel
                    for (int i = 0; i < worksheet.Rows.Count; i++)
                    {
                        if (worksheet.Rows[i] != null)
                        {
                            //La primera columna de la hoja Excel debe ser el código de Producto
                            string Cod_Prod = worksheet.Rows[i].Cells[0].Value;
                            string Num_RegSan = string.Empty;
                            string Cod_GTIN = string.Empty;
                            //La segunda columna de la hoja de Excel debe ser el número de registro sanitario(Num_RegSan).
                            if (worksheet.Rows[i].Cells.Count > 1)
                            {
                                Num_RegSan = worksheet.Rows[i].Cells[1].Value;
                            }
                            //La tercera columna de la hoja de Excel debe ser el codigo GTIN(Cod_GTIN).
                            if (worksheet.Rows[i].Cells.Count > 2)
                            {
                                Cod_GTIN = worksheet.Rows[i].Cells[2].Value;
                            }

                            if (!string.IsNullOrEmpty(Cod_Prod))
                            {
                                //Se verifica que el codigo de Producto leido en el excel sea un Producto registrado en la tabla Catalogo_Productos de base de datos.                    
                                Catalogo_Productos CP = new MedicalManager().GetCatalogo_ProductosByCod_Prod(Cod_Prod);
                                if (CP is Catalogo_Productos)
                                {
                                    if (CP.Num_RegSan == Num_RegSan)
                                    {
                                        if (!string.IsNullOrEmpty(Cod_GTIN))
                                        {
                                            CP.Cod_GTIN = Cod_GTIN;
                                            CPList.Add(CP);
                                            // Se actualiza el campo Cod_GTIN de la tabla Catalogo_Productos.
                                            new MedicalManager().UpdateCatalogo_ProductosByCod_GTIN(CP);
                                        }                                        
                                    }
                                }                               
                            }
                            int percent = (int)((double)100 * ((double)i / TotalRegisters));
                            Console.SetCursorPosition(0, ConsoleTop);
                            Console.WriteLine("Progreso: {0}", percent);
                        }
                    }

                    Console.WriteLine("\nNúmero de codigos GTIN extraidos: {0}", CPList.Count);
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
