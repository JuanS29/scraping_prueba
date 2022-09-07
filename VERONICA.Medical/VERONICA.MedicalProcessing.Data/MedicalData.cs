using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using VERONICA.EncryptDecrypt;
using VERONICA.MedicalProcessing.Models;

namespace VERONICA.MedicalProcessing.Data
{
    public class MedicalData
    {
        private string connectionDB = string.Empty;
        public MedicalData()
        {
            connectionDB = ConfigurationManager.AppSettings["DBConnection"];

            try
            {
                if (Convert.ToBoolean(Convert.ToInt16(ConfigurationManager.AppSettings["Decrypt"])))
                {
                    connectionDB = new Crypt().EncryptDecrypt(connectionDB, TypeCrypt.Decrypt);
                }
            }
            catch { }
        }

        public List<Catalogo_Productos> GetAllCatalogo_Productos()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<Catalogo_Productos>("SELECT * " +
                    "FROM [medical].[dbo].[Catalogo_Productos] ", para, null, true, 0, CommandType.Text).ToList();
            }
        }

        public List<string> GetAllCod_Estab()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<string>("SELECT DISTINCT [Cod_Estab] " +
                    "FROM [medical].[dbo].[Product_Establishment] " +
                    "ORDER BY [Cod_Estab] ", para, null, true, 0, CommandType.Text).ToList();
            }
        }

        public List<string> GetAllTMP_CODIGO_ESTABLECIMIENTO()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<string>("SELECT [CODIGO_ESTABLECIMIENTO] " +
                    "FROM [medical].[dbo].[TMP_ESTABLECIMIENTOS] " +
                    "ORDER BY [CODIGO_ESTABLECIMIENTO] ", para, null, true, 0, CommandType.Text).ToList();
            }
        }

        public Catalogo_Productos GetCatalogo_ProductosByCod_Prod(string Cod_Prod)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                para.Add("@Cod_Prod", Cod_Prod);
                return con.Query<Catalogo_Productos>("SELECT * " +
                    "FROM [medical].[dbo].[Catalogo_Productos] " +
                    "WHERE [Cod_Prod] = @Cod_Prod", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public int UpdateESTABLECIMIENTOS_UPDATE(ESTABLECIMIENTOS_UPDATE EU)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                para.Add("@PK_ESTABLECIMIENTOS_UPDATE", EU.PK_ESTABLECIMIENTOS_UPDATE);
                para.Add("@CODIGO_ESTABLECIMIENTO", EU.CODIGO_ESTABLECIMIENTO);
                para.Add("@DIRECCION_SALUD", EU.DIRECCION_SALUD);
                para.Add("@CLASIFICACION", EU.CLASIFICACION);
                para.Add("@NOMBRECOMERCIAL", EU.NOMBRECOMERCIAL);
                para.Add("@RAZONSOCIAL", EU.RAZONSOCIAL);
                para.Add("@DIRECCION", EU.DIRECCION);
                para.Add("@DEPARTAMENTO", EU.DEPARTAMENTO);
                para.Add("@PROVINCIA", EU.PROVINCIA);
                para.Add("@DISTRITO", EU.DISTRITO);
                para.Add("@RUC", EU.RUC);
                para.Add("@ESTADO", EU.ESTADO);
                para.Add("@HORARIO", EU.HORARIO);
                para.Add("@GRUPOS_PRODUCTO", EU.GRUPOS_PRODUCTO);
                para.Add("@EMPADRONAMIENTO", EU.EMPADRONAMIENTO);
                para.Add("@FECHAINICIO", EU.FECHAINICIO);
                para.Add("@CATEGORIA", EU.CATEGORIA);
                para.Add("@DEPPROVDIST", EU.DEPPROVDIST);
                return con.Query<int>("UPDATE [medical].[dbo].[ESTABLECIMIENTOS_UPDATE] SET [CODIGO_ESTABLECIMIENTO] = @CODIGO_ESTABLECIMIENTO,[DIRECCION_SALUD] = @DIRECCION_SALUD,[CLASIFICACION] = @CLASIFICACION,[NOMBRECOMERCIAL] = @NOMBRECOMERCIAL,[RAZONSOCIAL] = @RAZONSOCIAL,[DIRECCION] = @DIRECCION,[DEPARTAMENTO] = @DEPARTAMENTO,[PROVINCIA] = @PROVINCIA,[DISTRITO] = @DISTRITO,[RUC] = @RUC,[ESTADO] = @ESTADO,[HORARIO] = @HORARIO,[GRUPOS_PRODUCTO] = @GRUPOS_PRODUCTO,[EMPADRONAMIENTO] = @EMPADRONAMIENTO,[FECHAINICIO] = @FECHAINICIO,[CATEGORIA] = @CATEGORIA,[DEPPROVDIST] = @DEPPROVDIST " +
                    "OUTPUT inserted.[PK_ESTABLECIMIENTOS_UPDATE] " +
                    "WHERE [PK_ESTABLECIMIENTOS_UPDATE] = @PK_ESTABLECIMIENTOS_UPDATE ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public void UpdateCatalogo_ProductosByID(Catalogo_Productos CatProd)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                para.Add("@PK_Catalogo_Producto", CatProd.PK_Catalogo_Producto);
                para.Add("@Clasi_ATC", CatProd.Clasi_ATC);
                para.Add("@Prin_Activo", CatProd.Prin_Activo);                
                con.Query<int>("UPDATE [medical].[dbo].[Catalogo_Productos] SET [Clasi_ATC] = @Clasi_ATC,[Prin_Activo] = @Prin_Activo " +
                    "WHERE [PK_Catalogo_Producto] = @PK_Catalogo_Producto ", para, null, true, 0, CommandType.Text);
            }
        }

        public void UpdateCatalogo_ProductosByCod_GTIN(Catalogo_Productos CatProd)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                para.Add("@PK_Catalogo_Producto", CatProd.PK_Catalogo_Producto);
                para.Add("@Cod_GTIN", CatProd.Cod_GTIN);
                con.Query<int>("UPDATE [medical].[dbo].[Catalogo_Productos] SET [Cod_GTIN] = @Cod_GTIN " +
                    "WHERE [PK_Catalogo_Producto] = @PK_Catalogo_Producto ", para, null, true, 0, CommandType.Text);
            }
        }

        public void UpdateESTABLECIMIENTOSBySEGMENTACION(ESTABLECIMIENTOS E)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                para.Add("@PK_ESTABLECIMIENTO", E.PK_ESTABLECIMIENTO);
                para.Add("@SEGMENTACION", E.SEGMENTACION);
                para.Add("@CADENA", E.CADENA);
                con.Query<int>("UPDATE [medical].[dbo].[ESTABLECIMIENTOS] SET [SEGMENTACION] = @SEGMENTACION, [CADENA] = @CADENA " +
                    "WHERE [PK_ESTABLECIMIENTO] = @PK_ESTABLECIMIENTO ", para, null, true, 0, CommandType.Text);
            }
        }

        public void usp_Catalogo_Productos()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                con.Query("EXEC [medical].[dbo].[usp_update_or_insert_Catalogo_Productos] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public void usp_Product_QuitarDuplicados()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                con.Query("EXEC [medical].[dbo].[usp_Product_QuitarDuplicados] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public void usp_update_or_insert_ESTABLECIMIENTOS()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                con.Query("EXEC [medical].[dbo].[usp_update_or_insert_ESTABLECIMIENTOS] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public void TruncateTMP_Catalogo_Productos()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                con.Query("TRUNCATE TABLE [medical].[dbo].[TMP_Catalogo_Productos] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public void TruncateTMP_Product_Establishment()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                con.Query("TRUNCATE TABLE [medical].[dbo].[TMP_Product_Establishment] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public void TruncateTMP_ESTABLECIMIENTOS()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                con.Query("TRUNCATE TABLE [medical].[dbo].[TMP_ESTABLECIMIENTOS] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public ESTABLECIMIENTOS GetESTABLECIMIENTOSByCod_Estab(string Cod_Estab)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                para.Add("@Cod_Estab", Cod_Estab);
                return con.Query<ESTABLECIMIENTOS>("SELECT * " +
                    "FROM [medical].[dbo].[ESTABLECIMIENTOS] " +
                    "WHERE [CODIGO_ESTABLECIMIENTO] = @Cod_Estab", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public List<ESTABLECIMIENTOS> GetAllESTABLECIMIENTOS()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<ESTABLECIMIENTOS>("SELECT * " +
                    "FROM [medical].[dbo].[ESTABLECIMIENTOS] ", para, null, true, 0, CommandType.Text).ToList();
            }
        }

        public int GetCountOfEstablishments()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<int>("SELECT Count([PK_ESTABLECIMIENTO]) " +
                    "FROM [medical].[dbo].[ESTABLECIMIENTOS] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public int GetCountOfCatalogo_Productos()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<int>("SELECT Count([PK_Catalogo_Producto]) " +
                    "FROM [medical].[dbo].[Catalogo_Productos] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public int GetTMP_ESTABLECIMIENTOSCount()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<int>("SELECT Count([PK_ESTABLECIMIENTOS]) " +
                    "FROM [medical].[dbo].[TMP_ESTABLECIMIENTOS] ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public Product_Establishment GetLastTMP_Product_Establishment()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<Product_Establishment>("SELECT TOP (100) * " +
                    "FROM [medical].[dbo].[TMP_Product_Establishment] " +
                    "ORDER BY [PK_Product_Establishment] DESC ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public List<string> GetDistinctNom_Prod()
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                return con.Query<string>("SELECT distinct [Nom_Prod] " +
                    "FROM [medical].[dbo].[Catalogo_Productos] ", para, null, true, 0, CommandType.Text).ToList();
            }
        }

        public int InsertESTABLECIMIENTOS_UPDATE(ESTABLECIMIENTOS_UPDATE EU)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                para.Add("@CODIGO_ESTABLECIMIENTO", EU.CODIGO_ESTABLECIMIENTO);
                para.Add("@DIRECCION_SALUD", EU.DIRECCION_SALUD);
                para.Add("@CLASIFICACION", EU.CLASIFICACION);
                para.Add("@NOMBRECOMERCIAL", EU.NOMBRECOMERCIAL);
                para.Add("@RAZONSOCIAL", EU.RAZONSOCIAL);
                para.Add("@DIRECCION", EU.DIRECCION);
                para.Add("@DEPARTAMENTO", EU.DEPARTAMENTO);
                para.Add("@PROVINCIA", EU.PROVINCIA);
                para.Add("@DISTRITO", EU.DISTRITO);
                para.Add("@RUC", EU.RUC);
                para.Add("@ESTADO", EU.ESTADO);
                para.Add("@HORARIO", EU.HORARIO);
                para.Add("@GRUPOS_PRODUCTO", EU.GRUPOS_PRODUCTO);
                para.Add("@EMPADRONAMIENTO", EU.EMPADRONAMIENTO);
                para.Add("@FECHAINICIO", EU.FECHAINICIO);
                para.Add("@CATEGORIA", EU.CATEGORIA);
                para.Add("@DEPPROVDIST", EU.DEPPROVDIST);
                return con.Query<int>("INSERT INTO [medical].[dbo].[TMP_ESTABLECIMIENTOS] ([CODIGO_ESTABLECIMIENTO],[DIRECCION_SALUD],[CLASIFICACION],[NOMBRECOMERCIAL],[RAZONSOCIAL],[DIRECCION],[DEPARTAMENTO],[PROVINCIA],[DISTRITO],[RUC],[ESTADO],[HORARIO],[GRUPOS_PRODUCTO],[EMPADRONAMIENTO],[FECHAINICIO],[CATEGORIA],[DEPPROVDIST])  " +
                    "OUTPUT inserted.[PK_ESTABLECIMIENTOS] " +
                    "VALUES(@CODIGO_ESTABLECIMIENTO,@DIRECCION_SALUD,@CLASIFICACION,@NOMBRECOMERCIAL,@RAZONSOCIAL,@DIRECCION,@DEPARTAMENTO,@PROVINCIA,@DISTRITO,@RUC,@ESTADO,@HORARIO,@GRUPOS_PRODUCTO,@EMPADRONAMIENTO,@FECHAINICIO,@CATEGORIA,@DEPPROVDIST) ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public int InsertProductSheet(ProductSheet PS)
        {
            using (SqlConnection con = new SqlConnection(connectionDB))
            {
                var para = new DynamicParameters();
                para.Add("@Medicine", PS.Medicine);
                para.Add("@Presentation", PS.Presentation);
                para.Add("@Amount", PS.Amount);
                para.Add("@Country", PS.Country);
                para.Add("@Registry", PS.Registry);
                para.Add("@Condition", PS.Condition);
                para.Add("@Trademark", PS.Trademark);
                para.Add("@Titular", PS.Titular);
                para.Add("@Manufacturer", PS.Manufacturer);
                para.Add("@Establishment", PS.Establishment);
                para.Add("@Address", PS.Address);
                para.Add("@Location", PS.Location);
                para.Add("@Phone", PS.Phone);
                para.Add("@Schedule", PS.Schedule);
                para.Add("@TechnicalDirector", PS.TechnicalDirector);
                para.Add("@Cod_Prod", PS.Cod_Prod);
                para.Add("@Cod_Estab", PS.Cod_Estab);
                return con.Query<int>("INSERT INTO [medical].[dbo].[ProductSheet] ([Medicine],[Presentation],[Amount],[Country],[Registry],[Condition],[Trademark],[Titular],[Manufacturer],[Establishment],[Address],[Location],[Phone],[Schedule],[TechnicalDirector],[Cod_Prod],[Cod_Estab])  " +
                    "OUTPUT inserted.[PK_ProductSheet] " +
                    "VALUES(@Medicine,@Presentation,@Amount,@Country,@Registry,@Condition,@Trademark,@Titular,@Manufacturer,@Establishment,@Address,@Location,@Phone,@Schedule,@TechnicalDirector,@Cod_Prod,@Cod_Estab) ", para, null, true, 0, CommandType.Text).FirstOrDefault();
            }
        }

        public void BulkInsertProductSheet(List<ProductSheet> PSList)
        {
            DataTable dt = PSList.ToDataTable<ProductSheet>();
            using (SqlConnection connection = new SqlConnection(connectionDB))
            {
                // make sure to enable triggers
                // more on triggers in next post
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                // set the destination table name
                bulkCopy.DestinationTableName = "[medical].[dbo].[ProductSheet]";
                connection.Open();

                // write the data in the "dataTable"
                bulkCopy.WriteToServer(dt);
                connection.Close();
            }
            // reset
            //this.dataTable.Clear();
        }

        public void BulkInsertCatalogo_Productos(List<Catalogo_Productos> CPList)
        {
            DataTable dt = CPList.ToDataTable<Catalogo_Productos>();
            using (SqlConnection connection = new SqlConnection(connectionDB))
            {
                // make sure to enable triggers
                // more on triggers in next post
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                // set the destination table name
                bulkCopy.DestinationTableName = "[medical].[dbo].[TMP_Catalogo_Productos]";
                connection.Open();

                // write the data in the "dataTable"
                bulkCopy.WriteToServer(dt);
                connection.Close();
            }
            // reset
            //this.dataTable.Clear();
        }

        public void BulkInsertProduct_Establishment(List<Product_Establishment> PEList)
        {
            DataTable dt = PEList.ToDataTable<Product_Establishment>();
            using (SqlConnection connection = new SqlConnection(connectionDB))
            {
                // make sure to enable triggers
                // more on triggers in next post
                SqlBulkCopy bulkCopy =
                    new SqlBulkCopy
                    (
                    connection,
                    SqlBulkCopyOptions.TableLock |
                    SqlBulkCopyOptions.FireTriggers |
                    SqlBulkCopyOptions.UseInternalTransaction,
                    null
                    );

                // set the destination table name
                bulkCopy.DestinationTableName = "[medical].[dbo].[TMP_Product_Establishment]";
                connection.Open();

                // write the data in the "dataTable"
                bulkCopy.WriteToServer(dt);
                connection.Close();
            }
            // reset
            //this.dataTable.Clear();
        }
    }

    public static class BulkUploadToSqlHelper
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
