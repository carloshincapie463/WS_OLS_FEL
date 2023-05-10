using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ws_OLS.Clases
{
    public class MapaResponse
    {
        public string correlativoInterno { get; set; }
        public string fechaDoc { get; set; }
        public string fechaRes { get; set; }
        public string itemnum { get; set; }
        public string message { get; set; }
        public string numDoc { get; set; }
        public string resolucion { get; set; }
        public string serie { get; set; }
        public string tipoDoc { get; set; }
        public string urlMail { get; set; }
        public string urlSMS { get; set; }
        public string msgHDA { get; set; }
        public string codigoGeneracion { get; set; }
        public string selloRecibido { get; set; }
        public string fhProcesamiento { get; set; }
        public string numControl { get; set; }
    }

    public class MapaResponseAnulacion
    {
        public string result { get; set; }
    }
}