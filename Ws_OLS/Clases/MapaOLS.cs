using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics.Contracts;

namespace Ws_OLS.Clases
{
	public class RespuestaOLS
	{
        public string numeroDocumento { get; set; }
        public MapaResponse respuestaOlShttp { get; set; }
        public bool ResultadoSatisfactorio { get; set; }
        public string mensajeCompleto { get; set; }
    }

	public class OlsCampos
	{
		public List<Maindata> maindata { get; set; }
	}

	public class Maindata
	{
        //public string resolucion { get; set; }
        //public string resInicio { get; set; }
        //public string resFin { get; set; }
        //public string resFecha { get; set; }
        //public string nit { get; set; }
        //public string nrc { get; set; }
        //public string fechaEnvio { get; set; }
        //public string fechaEmision { get; set; }
        //public string terminal { get; set; }
        //public string numFactura { get; set; }
        //public string correlativoInterno { get; set; }
        //public string numeroTransaccion { get; set; }
        //public string codigoUsuario { get; set; }
        //public string nombreUsuario { get; set; }
        //public string correoUsuario { get; set; }
        //public string serie { get; set; }
        //public string cajaSuc { get; set; }
        //public string tipoDocumento { get; set; }
        //public string pdv { get; set; }
        //public string nitCliente { get; set; }
        //public string duiCliente { get; set; }
        //public string nrcCliente { get; set; }
        //public string codigoCliente { get; set; }
        //public string nombreCliente { get; set; }
        //public string direccionCliente { get; set; }
        //public string departamento { get; set; }
        //public string municipio { get; set; }
        //public string giro { get; set; }
        //public string codicionPago { get; set; }
        //public string montoLetras { get; set; }
        //public string CCFAnterior { get; set; }
        //public string vtaACuentaDe { get; set; }
        //public string notaRemision { get; set; }
        //public string noFecha { get; set; }
        //public double saldoCapital { get; set; }
        //public double sumas { get; set; }
        //public double subTotalVentasExentas { get; set; }
        //public double subTotalVentasNoSujetas { get; set; }
        //public double subTotalVentasGravadas { get; set; }
        //public double iva { get; set; }
        //public double renta { get; set; }
        //public double impuesto { get; set; }
        //public double ventasGravadas { get; set; }
        //public double ventasExentas { get; set; }
        //public double ventasNoSujetas { get; set; }
        //public double totalExportaciones { get; set; }
        //public double descuentos { get; set; }
        //public double abonos { get; set; }
        //public double cantidadTotal { get; set; }
        //public double ventaTotal { get; set; }
        //public double ventasGravadas13 { get; set; }
        //public double ventasGravadas0 { get; set; }
        //public double ventasNoGravadas { get; set; }
        //public double ivaPercibido1 { get; set; }
        //public double ivaPercibido2 { get; set; }
        //public double ivaRetenido1 { get; set; }
        //public double ivaRetenido13 { get; set; }
        //public double contribucionSeguridad { get; set; }
        //public double fovial { get; set; }
        //public double cotrans { get; set; }
        //public double contribucionTurismo5 { get; set; }
        //public double contribucionTurismo7 { get; set; }
        //public double impuestoEspecifico { get; set; }
        //public double cesc { get; set; }
        //public string observacionesDte { get; set; }
        //public string campo1 { get; set; }
        //public string campo2 { get; set; }
        //public string campo3 { get; set; }
        //public string campo4 { get; set; }

        public string direccionEmisor { get; set; }
        public string observacionesDte { get; set; }
        public double cantidadTotal { get; set; }
        public string pdv { get; set; }
        public string idMunicipioReceptor { get; set; }
        public string resFin { get; set; }
        public double fovial { get; set; }
        public double ventasGravadas0 { get; set; }
        public double totalExportaciones { get; set; }
        public string direccionCliente { get; set; }
        public string formatodocumento { get; set; }
        public string nitCliente { get; set; }
        public string correlativoInterno { get; set; }
        public double subTotalVentasGravadas { get; set; }
        public double ventaTotal { get; set; }
        public double contribucionSeguridad { get; set; }
        public string tipoItemExpor { get; set; }
        public string nit { get; set; }
        public double subTotalVentasNoSujetas { get; set; }
        public string campo1 { get; set; }
        public double ventasGravadas { get; set; }
        public double cesc { get; set; }
        public string campo3 { get; set; }
        public string campo2 { get; set; }
        public string nrc { get; set; }
        public string campo4 { get; set; }
        public string tipoDteRel { get; set; }
        public string tipoCatContribuyente { get; set; }
        public string CCFAnterior { get; set; }
        public string numeroTransaccion { get; set; }
        public string municipio { get; set; }
        public string codSucE { get; set; }
        public string resInicio { get; set; }
        public string fechaEmision { get; set; }
        public string idMunicipioEmisor { get; set; }
        public string emailE { get; set; }
        public string giro { get; set; }
        public List<Detalle> detalle { get; set; }
        public string idDepartamentoReceptor { get; set; }
        public string codigoActividadEconomica { get; set; }
        public string tipoDocumento { get; set; }
        public string fechaEnvio { get; set; }
        public string duiCliente { get; set; }
        public double sumas { get; set; }
        public double cotrans { get; set; }
        public double impuestoEspecifico { get; set; }
        public string vtaACuentaDe { get; set; }
        public double abonos { get; set; }
        public double ivaPercibido1 { get; set; }
        public double ivaPercibido2 { get; set; }
        public double renta { get; set; }
        public string nombrePais { get; set; }
        public double descuentos { get; set; }
        public string fechaHoraGeneracion { get; set; }
        public string tipoPersona { get; set; }
        public string resolucion { get; set; }
        public string numFactura { get; set; }
        public string nombreUsuario { get; set; }
        public double saldoCapital { get; set; }
        public double ventasExentas { get; set; }
        public string codicionPago { get; set; }
        public string numControl { get; set; }
        public string nombreCliente { get; set; }
        public double subTotalVentasExentas { get; set; }
        public string correoUsuario { get; set; }
        public double iva { get; set; }
        public double ivaRetenido13 { get; set; }
        public double ivaRetenido1 { get; set; }
        public double ventasGravadas13 { get; set; }
        public string noFecha { get; set; }
        public string resFecha { get; set; }
        public double ventasNoGravadas { get; set; }
        public string montoLetras { get; set; }
        public double ventasNoSujetas { get; set; }
        public double contribucionTurismo5 { get; set; }
        public double contribucionTurismo7 { get; set; }
        public string codPais { get; set; }
        public string terminal { get; set; }
        public string codGeneracion { get; set; }
        public string telsucE { get; set; }
        public string campoExtFE { get; set; }
        public string selloGeneracion { get; set; }
        public string idDepartamentoEmisor { get; set; }
        public double impuesto { get; set; }
        public string codigoUsuario { get; set; }
        public string notaRemision { get; set; }
        public string codigoCliente { get; set; }
        public string serie { get; set; }
        public string departamento { get; set; }
        public List<Contacto> contactos { get; set; }
        public string nrcCliente { get; set; }
        public string cajaSuc { get; set; }
        public string numeroControl { get; set; }
        public string codigoGeneracion { get; set; }
        public string modeloFacturacion { get; set; }
        public string tipoTransmision { get; set; }
        public object codContingencia { get; set; }
        public object motivoContin { get; set; }
        public object docRelTipo { get; set; }
        public object docRelNum { get; set; }
        public object docRelFecha { get; set; }
        public string nombreComercialCl { get; set; }
        public object otrosDocIdent { get; set; }
        public object otrosDocDescri { get; set; }
        public object ventCterNit { get; set; }
        public object ventCterNombre { get; set; }
        public double montGDescVentNoSujetas { get; set; }
        public double montGDescVentExentas { get; set; }
        public double montGDescVentGrav { get; set; }
        public double totOtroMonNoAfec { get; set; }
        public double totalAPagar { get; set; }
        public object responsableEmisor { get; set; }
        public object numDocEmisor { get; set; }
        public object responsableReceptor { get; set; }
        public object numDocReceptor { get; set; }
        public object nomConductor { get; set; }
        public object numIdenConductor { get; set; }
        public object modTransp { get; set; }
        public object numIdTransp { get; set; }
        public object formaPago { get; set; }
        public string plazo { get; set; }
        public double seguro { get; set; }
        public double flete { get; set; }
        public List<ArTributo> arTributos { get; set; }
        public bool mostrarTributo { get; set; }
        public string fInicioContin { get; set; }
        public string fFinContin { get; set; }
        public string horaIniContin { get; set; }
        public string horaFinContin { get; set; }
        public string tipoDocumentoReceptor { get; set; }
        public string bienTitulo { get; set; }

    }

	public class Contacto
	{
        public string whatsapp { get; set; }
        public string sms { get; set; }
        public string email { get; set; }
		public string telefono { get; set; }
	}

	public class Detalle
	{
        //public double cantidad { get; set; }
        //public string descripcion { get; set; }
        ////public double precioUnitario { get; set; }
        //public decimal precioUnitario { get; set; }
        //public double ventasNoSujetas { get; set; }
        //public double ventasExentas { get; set; }
        //public double ventasGravadas { get; set; }
        //public string desc { get; set; }
        //public string fecha { get; set; }
        //public string delAl { get; set; }
        //public string exportaciones { get; set; }

        public string descripcion { get; set; }
        public object codTributo { get; set; }
        public List<string> tributos { get; set; }
        public decimal precioUnitario { get; set; }
        public double ventasNoSujetas { get; set; }
        public decimal ivaItem { get; set; }
        public string delAl { get; set; }
        public string exportaciones { get; set; }
        public string numDocRel { get; set; }
        public int uniMedidaCodigo { get; set; }
        public double ventasExentas { get; set; }
        public string fecha { get; set; }
        public int tipoItem { get; set; }
        public string tipoDteRel { get; set; }
        public string codigoRetencionMH { get; set; }
        public double cantidad { get; set; }
        public double ventasGravadas { get; set; }
        public double ivaRetenido { get; set; }
        public string desc { get; set; }
        public double descuentoItem { get; set; }
        public double otroMonNoAfec { get; set; }
    }

	public class Unidadmedida
	{
		public string serie { get; set; }
	}

    public class ArTributo
    {
        public string codigoTributo { get; set; }
        public string descripcionTributo { get; set; }
        public double valorTributo { get; set; }
    }

}

//JSON FEL
//{
//    "maindata" : [


//{
//        "direccionEmisor": "residencial la gloria poligono F-3 #26",
//	"observacionesDte": "",
//	"cantidadTotal": 1.0,
//	"pdv": "CENTRAL",
//	"idMunicipioReceptor": "14",
//	"resFin": "",
//	"fovial": 0.0,
//	"ventasGravadas0": 0.0,
//	"totalExportaciones": 0.0,
//	"direccionCliente": "Blvd Constitución y Alameda Juan Pablo II, Col. Escalón, San Salvador",
//	"formatodocumento": "carta",
//	"nitCliente": "0614-130419-104-4",
//	"correlativoInterno": "CCF_20230323_15_44_41759",
//	"subTotalVentasGravadas": 5480.9972,
//	"ventaTotal": 5481.0,
//	"contribucionSeguridad": 0.0,
//	"tipoItemExpor": "",
//	"nit": "0614-110418-102-8",
//	"subTotalVentasNoSujetas": 0.0,
//	"campo1": "",
//	"ventasGravadas": 4850.44,
//	"cesc": 0.0,
//	"campo3": "",
//	"campo2": "",
//	"nrc": "268954-2",
//	"campo4": "",
//	"tipoCatContribuyente": "",
//	"CCFAnterior": "",
//	"numeroTransaccion": "",
//	"municipio": "SAN SALVADOR",
//	"codSucE": "",
//	"resInicio": "",
//	"fechaEmision": "23/03/2023",
//	"idMunicipioEmisor": "03",
//	"emailE": "",
//	"giro": "Planes se seguro",
//	"detalle": [{
//            "descripcion": "PLATAFORMA DE SOFTWARE DOC-I PORTAL",
//		"codTributo":null,			
//	    "tributos" : [ "20" ],	    
//		"precioUnitario": 4850.44,
//		"ventasNoSujetas": 0.0,
//		"ivaItem": 0.0,
//		"delAl": "",
//		"exportaciones": 0.0,
//		"numDocRel": "",
//		"uniMedidaCodigo": 0,
//		"ventasExentas": 0.0,
//		"fecha": "",
//		"tipoItem": 2,
//		"tipoDteRel": "",
//		"codigoRetencionMH": "",	  
//		"cantidad": 1.0,
//		"ventasGravadas": 4850.44,
//		"ivaRetenido": 0.0,
//		"desc": "",
//		"descuentoItem": 0.0,
//		"otroMonNoAfec": 0.0}],
//	"idDepartamentoReceptor": "06",
//	"codigoActividadEconomica": "65200",
//	"tipoDocumento": "CCF",
//	"fechaEnvio": "23/03/2023",
//	"duiCliente": "",
//	"sumas": 4850.44,
//	"cotrans": 0.0,
//	"impuestoEspecifico": 0.0,
//	"vtaACuentaDe": "",
//	"abonos": 0.0,
//	"ivaPercibido1": 0.0,
//	"ivaPercibido2": 0.0,
//	"renta": 0.0,
//	"nombrePais": "",
//	"descuentos": 0.0,
//	"fechaHoraGeneracion": "",
//	"tipoPersona": "",
//	"resolucion": "",
//	"numFactura": "",
//	"nombreUsuario": "",
//	"saldoCapital": 0.0,
//	"ventasExentas": 0.0,
//	"codicionPago": "CREDITO A 60 DIAS",
//	"numControl": "",
//	"nombreCliente": "PRUEBA, S.A. DE C.V",
//	"subTotalVentasExentas": 0.0,
//	"correoUsuario": "omar.lopez@ols.sv",
//	"iva": 630.5572,
//	"ivaRetenido13": 0.0,
//	"ivaRetenido1": 0.0,
//	"ventasGravadas13": 0.0,
//	"noFecha": "",
//	"resFecha": "",
//	"ventasNoGravadas": 0.0,
//	"montoLetras": "**CINCO MIL CUATROCIENTOS OCHENTA Y UNO 00\/100 U.S. DOLARES**",
//	"ventasNoSujetas": 0.0,
//	"contribucionTurismo5": 0.0,
//	"contribucionTurismo7": 0.0,
//	"codPais": "",
//	"terminal": "",
//	"codGeneracion": "",
//	"telsucE": "",
//	"campoExtFE": "",
//	"selloGeneracion": "",
//	"idDepartamentoEmisor": "02",
//	"impuesto": 0.0,
//	"codigoUsuario": "1",
//	"notaRemision": "",
//	"codigoCliente": "",
//	"serie": "",
//	"departamento": "SAN SALVADOR",
//	"contactos": [{
//            "whatsapp": "",
//		"sms": "",
//		"telefono": "63033014",
//		"email": "cliente.prueba@dominio.com"}],
//	"nrcCliente": "280357-3",
//	"cajaSuc": "OLS", 
//	"numeroControl" : "",
//    "codigoGeneracion" : "",	
//	"modeloFacturacion": "1",
//	"tipoTransmision": "1",
//	"codContingencia":null,
//	"motivoContin" : null,
//	"docRelTipo": null,
//	"docRelNum": null,
//	"docRelFecha": null,
//	"nombreComercialCl": "PRUEBA, S.A. DE C.V",
//	"otrosDocIdent": null,
//	"otrosDocDescri": null,		
//	"ventCterNit": null,
//	"ventCterNombre": null,	
//	"montGDescVentNoSujetas": 0.0,
//	"montGDescVentExentas": 0.0,
//	"montGDescVentGrav": 0.0,
//	"totOtroMonNoAfec": 0.0,
//	"totalAPagar": 5481.0,
//	"responsableEmisor": null,
//	"numDocEmisor": null,
//	"responsableReceptor": null,
//	"numDocReceptor": null,
//	"nomConductor": null,
//	"numIdenConductor": null,
//	"modTransp": null,
//	"numIdTransp": null,
//	"formaPago": null,
//	"plazo": "",
//	"seguro": 0.0,
//	"flete": 0.0,
//	"arTributos" : [ {
//            "codigoTributo" : "20",
//    "descripcionTributo" : "Impuesto al Valor Agregado 13%",
//    "valorTributo" : 1261.11} ],
//	"mostrarTributo": false,
//	"fInicioContin": "",
//	"fFinContin": "",
//	"horaIniContin": "",
//	"horaFinContin": ""

//    "tipoDocumentoReceptor": "",
//	"bienTitulo": ""

//    }
//		 ]
//}



//{
//    "maindata" : [
//        {
//            "resolucion" : "00000-REST-IN-00000-0000",
//            "resInicio" : "doc",
//            "resFin" : "doc000000",
//            "resFecha" : "3doc/0doc/20doc7",
//            "nit" : "06doc4-090588-doc03-8",
//            "nrc" : "doc227-3",
//            "fechaEnvio" : "04/doc2/2020 doc8:20:doc0",
//            "fechaEmision" : "04/doc2/2020",
//            "terminal" : "doc3doc",
//            "numFactura" : "doc50",
//            "correlativoInterno" : "doc5doc",
//            "numeroTransaccion" : "455477",
//            "codigoUsuario" : "456",
//            "nombreUsuario" : "Nombre Usuario",
//            "correoUsuario" : "usuario@dominio.com",
//            "serie" : "0000SC000docC",
//            "cajaSuc": "CENTRAL",
//            "tipoDocumento" : "CCF",
//            "pdv" : "TIENDA MERCADITO",
//            "nitCliente" : "06doc4-doc50doc78-doc03-9",
//            "duiCliente" : "doc2doc4587-doc",
//            "nrcCliente" : "doc254-7",
//            "codigoCliente" : "doc234",
//            "nombreCliente" : "Nombre completo Cliente",
//            "direccionCliente" :  "Direccion completa Cliente",
//            "departamento" : "San Savador",
//            "municipio" : "San Salvador",
//            "giro" : "XXXXXXXXXXXXXX",
//            "codicionPago" : "XXXXXXXXXXXX",
//            "montoLetras" : "XXXXXXXXXXXXX",
//            "CCFAnterior": "XXXXXXXXXXXXX",
//            "vtaACuentaDe" : "XXXXXXXXXXXXX",
//            "notaRemision" : "XXXXXXXXXXXXX",
//            "noFecha" : "XXXXXXXXXXXXX",
//            "saldoCapital" : 0,
//            "sumas" : 0,
//            "subTotalVentasExentas" : 0,
//            "subTotalVentasNoSujetas" : 0,
//			 "iva":doc254.doc2,
//            "ventasGravadas" : 4doc5doc.25,
//            "ventasExentas" : 4doc25.75,
//            "ventasNoSujetas" : doc225.doc5,
//            "totalExportaciones": 0.0,
//            "descuentos" : 0,
//            "abonos" : 0,
//            "cantidadTotal" : 2,
//            "ventaTotal" : 2325.45,
//            "ventasGravadasdoc3" : 0,
//            "ventasGravadas0" : 0,
//            "ventasNoGravadas" : 0,
//            "ivaPercibidodoc" : 0,
//            "ivaPercibido2" : 0,
//            "ivaRetenidodoc" : 0,
//            "ivaRetenidodoc3" : 0,
//            "contribucionSeguridad" : 0,
//            "fovial" : 0,
//            "cotrans" : 0,
//            "contribucionTurismo5" : 0,
//            "contribucionTurismo7" : 0,
//            "impuestoEspecifico" : 0,
//            "cesc" : doc2.65,
//            "observacionesDte" : "",
//            "campodoc" : "",
//            "campo2" : "",
//            "campo3" : "",
//            "campo4" : "",
//            "contactos" : [
//                {
//                    "email" : "correodoc@dominio.com",
//                    "telefono": "0000docdocdocdoc"
//                },
//                {
//                    "email" : "correo2@dominio.com",
//                    "telefono": "00002222"
//                }
//            ],
//            "detalle" : [
//                {
//                    "cantidad" : doc,
//                    "descripcion" : "Chocolate Blanco",
//                    "precioUnitario" : doc.50,
//                    "ventasNoSujetas" : 0,
//                    "ventasExentas" : 0,
//					"ventasGravadas" : doc.50,
//                    "desc" : "(5.00%) doc.26",
//					"fecha" : "0doc/doc0/2020",
//                    "delAl" : "Del doc - al doc00",
//					"exportaciones" : 0.0,
//                    "unidadMedida" : [
//                        {
//                            "serie" : "(ICC 8950304doc0280908796)"
//                        },
//                        {
//                            "serie" : "(#REF 37350200)"
//                        },
//                        {
//                            "serie" : "(#REF ICC 8950304doc02809087963)"
//                        }
//                    ]
//                },
//                {
//                    "cantidad" : doc0,
//                    "descripcion" : "Camisa Sportdoc",
//                    "precioUnitario" : 8.75,
//                    "ventasNoSujetas" : 0,
//                    "ventasExentas" : 0,
//                    "ventasGravadas" : 87.50,
//                    "desc" : "(doc5.00%) doc.26",
//                    "fecha" : "02/doc0/2020", 
//                    "delAl" : "Del doc00 - al doc0doc", 
//					"exportaciones" : 0.0, 
//                    "unidadMedida" :  [
//                        {
//                            "serie" : "77567896"
//                        }
//                    ]
//                }
//            ]
//        }
//    ]
//}