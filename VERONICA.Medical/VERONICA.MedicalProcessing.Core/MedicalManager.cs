using System;
using System.Collections.Generic;
using VERONICA.MedicalProcessing.Data;
using VERONICA.MedicalProcessing.Models;

namespace VERONICA.MedicalProcessing.Core
{
    public class MedicalManager
    {
        /// <summary>
        ///  Función para traer la lista de medicamentos de la tabla Catalogo_Productos de la base de datos.
        /// </summary>
        /// <returns>Lista de información de medicamentos en la clase entidad Catalogo_Productos</returns>
        public List<Catalogo_Productos> GetAllCatalogo_Productos()
        {
            return new MedicalData().GetAllCatalogo_Productos();
        }

        /// <summary>
        ///  Función para traer la lista de codigos de establecimientos de la base de datos.
        /// </summary>
        /// <returns>Lista de los códigos de establecimiento</returns>
        public List<string> GetAllCod_Estab()
        {
            return new MedicalData().GetAllCod_Estab();
        }

        /// <summary>
        /// Función para traer la lista de codigos de establecimientos de la tabla TMP_ESTABLECIMIENTOS en la base de datos.
        /// </summary>
        /// <returns>Lista de los códigos de establecimiento de la tabla TMP_ESTABLECIMIENTOS</returns>
        public List<string> GetAllTMP_CODIGO_ESTABLECIMIENTO()
        {
            return new MedicalData().GetAllTMP_CODIGO_ESTABLECIMIENTO();
        }

        /// <summary>
        /// Función para traer la informacion de la base de datos de un Producto buscado por su código.
        /// </summary>
        /// <param name="Cod_Prod">Código de Producto</param>
        /// <returns></returns>
        public Catalogo_Productos GetCatalogo_ProductosByCod_Prod(string Cod_Prod)
        {
            return new MedicalData().GetCatalogo_ProductosByCod_Prod(Cod_Prod);
        }

        /// <summary>
        /// Función para traer la informacion de la base de datos de un establecimiento buscado por su código.
        /// </summary>
        /// <param name="Cod_Estab">Código del establecimiento</param>
        /// <returns>Clase Entidad que representa un Establecimiento.</returns>
        public ESTABLECIMIENTOS GetESTABLECIMIENTOSByCod_Estab(string Cod_Estab)
        {
            return new MedicalData().GetESTABLECIMIENTOSByCod_Estab(Cod_Estab);
        }

        public List<ESTABLECIMIENTOS> GetAllESTABLECIMIENTOS()
        {
            return new MedicalData().GetAllESTABLECIMIENTOS();
        }

        /// <summary>
        /// Función para traer la cantidad de establecimientos registrados en la base de datos. 
        /// </summary>
        /// <returns>Cantidad de establecimientos representados un número entero</returns>
        public int GetCountOfEstablishments()
        {
            return new MedicalData().GetCountOfEstablishments();
        }

        /// <summary>
        /// Función para traer la cantidad de Medicamentos registrados en la base de datos. 
        /// </summary>
        /// <returns>Cantidad de Medicamentos representados un número entero<</returns>
        public int GetCountOfCatalogo_Productos()
        {
            return new MedicalData().GetCountOfCatalogo_Productos();
        }

        public int GetTMP_ESTABLECIMIENTOSCount()
        {
            return new MedicalData().GetTMP_ESTABLECIMIENTOSCount();
        }

        public Product_Establishment GetLastTMP_Product_Establishment()
        {
            return new MedicalData().GetLastTMP_Product_Establishment();
        }

        /// <summary>
        /// Función para insertar un establecimiento en la base de datos. 
        /// </summary>
        /// <param name="EU">Clase Entidad que representa un Establecimiento.</param>
        /// <returns>Retorna el PK_ESTABLECIMIENTOS si la inserción se hizo correctamente.</returns>
        public int InsertESTABLECIMIENTOS_UPDATE(ESTABLECIMIENTOS_UPDATE EU)
        {
            return new MedicalData().InsertESTABLECIMIENTOS_UPDATE(EU);
        }

        public int InsertProductSheet(ProductSheet PS)
        {
            return new MedicalData().InsertProductSheet(PS);
        }

        public void BulkInsertProductSheet(List<ProductSheet> PSList)
        {
            new MedicalData().BulkInsertProductSheet(PSList);
        }

        public int UpdateESTABLECIMIENTOS_UPDATE(ESTABLECIMIENTOS_UPDATE EU)
        {
            return new MedicalData().UpdateESTABLECIMIENTOS_UPDATE(EU);
        }

        public void BulkInsertProduct_Establishment(List<Product_Establishment> PEList)
        {
            new MedicalData().BulkInsertProduct_Establishment(PEList);
        }

        /// <summary>
        /// Función para Actulizar los campos Clasi_ATC y Prin_Activo de un producto en la base de datos.  
        /// </summary>
        /// <param name="CatProd">Clase Entidad de Catalogo_Productos</param>
        public void UpdateCatalogo_ProductosByID(Catalogo_Productos CatProd)
        {
            new MedicalData().UpdateCatalogo_ProductosByID(CatProd);
        }

        /// <summary>
        /// Función para Actulizar el campo Cod_GTIN de un producto en la base de datos. 
        /// </summary>
        /// <param name="CatProd">Clase Entidad de Catalogo_Productos</param>
        public void UpdateCatalogo_ProductosByCod_GTIN(Catalogo_Productos CatProd)
        {
            new MedicalData().UpdateCatalogo_ProductosByCod_GTIN(CatProd);
        }

        /// <summary>
        /// Función para Actulizar los campos SEGMENTACION y CADENA de un establecimiento en la base de datos. 
        /// </summary>
        /// <param name="E">Clase Entidad de ESTABLECIMIENTOS</param>
        public void UpdateESTABLECIMIENTOSBySEGMENTACION(ESTABLECIMIENTOS E)
        {
            new MedicalData().UpdateESTABLECIMIENTOSBySEGMENTACION(E);
        }

        /// <summary>
        /// Función para Insertar Medicamentos en la tabla Catalogo_Productos de la base de datos. 
        /// </summary>
        /// <param name="CPList">Lista de la clase entidad Catalogo_Productos</param>
        public void BulkInsertCatalogo_Productos(List<Catalogo_Productos> CPList)
        {
            new MedicalData().BulkInsertCatalogo_Productos(CPList);
        }

        /// <summary>
        /// Función que llama a un Procedimiento almacenado de base da datos que actuliza la tabla Catalogo_Productos a partir de la tabla TMP_Catalogo_Productos. 
        /// </summary>
        public void usp_Catalogo_Productos()
        {
            new MedicalData().usp_Catalogo_Productos();
        }

        /// <summary>
        /// Función que llama a un Procedimiento almacenado de base da datos que actuliza la tabla Product_Establishment a partir de la tabla TMP_Product_Establishment eliminando los datos duplicados. 
        /// </summary>
        public void usp_Product_QuitarDuplicados()
        {
            new MedicalData().usp_Product_QuitarDuplicados();
        }

        /// <summary>
        /// Función que llama a un Procedimiento almacenado de base da datos que actuliza la tabla ESTABLECIMIENTOS a partir de la tabla TMP_ESTABLECIMIENTOS. 
        /// </summary>
        public void usp_update_or_insert_ESTABLECIMIENTOS()
        {
            new MedicalData().usp_update_or_insert_ESTABLECIMIENTOS();
        }

        /// <summary>
        /// Funcion que ejecuta un Truncate en la tabla TMP_Catalogo_Productos de base de datos.
        /// </summary>
        public void TruncateTMP_Catalogo_Productos()
        {
            new MedicalData().TruncateTMP_Catalogo_Productos();
        }

        /// <summary>
        /// Funcion que ejecuta un Truncate en la tabla TMP_Product_Establishment de base de datos.
        /// </summary>
        public void TruncateTMP_Product_Establishment()
        {
            new MedicalData().TruncateTMP_Product_Establishment();
        }

        /// <summary>
        /// Funcion que ejecuta un Truncate en la tabla TMP_ESTABLECIMIENTOS de base de datos.
        /// </summary>
        public void TruncateTMP_ESTABLECIMIENTOS()
        {
            new MedicalData().TruncateTMP_ESTABLECIMIENTOS();
        }

        /// <summary>
        /// Función para traer la lista de Medicamentos con distintos nombres de la tabla Catalogo_Productos en la base de datos. 
        /// </summary>
        /// <returns>Lista de nombres de Medicamentos</returns>
        public List<string> GetDistinctNom_Prod()
        {
            return new MedicalData().GetDistinctNom_Prod();
        }
    }
}
