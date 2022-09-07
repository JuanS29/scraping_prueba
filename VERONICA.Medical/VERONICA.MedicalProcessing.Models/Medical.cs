using Newtonsoft.Json;
using System;

namespace VERONICA.MedicalProcessing.Models
{
    public class ESTABLECIMIENTOS
    {
        public int PK_ESTABLECIMIENTO { get; set; }

        public string CODIGO_ESTABLECIMIENTO { get; set; }

        public string DIRECCION_SALUD { get; set; }

        public string CLASIFICACION { get; set; }

        public string NOMBRECOMERCIAL { get; set; }

        public string RAZONSOCIAL { get; set; }
        [JsonIgnore]
        public string DIRECCION { get; set; }

        public string DEPARTAMENTO { get; set; }

        public string PROVINCIA { get; set; }

        public string DISTRITO { get; set; }

        public string RUC { get; set; }
        [JsonIgnore]
        public string ESTADO { get; set; }

        [JsonIgnore]
        public string HORARIO { get; set; }
        [JsonIgnore]
        public string GRUPOS_PRODUCTO { get; set; }
        [JsonIgnore]
        public string EMPADRONAMIENTO { get; set; }

        [JsonIgnore]
        public DateTime dateinclude { get; set; }

        [JsonIgnore]
        public DateTime dateupdate { get; set; }
        [JsonIgnore]
        public string SEGMENTACION { get; set; }
        [JsonIgnore]
        public string CADENA { get; set; }
    }

    public class Catalogo_Productos
    {
        public int PK_Catalogo_Producto { get; set; }

        public string Cod_Prod { get; set; }

        public string Nom_Prod { get; set; }

        public string Concent { get; set; }

        public string Nom_Form_Farm { get; set; }

        public string Nom_Form_Farm_Simplif { get; set; }

        public string Presentac { get; set; }

        public float? Fracciones { get; set; }

        public DateTime? Fec_Vcto_Reg_Sanitario { get; set; }

        public string Num_RegSan { get; set; }

        public string Nom_Titular { get; set; }

        public string Situacion { get; set; }

        public string Clasi_ATC { get; set; }

        public string Prin_Activo { get; set; }

        public DateTime? dateinclude { get; set; }

        public DateTime? dateupdate { get; set; }
        public string Cod_GTIN { get; set; }
    }


    public class ProductSheet
    {
        public int PK_ProductSheet { get; set; }

        public string Medicine { get; set; }        

        public string Presentation { get; set; }

        public decimal Amount { get; set; }

        public string Country { get; set; }

        public string Registry { get; set; }

        public string Condition { get; set; }

        public string Trademark { get; set; }

        public string Titular { get; set; }

        public string Manufacturer { get; set; }

        public string Establishment { get; set; }

        public string Address { get; set; }

        public string Location { get; set; }

        public string Phone { get; set; }

        public string Schedule { get; set; }

        public string TechnicalDirector { get; set; }
        public string Cod_Prod { get; set; }
        public string Cod_Estab { get; set; }
    }

    public class ESTABLECIMIENTOS_UPDATE
    {
        public int PK_ESTABLECIMIENTOS_UPDATE { get; set; }

        public string CODIGO_ESTABLECIMIENTO { get; set; }

        public string DIRECCION_SALUD { get; set; }

        public string CLASIFICACION { get; set; }

        public string NOMBRECOMERCIAL { get; set; }

        public string RAZONSOCIAL { get; set; }

        public string DIRECCION { get; set; }

        public string DEPARTAMENTO { get; set; }

        public string PROVINCIA { get; set; }

        public string DISTRITO { get; set; }

        public string RUC { get; set; }

        public string ESTADO { get; set; }

        public string HORARIO { get; set; }

        public string GRUPOS_PRODUCTO { get; set; }

        public string EMPADRONAMIENTO { get; set; }

        public DateTime? dateinclude { get; set; }

        public DateTime? dateupdate { get; set; }

        public DateTime? FECHAINICIO { get; set; }

        public string CATEGORIA { get; set; }

        public string DEPPROVDIST { get; set; }
    }

    public class Product_Establishment
    {
        public int PK_Product_Establishment { get; set; }

        public string EstablishmentType { get; set; }

        public string Product { get; set; }

        public string Laboratory { get; set; }

        public string EstablishmentName { get; set; }

        public decimal Amount { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string Cod_Prod { get; set; }

        public string Cod_Estab { get; set; }
        public string UbigeoValue { get; set; }     
    }
}
