using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VERONICA.DigemidBrowser
{
    public class Root
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public List<Datum> data { get; set; }
        public object entidad { get; set; }
        public object cantidad { get; set; }
        public object codigoEntidad { get; set; }
        public object codigosValidos { get; set; }
        public object paginas { get; set; }
        public object totalData { get; set; }
        public object resultadoProceso { get; set; }
    }

    public class Datum
    {
        public object codigoProducto { get; set; }
        public string nombreProducto { get; set; }
        public string concent { get; set; }
        public object presentacion { get; set; }
        public object fracciones { get; set; }
        public string nombreFormaFarmaceutica { get; set; }
        public object nroRegistroSanitario { get; set; }
        public object titular { get; set; }
        public int grupo { get; set; }
        public string codGrupoFF { get; set; }
    }

    public class RootProductos
    {
        public string codigo { get; set; }
        public string mensaje { get; set; }
        public List<DatumProducto> data { get; set; }
        public Entidad entidad { get; set; }
        public object cantidad { get; set; }
        public object codigoEntidad { get; set; }
        public object codigosValidos { get; set; }
        public object paginas { get; set; }
        public object totalData { get; set; }
        public object resultadoProceso { get; set; }
    }

    public class Entidad
    {
        public string codEstab { get; set; }
        public int codProdE { get; set; }
        public object fecha { get; set; }
        public object nombreProducto { get; set; }
        public double precio1 { get; set; }
        public object precio2 { get; set; }
        public object precio3 { get; set; }
        public object codGrupoFF { get; set; }
        public object ubicodigo { get; set; }
        public object direccion { get; set; }
        public object telefono { get; set; }
        public object nomGrupoFF { get; set; }
        public object setcodigo { get; set; }
        public object nombreComercial { get; set; }
        public object grupo { get; set; }
        public object totalPA { get; set; }
        public object concent { get; set; }
        public object nombreFormaFarmaceutica { get; set; }
        public int fracciones { get; set; }
        public object totalRegistros { get; set; }
        public object nombreLaboratorio { get; set; }
        public object nombreTitular { get; set; }
        public object catCodigo { get; set; }
        public string nombreSustancia { get; set; }
        public object fabricante { get; set; }
        public object departamento { get; set; }
        public object provincia { get; set; }
        public object distrito { get; set; }
    }

    public class DatumProducto
    {
        public string codEstab { get; set; }
        public int codProdE { get; set; }
        public string fecha { get; set; }
        public string nombreProducto { get; set; }
        public double precio1 { get; set; }
        public object precio2 { get; set; }
        public object precio3 { get; set; }
        public string codGrupoFF { get; set; }
        public string ubicodigo { get; set; }
        public string direccion { get; set; }
        public string telefono { get; set; }
        public string nomGrupoFF { get; set; }
        public string setcodigo { get; set; }
        public string nombreComercial { get; set; }
        public string grupo { get; set; }
        public string totalPA { get; set; }
        public string concent { get; set; }
        public string nombreFormaFarmaceutica { get; set; }
        public int fracciones { get; set; }
        public int totalRegistros { get; set; }
        public string nombreLaboratorio { get; set; }
        public string nombreTitular { get; set; }
        public string catCodigo { get; set; }
        public string nombreSustancia { get; set; }
        public object fabricante { get; set; }
        public string departamento { get; set; }
        public string provincia { get; set; }
        public string distrito { get; set; }
    }

    public class ErrorRoot
    {
        public DateTime timestamp { get; set; }
        public int status { get; set; }
        public string error { get; set; }
        public string message { get; set; }
        public string path { get; set; }
    }
}
