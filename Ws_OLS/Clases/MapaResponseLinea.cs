using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ws_OLS.Clases
{
    public class MapaResponseLinea
    {
        public class RespuestaLinea
        {
            public string response { get; set; }
            public string message { get; set; }
        }

        public class EnvioLinea
        {
            public string nitEmisor { get; set; }
        }

        public class Doctype
        {
            public string doctype { get; set; }
        }

        public class EnvioConsulta
        {
            public string nitEmisor { get; set; }
            public string ambiente { get; set; }
            public List<Doctype> doctypes { get; set; }
        }

        public class DoctypeRespuesta
        {
            public string doctype { get; set; }
            public string numerocontrol { get; set; }
            public string codigodegeneracion { get; set; }
            public DateTime? fechaemision { get; set; }
            public string sello { get; set; }
            public string sellorecibidoinvalidado { get; set; }
            public string codgeneracioninvalidado { get; set; }
            public string statusmh { get; set; }
            public string mh { get; set; }
            public string message { get; set; }
        }

        public class RespuestaConsulta
        {
            public string response { get; set; }
            public List<DoctypeRespuesta> doctypes { get; set; }
            public object messages { get; set; }
        }





    }
}