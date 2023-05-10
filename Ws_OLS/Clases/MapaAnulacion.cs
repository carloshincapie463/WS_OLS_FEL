using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ws_OLS.Clases
{
	public class MapaAnulacion
	{
        //public string fechaDoc { get; set; }
        //public int numDoc { get; set; }
        //public string tipoDoc { get; set; }
        //public string nit { get; set; }
        //public string correlativoInterno { get; set; }
        //public string fechaAnulacion { get; set; }

        public string fechaDoc { get; set; }
        public int numDoc { get; set; }
        public string tipoDoc { get; set; }
        public string nit { get; set; }
        public string correlativointerno { get; set; }
        public string fechaAnulacion { get; set; }
        public int tipoAnulacion { get; set; }
        public string codigoGeneracion { get; set; }
        public string codigoGeneracionR { get; set; }
        public string motivoAnulacion { get; set; }
        public string nombreResponsable { get; set; }
        public string tipDocResponsable { get; set; }
        public string numDocResponsable { get; set; }
        public string nombreSolicita { get; set; }
        public string tipDocsolicita { get; set; }
        public string numDocSolicita { get; set; }
    }
}