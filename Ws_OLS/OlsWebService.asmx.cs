using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Services;
using Ws_OLS.Clases;

namespace Ws_OLS
{
    /// <summary>
    /// Descripción breve de OlsWebServiceasmx
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    public class OlsWebServiceasmx : System.Web.Services.WebService
    {
        private Facturas _facturas = new Facturas();
        private FacturasSalaVentas _facturasSala = new FacturasSalaVentas();
        private readonly Creditos_Fiscales _fCreditos = new Creditos_Fiscales();
        private readonly Notas_de_Credito _nCreditos = new Notas_de_Credito();
        private readonly NotasCreditoHH _nCreditosHH = new NotasCreditoHH();
        private readonly Notas_de_Remision _nRemision = new Notas_de_Remision();
        private readonly Rutas _rutas = new Rutas();
        private readonly OlsCampos _ols = new OlsCampos(); //cabeza
        private readonly Maindata _olsMain = new Maindata(); //central
        private ControlDatosOLS controlOLS = new ControlDatosOLS();
        private int[] Documentos = new int[] { 1, 2, 3, 6, 7 };
        private string FC_tipo = "";
        private string FC_estado = "";
        private int anulacion = 0;

        //GENERA TOKEN DIARIO

        //VALORES DEL CONFIG
        //URL
        private readonly string UrlToken = ConfigurationManager.AppSettings["Token"];

        private readonly string UrlJson = ConfigurationManager.AppSettings["EnvioJSON"];
        private readonly string UrlJsonAnulacion = ConfigurationManager.AppSettings["EnvioJSONAnulacion"];
        private readonly string UrlRevisaLinea = ConfigurationManager.AppSettings["RevisaLinea"];
        private readonly string UrlObtieneFactura = ConfigurationManager.AppSettings["InfoFactura"];
        private readonly string AmbienteCodigo = ConfigurationManager.AppSettings["ambiente"];

        //CREDENCIALES
        private readonly string UsuarioHead = ConfigurationManager.AppSettings["Usuario"];

        private readonly string PasswordHead = ConfigurationManager.AppSettings["PasswordHead"];

        //CREDENCIALES
        private readonly string Usuario = ConfigurationManager.AppSettings["userName"];

        private readonly string Password = ConfigurationManager.AppSettings["password"];

        private readonly string IdCompany = ConfigurationManager.AppSettings["idCompany"];

        /// <summary>
        /// ENVIO GLOBAL DE DATOS
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        [WebMethod(Description = "Envia todos los documentos a OLS por ruta")]
        public string EnvioFacturacionXRuta(int ruta, string fecha)
        {
            string Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            string respuestaInternaGlobal = "";
            for (int i = 0; i < Documentos.Length; i++)
            {
                //ITERA DEL 1 AL 6 REPRESENTANDO CADA DOCUMENTO
                //1-Factura     //FACTUAEBAJADA
                //2-Nota de Credito **ccfanterior / Reparto.DocumentosFacturasEBajada -- Liquidacion
                //3-Hoja de Carga //NOTA REMISION //handheld.NotaRemisionBajada
                //6-Comprobante de Credito Fiscal //FACTURAEBAJADA
                //7-ANULACION CLIENTE FINAL //HandHeld.FacturaEBajada

                if (Documentos[i] == 1 || Documentos[i] == 6 || Documentos[i] == 7)
                {
                    if (Documentos[i] == 1)
                    {
                        FC_tipo = "F";
                        FC_estado = "FAC";
                    }
                    else if (Documentos[i] == 6)
                    {
                        FC_tipo = "C";
                        FC_estado = "FAC";
                    }
                    else
                    {
                        FC_tipo = "F";
                        FC_estado = "ANU";
                        anulacion = 0;
                    }

                    respuestaInternaGlobal = respuestaInternaGlobal + EnviaFacturas(ruta, fecha, Documentos[i], -1, true);
                }
                else if (Documentos[i] == 2)
                {
                    //respuestaInternaGlobal = respuestaInternaGlobal + EnviarNotasCreditos(ruta, fecha, Documentos[i], -1);
                }
                else if (Documentos[i] == 3)
                {
                    respuestaInternaGlobal = respuestaInternaGlobal + EnviarNotasRemision(ruta, fecha, Documentos[i], -1);
                }
            }

            return respuestaInternaGlobal;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <param name="docPos"></param>
        /// <param name="numFac"></param>
        /// <returns></returns>
        [WebMethod(Description = "Revisa si el servicio esta en linea")]
        public MapaResponseLinea.RespuestaLinea RevisaServicioOLS()
        {
            string Token = "";
            string revisaT = RevisarToken();
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            MapaResponseLinea.RespuestaLinea respuesta = new MapaResponseLinea.RespuestaLinea();
            List<MapaResponseLinea.EnvioLinea> listaOLS = new List<MapaResponseLinea.EnvioLinea>();
            MapaResponseLinea.EnvioLinea maindata = new MapaResponseLinea.EnvioLinea();
            maindata.nitEmisor = "0614-130571-001-2";
            listaOLS.Add(maindata);

            string jsonString = JsonConvert.SerializeObject(listaOLS);
            string jsonFinal = jsonString;

            RestClient cliente = new RestClient(UrlRevisaLinea)
            {
                //Authenticator = new HttpBasicAuthenticator(Usuario, Password),
                Timeout = 900000
            };
            RestRequest request = new RestRequest
            {
                Method = Method.POST
            };
            request.Parameters.Clear();
            request.AddHeader("Authorization", Token);
            request.AddParameter("application/json", jsonFinal, ParameterType.RequestBody);
            IRestResponse respond = cliente.Execute(request);
            string content = respond.Content;
            HttpStatusCode httpStatusCode = respond.StatusCode;
            int numericStatusCode = (int)httpStatusCode;

            if (numericStatusCode == 200) //REVISA SU CODIGO DE ESTADO, SI ES 200 NO HAY ERROR EN EL JSON
            {
                //JObject jsonObjectX = JObject.Parse(content);
                dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                string jsontt = jsonRespuesta.result;
                //dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
                //string resultJson = jsonRespuesta2.result;
                dynamic resultObject = JsonConvert.DeserializeObject(jsontt);
                string docs = resultObject.ToString();
                string jsonTotal = @"[" + docs + "]";
                List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);
            }

            //    string docs = resultObject.ToString();
            //    string jsonTotal = @"[" + docs + "]";
            //    List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);

            //    if (jsonDocs[0].status == 1) //SI EL MENSAJE ES OK, EL JSON LLEGO A OLS
            //    {
            //        string facturaTemp = DatosRawAnulacion.correlativoInterno;
            //        int rutaTemp = Convert.ToInt32(fac_ruta[1]);

            //        string descripcion = "Documento anulado a las " + DateTime.Now.ToString();
            //        controlOLS.ActualizaHH_Anulacion(descripcion, jsonDocs[0].selloRecibido, jsonDocs[0].codigoGeneracion, Convert.ToInt32(fac_ruta[1]), fac_ruta[0]);
            //        controlOLS.BorraReparto_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0]);
            //        controlOLS.BorraDevolucion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0]);
            //        controlOLS.InsertaAnulacion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], fac_ruta[2]);
            //        controlOLS.BorraPagos_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0]);

            //        //string resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
            //        //string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

            //        //controlOLS.RecLogBitacora(
            //        //						1,
            //        //						"ANU",
            //        //						Convert.ToInt32(facturaTemp),
            //        //						resolucionTemp,
            //        //						serieTemp,
            //        //						"Documento enviado para la ruta " + rutaTemp,
            //        //						numericStatusCode
            //        //					  ); //SE REGISTRA EN LA BITACORA

            //        //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
            //        //resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
            //        //facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();

            //        //controlOLS.CambiaEstadoFANU(
            //        //                    rutaTemp,
            //        //                    "F",
            //        //                    fechaAnu,
            //        //                    facturaTemp,
            //        //                    jsonDocs[0].selloRecibido
            //        //                  ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO

            //        //respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
            //        //               "Tipo documento: " + DIC + "\n" +
            //        //               "Error:" + jsonDocs[0].message + "\n" +
            //        //               "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

            //        //respuestaOLS.mensajeCompleto = respuestaMetodo;
            //        //respuestaOLS.respuestaOlShttp = jsonDocs[0];
            //        //respuestaOLS.numeroDocumento = facturaTemp;
            //        //respuestaOLS.ResultadoSatisfactorio = true;

            //        //return respuestaMetodo = @"Documento #" + facturaTemp + "enviado!!!\n" +
            //        //                "Tipo documento: ANU\n" +
            //        //                "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

            //        respuestaMetodo = @"Documento #" + fac_ruta[0] + "enviado!!!\n" +
            //                       "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
            //                       "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

            //        respuestaOLS1.mensajeCompleto = respuestaMetodo;
            //        respuestaOLS1.numeroDocumento = facturaTemp;
            //        respuestaOLS1.respuestaOlShttp = jsonDocs[0];
            //        //respuestaOLS.res = jsonDocs[0];

            //        return respuestaOLS1;
            //    }
            //    else
            //    {
            //        //string facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
            //        //int rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
            //        //string resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
            //        //string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

            //        //dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
            //        //dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
            //        //string resultJson = jsonRespuesta2.result;
            //        //dynamic resultObject = JsonConvert.DeserializeObject(resultJson);

            //        //string docs = resultObject.ToString();
            //        //string jsonTotal = @"[" + docs + "]";
            //        //List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);

            //        anulacion = 0;
            //        //controlOLS.RecLogBitacora(
            //        //						0,
            //        //						"ANU",
            //        //						Convert.ToInt32(facturaTemp),
            //        //						resolucionTemp,
            //        //						serieTemp,
            //        //						jsonDocs[0].result + " en la ruta: " + rutaTemp,
            //        //						numericStatusCode
            //        //					  ); //SE REGISTRA ERROR EN LA BITACORA
            //        //return respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
            //        //                "Tipo documento: ANU\n" +
            //        //                "Error:" + jsonDocs[0].result + "\n" +
            //        //                "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

            //        respuestaMetodo = @"Documento #" + fac_ruta[0] + " no fue enviado!!!\n" +
            //                       "Tipo documento: F " +
            //                       "Error:" + jsonDocs[0].statusMsg + "\n" +
            //                       "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

            //        respuestaOLS1.mensajeCompleto = respuestaMetodo;
            //        respuestaOLS1.numeroDocumento = fac_ruta[0];
            //        respuestaOLS1.respuestaOlShttp = jsonDocs[0];
            //        //respuestaOLS.res = jsonDocs[0];

            //        return respuestaOLS1;

            //        //respuestaOLS.mensajeCompleto = respuestaMetodo;
            //        //respuestaOLS.respuestaOlShttp = jsonDocs[0];
            //        //respuestaOLS.numeroDocumento = fac_ruta[0];
            //        //respuestaOLS.ResultadoSatisfactorio = true;
            //    }
            //}
            //else
            //{
            //    if (numericStatusCode == 999)
            //    {
            //        respuestaMetodo = @"Documento #" + fac_ruta[0] + "enviado!!!\n" +
            //                      "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
            //                      "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

            //        respuestaOLS1.mensajeCompleto = respuestaMetodo;
            //        respuestaOLS1.numeroDocumento = fac_ruta[0];
            //        respuestaOLS1.respuestaOlShttp = null;
            //        //respuestaOLS.res = jsonDocs[0];

            //        return respuestaOLS1;
            //    }

            //    dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
            //    dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
            //    string resultJson = jsonRespuesta2.result;
            //    dynamic resultObject = JsonConvert.DeserializeObject(resultJson);

            //    string docs = resultObject.ToString();
            //    string jsonTotal = @"[" + docs + "]";
            //    List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);
            //    //controlOLS.RecLogBitacora(
            //    //							0,
            //    //							"ANU",
            //    //							Convert.ToInt32(facturaTemp),
            //    //							resolucionTemp,
            //    //							serieTemp,
            //    //							"BAD REQUEST: Debido a Gateway time o Error de Sintaxis en la ruta: " + DatosRaw.Select(x => x.cajaSuc).ToString(),
            //    //							numericStatusCode
            //    //						  ); //SE REGISTRA ERROR EN LA BITACORA
            //    //return respuestaMetodo = @"Documento #" + DatosRaw.Select(x => x.numFactura).ToString() + "no fue anulado!!!\n" +
            //    //                        "Tipo documento: FAC/ANU\n" +
            //    //                        "Error:BAD REQUEST: Debido a Gateway time o Error de Sintaxis\n" +
            //    //                        "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

            //    respuestaMetodo = @"Documento #" + fac_ruta[0] + "no fue ANULADO!!!\n" +
            //                      "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
            //                      "Error:" + jsonDocs[0].statusMsg + "\n" +
            //                      "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

            //    respuestaOLS1.mensajeCompleto = respuestaMetodo;
            //    respuestaOLS1.numeroDocumento = fac_ruta[0];
            //    respuestaOLS1.respuestaOlShttp = null;

            return respuesta;
        }

        [WebMethod(Description = "Revisa si datos estan en OLS/Hacienda")]
        public List<MapaResponseLinea.RespuestaConsulta> RevisaInformacionDocumento(string iden, string numeroFac)
        {
            string Token = "";
            string revisaT = RevisarToken();
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            MapaResponseLinea.RespuestaConsulta respuesta = new MapaResponseLinea.RespuestaConsulta();
            List<MapaResponseLinea.EnvioConsulta> listaOLS = new List<MapaResponseLinea.EnvioConsulta>();
            MapaResponseLinea.EnvioConsulta maindata = new MapaResponseLinea.EnvioConsulta();
            maindata.nitEmisor = "0614-130571-001-2";
            maindata.ambiente = AmbienteCodigo;

            List<MapaResponseLinea.Doctype> doctypes = new List<MapaResponseLinea.Doctype>
                {
                    new MapaResponseLinea.Doctype
                    {
                        doctype=iden+","+numeroFac
                    }
                };
            maindata.doctypes = doctypes;

            listaOLS.Add(maindata);

            string jsonString = JsonConvert.SerializeObject(listaOLS);
            string jsonFinal = jsonString;
            string jsonSinCorchetes = jsonFinal.Trim('[', ']');

            RestClient cliente = new RestClient(UrlObtieneFactura)
            {
                //Authenticator = new HttpBasicAuthenticator(Usuario, Password),
                Timeout = 900000
            };
            RestRequest request = new RestRequest
            {
                Method = Method.POST
            };

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            request.Parameters.Clear();
            request.AddHeader("Authorization", Token);
            request.AddParameter("application/json", jsonSinCorchetes, ParameterType.RequestBody);
            IRestResponse respond = cliente.Execute(request);
            string content = respond.Content;
            HttpStatusCode httpStatusCode = respond.StatusCode;
            int numericStatusCode = (int)httpStatusCode;

            List<MapaResponseLinea.RespuestaConsulta> jsonDocs;

            if (numericStatusCode == 200) //REVISA SU CODIGO DE ESTADO, SI ES 200 NO HAY ERROR EN EL JSON
            {
                //JObject jsonObjectX = JObject.Parse(content);

                dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                //string jsontt = jsonRespuesta.result;
                ////dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
                ////string resultJson = jsonRespuesta2.result;
                //dynamic resultObject = JsonConvert.DeserializeObject(jsontt);
                //string docs = resultObject.ToString();
                string jsonTotal = @"[" + jsonRespuesta + "]";
                jsonTotal = jsonTotal.Replace("nulo", "");
                jsonDocs = JsonConvert.DeserializeObject<List<MapaResponseLinea.RespuestaConsulta>>(jsonTotal);
            }
            else
            {
                jsonDocs = null;
            }

            return jsonDocs;
        }

        /// <summary>
        /// ENVIA FACTURAS POR RUTA Y FECHA
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        [WebMethod(Description = "Envia solo Facturas/Creditos Fiscales/Anulaciones")]
        public RespuestaOLS EnviaFacturas(int ruta, string fecha, int docPos, long numFac, bool reenvioOLS)
        {
            string Token = "";
            string revisaT = RevisarToken();
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            RespuestaOLS respuestaOLS = new RespuestaOLS();

            string respuestaProceso = "";
            if (docPos == 1)
            {
                FC_tipo = "F";
                FC_estado = "FAC";
            }
            else if (docPos == 6)
            {
                FC_tipo = "C";
                FC_estado = "FAC";
            }

            List<MapaResponseLinea.RespuestaConsulta> respuestaCons = new List<MapaResponseLinea.RespuestaConsulta>();
            //*****FACTURAS O COMPROBATES DE CREDITO FISCAL******/
            /*****TABLA: Handheld.FacturaEBajada**********/
            string respuestaEnvio = "";
            string respuestaAnulacion = "";
            string correlativoConsulta = "";
            string tipoDocCon = "";
            bool facturasFEL = false;
            int idSerieCon = 0;
            int idRutaCons = 0;
            string fechaCon = "";
            string numeroCon = "";
            //facturasFEL = _facturas.GetRutaFEL(ruta);
            List<Maindata> ListaOLS = new List<Maindata>();
            ListaOLS.Clear();
            DataTable FacturasTabla;
            if (docPos == 7)
            {
                FacturasTabla = _facturas.CantidadFacturas(ruta, FC_tipo, FC_estado, fecha, numFac, docPos);
            }
            else
            {
                FacturasTabla = _facturas.CantidadFacturas(ruta, FC_tipo, FC_estado, fecha, numFac, docPos);
            }

            //REVISA DATOS SI EXISTE FACTURA O NO
            foreach (DataRow row in FacturasTabla.Rows)
            {
                correlativoConsulta = "AV_" + row["idSerie"].ToString().Trim() + "_" + row["Numero"].ToString().Trim();
                idSerieCon = Convert.ToInt32(row["idSerie"].ToString().Trim());
                idRutaCons = Convert.ToInt32(row["IdRuta"].ToString().Trim());
                tipoDocCon = row["TipoDocumento"].ToString().Trim() == "F" ? "fac" : "ccf";
                DateTime fechaHora = DateTime.Parse(row["Fecha"].ToString());
                fechaCon = fechaHora.ToString("yyyy-MM-dd");
                numeroCon = row["Numero"].ToString().Trim();
                respuestaCons = RevisaInformacionDocumento(tipoDocCon, correlativoConsulta);
            }

            if (respuestaCons == null)
            {
                respuestaOLS.mensajeCompleto = "Servicio caido";
                respuestaOLS.respuestaOlShttp = null;
                respuestaOLS.numeroDocumento = correlativoConsulta;
                respuestaOLS.ResultadoSatisfactorio = false;
                respuestaOLS.esContigencia = true;
                return respuestaOLS;
            }

            //respuestaCons[0].doctypes[0].mh = null;

            if (respuestaCons[0].doctypes[0].mh == null)
            {
                respuestaOLS.mensajeCompleto = "Hacienda caida";
                respuestaOLS.respuestaOlShttp = null;
                respuestaOLS.numeroDocumento = correlativoConsulta;
                respuestaOLS.ResultadoSatisfactorio = false;
                respuestaOLS.esContigencia = true;
                return respuestaOLS;
            }

            if (respuestaCons[0].doctypes[0].mh == "0")
            {
                respuestaOLS.mensajeCompleto = "Hacienda caida";
                respuestaOLS.respuestaOlShttp = null;
                respuestaOLS.numeroDocumento = correlativoConsulta;
                respuestaOLS.ResultadoSatisfactorio = false;
                respuestaOLS.esContigencia = true;
                return respuestaOLS;
            }
            else
            {
                if (respuestaCons[0].doctypes[0].statusmh == "3" || respuestaCons[0].doctypes[0].statusmh == "4" || respuestaCons[0].doctypes[0].statusmh == "5") // 3-OK  4-INVALIDADO CORRECTO
                                                                                                                                                                  //if (respuestaCons[0].doctypes[0].statusmh == "3" || respuestaCons[0].doctypes[0].statusmh == "4") // 3-OK  4-INVALIDADO CORRECTO
                {
                    respuestaCons[0].doctypes[0].sello = respuestaCons[0].doctypes[0].sello.Replace("nulo", "");
                    respuestaCons[0].doctypes[0].codigodegeneracion = respuestaCons[0].doctypes[0].codigodegeneracion.Replace("nulo", "");
                    respuestaCons[0].doctypes[0].numerocontrol = respuestaCons[0].doctypes[0].numerocontrol.Replace("nulo", "");

                    if (!String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].sello) && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].codigodegeneracion) && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].numerocontrol) && respuestaCons[0].doctypes[0].sello != "0")
                    {
                        string respuestaMetodo = @"Documento #" + correlativoConsulta + " enviado!!!\n" +
                                            "Tipo documento: " + tipoDocCon == "ccf" ? "CCF" : "FAC" + "\n" +
                                            "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                        respuestaOLS.mensajeCompleto = respuestaMetodo;
                        respuestaOLS.numeroDocumento = correlativoConsulta;

                        //List<MapaResponse> respuestaOLSX = new List<MapaResponse>();
                        MapaResponse respuestaOLSX = new MapaResponse();
                        respuestaOLSX.codigoGeneracion = respuestaCons[0].doctypes[0].codigodegeneracion;
                        respuestaOLSX.selloRecibido = respuestaCons[0].doctypes[0].sello;
                        respuestaOLSX.numControl = respuestaCons[0].doctypes[0].numerocontrol;

                        respuestaOLS.respuestaOlShttp = respuestaOLSX;
                        respuestaOLS.ResultadoSatisfactorio = true;

                        ControlDatosOLS controlOls = new ControlDatosOLS();
                        //controlOLS.CambiaEstadoSello(idRutaCons, tipoDocCon == "ccf" ? "C" : "F", fechaCon, numeroCon, respuestaOLSX.selloRecibido);
                        controlOLS.CambiaEstadoSello(idRutaCons, tipoDocCon == "ccf" ? "C" : "F", fechaCon, numeroCon, respuestaOLSX.selloRecibido, respuestaOLSX.codigoGeneracion, respuestaOLSX.numControl);

                        return respuestaOLS;
                        //}
                        //else if ((String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].sello) || respuestaCons[0].doctypes[0].sello == "0") && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].codigodegeneracion) && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].numerocontrol))
                        //else if ((String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].sello) || respuestaCons[0].doctypes[0].sello == "0") && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].codigodegeneracion) && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].numerocontrol))
                        //{
                        //    MapaResponse respuestaOLSX = new MapaResponse();
                        //    respuestaOLSX.codigoGeneracion = respuestaCons[0].doctypes[0].codigodegeneracion;
                        //    respuestaOLSX.selloRecibido = respuestaCons[0].doctypes[0].sello;
                        //    respuestaOLSX.numControl = respuestaCons[0].doctypes[0].numerocontrol;

                        //    respuestaOLS.mensajeCompleto = "Contigencia sin firmar";
                        //    respuestaOLS.respuestaOlShttp = respuestaOLSX;
                        //    respuestaOLS.numeroDocumento = correlativoConsulta;
                        //    respuestaOLS.ResultadoSatisfactorio = false;

                        //    return respuestaOLS;
                        //}
                    }
                }
                else if (respuestaCons[0].doctypes[0].statusmh == "0") //0-CONTIGENCIA SIN FIRMAR
                {
                    MapaResponse respuestaOLSX = new MapaResponse();
                    respuestaOLSX.codigoGeneracion = respuestaCons[0].doctypes[0].codigodegeneracion;
                    respuestaOLSX.selloRecibido = respuestaCons[0].doctypes[0].sello;
                    respuestaOLSX.numControl = respuestaCons[0].doctypes[0].numerocontrol;

                    respuestaOLS.mensajeCompleto = "Contigencia sin firmar, por favor validar en 3 minutos!!";
                    respuestaOLS.respuestaOlShttp = respuestaOLSX;
                    respuestaOLS.numeroDocumento = correlativoConsulta;
                    respuestaOLS.ResultadoSatisfactorio = false;

                    return respuestaOLS;
                }
                else if (respuestaCons[0].doctypes[0].statusmh == "2" && !reenvioOLS) //0-CONTIGENCIA SIN FIRMAR
                {
                    MapaResponse respuestaOLSX = new MapaResponse();
                    respuestaOLSX.codigoGeneracion = respuestaCons[0].doctypes[0].codigodegeneracion;
                    respuestaOLSX.selloRecibido = respuestaCons[0].doctypes[0].sello;
                    respuestaOLSX.numControl = respuestaCons[0].doctypes[0].numerocontrol;

                    respuestaOLS.mensajeCompleto = "ERROR-" + respuestaCons[0].doctypes[0].message;
                    respuestaOLS.respuestaOlShttp = respuestaOLSX;
                    respuestaOLS.numeroDocumento = correlativoConsulta;
                    respuestaOLS.ResultadoSatisfactorio = false;
                    respuestaOLS.esContigencia = true;

                    return respuestaOLS;
                }
            }

            //itera y recupera campos de las tablas
            foreach (DataRow row in FacturasTabla.Rows)
            {
                try
                {
                    Maindata maindata = new Maindata();

                    #region Cabecera

                    ListaOLS.Clear();
                    maindata.resolucion = _facturas.GetResolucion(ruta, row["idSerie"].ToString()).Trim();
                    maindata.resInicio = _facturas.GetResInicio(ruta, row["idSerie"].ToString()).Trim();
                    maindata.resFin = _facturas.GetResFin(ruta, row["idSerie"].ToString()).Trim();
                    //maindata.nit = _facturas.GetNit(ruta, row["idSerie"].ToString()).Trim();
                    maindata.nit = "0614-130571-001-2";

                    //maindata.resFecha = (_facturas.GetRestFecha(ruta, row["idSerie"].ToString())).Substring(0, _facturas.GetRestFecha(ruta, row["idSerie"].ToString()).Length - 9);  //SIN HORA SOLO FECHA
                    if (string.IsNullOrWhiteSpace(_facturas.GetRestFecha(ruta, row["idSerie"].ToString())))
                    {
                        maindata.resFecha = (Convert.ToDateTime("01/01/1900")).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.resFecha = (Convert.ToDateTime(_facturas.GetRestFecha(ruta, row["idSerie"].ToString()))).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }

                    maindata.nrc = "233-0";
                    maindata.fechaEnvio = DateTime.Now.ToString().Trim();
                    //maindata.fechaEmision = (row["Fechahora"].ToString()).Substring(0, row["Fechahora"].ToString().Length - 9); //SIN FECHA SOLO HORA
                    //maindata.fechaEmision = (Convert.ToDateTime(row["Fechahora"].ToString())).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    if (string.IsNullOrWhiteSpace(row["Fechahora"].ToString()))
                    {
                        maindata.fechaEmision = (Convert.ToDateTime("01/01/1900")).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.fechaEmision = (Convert.ToDateTime(row["Fechahora"].ToString())).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    maindata.terminal = ruta.ToString().Trim();
                    maindata.numFactura = row["Numero"].ToString().Trim();
                    maindata.correlativoInterno = "AV_" + row["idSerie"].ToString().Trim() + "_" + row["Numero"].ToString().Trim();
                    maindata.numeroTransaccion = row["NumeroPedido"].ToString().Trim(); //numero de pedido
                    maindata.codigoUsuario = row["idempleado"].ToString().Trim();
                    maindata.nombreUsuario = _facturas.GetNombreUsuario(row["idempleado"].ToString());
                    maindata.correoUsuario = "";
                    maindata.serie = _facturas.GetNumSerie(ruta, row["idSerie"].ToString()).Trim();
                    string str = _facturas.GetRutaReparto(ruta.ToString());
                    int targetLength = 4; // Longitud deseada de 4 posiciones

                    string paddedStr = str.PadLeft(targetLength, '0');
                    maindata.cajaSuc = _facturas.GetCentro(ruta.ToString()) + paddedStr;
                    maindata.tipoDocumento = row["TipoDocumento"].ToString().Trim() == "F" ? "FAC" : "CCF";
                    maindata.pdv = _facturas.GetNombreEstablecimiento(row["IdCliente"].ToString()); //ESTABLECIMIENTO
                                                                                                    //maindata.nitCliente = _facturas.GetNITCliente(row["IdCliente"].ToString().Trim()).Trim(); //DEBE APLICARSE TRIM
                    string tipoDocTempNIT = "";
                    if (docPos == 1) //SI ES FACTURA O ANULACION
                    {
                        maindata.nitCliente = _facturas.GetDUI(row["IdCliente"].ToString().Trim()).Trim();        //BUSCA EL DUI
                        if (maindata.nitCliente == "")
                        {
                            maindata.nitCliente = _facturas.GetNITCliente(row["IdCliente"].ToString().Trim()).Trim(); //SI EL DUI ES VACIO BUSCA EL NIT
                            if (maindata.nitCliente == "")
                            {
                                //maindata.nitCliente = "00000000000000"; //SI EL NIT ES VACIO ENVIA CEROS
                                //tipoDocTempNIT = "Otro";

                                maindata.nitCliente = ""; //SI EL NIT ES VACIO ENVIA CEROS
                                tipoDocTempNIT = "";
                            }
                            else
                            {
                                tipoDocTempNIT = "NIT";
                            }
                        }
                        else
                        {
                            tipoDocTempNIT = "DUI";
                        }
                    }
                    else //CREDITO FISCAL
                    {
                        //maindata.duiCliente = _facturas.GetDUI(row["IdCliente"].ToString().Trim()).Trim();        //DEBE APLICARSE TRIM
                        maindata.nitCliente = _facturas.GetNITCliente(row["IdCliente"].ToString().Trim()).Trim(); //DEBE APLICARSE TRIM
                        tipoDocTempNIT = "NIT";
                        //maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    }
                    //maindata.nrcCliente = "06141305710012";  //DEBE APLICARSE TRIM
                    maindata.duiCliente = _facturas.GetDUI(row["IdCliente"].ToString().Trim()).Trim();        //DEBE APLICARSE TRIM
                    if (FC_tipo == "C") //OBIG CFF, NCM, NR
                    {
                        maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    }
                    else
                    {
                        maindata.nrcCliente = "";  //DEBE APLICARSE TRIM
                    }
                    //if (docPos == 1)
                    //{
                    //    maindata.nrcCliente = "";  //DEBE APLICARSE TRIM
                    //    maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    //    if (maindata.nrcCliente == "")
                    //    {
                    //        maindata.nrcCliente = maindata.nitCliente;
                    //    }

                    //}
                    //else
                    //{
                    //    maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    //}
                    maindata.codigoCliente = row["IdCliente"].ToString().Trim();
                    maindata.nombreCliente = _facturas.GetNombreCliente(maindata.codigoCliente).Trim();
                    maindata.direccionCliente = _facturas.GetDireccion(maindata.codigoCliente).Trim();
                    maindata.departamento = _facturas.GetDepartamento(maindata.codigoCliente).Trim();
                    maindata.municipio = _facturas.GetMunicipio(maindata.codigoCliente).Trim();
                    //maindata.giro = _facturas.GetGiroNegocio(maindata.codigoCliente).Trim();
                    ///LLENAR CATALOGO
                    if (FC_tipo == "C") //OBIG CFF, NCM, NR
                    {
                        maindata.codigoActividadEconomica = _facturas.GetGiroNegocio2(row["IdCliente"].ToString().Trim());
                        maindata.giro = _facturas.GetActividadEconomica(maindata.codigoActividadEconomica);
                    }
                    else  //FAC
                    {
                        maindata.codigoActividadEconomica = "";
                        maindata.giro = "";
                    }

                    maindata.codicionPago = row["IdCondicionPago"].ToString().Trim() == "1" ? "CONTADO" : "CREDITO";
                    //maindata.ventaTotal = Convert.ToDouble(row["Total"].ToString());
                    if (FC_tipo == "C")
                    {
                        //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["TotalIva"]);
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ventaTotal = Convert.ToDouble(row["Total"].ToString());
                        }
                        else
                        {
                            maindata.ventaTotal = Convert.ToDouble(Convert.ToDecimal(row["Total"].ToString()) - Convert.ToDecimal(row["Percepcion"]));
                            //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["TotalIva"]);
                        }
                    }
                    else
                    {
                        maindata.ventaTotal = Convert.ToDouble(row["Total"].ToString());
                    }
                    maindata.montoLetras = _facturas.GetMontoLetras(maindata.ventaTotal).Trim();
                    maindata.CCFAnterior = "";
                    maindata.vtaACuentaDe = "";
                    maindata.notaRemision = "";
                    maindata.noFecha = "";
                    maindata.saldoCapital = 0;
                    maindata.idDepartamentoReceptor = _facturas.GetIdDepartamento(row["idCliente"].ToString());
                    maindata.idDepartamentoEmisor = "05";
                    maindata.direccionEmisor = "0";
                    //maindata.fechaEnvio = DateTime.Now.Date.ToString();
                    maindata.idMunicipioEmisor = "11";
                    maindata.idMunicipioReceptor = _facturas.GetIdMunicipio(row["IdCliente"].ToString());
                    //maindata.codigoActividadEconomica = "01460";
                    maindata.tipoCatContribuyente = "0";

                    if (FC_tipo == "F")
                    {
                        maindata.sumas = Convert.ToDouble(row["Total"].ToString());
                    }
                    else
                    {
                        maindata.sumas = Convert.ToDouble(row["SubTotal"].ToString());
                    }
                    maindata.subTotalVentasExentas = 0;
                    maindata.subTotalVentasNoSujetas = 0;
                    if (FC_tipo == "C")
                    {
                        //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["TotalIva"]);
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.subTotalVentasGravadas = Convert.ToDouble(Convert.ToDecimal(row["SubTotal"]) + Convert.ToDecimal(row["TotalIva"]));
                        }
                        else
                        {
                            //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["TotalIva"])+Convert.ToDouble(row["Percepcion"]);
                            maindata.subTotalVentasGravadas = Convert.ToDouble(Convert.ToDecimal(row["SubTotal"]) + Convert.ToDecimal(row["TotalIva"]));
                        }
                    }
                    else
                    {
                        maindata.subTotalVentasGravadas = 0;
                    }

                    if (FC_tipo == "F")
                    {
                        maindata.iva = Convert.ToDouble(row["TotalIva"].ToString());
                    }
                    else
                    {
                        maindata.iva = Convert.ToDouble(row["TotalIva"].ToString());
                    }
                    maindata.renta = 0;
                    maindata.impuesto = Convert.ToDouble(row["TotalIva"].ToString());
                    if (FC_tipo == "C")
                    {
                        maindata.ventasGravadas = Convert.ToDouble(row["SubTotal"].ToString());
                    }
                    else
                    { //SI ES FACTYRA
                        maindata.ventasGravadas = Convert.ToDouble(row["Total"].ToString());
                    }
                    //maindata.ventasGravadas = Convert.ToDouble(row["SubTotal"].ToString());
                    maindata.ventasExentas = 0;
                    maindata.ventasNoSujetas = 0;
                    maindata.totalExportaciones = 0;
                    maindata.descuentos = Convert.ToDouble(row["TotalDescuentos"].ToString());
                    maindata.abonos = 0;
                    maindata.cantidadTotal = _facturas.GetCantidadTotal(ruta, row["Numero"].ToString());
                    maindata.ventasGravadas13 = 0;
                    maindata.ventasGravadas0 = 0;
                    maindata.ventasNoGravadas = 0;
                    //maindata.ivaPercibido1 = Convert.ToDouble(row["Total"].ToString());
                    if (FC_tipo == "F")
                    {
                        maindata.ivaPercibido1 = 0;
                        maindata.ivaPercibido2 = 0;
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ivaRetenido1 = 0;
                        }
                        else
                        {
                            maindata.ivaRetenido1 = Convert.ToDouble(row["Percepcion"].ToString());
                        }
                    }
                    else
                    {
                        maindata.ivaRetenido1 = 0;
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ivaPercibido1 = 0;
                        }
                        else
                        {
                            maindata.ivaPercibido1 = Convert.ToDouble(row["Percepcion"].ToString());
                        }
                    }
                    maindata.ivaRetenido13 = 0;
                    maindata.contribucionSeguridad = 0;
                    maindata.fovial = 0;
                    maindata.cotrans = 0;
                    maindata.contribucionTurismo5 = 0;
                    maindata.contribucionTurismo7 = 0;
                    maindata.impuestoEspecifico = 0;
                    maindata.cesc = 0;
                    maindata.observacionesDte = "";
                    maindata.campo1 = "";
                    maindata.campo2 = "0|" + row["IdCliente"].ToString() + "|" + _facturas.GetCodigoClientePrincipal(row["IdCliente"].ToString()) + "|" + _facturas.GetCentro(ruta.ToString()) + "|" + _facturas.GetZonaRuta(ruta.ToString()) + "|" + _facturas.GetCodigoRutaVenta(ruta.ToString()) + "|";

                    if (FC_tipo == "F") //añade si tipofacturacion
                    {
                        maindata.campo2 = maindata.campo2 + "GT58|";
                    }
                    else
                    {
                        maindata.campo2 = maindata.campo2 + "GT57|";
                    }

                    maindata.campo2 = maindata.campo2 + "|" + _facturas.GetRutaVenta(row["IdCliente"].ToString()) + "|" + _facturas.GetRutaReparto(ruta.ToString());
                    maindata.campo2 = maindata.campo2 + "||||||||||||";
                    if (row["NumeroPedido"].ToString() == "" || row["NumeroPedido"].ToString() == "0") //revisa la secuencia
                    {
                        maindata.campo2 = maindata.campo2 + "000";
                    }
                    else
                    {
                        maindata.campo2 = maindata.campo2 + _facturas.GetSecuencia(row["NumeroPedido"].ToString());
                    }
                    
                    maindata.campo2 = maindata.campo2 + "OC:"+_facturas.GetOrdenCompraHH(row["NumeroPedido"].ToString());
                    maindata.campo3 = "";
                    maindata.campo4 = "||||";

                    //CAMPOS NUEVOS FEL

                    maindata.numeroControl = _facturas.GetCodigoNumControl(ruta, FC_tipo, fecha, row["Numero"].ToString());
                    maindata.codigoGeneracion = _facturas.GetCodigoGeneracion(ruta, FC_tipo, fecha, row["Numero"].ToString());
                    maindata.modeloFacturacion = maindata.codigoGeneracion != "0" ? "2" : "1";
                    maindata.tipoTransmision = maindata.codigoGeneracion != "0" ? "2" : "1";
                    maindata.codContingencia = maindata.codigoGeneracion == "0" ? "" : "3";
                    if (maindata.codigoGeneracion == "0")
                    {
                        maindata.codigoGeneracion = null;
                    }
                    if (maindata.numeroControl == "0")
                    {
                        maindata.numeroControl = null;
                    }
                    maindata.motivoContin = null;
                    maindata.fInicioContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("yyyy-MM-dd") : null;
                    maindata.fFinContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("yyyy-MM-dd") : null;
                    maindata.horaIniContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("HH:mm:ss") : null;
                    maindata.horaFinContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("HH:mm:ss") : null;
                    maindata.docRelTipo = null;  //SOLO PARA NC Y NR
                    maindata.docRelNum = null;
                    maindata.docRelFecha = null;
                    maindata.nombreComercialCl = "";
                    maindata.otrosDocIdent = ""; //F-CCF      //*****PREGUNTAR*****//
                    maindata.otrosDocDescri = "";       //F-CCF                    //*****PREGUNTAR*****//
                    maindata.ventCterNit = "";
                    maindata.ventCterNombre = "";
                    maindata.montGDescVentNoSujetas = 0.0;
                    maindata.montGDescVentExentas = 0.0;
                    maindata.montGDescVentGrav = 0.0;
                    maindata.totOtroMonNoAfec = 0.0;
                    maindata.totalAPagar = Convert.ToDouble(row["Total"].ToString());
                    maindata.responsableEmisor = "";
                    maindata.numDocEmisor = "";
                    maindata.responsableReceptor = "";
                    maindata.numDocReceptor = "";
                    maindata.nomConductor = "";
                    maindata.numIdenConductor = "";
                    maindata.modTransp = "";
                    maindata.numIdTransp = "";
                    maindata.formaPago = "";
                    maindata.plazo = "";
                    maindata.seguro = 0.0;
                    maindata.flete = 0.0;
                    maindata.arTributos = null;

                    ArTributo tributox = new ArTributo();
                    tributox.valorTributo = Convert.ToDouble(row["TotalIva"].ToString());
                    tributox.codigoTributo = "20";
                    tributox.descripcionTributo = "Impuesto al Valor Agregado 13%";
                    maindata.arTributos = new List<ArTributo>();
                    maindata.arTributos.Add(tributox);

                    maindata.mostrarTributo = false;
                    maindata.bienTitulo = "0";
                    maindata.tipoDocumentoReceptor = tipoDocTempNIT;

                    //CALLEJAS
                    maindata.campoExtFE = "OrdenCompra|Número de Orden de Compra|" + _facturas.GetOrdenCompraHH(row["NumeroPedido"].ToString())+"|||";

                    //maindata.mostrarTributo = false;
                    //maindata.bienTitulo = "0";
                    //maindata.tipoDocumentoReceptor = tipoDocTempNIT;
                    maindata.formatodocumento = "movil400";

                    ListaOLS.Add(maindata);

                    #endregion Cabecera

                    #region Contacto

                    //REGION CONTACTO
                    List<Contacto> ListaContactos = new List<Contacto>
                {
                    new Contacto
                        {
                            whatsapp="",
                            sms="",
                            email = _facturas.GetCorreo(ListaOLS[0].codigoCliente)=="" || _facturas.GetCorreo(ListaOLS[0].codigoCliente)==null ? "cmia-fel-sv@somoscmi.com":_facturas.GetCorreo(ListaOLS[0].codigoCliente),
                            telefono = _facturas.GetTelefono(ListaOLS[0].codigoCliente)=="" || _facturas.GetTelefono(ListaOLS[0].codigoCliente)==null ? "22021000":_facturas.GetTelefono(ListaOLS[0].codigoCliente),
                        }
                };
                    maindata.contactos = ListaContactos;

                    #endregion Contacto

                    #region Detalle

                    //DETALLE
                    DataTable DetalleFactura = _facturas.CantidadDetalle(ruta, row["Numero"].ToString(), fecha, Convert.ToInt32(row["idSerie"].ToString()));

                    List<Detalle> detalleOLS = new List<Detalle>();

                    List<Unidadmedida> UnidadeMedidaOLS = new List<Unidadmedida>();

                    double cantidadTem = 0;

                    if (docPos == 6) //es un detalle diferente si es un CCF
                    {
                        foreach (DataRow rowDeta in DetalleFactura.Rows)
                        {
                            if (_facturas.CompruebaUnidadMedida(rowDeta["IdProductos"].ToString()) == "1")
                            {
                                //cantidadTem = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                                cantidadTem = Convert.ToDouble(rowDeta["Unidades"].ToString());
                            }
                            else
                            {
                                //cantidadTem = _facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                                cantidadTem = Convert.ToDouble(rowDeta["Peso"].ToString());
                            }
                            Detalle detalle = new Detalle();

                            detalleOLS.Add(
                                new Detalle
                                {
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    descripcion = rowDeta["IdProductos"].ToString() + "|" + "PLU:" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + Convert.ToDouble(rowDeta["Unidades"].ToString()) + "|" + Convert.ToDouble(rowDeta["Peso"].ToString()) + "|",
                                    codTributo = null,
                                    tributos = new List<string>() { "20" },
                                    //precioUnitario = _facturas.GetPrecioUnitarioDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()), //-
                                    precioUnitario = Convert.ToDecimal((Convert.ToDecimal(rowDeta["PrecioUnitario"].ToString()) + Convert.ToDecimal(rowDeta["DescuentoPorPrecio"].ToString())).ToString("N4")),
                                    ventasNoSujetas = 0,
                                    //ivaItem = _facturas.GetIVALineaFac(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()), //-
                                    ivaItem = String.IsNullOrWhiteSpace(rowDeta["Iva"].ToString()) ? 0 : Convert.ToDecimal(rowDeta["Iva"].ToString()),
                                    delAl = "",
                                    exportaciones = "0.0",
                                    numDocRel = "",
                                    uniMedidaCodigo = _facturas.GetUnidadFacturacion(rowDeta["IdProductos"].ToString()) == 1 ? 59 : 36,
                                    ventasExentas = 0,
                                    fecha = "",
                                    tipoItem = 2, //CONSULTAR
                                    tipoDteRel = "",
                                    codigoRetencionMH = "",
                                    cantidad = cantidadTem,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    ventasGravadas = Convert.ToDouble((Convert.ToDouble(rowDeta["Valor"].ToString()) - Convert.ToDouble(rowDeta["Iva"].ToString())).ToString("N4")),
                                    ivaRetenido = 0.0,
                                    //desc = _facturas.GetDescuentoPrecioDetalle(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    desc = rowDeta["DescuentoPorPrecio"].ToString(),
                                    descuentoItem = 0.0,
                                    otroMonNoAfec = 0.0
                                });
                            //detalleOLS.Add(detalle);
                            maindata.detalle = detalleOLS;
                        }
                    }
                    else
                    {
                        foreach (DataRow rowDeta in DetalleFactura.Rows)
                        {
                            if (_facturas.CompruebaUnidadMedida(rowDeta["IdProductos"].ToString()) == "1")
                            {
                                //cantidadTem = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                                cantidadTem = Convert.ToDouble(rowDeta["Unidades"].ToString());
                            }
                            else
                            {
                                //cantidadTem = _facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                                cantidadTem = Convert.ToDouble(rowDeta["Peso"].ToString());
                            }

                            Detalle detalle = new Detalle();
                            detalleOLS.Add(
                                new Detalle
                                {
                                    //cantidad = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    //precioUnitario = _facturas.GetPrecioUnitarioDetalleFAC(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //ventasNoSujetas = 0,
                                    //ventasExentas = 0,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //desc = _facturas.GetDescuentoPrecioDetalle(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //fecha = "",
                                    //delAl = "",
                                    //exportaciones = "0.0"

                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + Convert.ToDouble(rowDeta["Peso"].ToString()) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    descripcion = rowDeta["IdProductos"].ToString() + "|" + "PLU:" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + Convert.ToDouble(rowDeta["Unidades"].ToString()) + "|" + Convert.ToDouble(rowDeta["Peso"].ToString()) + "|",
                                    codTributo = null,
                                    tributos = null,
                                    //precioUnitario = _facturas.GetPrecioUnitarioDetalleFAC(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()), //-
                                    precioUnitario = _facturas.CompruebaUnidadMedida(rowDeta["IdProductos"].ToString()) == "1" ? Convert.ToDecimal((Convert.ToDecimal(rowDeta["Valor"].ToString()) / Convert.ToDecimal(rowDeta["Unidades"].ToString())).ToString("N4")) : Convert.ToDecimal((Convert.ToDecimal(rowDeta["Valor"].ToString()) / Convert.ToDecimal(rowDeta["Peso"].ToString())).ToString("N4")),
                                    ventasNoSujetas = 0,
                                    //ivaItem = _facturas.GetIVALineaFac(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()), //-
                                    ivaItem = String.IsNullOrWhiteSpace(rowDeta["Iva"].ToString()) ? 0 : Convert.ToDecimal(rowDeta["Iva"].ToString()),
                                    delAl = "",
                                    exportaciones = "0.0",
                                    numDocRel = "",
                                    uniMedidaCodigo = _facturas.GetUnidadFacturacion(rowDeta["IdProductos"].ToString()) == 1 ? 59 : 36,
                                    ventasExentas = 0,
                                    fecha = "",
                                    tipoItem = 2, //CONSULTAR
                                    tipoDteRel = "",
                                    codigoRetencionMH = "",
                                    //cantidad = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    cantidad = cantidadTem,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    ventasGravadas = Convert.ToDouble(rowDeta["Valor"].ToString()),
                                    ivaRetenido = 0.0,
                                    //desc = _facturas.GetDescuentoPrecioDetalle(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    desc = rowDeta["DescuentoPorPrecio"].ToString(),
                                    descuentoItem = 0.0,
                                    otroMonNoAfec = 0.0
                                });
                            //detalleOLS.Add(detalle);
                            maindata.detalle = detalleOLS;
                        }
                    }

                    #endregion Detalle

                    if (docPos == 7 && row["FELAutorizacion"].ToString() != "" && row["FeLAnulacionNumero"].ToString() == "")
                    {
                        anulacion = 1;
                    }
                    else if (docPos == 7 && row["FELAutorizacion"].ToString() == "" && row["FeLAnulacionNumero"].ToString() == "")
                    {
                        #region ENVIAR/RECEPCION DATA

                        respuestaOLS = EnvioDataOLS(ListaOLS, docPos, fecha, Token);
                        respuestaEnvio = respuestaOLS.mensajeCompleto;

                        #endregion ENVIAR/RECEPCION DATA
                    }
                    else if (docPos != 7)
                    {
                        #region ENVIAR/RECEPCION DATA

                        respuestaOLS = EnvioDataOLS(ListaOLS, docPos, fecha, Token);
                        respuestaEnvio = respuestaOLS.mensajeCompleto;

                        #endregion ENVIAR/RECEPCION DATA
                    }

                    #region Anulacion

                    if (anulacion == 1)
                    {
                        //MAPEA CAMPOS
                        List<MapaAnulacion> ListaAnular = new List<MapaAnulacion>();
                        MapaAnulacion mapaAnulacion = new MapaAnulacion
                        {
                            fechaDoc = (Convert.ToDateTime(maindata.fechaEmision)).ToString("dd-MM-yyyy"),
                            numDoc = Convert.ToInt32(maindata.numFactura),
                            tipoDoc = "FAC_movil",
                            correlativoInterno = (maindata.numFactura),
                            nitEmisor = "0614-130571-001-2",
                            fechaAnulacion = (Convert.ToDateTime(maindata.fechaEmision)).ToString("dd/MM/yyyy"),
                        };
                        ListaAnular.Add(mapaAnulacion);
                        //respuestaAnulacion = EnviaDataAnulacion(ListaAnular, ListaOLS, fecha);

                        respuestaProceso = respuestaProceso + respuestaEnvio + "\n" + respuestaAnulacion;
                    }
                    else
                    {
                        respuestaProceso = respuestaProceso + respuestaEnvio;
                    }

                    #endregion Anulacion
                }
                catch (Exception ex)
                {
                    var s = new StackTrace(ex);
                    var thisasm = Assembly.GetExecutingAssembly();
                    var methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                    string errorMsj = @"Error interno:" + ex.Message.ToString() + "\n" +
                             "Metodo:" + methodname;
                    //GrabarErrorInternos(ruta, fecha, docPos, numFac, errorMsj);
                    respuestaOLS.mensajeCompleto = errorMsj;
                    respuestaOLS.ResultadoSatisfactorio = false;
                }
            }

            anulacion = 0;
            return respuestaOLS;
        }

        /// <summary>
        /// ENVIA FACTURAS POR RUTA Y FECHA
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        [WebMethod(Description = "Envia solo Facturas/Creditos Fiscales/Anulaciones de Salas de Ventas")]
        public RespuestaOLS EnviaFacturasSalaVentas(int ruta, string fecha, int docPos, long numFac, bool reenvioOLS)
        {
            string Token = "";
            string revisaT = RevisarToken();
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            RespuestaOLS respuestaOLS = new RespuestaOLS();

            string respuestaProceso = "";
            if (docPos == 1)
            {
                FC_tipo = "F";
                FC_estado = "FAC";
            }
            else if (docPos == 6)
            {
                FC_tipo = "C";
                FC_estado = "FAC";
            }

            List<MapaResponseLinea.RespuestaConsulta> respuestaCons = new List<MapaResponseLinea.RespuestaConsulta>();
            //*****FACTURAS O COMPROBATES DE CREDITO FISCAL******/
            /*****TABLA: Handheld.FacturaEBajada**********/
            string respuestaEnvio = "";
            string respuestaAnulacion = "";
            string correlativoConsulta = "";
            string tipoDocCon = "";
            bool facturasFEL = false;
            int idSerieCon = 0;
            int idRutaCons = 0;
            string fechaCon = "";
            string numeroCon = "";
            //facturasFEL = _facturas.GetRutaFEL(ruta);
            List<Maindata> ListaOLS = new List<Maindata>();
            ListaOLS.Clear();
            DataTable FacturasTabla;

            FacturasTabla = _facturasSala.CantidadFacturas(ruta, fecha, numFac);
            //REVISA DATOS SI EXISTE FACTURA O NO
            foreach (DataRow row in FacturasTabla.Rows)
            {
                correlativoConsulta = "AV_" + row["NumeroSerie"].ToString().Trim() + "_" + row["Correlativo"].ToString().Trim();
                idSerieCon = Convert.ToInt32(row["NumeroSerie"].ToString().Trim());
                idRutaCons = Convert.ToInt32(row["IdSucursal"].ToString().Trim());
                tipoDocCon = row["TipoDoc"].ToString().Trim() == "F" ? "fac" : "ccf";
                DateTime fechaHora = DateTime.Parse(row["FechaHora"].ToString());
                fechaCon = fechaHora.ToString("yyyy-MM-dd");
                numeroCon = row["Correlativo"].ToString().Trim();
                respuestaCons = RevisaInformacionDocumento(tipoDocCon, correlativoConsulta);
            }

            if (respuestaCons == null)
            {
                respuestaOLS.mensajeCompleto = "Servicio OLS caido";
                respuestaOLS.respuestaOlShttp = null;
                respuestaOLS.numeroDocumento = correlativoConsulta;
                respuestaOLS.ResultadoSatisfactorio = false;
                respuestaOLS.esContigencia = true;
                return respuestaOLS;
            }

            //respuestaCons[0].doctypes[0].mh = null;

            if (respuestaCons[0].doctypes[0].mh == null)
            {
                respuestaOLS.mensajeCompleto = "Hacienda caida";
                respuestaOLS.respuestaOlShttp = null;
                respuestaOLS.numeroDocumento = correlativoConsulta;
                respuestaOLS.ResultadoSatisfactorio = false;
                respuestaOLS.esContigencia = true;
                return respuestaOLS;
            }

            if (respuestaCons[0].doctypes[0].mh == "0")
            {
                respuestaOLS.mensajeCompleto = "Hacienda caida";
                respuestaOLS.respuestaOlShttp = null;
                respuestaOLS.numeroDocumento = correlativoConsulta;
                respuestaOLS.ResultadoSatisfactorio = false;
                respuestaOLS.esContigencia = true;
                return respuestaOLS;
            }
            else
            {
                if (respuestaCons[0].doctypes[0].statusmh == "3" || respuestaCons[0].doctypes[0].statusmh == "4" || respuestaCons[0].doctypes[0].statusmh == "5") // 3-OK  4-INVALIDADO CORRECTO
                                                                                                                                                                  //if (respuestaCons[0].doctypes[0].statusmh == "3" || respuestaCons[0].doctypes[0].statusmh == "4") // 3-OK  4-INVALIDADO CORRECTO
                {
                    respuestaCons[0].doctypes[0].sello = respuestaCons[0].doctypes[0].sello.Replace("nulo", "");
                    respuestaCons[0].doctypes[0].codigodegeneracion = respuestaCons[0].doctypes[0].codigodegeneracion.Replace("nulo", "");
                    respuestaCons[0].doctypes[0].numerocontrol = respuestaCons[0].doctypes[0].numerocontrol.Replace("nulo", "");

                    if (!String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].sello) && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].codigodegeneracion) && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].numerocontrol) && respuestaCons[0].doctypes[0].sello != "0")
                    {
                        string respuestaMetodo = @"Documento #" + correlativoConsulta + " enviado!!!\n" +
                                            "Tipo documento: " + tipoDocCon == "ccf" ? "CCF" : "FAC" + "\n" +
                                            "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                        respuestaOLS.mensajeCompleto = respuestaMetodo;
                        respuestaOLS.numeroDocumento = correlativoConsulta;

                        //List<MapaResponse> respuestaOLSX = new List<MapaResponse>();
                        MapaResponse respuestaOLSX = new MapaResponse();
                        respuestaOLSX.codigoGeneracion = respuestaCons[0].doctypes[0].codigodegeneracion;
                        respuestaOLSX.selloRecibido = respuestaCons[0].doctypes[0].sello;
                        respuestaOLSX.numControl = respuestaCons[0].doctypes[0].numerocontrol;

                        respuestaOLS.respuestaOlShttp = respuestaOLSX;
                        respuestaOLS.ResultadoSatisfactorio = true;

                        ControlDatosOLS controlOls = new ControlDatosOLS();
                        controlOLS.CambiaEstadoSello_SalaVenta(idRutaCons, tipoDocCon == "ccf" ? "C" : "F", fechaCon, numeroCon, respuestaOLSX.selloRecibido, respuestaOLSX.codigoGeneracion, respuestaOLSX.numControl);

                        return respuestaOLS;
                        //}
                        //else if ((String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].sello) || respuestaCons[0].doctypes[0].sello == "0") && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].codigodegeneracion) && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].numerocontrol))
                        //else if ((String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].sello) || respuestaCons[0].doctypes[0].sello == "0") && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].codigodegeneracion) && !String.IsNullOrWhiteSpace(respuestaCons[0].doctypes[0].numerocontrol))
                        //{
                        //    MapaResponse respuestaOLSX = new MapaResponse();
                        //    respuestaOLSX.codigoGeneracion = respuestaCons[0].doctypes[0].codigodegeneracion;
                        //    respuestaOLSX.selloRecibido = respuestaCons[0].doctypes[0].sello;
                        //    respuestaOLSX.numControl = respuestaCons[0].doctypes[0].numerocontrol;

                        //    respuestaOLS.mensajeCompleto = "Contigencia sin firmar";
                        //    respuestaOLS.respuestaOlShttp = respuestaOLSX;
                        //    respuestaOLS.numeroDocumento = correlativoConsulta;
                        //    respuestaOLS.ResultadoSatisfactorio = false;

                        //    return respuestaOLS;
                        //}
                    }
                }
                else if (respuestaCons[0].doctypes[0].statusmh == "0") //0-CONTIGENCIA SIN FIRMAR
                {
                    MapaResponse respuestaOLSX = new MapaResponse();
                    respuestaOLSX.codigoGeneracion = respuestaCons[0].doctypes[0].codigodegeneracion;
                    respuestaOLSX.selloRecibido = respuestaCons[0].doctypes[0].sello;
                    respuestaOLSX.numControl = respuestaCons[0].doctypes[0].numerocontrol;

                    respuestaOLS.mensajeCompleto = "Contigencia sin firmar, por favor validar en 3 minutos!!";
                    respuestaOLS.respuestaOlShttp = respuestaOLSX;
                    respuestaOLS.numeroDocumento = correlativoConsulta;
                    respuestaOLS.ResultadoSatisfactorio = false;

                    return respuestaOLS;
                }
                else if (respuestaCons[0].doctypes[0].statusmh == "2" && !reenvioOLS) //0-CONTIGENCIA SIN FIRMAR
                {
                    MapaResponse respuestaOLSX = new MapaResponse();
                    respuestaOLSX.codigoGeneracion = respuestaCons[0].doctypes[0].codigodegeneracion;
                    respuestaOLSX.selloRecibido = respuestaCons[0].doctypes[0].sello;
                    respuestaOLSX.numControl = respuestaCons[0].doctypes[0].numerocontrol;

                    respuestaOLS.mensajeCompleto = "ERROR-" + respuestaCons[0].doctypes[0].message;
                    respuestaOLS.respuestaOlShttp = respuestaOLSX;
                    respuestaOLS.numeroDocumento = correlativoConsulta;
                    respuestaOLS.ResultadoSatisfactorio = false;
                    respuestaOLS.esContigencia = true;

                    return respuestaOLS;
                }
            }

            //itera y recupera campos de las tablas
            foreach (DataRow row in FacturasTabla.Rows)
            {
                try
                {
                    Maindata maindata = new Maindata();

                    #region Cabecera

                    ListaOLS.Clear();
                    maindata.resolucion = _facturasSala.GetResolucion(Convert.ToInt32(row["IdSucursal"].ToString()), row["NumeroSerie"].ToString()).Trim();
                    maindata.resInicio = _facturasSala.GetResInicio(Convert.ToInt32(row["IdSucursal"].ToString()), row["NumeroSerie"].ToString()).Trim();
                    maindata.resFin = _facturasSala.GetResFin(Convert.ToInt32(row["IdSucursal"].ToString()), row["NumeroSerie"].ToString()).Trim();
                    //maindata.nit = _facturas.GetNit(ruta, row["idSerie"].ToString()).Trim();
                    maindata.nit = "0614-130571-001-2";

                    //maindata.resFecha = (_facturas.GetRestFecha(ruta, row["idSerie"].ToString())).Substring(0, _facturas.GetRestFecha(ruta, row["idSerie"].ToString()).Length - 9);  //SIN HORA SOLO FECHA
                    if (string.IsNullOrWhiteSpace(_facturasSala.GetRestFecha(Convert.ToInt32(row["IdSucursal"].ToString()), row["NumeroSerie"].ToString())))
                    {
                        maindata.resFecha = (Convert.ToDateTime("01/01/1900")).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.resFecha = (Convert.ToDateTime(_facturasSala.GetRestFecha(Convert.ToInt32(row["IdSucursal"].ToString()), row["NumeroSerie"].ToString()))).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }

                    maindata.nrc = "233-0";
                    maindata.fechaEnvio = DateTime.Now.ToString().Trim();
                    //maindata.fechaEmision = (row["Fechahora"].ToString()).Substring(0, row["Fechahora"].ToString().Length - 9); //SIN FECHA SOLO HORA
                    //maindata.fechaEmision = (Convert.ToDateTime(row["Fechahora"].ToString())).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    if (string.IsNullOrWhiteSpace(row["Fechahora"].ToString()))
                    {
                        maindata.fechaEmision = (Convert.ToDateTime("01/01/1900")).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.fechaEmision = (Convert.ToDateTime(row["Fechahora"].ToString())).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    maindata.terminal = row["IdSucursal"].ToString();
                    maindata.numFactura = row["Correlativo"].ToString().Trim();
                    maindata.correlativoInterno = "AV_" + row["NumeroSerie"].ToString().Trim() + "_" + row["Correlativo"].ToString().Trim();
                    maindata.numeroTransaccion = row["Correlativo"].ToString().Trim(); //numero de pedido
                    maindata.codigoUsuario = ""; //FALTA LLENAR
                    maindata.nombreUsuario = ""; //FALTA LLENAR
                    maindata.correoUsuario = "";
                    maindata.serie = row["NumeroSerie"].ToString();
                    string str = row["IdSucursal"].ToString();
                    int targetLength = 4; // Longitud deseada de 4 posiciones

                    string paddedStr = str.PadLeft(targetLength, '0');
                    maindata.cajaSuc = _facturasSala.GetCentro(ruta.ToString()) + paddedStr;
                    maindata.tipoDocumento = row["TipoDoc"].ToString().Trim() == "F" ? "FAC" : "CCF";
                    maindata.pdv = _facturasSala.GetNombreEstablecimiento(Convert.ToInt32(row["IdSucursal"].ToString())); //ESTABLECIMIENTO
                                                                                                                          //maindata.nitCliente = _facturas.GetNITCliente(row["IdCliente"].ToString().Trim()).Trim(); //DEBE APLICARSE TRIM
                    string tipoDocTempNIT = "";
                    if (docPos == 1) //SI ES FACTURA O ANULACION
                    {
                        string nitClienteString = _facturasSala.GetDUI(fecha, row["Correlativo"].ToString().Trim()).Trim();
                        double nitClienteNumber;

                        if (double.TryParse(nitClienteString, out nitClienteNumber))
                        {
                            // Es un número, hacer algo con él si es necesario
                            if (nitClienteString == "")
                            {
                                nitClienteString = _facturasSala.GetDUI(fecha, row["Correlativo"].ToString().Trim()).Trim();   //SI EL DUI ES VACIO BUSCA EL NIT
                                if (maindata.nitCliente == "")
                                {
                                    nitClienteString = "00000000000000"; //SI EL NIT ES VACIO ENVIA CEROS
                                    tipoDocTempNIT = "Otro";

                                    //maindata.nitCliente = ""; //SI EL NIT ES VACIO ENVIA CEROS
                                    //tipoDocTempNIT = "";
                                }
                                else
                                {
                                    tipoDocTempNIT = "NIT";
                                }
                            }
                            else
                            {
                                if (nitClienteString.Trim().Length > 9)
                                {
                                    tipoDocTempNIT = "NIT";
                                }
                                else
                                {
                                    tipoDocTempNIT = "DUI";
                                }
                                
                            }
                        }
                        else
                        {
                            // No es un número, asignar nueve ceros
                            nitClienteString = "000000000";
                            tipoDocTempNIT = "DUI";
                        }

                        maindata.nitCliente = nitClienteString;     //BUSCA EL DUI
                    }
                    else //CREDITO FISCAL
                    {
                        //maindata.duiCliente = _facturas.GetDUI(row["IdCliente"].ToString().Trim()).Trim();        //DEBE APLICARSE TRIM
                        //maindata.nitCliente = _facturasSala.GetDUI(fecha, row["Correlativo"].ToString().Trim()).Trim();   //DEBE APLICARSE TRIM
                        string nitClienteString = _facturasSala.GetDUI(fecha, row["Correlativo"].ToString().Trim()).Trim();
                        double nitClienteNumber;

                        if (double.TryParse(nitClienteString, out nitClienteNumber))
                        {
                            nitClienteString = _facturasSala.GetDUI(fecha, row["Correlativo"].ToString().Trim()).Trim();
                            tipoDocTempNIT = "DUI";
                        }
                        else
                        {
                            // No es un número, asignar nueve ceros
                            nitClienteString = "000000000";
                            tipoDocTempNIT = "DUI";
                        }

                        maindata.nitCliente = nitClienteString;
                    }
                    //maindata.nrcCliente = "06141305710012";  //DEBE APLICARSE TRIM
                    maindata.duiCliente = _facturasSala.GetDUI(fecha, row["Correlativo"].ToString().Trim()).Trim();          //DEBE APLICARSE TRIM
                    if (FC_tipo == "C") //OBIG CFF, NCM, NR
                    {
                        //maindata.nrcCliente = _facturasSala.GetDUI(fecha, row["Correlativo"].ToString().Trim()).Trim();    //DEBE APLICARSE TRIM
                        maindata.nrcCliente = _facturasSala.GetNRC(row["Correlativo"].ToString().Trim());
                    }
                    else
                    {
                        maindata.nrcCliente = _facturasSala.GetNRC(row["Correlativo"].ToString().Trim());
                    }
                    //if (docPos == 1)
                    //{
                    //    maindata.nrcCliente = "";  //DEBE APLICARSE TRIM
                    //    maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    //    if (maindata.nrcCliente == "")
                    //    {
                    //        maindata.nrcCliente = maindata.nitCliente;
                    //    }

                    //}
                    //else
                    //{
                    //    maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    //}
                    maindata.codigoCliente = row["idcliente"].ToString().Trim();
                    maindata.nombreCliente = row["Nombre"].ToString().Trim();
                    maindata.direccionCliente = _facturasSala.GetDireccion(row["Correlativo"].ToString()).Trim() == "" ? "SIN DIRECCION" : _facturasSala.GetDireccion(row["Correlativo"].ToString()).Trim();
                    maindata.departamento = _facturasSala.GetDepartamento(maindata.codigoCliente).Trim() == "" ? "SAN SALVADOR" : _facturasSala.GetDepartamento(maindata.codigoCliente).Trim();
                    maindata.municipio = _facturasSala.GetMunicipio(maindata.codigoCliente).Trim() == "" ? "SAN SALVADOR" : _facturasSala.GetMunicipio(maindata.codigoCliente).Trim();
                    //maindata.giro = _facturas.GetGiroNegocio(maindata.codigoCliente).Trim();
                    ///LLENAR CATALOGO
                    if (FC_tipo == "C") //OBIG CFF, NCM, NR
                    {
                        string resultadoActividad = String.IsNullOrWhiteSpace(_facturasSala.GetGiroNegocio2(row["IdCliente"].ToString())) ? "10005" : _facturasSala.GetGiroNegocio2(row["IdCliente"].ToString().Trim());
                        maindata.codigoActividadEconomica = resultadoActividad;

                        maindata.giro = resultadoActividad == "10005" ? "Otros" : _facturas.GetActividadEconomica(maindata.codigoActividadEconomica);
                    }
                    else  //FAC
                    {
                        string resultadoActividad = String.IsNullOrWhiteSpace(_facturasSala.GetGiroNegocio2(row["IdCliente"].ToString())) ? "10005" : _facturasSala.GetGiroNegocio2(row["IdCliente"].ToString().Trim());
                        maindata.codigoActividadEconomica = resultadoActividad;

                        maindata.giro = resultadoActividad == "10005" ? "Otros" : _facturas.GetActividadEconomica(maindata.codigoActividadEconomica);
                    }

                    maindata.codicionPago = (row["credito"].ToString().Trim() == "1" || Convert.ToBoolean(row["credito"].ToString().Trim()) == true) ? "CREDITO" : "CONTADO";
                    //maindata.ventaTotal = Convert.ToDouble(row["Total"].ToString());
                    if (FC_tipo == "C")
                    {
                        //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["TotalIva"]);
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ventaTotal = Convert.ToDouble(row["Total"].ToString());
                        }
                        else
                        {
                            maindata.ventaTotal = Convert.ToDouble(row["Total"].ToString()) - Convert.ToDouble(row["Percepcion"]);
                            //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["TotalIva"]);
                        }
                    }
                    else
                    {
                        maindata.ventaTotal = Convert.ToDouble(row["Total"].ToString());
                    }
                    maindata.montoLetras = _facturas.GetMontoLetras(maindata.ventaTotal).Trim();
                    maindata.CCFAnterior = "";
                    maindata.vtaACuentaDe = "";
                    maindata.notaRemision = "";
                    maindata.noFecha = "";
                    maindata.saldoCapital = 0;
                    //maindata.idDepartamentoReceptor = _facturasSala.GetIdDepartamento(row["idCliente"].ToString()) == "" ? "06" : _facturasSala.GetIdDepartamento(row["idCliente"].ToString());
                    //maindata.idMunicipioReceptor = _facturasSala.GetIdMunicipio(row["IdCliente"].ToString()) == "" ? "14" : _facturasSala.GetIdMunicipio(row["IdCliente"].ToString());
                    if (_facturasSala.GetIdDepartamento(row["idCliente"].ToString()) == "" || _facturasSala.GetIdMunicipio(row["IdCliente"].ToString()) == "")
                    {
                        maindata.idDepartamentoReceptor = "06";
                        maindata.idMunicipioReceptor = "14";
                    }
                    else
                    {
                        maindata.idDepartamentoReceptor = _facturasSala.GetIdDepartamento(row["idCliente"].ToString()) == "" ? "06" : _facturasSala.GetIdDepartamento(row["idCliente"].ToString());
                        maindata.idMunicipioReceptor = _facturasSala.GetIdMunicipio(row["IdCliente"].ToString()) == "" ? "14" : _facturasSala.GetIdMunicipio(row["IdCliente"].ToString());
                    }
                    maindata.idDepartamentoEmisor = "05";
                    maindata.direccionEmisor = _facturasSala.GetDireccionSucursal(row["NumeroSerie"].ToString().Trim());
                    maindata.fechaEnvio = DateTime.Now.Date.ToString();
                    maindata.idMunicipioEmisor = "11";

                    //maindata.codigoActividadEconomica = "01460";
                    maindata.tipoCatContribuyente = "0";

                    decimal totalXTemp;
                    totalXTemp = Convert.ToDecimal(row["Total"]);
                    decimal resultadoTotal = totalXTemp / Convert.ToDecimal(1.13);
                    resultadoTotal = resultadoTotal * Convert.ToDecimal(0.13);

                    if (FC_tipo == "F")
                    {
                        maindata.sumas = Convert.ToDouble(row["Total"].ToString());
                    }
                    else
                    {
                        maindata.sumas = Convert.ToDouble(Convert.ToDecimal(_facturasSala.GetSubTotal(row["Correlativo"].ToString())) - Convert.ToDecimal((resultadoTotal).ToString("N2")));
                    }
                    maindata.subTotalVentasExentas = 0;
                    maindata.subTotalVentasNoSujetas = 0;

                    if (FC_tipo == "C")
                    {
                        //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["TotalIva"]);
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.subTotalVentasGravadas = Convert.ToDouble(Convert.ToDecimal(_facturasSala.GetSubTotal(row["Correlativo"].ToString())) + Convert.ToDecimal(resultadoTotal.ToString("F2")));
                        }
                        else
                        {
                            //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["TotalIva"])+Convert.ToDouble(row["Percepcion"]);
                            maindata.subTotalVentasGravadas = Convert.ToDouble(row["Total"]);
                        }
                    }
                    else
                    {
                        maindata.subTotalVentasGravadas = 0;
                    }

                    if (FC_tipo == "F")
                    {
                        maindata.iva = Convert.ToDouble(resultadoTotal.ToString("N2"));
                    }
                    else
                    {
                        maindata.iva = Convert.ToDouble(resultadoTotal.ToString("N2"));
                    }
                    maindata.renta = 0;
                    maindata.impuesto = Convert.ToDouble(resultadoTotal.ToString("N2"));
                    if (FC_tipo == "C")
                    {
                        maindata.ventasGravadas = Convert.ToDouble(Convert.ToDecimal(_facturasSala.GetSubTotal(row["Correlativo"].ToString())) - Convert.ToDecimal((resultadoTotal).ToString("N2")));
                    }
                    else
                    { //SI ES FACTYRA
                        maindata.ventasGravadas = Convert.ToDouble(row["Total"].ToString());
                    }
                    //maindata.ventasGravadas = Convert.ToDouble(row["SubTotal"].ToString());
                    maindata.ventasExentas = 0;
                    maindata.ventasNoSujetas = 0;
                    maindata.totalExportaciones = 0;
                    maindata.descuentos = 0;
                    maindata.abonos = 0;
                    maindata.cantidadTotal = _facturasSala.GetCantidadTotal(row["Correlativo"].ToString());
                    maindata.ventasGravadas13 = 0;
                    maindata.ventasGravadas0 = 0;
                    maindata.ventasNoGravadas = 0;
                    //maindata.ivaPercibido1 = Convert.ToDouble(row["Total"].ToString());
                    if (FC_tipo == "F")
                    {
                        maindata.ivaPercibido1 = 0;
                        maindata.ivaPercibido2 = 0;
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ivaRetenido1 = 0;
                        }
                        else
                        {
                            maindata.ivaRetenido1 = Convert.ToDouble(row["Percepcion"].ToString());
                        }
                    }
                    else
                    {
                        maindata.ivaRetenido1 = 0;
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ivaPercibido1 = 0;
                        }
                        else
                        {
                            maindata.ivaPercibido1 = Convert.ToDouble(row["Percepcion"].ToString());
                        }
                    }
                    maindata.ivaRetenido13 = 0;
                    maindata.contribucionSeguridad = 0;
                    maindata.fovial = 0;
                    maindata.cotrans = 0;
                    maindata.contribucionTurismo5 = 0;
                    maindata.contribucionTurismo7 = 0;
                    maindata.impuestoEspecifico = 0;
                    maindata.cesc = 0;
                    maindata.observacionesDte = "";
                    maindata.campo1 = "";
                    maindata.campo2 = "0|" + row["IdCliente"].ToString() + "|" + (row["IdCliente"].ToString()) + "|" + _facturasSala.GetCentro(ruta.ToString()) + "|" + _facturasSala.GetZonaRuta(ruta.ToString()) + "|" + _facturasSala.GetCodigoRutaVenta(ruta.ToString()) + "|";

                    if (FC_tipo == "F") //añade si tipofacturacion
                    {
                        maindata.campo2 = maindata.campo2 + "GT58|";
                    }
                    else
                    {
                        maindata.campo2 = maindata.campo2 + "GT57|";
                    }

                    maindata.campo2 = maindata.campo2 + "||" + _facturasSala.GetRutaReparto(ruta.ToString());
                    maindata.campo2 = maindata.campo2 + "||||||||||||";
                    if (row["Correlativo"].ToString() == "" || row["Correlativo"].ToString() == "0") //revisa la secuencia
                    {
                        maindata.campo2 = maindata.campo2 + "000";
                    }
                    //else
                    //{
                    //    maindata.campo2 = maindata.campo2 + _facturas.GetSecuencia(row["Correlativo"].ToString());
                    //}
                    maindata.campo3 = "";
                    maindata.campo4 = "||||";

                    //CAMPOS NUEVOS FEL

                    maindata.numeroControl = _facturasSala.GetCodigoNumControl(ruta, FC_tipo, fecha, row["Correlativo"].ToString());
                    maindata.codigoGeneracion = _facturasSala.GetCodigoGeneracion(ruta, FC_tipo, fecha, row["Correlativo"].ToString());
                    maindata.modeloFacturacion = maindata.codigoGeneracion != "0" ? "2" : "1";
                    maindata.tipoTransmision = maindata.codigoGeneracion != "0" ? "2" : "1";
                    maindata.codContingencia = maindata.codigoGeneracion == "0" ? "" : "3";
                    if (maindata.codigoGeneracion == "0")
                    {
                        maindata.codigoGeneracion = null;
                    }
                    if (maindata.numeroControl == "0")
                    {
                        maindata.numeroControl = null;
                    }
                    maindata.motivoContin = null;
                    maindata.fInicioContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("yyyy-MM-dd") : null;
                    maindata.fFinContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("yyyy-MM-dd") : null;
                    maindata.horaIniContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("HH:mm:ss") : null;
                    maindata.horaFinContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("HH:mm:ss") : null;
                    maindata.docRelTipo = null;  //SOLO PARA NC Y NR
                    maindata.docRelNum = null;
                    maindata.docRelFecha = null;
                    maindata.nombreComercialCl = "";
                    maindata.otrosDocIdent = ""; //F-CCF      //*****PREGUNTAR*****//
                    maindata.otrosDocDescri = "";       //F-CCF                    //*****PREGUNTAR*****//
                    maindata.ventCterNit = "";
                    maindata.ventCterNombre = "";
                    maindata.montGDescVentNoSujetas = 0.0;
                    maindata.montGDescVentExentas = 0.0;
                    maindata.montGDescVentGrav = 0.0;
                    maindata.totOtroMonNoAfec = 0.0;
                    maindata.totalAPagar = Convert.ToDouble(row["Total"].ToString());
                    maindata.responsableEmisor = "";
                    maindata.numDocEmisor = "";
                    maindata.responsableReceptor = "";
                    maindata.numDocReceptor = "";
                    maindata.nomConductor = "";
                    maindata.numIdenConductor = "";
                    maindata.modTransp = "";
                    maindata.numIdTransp = "";
                    maindata.formaPago = "";
                    maindata.plazo = "";
                    maindata.seguro = 0.0;
                    maindata.flete = 0.0;
                    maindata.arTributos = null;

                    ArTributo tributox = new ArTributo();
                    tributox.valorTributo = Convert.ToDouble(maindata.iva);
                    tributox.codigoTributo = "20";
                    tributox.descripcionTributo = "Impuesto al Valor Agregado 13%";
                    maindata.arTributos = new List<ArTributo>();
                    maindata.arTributos.Add(tributox);

                    maindata.mostrarTributo = false;
                    maindata.bienTitulo = "0";
                    maindata.tipoDocumentoReceptor = tipoDocTempNIT;

                    //maindata.mostrarTributo = false;
                    //maindata.bienTitulo = "0";
                    //maindata.tipoDocumentoReceptor = tipoDocTempNIT;
                    maindata.formatodocumento = "movil400";

                    ListaOLS.Add(maindata);

                    #endregion Cabecera

                    #region Contacto

                    //REGION CONTACTO
                    List<Contacto> ListaContactos = new List<Contacto>
                {
                    new Contacto
                        {
                            whatsapp="",
                            sms="",
                            email = _facturasSala.GetCorreo(ListaOLS[0].codigoCliente)=="" || _facturasSala.GetCorreo(ListaOLS[0].codigoCliente)==null ? "cmia-fel-sv@somoscmi.com":_facturasSala.GetCorreo(ListaOLS[0].codigoCliente),
                            telefono = _facturasSala.GetTelefono(ListaOLS[0].codigoCliente)=="" || _facturasSala.GetTelefono(ListaOLS[0].codigoCliente)==null ? "22021000":_facturasSala.GetTelefono(ListaOLS[0].codigoCliente),
                        }
                };
                    maindata.contactos = ListaContactos;

                    #endregion Contacto

                    #region Detalle

                    //DETALLE
                    DataTable DetalleFactura = _facturasSala.CantidadDetalle(row["Correlativo"].ToString());

                    List<Detalle> detalleOLS = new List<Detalle>();

                    List<Unidadmedida> UnidadeMedidaOLS = new List<Unidadmedida>();

                    double cantidadTem = 0;

                    if (docPos == 6) //es un detalle diferente si es un CCF
                    {
                        foreach (DataRow rowDeta in DetalleFactura.Rows)
                        {
                            if (rowDeta["UnidadMedida"].ToString() == "1")
                            {
                                //cantidadTem = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                                string cantidadStr = rowDeta["Cantidad"].ToString();
                                double cantidadDouble = Convert.ToDouble(cantidadStr);
                                int cantidadRedondeada = Convert.ToInt32(Math.Round(cantidadDouble));
                                cantidadTem = cantidadRedondeada;
                            }
                            else
                            {
                                //cantidadTem = _facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                                cantidadTem = Convert.ToDouble(rowDeta["Peso"].ToString());
                            }
                            Detalle detalle = new Detalle();

                            decimal totalXTempUnitario;
                            totalXTempUnitario = Convert.ToDecimal(rowDeta["SubTotal"]);
                            decimal resultadoTotalUnitario = totalXTempUnitario / Convert.ToDecimal(1.13);
                            resultadoTotalUnitario = resultadoTotalUnitario * Convert.ToDecimal(0.13);

                            double totalTotal = Convert.ToDouble((Convert.ToDecimal(rowDeta["SubTotal"].ToString()) -
                                                                  Convert.ToDecimal(resultadoTotalUnitario)).ToString("N4"));

                            decimal precioUnitario1 = Math.Round(Convert.ToDecimal(totalTotal.ToString("N2")) /
                                                                 Convert.ToDecimal(cantidadTem.ToString("N2")), 4);

                            detalleOLS.Add(
                                new Detalle
                                {
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    descripcion = _facturasSala.GetCodigoSap(rowDeta["IdPlu"].ToString()) + "|" + "PLU:" + rowDeta["IdPlu"].ToString() + "|" + _facturasSala.GetNombreSAP(rowDeta["IdPlu"].ToString()) + "|" + Convert.ToDouble(rowDeta["Cantidad"].ToString()) + "|" + Convert.ToDouble(rowDeta["Peso"].ToString()) + "|",
                                    codTributo = null,
                                    tributos = new List<string>() { "20" },
                                    //precioUnitario = _facturas.GetPrecioUnitarioDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()), //-
                                    //precioUnitario = Convert.ToDecimal((Convert.ToDecimal(rowDeta["PrecioPlu"].ToString())).ToString("N4")),
                                    //precioUnitario = Convert.ToDecimal(Convert.ToDecimal(Convert.ToDecimal(totalTotal.ToString("N2"))/Convert.ToDecimal(cantidadTem.ToString(("N2")))).ToString("N2")),
                                    precioUnitario = precioUnitario1,
                                    ventasNoSujetas = 0,
                                    //ivaItem = _facturas.GetIVALineaFac(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()), //-
                                    ivaItem = Convert.ToDecimal(resultadoTotalUnitario.ToString("N2")),
                                    delAl = "",
                                    exportaciones = "0.0",
                                    numDocRel = "",
                                    uniMedidaCodigo = rowDeta["UnidadMedida"].ToString() == "1" ? 59 : 36,
                                    ventasExentas = 0,
                                    fecha = "",
                                    tipoItem = 2, //CONSULTAR
                                    tipoDteRel = "",
                                    codigoRetencionMH = "",
                                    cantidad = cantidadTem,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    ventasGravadas = Convert.ToDouble(Math.Round(Convert.ToDouble((Convert.ToDecimal(rowDeta["SubTotal"].ToString()) - Convert.ToDecimal(resultadoTotalUnitario)).ToString("N4")), 2)),
                                    //ventasGravadas = Convert.ToDouble((Convert.ToDecimal(rowDeta["SubTotal"].ToString()))),
                                    ivaRetenido = 0.0,
                                    //desc = _facturas.GetDescuentoPrecioDetalle(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    desc = "0",
                                    descuentoItem = 0.0,
                                    otroMonNoAfec = 0.0
                                });
                            //detalleOLS.Add(detalle);
                            maindata.detalle = detalleOLS;
                        }
                    }
                    else
                    {
                        foreach (DataRow rowDeta in DetalleFactura.Rows)
                        {
                            if (rowDeta["UnidadMedida"].ToString() == "1")
                            {
                                //cantidadTem = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                                string cantidadStr = rowDeta["Cantidad"].ToString();
                                double cantidadDouble = Convert.ToDouble(cantidadStr);
                                int cantidadRedondeada = Convert.ToInt32(Math.Round(cantidadDouble));
                                cantidadTem = cantidadRedondeada;
                            }
                            else
                            {
                                //cantidadTem = _facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                                cantidadTem = Convert.ToDouble(rowDeta["Peso"].ToString());
                            }
                            Detalle detalle = new Detalle();

                            decimal totalXTempUnitario;
                            totalXTempUnitario = Convert.ToDecimal(rowDeta["SubTotal"]);
                            decimal resultadoTotalUnitario = totalXTempUnitario / Convert.ToDecimal(1.13);
                            resultadoTotalUnitario = resultadoTotalUnitario * Convert.ToDecimal(0.13);

                            detalleOLS.Add(
                                new Detalle
                                {
                                    //cantidad = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    //precioUnitario = _facturas.GetPrecioUnitarioDetalleFAC(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //ventasNoSujetas = 0,
                                    //ventasExentas = 0,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //desc = _facturas.GetDescuentoPrecioDetalle(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //fecha = "",
                                    //delAl = "",
                                    //exportaciones = "0.0"

                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + Convert.ToDouble(rowDeta["Peso"].ToString()) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    descripcion = _facturasSala.GetCodigoSap(rowDeta["IdPlu"].ToString()) + "|" + "PLU:" + rowDeta["IdPlu"].ToString() + "|" + _facturasSala.GetNombreSAP(rowDeta["IdPlu"].ToString()) +  "|" + Convert.ToDouble(rowDeta["Cantidad"].ToString()) + "|" + Convert.ToDouble(rowDeta["Peso"].ToString()) + "|",
                                    codTributo = null,
                                    tributos = null,
                                    //precioUnitario = _facturas.GetPrecioUnitarioDetalleFAC(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()), //-
                                    //precioUnitario = rowDeta["UnidadMedida"].ToString() == "1" ? Convert.ToDecimal((Convert.ToDecimal(rowDeta["PrecioPlu"].ToString()) / Convert.ToDecimal(rowDeta["Cantidad"].ToString())).ToString("N4")) : Convert.ToDecimal((Convert.ToDecimal(rowDeta["PrecioPlu"].ToString()) / Convert.ToDecimal(rowDeta["Peso"].ToString())).ToString("N4")),
                                    precioUnitario = Convert.ToDecimal(rowDeta["PrecioPlu"].ToString()),
                                    ventasNoSujetas = 0,
                                    //ivaItem = _facturas.GetIVALineaFac(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()), //-
                                    ivaItem = Convert.ToDecimal(resultadoTotalUnitario.ToString("N2")),
                                    delAl = "",
                                    exportaciones = "0.0",
                                    numDocRel = "",
                                    uniMedidaCodigo = rowDeta["UnidadMedida"].ToString() == "1" ? 59 : 36,
                                    ventasExentas = 0,
                                    fecha = "",
                                    tipoItem = 2, //CONSULTAR
                                    tipoDteRel = "",
                                    codigoRetencionMH = "",
                                    //cantidad = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    cantidad = cantidadTem,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    ventasGravadas = Convert.ToDouble(rowDeta["SubTotal"].ToString()),
                                    ivaRetenido = 0.0,
                                    //desc = _facturas.GetDescuentoPrecioDetalle(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    desc = "0",
                                    descuentoItem = 0.0,
                                    otroMonNoAfec = 0.0
                                });
                            //detalleOLS.Add(detalle);
                            maindata.detalle = detalleOLS;
                        }
                    }

                    #endregion Detalle

                    if (docPos == 7 && row["FELAutorizacion"].ToString() != "" && row["FeLAnulacionNumero"].ToString() == "")
                    {
                        anulacion = 1;
                    }
                    else if (docPos == 7 && row["FELAutorizacion"].ToString() == "" && row["FeLAnulacionNumero"].ToString() == "")
                    {
                        #region ENVIAR/RECEPCION DATA

                        respuestaOLS = EnvioDataOLSSalaVentas(ListaOLS, docPos, fecha, Token);
                        respuestaEnvio = respuestaOLS.mensajeCompleto;

                        #endregion ENVIAR/RECEPCION DATA
                    }
                    else if (docPos != 7)
                    {
                        #region ENVIAR/RECEPCION DATA

                        respuestaOLS = EnvioDataOLSSalaVentas(ListaOLS, docPos, fecha, Token);
                        respuestaEnvio = respuestaOLS.mensajeCompleto;

                        #endregion ENVIAR/RECEPCION DATA
                    }

                    #region Anulacion

                    if (anulacion == 1)
                    {
                        //MAPEA CAMPOS
                        List<MapaAnulacion> ListaAnular = new List<MapaAnulacion>();
                        MapaAnulacion mapaAnulacion = new MapaAnulacion
                        {
                            fechaDoc = (Convert.ToDateTime(maindata.fechaEmision)).ToString("dd-MM-yyyy"),
                            numDoc = Convert.ToInt32(maindata.numFactura),
                            tipoDoc = "FAC_movil",
                            correlativoInterno = (maindata.numFactura),
                            nitEmisor = "0614-130571-001-2",
                            fechaAnulacion = (Convert.ToDateTime(maindata.fechaEmision)).ToString("dd/MM/yyyy"),
                        };
                        ListaAnular.Add(mapaAnulacion);
                        //respuestaAnulacion = EnviaDataAnulacion(ListaAnular, ListaOLS, fecha);

                        respuestaProceso = respuestaProceso + respuestaEnvio + "\n" + respuestaAnulacion;
                    }
                    else
                    {
                        respuestaProceso = respuestaProceso + respuestaEnvio;
                    }

                    #endregion Anulacion
                }
                catch (Exception ex)
                {
                    var s = new StackTrace(ex);
                    var thisasm = Assembly.GetExecutingAssembly();
                    var methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                    string errorMsj = @"Error interno:" + ex.Message.ToString() + "\n" +
                             "Metodo:" + methodname;
                    //GrabarErrorInternos(ruta, fecha, docPos, numFac, errorMsj);
                    respuestaOLS.mensajeCompleto = errorMsj;
                    respuestaOLS.ResultadoSatisfactorio = false;
                }
            }

            anulacion = 0;
            return respuestaOLS;
        }

        /// <summary>
        /// ENVIA FACTURAS POR RUTA Y FECHA
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>
        [WebMethod(Description = "Envia solo Facturas/Creditos Fiscales/Anulaciones de Facturas PreImpresas")]
        public RespuestaOLS EnviaFacturasPreImpresas(int ruta, string fecha, int docPos, string parametrosPreImpresa)
        {
            string Token = "";
            string revisaT = RevisarToken();
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            RespuestaOLS respuestaOLS = new RespuestaOLS();

            string respuestaProceso = "";
            if (docPos == 11)
            {
                FC_tipo = "F";
                FC_estado = "FAC";
            }
            else if (docPos == 66)
            {
                FC_tipo = "C";
                FC_estado = "FAC";
            }
            //else if (docPos == 7)
            //{
            //    FC_tipo = "F";
            //    FC_estado = "ANU";
            //    anulacion = 1;
            //}

            //*****FACTURAS O COMPROBATES DE CREDITO FISCAL******/
            /*****TABLA: Handheld.FacturaEBajada**********/

            string[] preimArra = parametrosPreImpresa.Split(',');
            long numFac = Convert.ToInt64(preimArra[0]);
            string ipImpresora = preimArra[1];
            string campo3X = preimArra[2];

            string respuestaEnvio = "";
            string respuestaAnulacion = "";
            bool facturasFEL = false;
            //facturasFEL = _facturas.GetRutaFEL(ruta);
            List<Maindata> ListaOLS = new List<Maindata>();
            ListaOLS.Clear();
            DataTable FacturasTabla;

            FacturasTabla = _facturas.CantidadFacturasPreImpresas(ruta, fecha, numFac, docPos);

            //if (docPos == 7)
            //{
            //    //FacturasTabla = _facturas.CantidadFacturas(ruta, FC_tipo, FC_estado, fecha, numFac, docPos);
            //}
            //else
            //{
            //    //FacturasTabla = _facturas.CantidadFacturas(ruta, FC_tipo, FC_estado, fecha, numFac, docPos);
            //    FacturasTabla = _facturas.CantidadFacturasPreImpresas(ruta, fecha, numFac, docPos);
            //}

            //itera y recupera campos de las tablas
            foreach (DataRow row in FacturasTabla.Rows)
            {
                try
                {
                    Maindata maindata = new Maindata();

                    #region Cabecera

                    ListaOLS.Clear();
                    maindata.resolucion = _facturas.GetResolucion(ruta, row["IdControlSerie"].ToString()).Trim(); //-
                    maindata.resInicio = _facturas.GetResInicio(ruta, row["IdControlSerie"].ToString()).Trim(); //-
                    maindata.resFin = _facturas.GetResFin(ruta, row["IdControlSerie"].ToString()).Trim(); //-
                    //maindata.nit = _facturas.GetNit(ruta, row["idSerie"].ToString()).Trim();
                    maindata.nit = "0614-130571-001-2";

                    //maindata.resFecha = (_facturas.GetRestFecha(ruta, row["idSerie"].ToString())).Substring(0, _facturas.GetRestFecha(ruta, row["idSerie"].ToString()).Length - 9);  //SIN HORA SOLO FECHA
                    if (string.IsNullOrWhiteSpace(_facturas.GetRestFecha(ruta, row["IdControlSerie"].ToString()))) //-
                    {
                        maindata.resFecha = (Convert.ToDateTime("01/01/1900")).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.resFecha = (Convert.ToDateTime(_facturas.GetRestFecha(ruta, row["IdControlSerie"].ToString()))).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }

                    maindata.nrc = row["NRC"].ToString();
                    maindata.fechaEnvio = DateTime.Now.ToString().Trim();
                    //maindata.fechaEmision = (row["Fechahora"].ToString()).Substring(0, row["Fechahora"].ToString().Length - 9); //SIN FECHA SOLO HORA
                    //maindata.fechaEmision = (Convert.ToDateTime(row["Fechahora"].ToString())).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    if (string.IsNullOrWhiteSpace(row["FechaFactura"].ToString()))
                    {
                        maindata.fechaEmision = (Convert.ToDateTime("01/01/1900")).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.fechaEmision = (Convert.ToDateTime(row["FechaFactura"].ToString())).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }

                    maindata.terminal = ruta.ToString().Trim();
                    maindata.numFactura = row["idFactura"].ToString().Trim();
                    //maindata.correlativoInterno = string.Concat("AV_", row["idFactura"].ToString().Trim());
                    maindata.correlativoInterno = string.Concat("AV_", row["Factura"].ToString().Trim());
                    maindata.numeroTransaccion = row["Factura"].ToString().Trim(); //numero de pedido
                    maindata.codigoUsuario = row["IdUsuarioGenera"].ToString().Trim();
                    maindata.nombreUsuario = _facturas.GetNombreUsuario(row["IdUsuarioGenera"].ToString());
                    maindata.correoUsuario = "";
                    maindata.serie = _facturas.GetNumSerie(ruta, row["IdControlSerie"].ToString()).Trim(); //-
                    maindata.cajaSuc = ruta.ToString().Trim();
                    maindata.tipoDocumento = row["TipoFactura"].ToString() == "C" ? "CCF" : "FAC";
                    maindata.pdv = _facturas.GetNombreEstablecimiento(row["idCliente"].ToString()); //ESTABLECIMIENTO
                                                                                                    //maindata.nitCliente = _facturas.GetNITCliente(row["IdCliente"].ToString().Trim()).Trim(); //DEBE APLICARSE TRIM

                    //string tipoDocTempNIT = "";
                    //if (docPos == 1) //SI ES FACTURA O ANULACION
                    //{
                    //    maindata.nitCliente = _facturas.GetDUI(row["IdCliente"].ToString().Trim()).Trim();        //BUSCA EL DUI
                    //    if (maindata.nitCliente == "")
                    //    {
                    //        maindata.nitCliente = _facturas.GetNITCliente(row["IdCliente"].ToString().Trim()).Trim(); //SI EL DUI ES VACIO BUSCA EL NIT
                    //        if (maindata.nitCliente == "")
                    //        {
                    //            maindata.nitCliente = "00000000000000"; //SI EL NIT ES VACIO ENVIA CEROS
                    //            tipoDocTempNIT = "Otro";
                    //        }
                    //        else
                    //        {
                    //            tipoDocTempNIT = "NIT";
                    //        }
                    //    }
                    //    else
                    //    {
                    //        tipoDocTempNIT = "DUI";
                    //    }
                    //}
                    //else //CREDITO FISCAL
                    //{
                    //    //maindata.duiCliente = _facturas.GetDUI(row["IdCliente"].ToString().Trim()).Trim();        //DEBE APLICARSE TRIM
                    //    maindata.nitCliente = _facturas.GetNITCliente(row["IdCliente"].ToString().Trim()).Trim(); //DEBE APLICARSE TRIM
                    //    tipoDocTempNIT = "NIT";
                    //    //maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    //}

                    string tipoDocTempNIT = "";
                    if (docPos == 11) //SI ES FACTURA
                    {
                        maindata.nitCliente = _facturas.GetDUI(row["idCliente"].ToString().Trim()).Trim();        //BUSCA EL DUI
                        if (maindata.nitCliente == "")
                        {
                            maindata.nitCliente = _facturas.GetNITCliente(row["idCliente"].ToString().Trim()).Trim(); //SI EL DUI ES VACIO BUSCA EL NIT
                            if (maindata.nitCliente == "")
                            {
                                maindata.nitCliente = "00000000000000"; //SI EL NIT ES VACIO ENVIA CEROS
                            }
                            else
                            {
                                tipoDocTempNIT = "NIT";
                            }
                        }
                        else
                        {
                            tipoDocTempNIT = "DUI";
                        }
                    }
                    else //CREDITO FISCAL
                    {
                        //maindata.duiCliente = _facturas.GetDUI(row["IdCliente"].ToString().Trim()).Trim();        //DEBE APLICARSE TRIM
                        maindata.nitCliente = _facturas.GetNITCliente(row["idCliente"].ToString().Trim()).Trim(); //DEBE APLICARSE TRIM
                        tipoDocTempNIT = "NIT";
                        //maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    }
                    //maindata.nrcCliente = "06141305710012";  //DEBE APLICARSE TRIM
                    maindata.duiCliente = _facturas.GetDUI(row["idCliente"].ToString().Trim()).Trim();        //DEBE APLICARSE TRIM
                    maindata.nrcCliente = _facturas.GetNRC(row["idCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    if (docPos == 11)
                    {
                        maindata.nrcCliente = "";
                    }
                    else
                    {
                        maindata.nrcCliente = _facturas.GetNRC(row["idCliente"].ToString().Trim()).Trim();  //DEBE APLICARSE TRIM
                    }
                    maindata.codigoCliente = row["idCliente"].ToString().Trim();
                    maindata.nombreCliente = _facturas.GetNombreCliente(maindata.codigoCliente).Trim();
                    maindata.direccionCliente = _facturas.GetDireccion(maindata.codigoCliente).Trim();
                    maindata.departamento = _facturas.GetDepartamento(maindata.codigoCliente).Trim();
                    maindata.municipio = _facturas.GetMunicipio(maindata.codigoCliente).Trim();
                    //maindata.giro = _facturas.GetGiroNegocio(maindata.codigoCliente).Trim();

                    //maindata.giro = "Cría de aves de corral y producción de huevos";
                    maindata.codicionPago = row["CondicionPago"].ToString().Trim();
                    maindata.ventaTotal = Convert.ToDouble(row["TotalFactura"].ToString());
                    if (docPos == 11)
                    {
                        maindata.ventaTotal = Convert.ToDouble(row["TotalFactura"].ToString());
                    }
                    else
                    {
                        string percepcion = row["Percepcion"].ToString();
                        if (!string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ventaTotal = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"].ToString()) -
                                                                   Convert.ToDecimal(row["Percepcion"].ToString()));
                        }
                        else
                        {
                            maindata.ventaTotal = Convert.ToDouble(row["TotalFactura"].ToString());
                        }
                    }
                    maindata.montoLetras = _facturas.GetMontoLetras(maindata.ventaTotal).Trim();
                    maindata.CCFAnterior = "";
                    maindata.vtaACuentaDe = "";
                    maindata.notaRemision = "";
                    maindata.noFecha = "";
                    maindata.saldoCapital = 0;
                    maindata.idDepartamentoReceptor = _facturas.GetIdDepartamento(row["idCliente"].ToString()); ;
                    maindata.idDepartamentoEmisor = "05";
                    maindata.direccionEmisor = "0";
                    maindata.fechaEnvio = DateTime.Now.Date.ToString();
                    maindata.idMunicipioEmisor = "05";
                    maindata.idMunicipioReceptor = _facturas.GetIdMunicipio(row["idCliente"].ToString());

                    ///LLENAR CATALOGO
                    if (FC_tipo == "C") //OBIG CFF, NCM, NR
                    {
                        maindata.codigoActividadEconomica = _facturas.GetGiroNegocio2(row["idCliente"].ToString());
                        //maindata.codigoActividadEconomica = "01282";
                        maindata.giro = _facturas.GetActividadEconomica(maindata.codigoActividadEconomica);
                        //maindata.giro = "OTROS";
                    }
                    else  //FAC
                    {
                        maindata.codigoActividadEconomica = _facturas.GetGiroNegocio2(row["idCliente"].ToString());
                        //maindata.codigoActividadEconomica = "01282";
                        maindata.giro = _facturas.GetActividadEconomica(maindata.codigoActividadEconomica);
                    }

                    maindata.tipoCatContribuyente = "0";

                    if (FC_tipo == "F")
                    {
                        maindata.sumas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"].ToString()) + Convert.ToDecimal(row["ValeMerma"]));
                    }
                    else
                    {
                        string percepcion = row["Percepcion"].ToString();
                        if (!string.IsNullOrEmpty(percepcion))
                        {
                            //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) - Convert.ToDouble(row["Percepcion"]);
                            //maindata.ventasGravadas = (Convert.ToDouble(row["SubTotal"].ToString()) - Convert.ToDouble(row["Iva"].ToString())) - Convert.ToDouble(row["Percepcion"]);
                            //maindata.sumas = (Convert.ToDouble(row["SubTotal"].ToString()) - Convert.ToDouble(row["Iva"].ToString())) - Convert.ToDouble(row["Percepcion"]);
                            maindata.sumas = Convert.ToDouble((Convert.ToDecimal(row["TotalFactura"].ToString()) - Convert.ToDecimal(row["Iva"].ToString())) - Convert.ToDecimal(row["Percepcion"]));
                        }
                        else
                        {
                            //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]);
                            //maindata.ventasGravadas = Convert.ToDouble(row["SubTotal"].ToString()) - Convert.ToDouble(row["Iva"].ToString());
                            maindata.sumas = Convert.ToDouble((Convert.ToDecimal(row["TotalFactura"].ToString()) - Convert.ToDecimal(row["Iva"].ToString())));
                        }
                    }
                    maindata.subTotalVentasExentas = 0;
                    maindata.subTotalVentasNoSujetas = 0;
                    if (FC_tipo == "C") ///CAMBIOS 1
                    {
                        string percepcion = row["Percepcion"].ToString();
                        if (!string.IsNullOrEmpty(percepcion))
                        {
                            maindata.subTotalVentasGravadas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"]) - Convert.ToDecimal(row["Percepcion"]));
                        }
                        else
                        {
                            maindata.subTotalVentasGravadas = Convert.ToDouble(row["TotalFactura"]);
                        }
                        //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) + Convert.ToDouble(row["Iva"]);
                        //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) - Convert.ToDouble(row["Iva"]);
                    }
                    else
                    {
                        //maindata.subTotalVentasGravadas = 0;
                        maindata.subTotalVentasGravadas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"].ToString()) + Convert.ToDecimal(row["ValeMerma"]));
                    }

                    if (FC_tipo == "F")
                    {
                        maindata.iva = Convert.ToDouble(row["Iva"].ToString());
                    }
                    else
                    {
                        maindata.iva = Convert.ToDouble(row["Iva"].ToString());
                    }
                    maindata.renta = 0;
                    maindata.impuesto = Convert.ToDouble(row["Iva"].ToString());
                    if (FC_tipo == "C")
                    {
                        string percepcion = row["Percepcion"].ToString();
                        if (!string.IsNullOrEmpty(percepcion))
                        {
                            //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]) - Convert.ToDouble(row["Percepcion"]);
                            maindata.ventasGravadas = Convert.ToDouble((Convert.ToDecimal(row["TotalFactura"].ToString()) - Convert.ToDecimal(row["Iva"].ToString())) - Convert.ToDecimal(row["Percepcion"]));
                        }
                        else
                        {
                            //maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"]);
                            maindata.ventasGravadas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"].ToString()) - Convert.ToDecimal(row["Iva"].ToString()));
                        }

                        //maindata.ventasGravadas = Convert.ToDouble(row["SubTotal"].ToString()) - Convert.ToDouble(row["Iva"].ToString());
                    }
                    else
                    { //SI ES FACTYRA
                        maindata.ventasGravadas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"].ToString())+ Convert.ToDecimal(row["ValeMerma"]));
                    }

                    maindata.ventasExentas = 0;
                    maindata.ventasNoSujetas = 0;
                    maindata.totalExportaciones = 0;
                    maindata.descuentos = Convert.ToDouble(row["ValeMerma"]);
                    maindata.abonos = 0;
                    maindata.cantidadTotal = _facturas.GetCantidadTotalPreImpresa(row["idFactura"].ToString());
                    maindata.ventasGravadas13 = 0;
                    maindata.ventasGravadas0 = 0;
                    maindata.ventasNoGravadas = 0;
                    //maindata.ivaPercibido1 = Convert.ToDouble(row["Total"].ToString());
                    if (FC_tipo == "F")
                    {
                        maindata.ivaPercibido1 = 0;
                        maindata.ivaPercibido2 = 0;
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ivaRetenido1 = 0;
                        }
                        else
                        {
                            maindata.ivaRetenido1 = Convert.ToDouble(row["Percepcion"].ToString());
                        }
                    }
                    else
                    {
                        maindata.ivaRetenido1 = 0;
                        string percepcion = row["Percepcion"].ToString();
                        if (string.IsNullOrEmpty(percepcion))
                        {
                            maindata.ivaPercibido1 = 0;
                        }
                        else
                        {
                            maindata.ivaPercibido1 = Convert.ToDouble(row["Percepcion"].ToString());
                        }
                    }
                    maindata.ivaRetenido13 = 0;
                    maindata.contribucionSeguridad = 0;
                    maindata.fovial = 0;
                    maindata.cotrans = 0;
                    maindata.contribucionTurismo5 = 0;
                    maindata.contribucionTurismo7 = 0;
                    maindata.impuestoEspecifico = 0;
                    maindata.cesc = 0;
                    maindata.observacionesDte = "";
                    maindata.campo1 = "";
                    maindata.campo2 = row["idCliente"].ToString() + "|" + _facturas.GetCodigoClientePrincipal(row["idCliente"].ToString()) + "|" + _facturas.GetCentro(ruta.ToString()) + "|" + _facturas.GetZonaRuta(ruta.ToString()) + "|" + _facturas.GetCodigoRutaVenta(ruta.ToString()) + "|";

                    if (FC_tipo == "F") //añade si tipofacturacion
                    {
                        maindata.campo2 = maindata.campo2 + "GT58|";
                    }
                    else
                    {
                        maindata.campo2 = maindata.campo2 + "GT57|";
                    }

                    

                    maindata.campo2 = ipImpresora + "|" + maindata.campo2;
                    maindata.campo2 = maindata.campo2 + _facturas.GetRutaVenta(row["IdCliente"].ToString()) + "|" + _facturas.GetRutaReparto(ruta.ToString());
                    maindata.campo2 = maindata.campo2 + "||||"+ _facturas.GetNumeroFacturaSGR_Preimpresa(row["idPedido"].ToString())+"|||||||"+_facturas.GetUsuarioGeneraPreImpresa(row["IdUsuarioGenera"].ToString())+"||";
                    if (row["Factura"].ToString() == "" || row["Factura"].ToString() == "0") //revisa la secuencia
                    {
                        maindata.campo2 = maindata.campo2 + "000";
                    }
                    else
                    {
                        maindata.campo2 = maindata.campo2 + _facturas.GetSecuencia(row["idFactura"].ToString());
                    }
                    maindata.campo2 = maindata.campo2 + "OC:" + _facturas.GetOrdenCompraPreImpresa(row["idFactura"].ToString());
                    maindata.campo3 = "";
                    //maindata.campo3 = campo3X;
                    //maindata.campo4 = _facturas.GetRutaReparto(ruta.ToString());
                    maindata.campo4 = campo3X;

                    //CAMPOS NUEVOS FEL

                    maindata.numeroControl = _facturas.GetNumControlPreImpresa(ruta, row["FechaFactura"].ToString(), row["Factura"].ToString());
                    maindata.codigoGeneracion = _facturas.GetCodigoGeneracionPreImpresa(ruta, row["FechaFactura"].ToString(), row["Factura"].ToString());
                    maindata.modeloFacturacion = maindata.codigoGeneracion != "0" ? "2" : "1";
                    maindata.tipoTransmision = maindata.codigoGeneracion != "0" ? "2" : "1";
                    maindata.codContingencia = maindata.codigoGeneracion == "0" ? "" : "3";
                    if (maindata.codigoGeneracion == "0")
                    {
                        maindata.codigoGeneracion = null;
                        maindata.numeroControl = null;
                    }
                    maindata.motivoContin = null;
                    maindata.fInicioContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("yyyy-MM-dd") : null;
                    maindata.fFinContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("yyyy-MM-dd") : null;
                    maindata.horaIniContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("HH:mm:ss") : null;
                    maindata.horaFinContin = maindata.codContingencia != null ? DateTime.Now.Date.ToString("HH:mm:ss") : null;
                    maindata.docRelTipo = null;  //SOLO PARA NC Y NR
                    maindata.docRelNum = null;
                    maindata.docRelFecha = null;
                    maindata.nombreComercialCl = "";
                    maindata.otrosDocIdent = ""; //F-CCF      //*****PREGUNTAR*****//
                    maindata.otrosDocDescri = "";       //F-CCF                    //*****PREGUNTAR*****//
                    maindata.ventCterNit = "";
                    maindata.ventCterNombre = "";
                    maindata.montGDescVentNoSujetas = 0.0;
                    maindata.montGDescVentExentas = 0.0;
                    maindata.montGDescVentGrav = Convert.ToDouble(row["ValeMerma"]);
                    maindata.totOtroMonNoAfec = 0.0;
                    maindata.totalAPagar = Convert.ToDouble(row["TotalFactura"].ToString());
                    maindata.responsableEmisor = "";
                    maindata.numDocEmisor = "";
                    maindata.responsableReceptor = "";
                    maindata.numDocReceptor = "";
                    maindata.nomConductor = "";
                    maindata.numIdenConductor = "";
                    maindata.modTransp = "";
                    maindata.numIdTransp = "";
                    maindata.formaPago = "";
                    maindata.plazo = "";
                    maindata.seguro = 0.0;
                    maindata.flete = 0.0;
                    maindata.arTributos = null;

                    ArTributo tributox = new ArTributo();
                    tributox.valorTributo = Convert.ToDouble(row["Iva"].ToString());
                    tributox.codigoTributo = "20";
                    tributox.descripcionTributo = "Impuesto al Valor Agregado 13%";
                    maindata.arTributos = new List<ArTributo>();
                    maindata.arTributos.Add(tributox);

                    maindata.mostrarTributo = false;
                    maindata.bienTitulo = "0";
                    maindata.tipoDocumentoReceptor = tipoDocTempNIT;
                    maindata.formatodocumento = "carta";

                    maindata.campoExtFE = "OrdenCompra|Número de Orden de Compra|" + _facturas.GetOrdenCompraPreImpresa(row["idFactura"].ToString())+"|||";

                    ListaOLS.Add(maindata);

                    #endregion Cabecera

                    #region Contacto

                    //REGION CONTACTO
                    List<Contacto> ListaContactos = new List<Contacto>
                {
                    new Contacto
                        {
                            whatsapp="",
                            sms="",
                            email = _facturas.GetCorreo(ListaOLS[0].codigoCliente)=="" || _facturas.GetCorreo(ListaOLS[0].codigoCliente)==null ? "cmia-fel-sv@somoscmi.com":_facturas.GetCorreo(ListaOLS[0].codigoCliente),
                            telefono = _facturas.GetTelefono(ListaOLS[0].codigoCliente)=="" || _facturas.GetTelefono(ListaOLS[0].codigoCliente)==null ? "74658546":_facturas.GetTelefono(ListaOLS[0].codigoCliente),
                        }
                };
                    maindata.contactos = ListaContactos;

                    #endregion Contacto

                    #region Detalle

                    //DETALLE
                    DataTable DetalleFactura = _facturas.CantidadDetallePreImpresa(row["idFactura"].ToString());

                    List<Detalle> detalleOLS = new List<Detalle>();

                    List<Unidadmedida> UnidadeMedidaOLS = new List<Unidadmedida>();

                    double cantidadTem = 0;

                    if (docPos == 66) //es un detalle diferente si es un CCF
                    {
                        foreach (DataRow rowDeta in DetalleFactura.Rows)
                        {
                            Detalle detalle = new Detalle();

                            //cantidadTem = _facturas.GetCantidadDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString());

                            //if (_facturas.CompruebaUnidadMedida(rowDeta["IdProductos"].ToString()) == "1")
                            if (_facturas.CompruebaUnidadMedidaPreimpresa(rowDeta["IdProductos"].ToString()) == "1")
                            {
                                cantidadTem = _facturas.GetCantidadDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString());
                            }
                            else
                            {
                                cantidadTem = _facturas.GetPesoProductoDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString());
                            }

                            detalleOLS.Add(
                                new Detalle
                                {
                                    //descripcion = rowDeta["idProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["idProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["idProductos"].ToString(), row["idCliente"].ToString()) + "|",
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + "PLU:" + _facturas.GetPLUProductoPreimpresas(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetUnidadesDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString()) + " UN"+ "|" + _facturas.GetPesoDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString())+ " LB"+"|",
                                    descripcion = rowDeta["IdProductos"].ToString() + "|" + "PLU:" + _facturas.GetPLUProductoPreimpresas(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + rowDeta["Unidades"].ToString() + " UN"+ "|" + rowDeta["Peso"].ToString() + " LB"+"|",
                                    codTributo = "",
                                    tributos = new List<string>() { "20" },
                                    //precioUnitario = _facturas.GetPrecioUnitarioDetallePreImpresaCCF(row["idFactura"].ToString(), rowDeta["idProductos"].ToString()),
                                    precioUnitario = _facturas.GetPrecioUnitarioDetallePreImpresaCCF(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString()),
                                    //precioUnitario = rowDeta["PrecioUnitario"],
                                    ventasNoSujetas = 0,
                                    ivaItem = _facturas.GetIVALineaFacPreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString()),
                                    delAl = "",
                                    exportaciones = "0.0",
                                    numDocRel = "",
                                    uniMedidaCodigo = _facturas.GetUnidadFacturacion(rowDeta["idProductos"].ToString()) == 1 ? 59 : 36,
                                    ventasExentas = 0,
                                    fecha = "",
                                    tipoItem = 2, //CONSULTAR
                                    tipoDteRel = "",
                                    codigoRetencionMH = "",
                                    //cantidad = _facturas.GetCantidadDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString()),
                                    cantidad = cantidadTem,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    ventasGravadas = _facturas.GetVentasGravadasDetalleCCFPreImpresa(row["idFactura"].ToString(), rowDeta["IdProductos"].ToString(), rowDeta["NumeroLinea"].ToString()),
                                    ivaRetenido = 0.0,
                                    //desc = _facturas.GetDescuentoPrecioDetallePreImpresa(row["idFactura"].ToString(), rowDeta["IdProductos"].ToString()),
                                    desc = "0.0",
                                    //descuentoItem = Convert.ToDouble(_facturas.GetValeMermaPrecioDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString())),
                                    descuentoItem = 0.0,
                                    otroMonNoAfec = 0.0
                                });
                            //detalleOLS.Add(detalle);
                            maindata.detalle = detalleOLS;
                        }
                    }
                    else
                    {
                        foreach (DataRow rowDeta in DetalleFactura.Rows)
                        {
                            if (_facturas.CompruebaUnidadMedidaPreimpresa(rowDeta["IdProductos"].ToString()) == "1")
                            {
                                cantidadTem = _facturas.GetCantidadDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString());
                            }
                            else
                            {
                                cantidadTem = _facturas.GetPesoProductoDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString());
                            }

                            Detalle detalle = new Detalle();
                            detalleOLS.Add(
                                new Detalle
                                {
                                    //cantidad = _facturas.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                    //precioUnitario = _facturas.GetPrecioUnitarioDetalleFAC(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //ventasNoSujetas = 0,
                                    //ventasExentas = 0,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //desc = _facturas.GetDescuentoPrecioDetalle(ruta, fecha, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    //fecha = "",
                                    //delAl = "",
                                    //exportaciones = "0.0"

                                    //descripcion = rowDeta["idProductos"].ToString() + "|" + (_facturas.GetPesoProductoDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _facturas.GetNombreProducto(rowDeta["idProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["idProductos"].ToString(), row["idCliente"].ToString()) + "|",
                                    //descripcion = rowDeta["IdProductos"].ToString() + "|" + "PLU:" + _facturas.GetPLUProductoPreimpresas(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|"  + _facturas.GetUnidadesDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString()) + " UN"+ "|" + _facturas.GetPesoDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString()) +" LB"+ "|",
                                    descripcion = rowDeta["IdProductos"].ToString() + "|" + "PLU:" + _facturas.GetPLUProductoPreimpresas(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|" + _facturas.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|"  + rowDeta["Unidades"].ToString() + " UN"+ "|" + rowDeta["Peso"].ToString() + " LB"+ "|",
                                    tributos = null,
                                    precioUnitario = _facturas.GetPrecioUnitarioDetalleFACPreImpreso(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString()),
                                    ventasNoSujetas = 0,
                                    ivaItem = _facturas.GetIVALineaFacPreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString()),
                                    delAl = "",
                                    exportaciones = "0.0",
                                    numDocRel = "",
                                    uniMedidaCodigo = _facturas.GetUnidadFacturacion(rowDeta["idProductos"].ToString()) == 1 ? 59 : 36,
                                    ventasExentas = 0,
                                    fecha = "",
                                    tipoItem = 2, //CONSULTAR
                                    tipoDteRel = "",
                                    codigoRetencionMH = "",
                                    //cantidad = _facturas.GetCantidadDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString()),
                                    cantidad = cantidadTem,
                                    //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                    ventasGravadas = _facturas.GetVentasGravadasDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString(), rowDeta["NumeroLinea"].ToString()),
                                    ivaRetenido = 0.0,
                                    //desc = _facturas.GetDescuentoPrecioDetallePreImpresa(row["idFactura"].ToString(), rowDeta["IdProductos"].ToString()),
                                    desc = "0.0",
                                    //descuentoItem = Convert.ToDouble(_facturas.GetValeMermaPrecioDetallePreImpresa(row["idFactura"].ToString(), rowDeta["idProductos"].ToString())),
                                    descuentoItem = 0.0,
                                    otroMonNoAfec = 0.0
                                }); ;
                            //detalleOLS.Add(detalle);
                            maindata.detalle = detalleOLS;
                        }
                    }

                    #endregion Detalle

                    if (docPos == 7 && row["FELAutorizacion"].ToString() != "" && row["FeLAnulacionNumero"].ToString() == "")
                    {
                        anulacion = 1;
                    }
                    else if (docPos == 7 && row["FELAutorizacion"].ToString() == "" && row["FeLAnulacionNumero"].ToString() == "")
                    {
                        #region ENVIAR/RECEPCION DATA

                        respuestaOLS = EnvioDataOLS(ListaOLS, docPos, fecha, Token);
                        respuestaEnvio = respuestaOLS.mensajeCompleto;

                        #endregion ENVIAR/RECEPCION DATA
                    }
                    else if (docPos != 7)
                    {
                        #region ENVIAR/RECEPCION DATA

                        respuestaOLS = EnvioDataOLSPreImpresa(ListaOLS, docPos, fecha, Token);
                        respuestaEnvio = respuestaOLS.mensajeCompleto;

                        #endregion ENVIAR/RECEPCION DATA
                    }

                    #region Anulacion

                    if (anulacion == 1)
                    {
                        //MAPEA CAMPOS
                        List<MapaAnulacion> ListaAnular = new List<MapaAnulacion>();
                        MapaAnulacion mapaAnulacion = new MapaAnulacion
                        {
                            fechaDoc = (Convert.ToDateTime(maindata.fechaEmision)).ToString("dd-MM-yyyy"),
                            numDoc = Convert.ToInt32(maindata.numFactura),
                            tipoDoc = "FAC_movil",
                            correlativoInterno = (maindata.numFactura),
                            nitEmisor = "0614-130571-001-2",
                            fechaAnulacion = (Convert.ToDateTime(maindata.fechaEmision)).ToString("dd/MM/yyyy"),
                        };
                        ListaAnular.Add(mapaAnulacion);
                        //respuestaAnulacion = EnviaDataAnulacion(ListaAnular, ListaOLS, fecha);

                        respuestaProceso = respuestaProceso + respuestaEnvio + "\n" + respuestaAnulacion;
                    }
                    else
                    {
                        respuestaProceso = respuestaProceso + respuestaEnvio;
                    }

                    #endregion Anulacion
                }
                catch (Exception ex)
                {
                    var s = new StackTrace(ex);
                    var thisasm = Assembly.GetExecutingAssembly();
                    var methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                    string errorMsj = @"Error interno:" + ex.Message.ToString() + "\n" +
                             "Metodo:" + methodname;
                    //GrabarErrorInternos(ruta, fecha, docPos, numFac, errorMsj);
                    respuestaOLS.mensajeCompleto = errorMsj;
                    respuestaOLS.ResultadoSatisfactorio = false;
                }
            }

            anulacion = 0;
            return respuestaOLS;

            //crea respuesta

        }

        /// <summary>
        /// ENVIA NOTAS DE CREDITO
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <param name="docPos"></param>
        /// <param name="numFac"></param>
        /// <returns></returns>
        [WebMethod(Description = "Envia solo Notas de Credito de SGR")]
        public RespuestaOLS EnviarNotasCreditosSGR(string fecha, int ruta, string parametrosPreImpresa)
        {
            RespuestaOLS respuestaOLS = new RespuestaOLS();

            string[] preimArra = parametrosPreImpresa.Split(',');
            //long numFac = Convert.ToInt64(preimArra[0]);
            string ipImpresora = preimArra[1];
            string campo3X = preimArra[2];

            //string[] preimArra = parametrosPreImpresa.Split(',');
            //long numFac = Convert.ToInt64(preimArra[0]);
            //string ipImpresora = preimArra[1];
            //string campo3X = preimArra[2];

            string Token = "";
            string revisaT = RevisarToken();
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            /**********NOTA DE CREDITO*************/
            /*********TABLA: Reparto.DocumentosFacturasEBajada********/
            string respuestaEnvio = "";
            List<Maindata> ListaOLS = new List<Maindata>();
            bool facturasFEL = false;
            //facturasFEL = _facturas.GetRutaFEL(ruta);
            ListaOLS.Clear();
            DataTable NCTabla = _nCreditos.CantidadNotasCredito(ruta, fecha, preimArra[0]);

            foreach (DataRow row in NCTabla.Rows)
            {
                try
                {
                    #region Cabecera

                    ListaOLS.Clear();
                    Maindata maindata = new Maindata();
                    maindata.correlativoInterno = "AV_" + _nCreditos.GetCorrelativoInterno(row["ZNROCF"].ToString().Trim());
                    maindata.serie = _nCreditos.GetNumSerieNC(row["ZNROCF"].ToString());
                    maindata.resolucion = _nCreditos.GetResolucionNC(maindata.serie);
                    maindata.resInicio = _nCreditos.GetResInicioNC(maindata.serie);
                    maindata.resFin = _nCreditos.GetResFinNC(maindata.serie);
                    //maindata.nit = _facturas.GetNit(ruta, row["idSerie"].ToString()).Trim();
                    maindata.nit = "0614-130571-001-2";
                    //maindata.resFecha = (_facturas.GetRestFecha(ruta, row["idSerie"].ToString())).Substring(0, _facturas.GetRestFecha(ruta, row["idSerie"].ToString()).Length - 9);  //SIN HORA SOLO FECHA
                    if (string.IsNullOrWhiteSpace(_nCreditos.GetRestFechaNC(maindata.serie)))
                    {
                        maindata.resFecha = (Convert.ToDateTime("01/01/1900")).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.resFecha = (Convert.ToDateTime(_nCreditos.GetRestFechaNC(maindata.serie))).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }

                    maindata.nrc = "233-0";
                    maindata.fechaEnvio = DateTime.Now.ToString().Trim();
                    //maindata.fechaEmision = (row["Fecha"].ToString()).Substring(0, row["Fecha"].ToString().Length - 9); //SIN FECHA SOLO HORA
                    //maindata.fechaEmision = (Convert.ToDateTime(row["Fecha"].ToString())).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    if (string.IsNullOrWhiteSpace(row["Fecha"].ToString()))
                    {
                        maindata.fechaEmision = (Convert.ToDateTime("01/01/1900")).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.fechaEmision = (Convert.ToDateTime(row["Fecha"].ToString())).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    maindata.terminal = ruta.ToString().Trim();
                    //maindata.numFactura = row["VBELNF"].ToString().Trim();
                    maindata.numFactura = _nCreditos.GetCorrelativoInterno(row["ZNROCF"].ToString().Trim());
                    //maindata.correlativoInterno = "AV_" + _nCreditos.GetCorrelativoInterno(row["VBELNF"].ToString().Trim(), row["ZOPERAC"].ToString());  //456
                    maindata.numeroTransaccion = "";
                    maindata.codigoUsuario = row["Vendedor"].ToString().Trim();
                    maindata.nombreUsuario = _nCreditos.GetNombreUsuario(row["Vendedor"].ToString());
                    maindata.correoUsuario = "";

                    //codigo cliente
                    maindata.codigoCliente = _nCreditos.GetClienteNC(row["ZNROCF"].ToString());

                    maindata.cajaSuc = ruta.ToString().Trim();
                    maindata.tipoDocumento = "NTC";
                    maindata.pdv = _facturas.GetNombreEstablecimiento(maindata.codigoCliente); //ESTABLECIMIENTO
                    maindata.nitCliente = _facturas.GetNITCliente(maindata.codigoCliente).Trim(); //DEBE APLICARSE TRIM

                    maindata.duiCliente = _facturas.GetDUI(maindata.codigoCliente).Trim();        //DEBE APLICARSE TRIM
                    maindata.nrcCliente = _facturas.GetNRC(maindata.codigoCliente).Trim();  //DEBE APLICARSE TRIM
                    //maindata.codigoCliente = _nCreditos.GetClienteNC(row["VBELNF"].ToString(), row["ZOPERAC"].ToString());
                    maindata.nombreCliente = _facturas.GetNombreCliente(maindata.codigoCliente).Trim();
                    maindata.direccionCliente = _facturas.GetDireccion(maindata.codigoCliente).Trim();
                    maindata.departamento = _facturas.GetDepartamento(maindata.codigoCliente).Trim();
                    maindata.municipio = _facturas.GetMunicipio(maindata.codigoCliente).Trim();
                    //maindata.giro = _facturas.GetGiroNegocio(maindata.codigoCliente).Trim();
                    maindata.codicionPago = "";
                    //maindata.ventaTotal = Convert.ToDouble(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString()));

                   
                    string percepcion = row["Percepcion"].ToString();
                    if (!string.IsNullOrEmpty(percepcion))
                    {
                        //maindata.subTotalVentasGravadas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"]) - Convert.ToDecimal(row["Percepcion"]));
                        //maindata.ventasGravadas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"]);
                        maindata.ventaTotal = Convert.ToDouble(Convert.ToDecimal(row["Total"]));
                    }
                    else
                    {
                        //maindata.ventasGravadas = Convert.ToDouble(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString()));
                        maindata.ventaTotal = Convert.ToDouble(Convert.ToDecimal(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString())) + Convert.ToDecimal(_nCreditos.GetIvaNc(row["ZNROCF"].ToString())));
                    }
                    maindata.montoLetras = _facturas.GetMontoLetras(maindata.ventaTotal).Trim();
                    maindata.CCFAnterior = _nCreditos.GetCCFAnteriorNC(row["ZNROCF"].ToString()); //123
                    maindata.vtaACuentaDe = "";
                    maindata.notaRemision = "";
                    maindata.noFecha = "";
                    maindata.saldoCapital = 0;
                    maindata.idDepartamentoReceptor = _facturas.GetIdDepartamento(maindata.codigoCliente); ;
                    maindata.idDepartamentoEmisor = "05";
                    maindata.direccionEmisor = "0";
                    maindata.fechaEnvio = DateTime.Now.Date.ToString();
                    maindata.idMunicipioEmisor = "05";
                    maindata.idMunicipioReceptor = _facturas.GetIdMunicipio(maindata.codigoCliente);
                    maindata.codigoActividadEconomica = _facturas.GetGiroNegocio2(maindata.codigoCliente);
                    maindata.giro = _facturas.GetActividadEconomica(maindata.codigoActividadEconomica);
                    //maindata.codigoActividadEconomica = "01282";
                    //maindata.giro = _facturas.GetActividadEconomica(row["idCliente"].ToString());
                    //maindata.giro = "OTROS";
                    //maindata.sumas = Convert.ToDouble(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString())) + Convert.ToDouble(_nCreditos.GetIvaNc(row["ZNROCF"].ToString()));
                    maindata.sumas = Convert.ToDouble(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString()));
                    maindata.subTotalVentasExentas = 0;
                    maindata.subTotalVentasNoSujetas = 0;

                    maindata.subTotalVentasGravadas = Convert.ToDouble(Convert.ToDecimal(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString())) + Convert.ToDecimal(_nCreditos.GetIvaNc(row["ZNROCF"].ToString())));
                    maindata.iva = Convert.ToDouble(_nCreditos.GetIvaNc(row["ZNROCF"].ToString()));
                    maindata.renta = 0;
                    maindata.impuesto = Convert.ToDouble(_nCreditos.GetIvaNc(row["ZNROCF"].ToString()));
                    maindata.ventasGravadas = Convert.ToDouble(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString()));
                    maindata.ventasExentas = 0;
                    maindata.ventasNoSujetas = 0;
                    maindata.totalExportaciones = 0;
                    maindata.descuentos = 0;
                    maindata.abonos = 0;
                    maindata.cantidadTotal = _nCreditos.GetCantidadTotal(row["ZNROCF"].ToString());
                    //maindata.cantidadTotal = Convert.ToDouble(_nCreditos.GetTotalNc(row["ZNROCF"].ToString()));
                    maindata.ventasGravadas13 = 0;
                    maindata.ventasGravadas0 = 0;
                    maindata.ventasNoGravadas = 0;
                    //maindata.ivaPercibido1 = Convert.ToDouble(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString()));
                    maindata.ivaPercibido1 = Convert.ToDouble(_nCreditos.GetPercepcionNc(row["ZNROCF"].ToString()));
                    //maindata.ivaPercibido1 = Convert.ToDouble(_nCreditos.GetTotalNc(row["ZNROCF"].ToString()));
                    maindata.ivaPercibido2 = 0;
                    maindata.ivaRetenido1 = 0;
                    //string percepcion = _nCreditos.GetPercepcionNc(row["ZNROCF"].ToString());
                    //if (string.IsNullOrEmpty(percepcion))
                    //{
                    //    maindata.ivaRetenido1 = 0;
                    //}
                    //else
                    //{
                    //    maindata.ivaRetenido1 = Convert.ToDouble(_nCreditos.GetPercepcionNc(row["ZNROCF"].ToString()));
                    //}
                    maindata.ivaRetenido13 = 0;
                    maindata.contribucionSeguridad = 0;
                    maindata.fovial = 0;
                    maindata.cotrans = 0;
                    maindata.contribucionTurismo5 = 0;
                    maindata.contribucionTurismo7 = 0;
                    maindata.impuestoEspecifico = 0;
                    maindata.cesc = 0;
                    maindata.observacionesDte = "";
                    maindata.campo1 = "";
                    //maindata.campo2 = _facturas.GetCodigoClientePrincipal(maindata.codigoCliente) + "|" + maindata.codigoCliente + "|" + _facturas.GetCentro(ruta.ToString()) + "|" + _facturas.GetZonaRuta(ruta.ToString()) + _facturas.GetCodigoRutaVenta(ruta.ToString()) + "|GT10";
                    //maindata.campo3 = _facturas.GetRutaVenta(maindata.codigoCliente);
                    //maindata.campo4 = _facturas.GetRutaReparto(ruta.ToString());

                    //maindata.campo2 = ipImpresora + "|" + maindata.campo2;

                    maindata.campo2 = maindata.codigoCliente + "|" + _facturas.GetCodigoClientePrincipal(maindata.codigoCliente) + "|" + _facturas.GetCentro(ruta.ToString()) + "|" + _facturas.GetZonaRuta(ruta.ToString()) + "|" + _facturas.GetCodigoRutaVenta(ruta.ToString()) + "|GT10|";

                    maindata.campo2 = ipImpresora + "|" + maindata.campo2;
                    maindata.campo2 = maindata.campo2 + _facturas.GetRutaVenta(maindata.codigoCliente) + "|" + _facturas.GetRutaReparto(ruta.ToString());
                    maindata.campo2 = maindata.campo2 + "|||||||||||" + _facturas.GetUsuarioGeneraPreImpresa(row["IdUsuarioCreacion"].ToString()) + "||";
                    //if (row["Factura"].ToString() == "" || row["Factura"].ToString() == "0") //revisa la secuencia
                    //{
                    //    maindata.campo2 = maindata.campo2 + "000";
                    //}
                    //else
                    //{
                    //    maindata.campo2 = maindata.campo2 + _facturas.GetSecuencia(row["idFactura"].ToString());
                    //}
                    //maindata.campo2 = maindata.campo2 + "OC:" + _facturas.GetOrdenCompraPreImpresa(row["idFactura"].ToString());
                    //maindata.campo2 = maindata.campo2 + "|" + _facturas.GetRutaVenta(maindata.codigoCliente) + "|" + _facturas.GetRutaReparto(ruta.ToString());
                    maindata.campo3 = "";
                    //maindata.campo3 = campo3X;vueno
                    //maindata.campo4 = _facturas.GetRutaReparto(ruta.ToString());
                    maindata.campo4 = campo3X;
                    //maindata.tipoDteRel = "05";
                    maindata.numeroControl = "";
                    maindata.codigoGeneracion = null;
                    maindata.modeloFacturacion = "1";
                    maindata.tipoTransmision = "1";
                    maindata.codContingencia = "";
                    maindata.motivoContin = null;
                    maindata.docRelTipo = "03";
                    maindata.docRelNum = maindata.CCFAnterior;
                    maindata.docRelFecha = _nCreditos.GetDocfec(row["ZNROCF"].ToString()).ToString("yyyy-MM-dd");
                    maindata.nombreComercialCl = "";
                    maindata.otrosDocIdent = "";
                    maindata.otrosDocDescri = "";
                    maindata.ventCterNit = "";
                    maindata.ventCterNombre = "";
                    maindata.montGDescVentNoSujetas = 0.0;
                    maindata.montGDescVentExentas = 0.0;
                    maindata.montGDescVentGrav = 0.0;
                    maindata.totOtroMonNoAfec = 0.0;
                    percepcion = row["Percepcion"].ToString();
                    if (!string.IsNullOrEmpty(percepcion))
                    {
                        //maindata.subTotalVentasGravadas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"]) - Convert.ToDecimal(row["Percepcion"]));
                        //maindata.ventasGravadas = Convert.ToDouble(Convert.ToDecimal(row["TotalFactura"]);
                        maindata.totalAPagar = Convert.ToDouble(Convert.ToDecimal(row["Total"]));
                    }
                    else
                    {
                        //maindata.ventasGravadas = Convert.ToDouble(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString()));
                        maindata.totalAPagar = Convert.ToDouble(Convert.ToDecimal(_nCreditos.GetSubTotalNc(row["ZNROCF"].ToString())) + Convert.ToDecimal(_nCreditos.GetIvaNc(row["ZNROCF"].ToString())));
                    }
                    
                    //maindata.totalAPagar = Convert.ToDouble(_nCreditos.GetTotalNc(row["ZNROCF"].ToString()));
                    maindata.responsableEmisor = "";
                    maindata.numDocEmisor = "";
                    maindata.responsableReceptor = "";
                    maindata.numDocReceptor = "";
                    maindata.nomConductor = "";
                    maindata.numIdenConductor = "";
                    maindata.modTransp = "";
                    maindata.numIdTransp = "";
                    maindata.formaPago = "";
                    maindata.plazo = "";
                    maindata.seguro = 0.0;
                    maindata.flete = 0.0;
                    maindata.arTributos = null;

                    ArTributo tributox = new ArTributo();
                    tributox.valorTributo = Convert.ToDouble(_nCreditos.GetIvaNc(row["ZNROCF"].ToString()));
                    tributox.codigoTributo = "20";
                    tributox.descripcionTributo = "Impuesto al Valor Agregado 13%";
                    maindata.arTributos = new List<ArTributo>();
                    maindata.arTributos.Add(tributox);

                    maindata.mostrarTributo = false;
                    maindata.bienTitulo = "0";
                    maindata.tipoDocumentoReceptor = "NIT";

                    ListaOLS.Add(maindata);

                    #endregion Cabecera

                    #region Contacto

                    //REGION CONTACTO
                    List<Contacto> ListaContactos = new List<Contacto>
                {
                    new Contacto
                        {
                            whatsapp="",
                            sms="",
                            email = _facturas.GetCorreo(ListaOLS[0].codigoCliente)=="" || _facturas.GetCorreo(ListaOLS[0].codigoCliente)==null ? "cmia-fel-sv@somoscmi.com":_facturas.GetCorreo(ListaOLS[0].codigoCliente),
                            telefono = _facturas.GetTelefono(ListaOLS[0].codigoCliente)=="" || _facturas.GetTelefono(ListaOLS[0].codigoCliente)==null ? "74658546":_facturas.GetTelefono(ListaOLS[0].codigoCliente),
                        }
                };
                    maindata.contactos = ListaContactos;

                    #endregion Contacto

                    #region Detalle

                    //DETALLE
                    DataTable DetalleNC = _nCreditos.CantidadDetalle(ruta, row["ZNROCF"].ToString(), fecha);

                    List<Detalle> detalleOLS = new List<Detalle>();

                    List<Unidadmedida> UnidadeMedidaOLS = new List<Unidadmedida>();

                    double cantidadTem = 0;

                    foreach (DataRow rowDeta in DetalleNC.Rows)
                    {
                        if (_facturas.CompruebaUnidadMedida(rowDeta["MATNR"].ToString()) == "1")
                        {
                            cantidadTem = _nCreditos.GetCantidadDetalle(row["ZNROCF"].ToString(), rowDeta["MATNR"].ToString());
                        }
                        else
                        {
                            cantidadTem = _nCreditos.GetPesoProductoDetalle(row["ZNROCF"].ToString(), rowDeta["MATNR"].ToString());
                        }

                        Detalle detalle = new Detalle();
                        detalleOLS.Add(
                            new Detalle
                            {
                                //cantidad = _nCreditos.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_nCreditos.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _nCreditos.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                //precioUnitario = _nCreditos.GetPrecioUnitarioDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                //ventasNoSujetas = 0,
                                //ventasExentas = 0,
                                //ventasGravadas = _nCreditos.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                //desc = "",
                                //fecha = "",
                                //delAl = "",
                                //exportaciones = "0.0"

                                //descripcion = rowDeta["MATNR"].ToString() + "|" + (_nCreditos.GetPesoProductoDetalle(row["ZNROCF"].ToString(), rowDeta["MATNR"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _nCreditos.GetNombreProducto(rowDeta["MATNR"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["MATNR"].ToString(), _nCreditos.GetClienteNC(row["ZNROCF"].ToString())) + "|",
                                descripcion = rowDeta["MATNR"].ToString() + "|" + "PLU:" + _facturas.GetPLUProductoPreimpresas(rowDeta["MATNR"].ToString(), maindata.codigoCliente) + "|" + _facturas.GetNombreProducto(rowDeta["MATNR"].ToString()) + "|" + (_nCreditos.GetUnidadesProductoDetalle(row["ZNROCF"].ToString(), rowDeta["MATNR"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + " UN" + "|" + (_nCreditos.GetPesoProductoDetalle(row["ZNROCF"].ToString(), rowDeta["MATNR"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + " LB" + "|",
                                //codTributo = "",
                                codTributo = null,
                                tributos = new List<string>() { "20" },
                                precioUnitario = _nCreditos.GetPrecioUnitarioDetalle(row["ZNROCF"].ToString(), rowDeta["MATNR"].ToString()),
                                ventasNoSujetas = 0,
                                ivaItem = _nCreditos.GetIVALineaNc(row["ZNROCF"].ToString(), rowDeta["MATNR"].ToString()),
                                delAl = "",
                                exportaciones = "0.0",
                                numDocRel = "",
                                uniMedidaCodigo = _facturas.GetUnidadFacturacion(rowDeta["MATNR"].ToString()) == 1 ? 59 : 36,
                                ventasExentas = 0,
                                fecha = "",
                                tipoItem = 2, //CONSULTAR
                                tipoDteRel = "03",
                                codigoRetencionMH = "",
                                //cantidad = _nCreditos.GetCantidadDetalle(row["VBELNF"].ToString(), rowDeta["MATNR"].ToString()),
                                cantidad = cantidadTem,
                                //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                ventasGravadas = _nCreditos.GetVentasGravadasDetalle(row["ZNROCF"].ToString(), rowDeta["MATNR"].ToString()),
                                ivaRetenido = 0.0,
                                desc = "0",
                                descuentoItem = 0.0,
                                otroMonNoAfec = 0.0
                            });
                        maindata.detalle = detalleOLS;
                    }

                    #endregion Detalle

                    #region ENVIAR/RECEPCION DATA

                    string nuevoNC = Token + "|" + preimArra[0];

                    respuestaOLS = EnvioDataOLS(ListaOLS, 2, fecha, nuevoNC);
                    respuestaEnvio = respuestaOLS.mensajeCompleto;

                    #endregion ENVIAR/RECEPCION DATA
                }
                catch (Exception ex)
                {
                    var stackTrace = new StackTrace(ex);
                    var thisAssembly = Assembly.GetExecutingAssembly();
                    var stackFrames = stackTrace.GetFrames();
                    var method = stackFrames
                        .Select(frame => frame.GetMethod())
                        .FirstOrDefault(m => m.Module.Assembly == thisAssembly);

                    var lineNumber = method != null ? stackFrames.First().GetFileLineNumber() : 0;
                    var methodName = method != null ? method.Name : "Unknown";
                    var fileName = method != null ? stackFrames.First().GetFileName() : "Unknown";
                    var className = method != null ? method.DeclaringType.FullName : "Unknown";
                    var namespaceName = method != null ? method.DeclaringType.Namespace : "Unknown";
                    var parameters = method != null ? method.GetParameters() : null;
                    var parameterInfo = parameters != null ? string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}")) : "No parameters";
                    var innerExceptionInfo = ex.InnerException != null ?
                        $"Excepción interna: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}" :
                        "No excepción interna";

                    var errorMsj = string.Format(
                        "Error interno: {0}\n" +
                        "Método: {1}\n" +
                        "Número de línea: {2}\n" +
                        "Archivo: {3}\n" +
                        "Clase: {4}\n" +
                        "Namespace: {5}\n" +
                        "Parámetros: {6}\n" +
                        "{7}",
                        ex.Message, methodName, lineNumber, fileName, className, namespaceName, parameterInfo, innerExceptionInfo);

                    respuestaOLS.mensajeCompleto = errorMsj;
                    respuestaOLS.ResultadoSatisfactorio = false;
                }
            }

            return respuestaOLS;
        }

        /// <summary>
        /// ENVIA NOTAS DE CREDITO
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <param name="docPos"></param>
        /// <param name="numFac"></param>
        /// <returns></returns>
        [WebMethod(Description = "Envia solo Notas de Credito")]
        public RespuestaOLS EnviarNotasCreditosHH(int ruta, string fecha, int docPos, long numFac)
        {
            RespuestaOLS respuestaOLS = new RespuestaOLS();

            string Token = "";
            string revisaT = RevisarToken();
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            /**********NOTA DE CREDITO*************/
            /*********TABLA: Reparto.DocumentosFacturasEBajada********/
            string respuestaEnvio = "";
            List<Maindata> ListaOLS = new List<Maindata>();
            bool facturasFEL = false;
            //facturasFEL = _facturas.GetRutaFEL(ruta);
            ListaOLS.Clear();
            DataTable NCTabla = _nCreditosHH.CantidadNotasCredito(ruta, fecha, numFac);

            foreach (DataRow row in NCTabla.Rows)
            {
                try
                {
                    #region Cabecera

                    ListaOLS.Clear();
                    Maindata maindata = new Maindata();
                    maindata.resolucion = _facturas.GetResolucion(ruta, row["idSerie"].ToString()).Trim();
                    maindata.resInicio = _facturas.GetResInicio(ruta, row["idSerie"].ToString()).Trim();
                    maindata.resFin = _facturas.GetResFin(ruta, row["idSerie"].ToString()).Trim();
                    //maindata.nit = _facturas.GetNit(ruta, row["idSerie"].ToString()).Trim();
                    maindata.nit = "0614-130571-001-2";
                    //maindata.resFecha = (_facturas.GetRestFecha(ruta, row["idSerie"].ToString())).Substring(0, _facturas.GetRestFecha(ruta, row["idSerie"].ToString()).Length - 9);  //SIN HORA SOLO FECHA
                    if (string.IsNullOrWhiteSpace(_facturas.GetRestFecha(ruta, row["idSerie"].ToString())))
                    {
                        maindata.resFecha = (Convert.ToDateTime("01/01/1900")).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.resFecha = (Convert.ToDateTime(_facturas.GetRestFecha(ruta, row["idSerie"].ToString()))).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }

                    maindata.nrc = "233-0";
                    maindata.fechaEnvio = DateTime.Now.ToString().Trim();
                    //maindata.fechaEmision = (row["Fecha"].ToString()).Substring(0, row["Fecha"].ToString().Length - 9); //SIN FECHA SOLO HORA
                    //maindata.fechaEmision = (Convert.ToDateTime(row["Fecha"].ToString())).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    if (string.IsNullOrWhiteSpace(row["Fecha"].ToString()))
                    {
                        maindata.fechaEmision = (Convert.ToDateTime("01/01/1900")).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.fechaEmision = (Convert.ToDateTime(row["Fecha"].ToString())).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    }
                    maindata.terminal = ruta.ToString().Trim();
                    maindata.numFactura = row["Numero"].ToString().Trim();
                    maindata.correlativoInterno = "AV_" + row["Numero"].ToString().Trim();  //456
                    maindata.numeroTransaccion = "";
                    maindata.codigoUsuario = row["idempleado"].ToString().Trim();
                    maindata.nombreUsuario = _facturas.GetNombreUsuario(row["idempleado"].ToString());
                    maindata.correoUsuario = "";
                    maindata.serie = _facturas.GetNumSerie(ruta, row["idSerie"].ToString()).Trim();
                    maindata.cajaSuc = ruta.ToString().Trim();
                    maindata.tipoDocumento = "NTC";
                    maindata.pdv = _facturas.GetNombreEstablecimiento(row["IdCliente"].ToString()); //ESTABLECIMIENTO //ESTABLECIMIENTO
                    maindata.nitCliente = _facturas.GetNITCliente(row["IdCliente"].ToString().Trim()).Trim(); //DEBE APLICARSE TRIM

                    maindata.duiCliente = _facturas.GetDUI(row["IdCliente"].ToString().Trim()).Trim();        //DEBE APLICARSE TRIM
                    maindata.nrcCliente = _facturas.GetNRC(row["IdCliente"].ToString().Trim()).Trim(); //DEBE APLICARSE TRIM
                    maindata.codigoCliente = row["IdCliente"].ToString().Trim();
                    maindata.nombreCliente = _facturas.GetNombreCliente(maindata.codigoCliente).Trim();
                    maindata.direccionCliente = _facturas.GetDireccion(maindata.codigoCliente).Trim();
                    maindata.departamento = _facturas.GetDepartamento(maindata.codigoCliente).Trim();
                    maindata.municipio = _facturas.GetMunicipio(maindata.codigoCliente).Trim();
                    maindata.giro = _facturas.GetGiroNegocio(maindata.codigoCliente).Trim();
                    maindata.codicionPago = "";
                    maindata.ventaTotal = Convert.ToDouble(row["Total"].ToString());
                    maindata.montoLetras = _facturas.GetMontoLetras(maindata.ventaTotal).Trim();
                    maindata.CCFAnterior = _nCreditosHH.GetCCFAnterior(ruta.ToString(), fecha, row["Numero"].ToString());
                    maindata.vtaACuentaDe = "";
                    maindata.notaRemision = "";
                    maindata.noFecha = "";
                    maindata.saldoCapital = 0;
                    maindata.idDepartamentoReceptor = _facturas.GetIdDepartamento(maindata.codigoCliente); ;
                    maindata.idDepartamentoEmisor = "05";
                    maindata.direccionEmisor = "0";
                    maindata.fechaEnvio = DateTime.Now.Date.ToString();
                    maindata.idMunicipioEmisor = "05";
                    maindata.idMunicipioReceptor = _facturas.GetIdMunicipio(maindata.codigoCliente);
                    maindata.codigoActividadEconomica = _facturas.GetGiroNegocio2(maindata.codigoCliente);
                    maindata.giro = _facturas.GetActividadEconomica(maindata.codigoCliente);
                    maindata.sumas = Convert.ToDouble(row["SubTotal"].ToString());
                    maindata.subTotalVentasExentas = 0;
                    maindata.subTotalVentasNoSujetas = 0;
                    maindata.subTotalVentasGravadas = Convert.ToDouble(row["SubTotal"].ToString());
                    maindata.iva = Convert.ToDouble(row["Iva"].ToString());
                    maindata.renta = 0;
                    maindata.impuesto = Convert.ToDouble(row["Iva"].ToString());
                    maindata.ventasGravadas = Convert.ToDouble(row["SubTotal"].ToString());
                    maindata.ventasExentas = 0;
                    maindata.ventasNoSujetas = 0;
                    maindata.totalExportaciones = 0;
                    maindata.descuentos = 0;
                    maindata.abonos = 0;
                    maindata.cantidadTotal = _nCreditosHH.GetCantidadTotal(ruta, row["Numero"].ToString(), fecha);
                    maindata.ventasGravadas13 = 0;
                    maindata.ventasGravadas0 = 0;
                    maindata.ventasNoGravadas = 0;
                    maindata.ivaPercibido1 = Convert.ToDouble(row["Total"].ToString());
                    maindata.ivaPercibido2 = 0;
                    string percepcion = row["Percepcion"].ToString();
                    if (string.IsNullOrEmpty(percepcion))
                    {
                        maindata.ivaRetenido1 = 0;
                    }
                    else
                    {
                        maindata.ivaRetenido1 = Convert.ToDouble(row["Percepcion"].ToString());
                    }
                    maindata.ivaRetenido13 = 0;
                    maindata.contribucionSeguridad = 0;
                    maindata.fovial = 0;
                    maindata.cotrans = 0;
                    maindata.contribucionTurismo5 = 0;
                    maindata.contribucionTurismo7 = 0;
                    maindata.impuestoEspecifico = 0;
                    maindata.cesc = 0;
                    maindata.observacionesDte = "";
                    maindata.campo1 = "";
                    maindata.campo2 = _facturas.GetCodigoClientePrincipal(maindata.codigoCliente) + "|" + maindata.codigoCliente + "|" + _facturas.GetCentro(ruta.ToString()) + "|" + _facturas.GetZonaRuta(ruta.ToString()) + _facturas.GetCodigoRutaVenta(ruta.ToString()) + "|GT10";
                    maindata.campo3 = _facturas.GetRutaVenta(maindata.codigoCliente);
                    maindata.campo4 = _facturas.GetRutaReparto(ruta.ToString());

                    maindata.numeroControl = "";
                    maindata.codigoGeneracion = null;
                    maindata.modeloFacturacion = "1";
                    maindata.tipoTransmision = "1";
                    maindata.codContingencia = "";
                    maindata.motivoContin = null;
                    maindata.docRelTipo = "";
                    maindata.docRelNum = "";
                    maindata.docRelFecha = "";
                    maindata.nombreComercialCl = "";
                    maindata.otrosDocIdent = "";
                    maindata.otrosDocDescri = "";
                    maindata.ventCterNit = "";
                    maindata.ventCterNombre = "";
                    maindata.montGDescVentNoSujetas = 0.0;
                    maindata.montGDescVentExentas = 0.0;
                    maindata.montGDescVentGrav = 0.0;
                    maindata.totOtroMonNoAfec = 0.0;
                    maindata.totalAPagar = Convert.ToDouble(row["SubTotal"].ToString());
                    maindata.responsableEmisor = "";
                    maindata.numDocEmisor = "";
                    maindata.responsableReceptor = "";
                    maindata.numDocReceptor = "";
                    maindata.nomConductor = "";
                    maindata.numIdenConductor = "";
                    maindata.modTransp = "";
                    maindata.numIdTransp = "";
                    maindata.formaPago = "";
                    maindata.plazo = "";
                    maindata.seguro = 0.0;
                    maindata.flete = 0.0;
                    maindata.arTributos = null;

                    ArTributo tributox = new ArTributo();
                    tributox.valorTributo = Convert.ToDouble(row["Iva"].ToString());
                    tributox.codigoTributo = "20";
                    tributox.descripcionTributo = "Impuesto al Valor Agregado 13%";
                    // maindata.arTributos = new List<ArTributo>();
                    maindata.arTributos.Add(tributox);

                    maindata.mostrarTributo = false;
                    maindata.bienTitulo = "0";
                    maindata.tipoDocumentoReceptor = "NIT";

                    ListaOLS.Add(maindata);

                    #endregion Cabecera

                    #region Contacto

                    //REGION CONTACTO
                    List<Contacto> ListaContactos = new List<Contacto>
                    {
                       new Contacto
                        {
                            whatsapp="",
                            sms="",
                            email = _facturas.GetCorreo(ListaOLS[0].codigoCliente)=="" || _facturas.GetCorreo(ListaOLS[0].codigoCliente)==null ? "victor.duarte@somoscmi.com":_facturas.GetCorreo(ListaOLS[0].codigoCliente),
                            telefono = _facturas.GetTelefono(ListaOLS[0].codigoCliente)=="" || _facturas.GetTelefono(ListaOLS[0].codigoCliente)==null ? "74658546":_facturas.GetTelefono(ListaOLS[0].codigoCliente),
                        }
                    };
                    maindata.contactos = ListaContactos;

                    #endregion Contacto

                    #region Detalle

                    //DETALLE
                    DataTable DetalleNC = _nCreditosHH.CantidadDetalle(ruta, row["Numero"].ToString(), fecha);

                    List<Detalle> detalleOLS = new List<Detalle>();

                    List<Unidadmedida> UnidadeMedidaOLS = new List<Unidadmedida>();

                    double cantidadTem = 0;

                    foreach (DataRow rowDeta in DetalleNC.Rows)
                    {
                        if (_facturas.CompruebaUnidadMedida(rowDeta["IdProductos"].ToString()) == "1")
                        {
                            cantidadTem = _nCreditosHH.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                        }
                        else
                        {
                            cantidadTem = _nCreditosHH.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString());
                        }

                        Detalle detalle = new Detalle();
                        detalleOLS.Add(
                            new Detalle
                            {
                                //cantidad = _nCreditos.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                //descripcion = rowDeta["IdProductos"].ToString() + "|" + (_nCreditos.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _nCreditos.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                //precioUnitario = _nCreditos.GetPrecioUnitarioDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                //ventasNoSujetas = 0,
                                //ventasExentas = 0,
                                //ventasGravadas = _nCreditos.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                //desc = "",
                                //fecha = "",
                                //delAl = "",
                                //exportaciones = "0.0"

                                descripcion = rowDeta["IdProductos"].ToString() + "|" + (_nCreditosHH.GetPesoProductoDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _nCreditosHH.GetNombreProducto(rowDeta["IdProductos"].ToString()) + "|" + _facturas.GetPLUProducto(rowDeta["IdProductos"].ToString(), row["IdCliente"].ToString()) + "|",
                                codTributo = null,
                                tributos = new List<string>() { "20" },
                                precioUnitario = _nCreditosHH.GetPrecioUnitarioDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                ventasNoSujetas = 0,
                                ivaItem = _nCreditosHH.GetIvaDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                delAl = "",
                                exportaciones = "0.0",
                                numDocRel = "",
                                uniMedidaCodigo = _facturas.GetUnidadFacturacion(rowDeta["IdProductos"].ToString()) == 1 ? 59 : 36,
                                ventasExentas = 0,
                                fecha = "",
                                tipoItem = 2, //CONSULTAR
                                tipoDteRel = "",
                                codigoRetencionMH = "",
                                //cantidad = _nCreditosHH.GetCantidadDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                cantidad = cantidadTem,
                                //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                ventasGravadas = _nCreditosHH.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                ivaRetenido = 0.0,
                                desc = "0",
                                descuentoItem = 0.0,
                                otroMonNoAfec = 0.0
                            });
                        maindata.detalle = detalleOLS;
                    }

                    #endregion Detalle

                    #region ENVIAR/RECEPCION DATA

                    respuestaOLS = EnvioDataOLS(ListaOLS, 2, fecha, Token);
                    respuestaEnvio = respuestaOLS.mensajeCompleto;

                    #endregion ENVIAR/RECEPCION DATA
                }
                catch (Exception ex)
                {
                    var s = new StackTrace(ex);
                    var thisasm = Assembly.GetExecutingAssembly();
                    var methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                    string errorMsj = @"Error interno:" + ex.Message.ToString() + "\n" +
                             "Metodo:" + methodname;
                    //GrabarErrorInternos(ruta, fecha, docPos, numFac, errorMsj);
                    respuestaOLS.mensajeCompleto = errorMsj;
                    respuestaOLS.ResultadoSatisfactorio = false;
                }
            }

            return respuestaOLS;
        }

        /// <summary>
        /// ENVIA SOLO NOTAS DE REMISION
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <param name="docPos"></param>
        /// <param name="numFac"></param>
        /// <returns></returns>
        [WebMethod(Description = "Envia solo Notas de Remision")]
        public RespuestaOLS EnviarNotasRemision(int ruta, string fecha, int docPos, int numFac)
        {
            RespuestaOLS respuestaOLS = new RespuestaOLS();
            try
            {
                /******NOTA DE REMISION******/
                /*****TABLA : Handheld.NotaRemisionBajada*******/
                string respuestaEnvio = "";
                List<Maindata> ListaOLS = new List<Maindata>();
                ListaOLS.Clear();
                DataTable NRTabla = _nRemision.CantidadNotasRemision(ruta, fecha, numFac);
                foreach (DataRow row in NRTabla.Rows)
                {
                    #region Cabecera

                    ListaOLS.Clear();
                    Maindata maindata = new Maindata();
                    maindata.resolucion = _facturas.GetResolucion(ruta, row["idSerie"].ToString()).Trim();
                    maindata.resInicio = _facturas.GetResInicio(ruta, row["idSerie"].ToString()).Trim();
                    maindata.resFin = _facturas.GetResFin(ruta, row["idSerie"].ToString()).Trim();
                    //maindata.nit = _facturas.GetNit(ruta, row["idSerie"].ToString()).Trim();
                    maindata.nit = "0614-130571-001-2";
                    //maindata.resFecha = (_facturas.GetRestFecha(ruta, row["idSerie"].ToString())).Substring(0, _facturas.GetRestFecha(ruta, row["idSerie"].ToString()).Length - 9);  //SIN HORA SOLO FECHA
                    if (string.IsNullOrWhiteSpace(_facturas.GetRestFecha(ruta, row["idSerie"].ToString())))
                    {
                        maindata.resFecha = (Convert.ToDateTime("01/01/1900")).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }
                    else
                    {
                        maindata.resFecha = (Convert.ToDateTime(_facturas.GetRestFecha(ruta, row["idSerie"].ToString()))).ToString("dd-MM-yyyy"); //SIN FECHA SOLO HORA
                    }
                    maindata.nrc = "233-0";
                    maindata.fechaEnvio = DateTime.Now.ToString().Trim();
                    //maindata.fechaEmision = (row["FechaGeneracion"].ToString()).Substring(0, row["FechaGeneracion"].ToString().Length - 9); //SIN FECHA SOLO HORA
                    maindata.fechaEmision = (Convert.ToDateTime(fecha)).ToString("dd/MM/yyyy"); //SIN FECHA SOLO HORA
                    maindata.terminal = ruta.ToString().Trim();
                    maindata.numFactura = row["Correlativo"].ToString().Trim();
                    maindata.correlativoInterno = row["Correlativo"].ToString().Trim();
                    maindata.numeroTransaccion = row["Correlativo"].ToString().Trim(); //numero de pedido
                    maindata.codigoUsuario = "";
                    maindata.nombreUsuario = "";
                    maindata.correoUsuario = "";
                    maindata.serie = _facturas.GetNumSerie(ruta, row["idSerie"].ToString()).Trim();
                    maindata.cajaSuc = ruta.ToString().Trim();
                    maindata.tipoDocumento = "NTR_movil";
                    maindata.pdv = Convert.ToString(ruta);
                    maindata.nitCliente = "06141305710012";
                    maindata.duiCliente = "";
                    maindata.nrcCliente = "";
                    maindata.codigoCliente = "3001240";
                    maindata.nombreCliente = "AVICOLA SALVADOREÑA, S.A. DE C.V.";
                    maindata.direccionCliente = "";
                    maindata.departamento = "";
                    maindata.municipio = "";
                    maindata.giro = "Cría de aves de corral y producción de huevos";
                    maindata.codicionPago = "";
                    maindata.ventaTotal = 0;
                    maindata.montoLetras = "";
                    maindata.CCFAnterior = "";
                    maindata.vtaACuentaDe = "";
                    maindata.notaRemision = row["Correlativo"].ToString().Trim(); ;
                    maindata.noFecha = (Convert.ToDateTime(fecha)).ToString("dd/MM/yyyy");
                    maindata.saldoCapital = 0;
                    maindata.sumas = 0;
                    maindata.subTotalVentasExentas = 0;
                    maindata.subTotalVentasNoSujetas = 0;
                    maindata.subTotalVentasGravadas = 0;
                    maindata.iva = 0;
                    maindata.renta = 0;
                    maindata.impuesto = 0;
                    maindata.ventasGravadas = 0;
                    maindata.ventasExentas = 0;
                    maindata.ventasNoSujetas = 0;
                    maindata.totalExportaciones = 0;
                    maindata.descuentos = 0;
                    maindata.abonos = 0;
                    maindata.cantidadTotal = _nRemision.GetCantidadTotal(ruta, row["Correlativo"].ToString(), fecha);
                    maindata.ventasGravadas13 = 0;
                    maindata.ventasGravadas0 = 0;
                    maindata.ventasNoGravadas = 0;
                    maindata.ivaPercibido1 = 0;
                    maindata.ivaPercibido2 = 0;
                    maindata.ivaRetenido1 = 0;
                    maindata.ivaRetenido13 = 0;
                    maindata.contribucionSeguridad = 0;
                    maindata.fovial = 0;
                    maindata.cotrans = 0;
                    maindata.contribucionTurismo5 = 0;
                    maindata.contribucionTurismo7 = 0;
                    maindata.impuestoEspecifico = 0;
                    maindata.cesc = 0;
                    maindata.observacionesDte = "";
                    maindata.campo1 = "";
                    maindata.campo2 = "06141305710012|06141305710012|" + _facturas.GetCentro(ruta.ToString()) + "|" + _facturas.GetZonaRuta(ruta.ToString()) + "|" + _facturas.GetCodigoRutaVenta(ruta.ToString()) + "|GT11";
                    maindata.campo3 = "";
                    maindata.campo4 = _facturas.GetRutaReparto(ruta.ToString());
                    //maindata.codigoActividadEconomica = "";
                    maindata.codigoActividadEconomica = "01460";

                    maindata.numeroControl = "";
                    maindata.codigoGeneracion = _nCreditos.GetCodigoGeneracion(ruta, row["NumeroPedido"].ToString(), row["Numero"].ToString());
                    maindata.modeloFacturacion = maindata.codigoGeneracion != null ? "2" : "1";
                    maindata.tipoTransmision = maindata.codigoGeneracion != null ? "2" : "1";
                    maindata.codContingencia = maindata.codigoGeneracion == null ? "" : "3";
                    maindata.motivoContin = null;
                    maindata.docRelTipo = "";
                    maindata.docRelNum = "";
                    maindata.docRelFecha = "";
                    maindata.nombreComercialCl = "";
                    maindata.otrosDocIdent = "";
                    maindata.otrosDocDescri = "";
                    maindata.ventCterNit = "";
                    maindata.ventCterNombre = "";
                    maindata.montGDescVentNoSujetas = 0.0;
                    maindata.montGDescVentExentas = 0.0;
                    maindata.montGDescVentGrav = 0.0;
                    maindata.totOtroMonNoAfec = 0.0;
                    maindata.totalAPagar = 0.0;
                    maindata.responsableEmisor = "";
                    maindata.numDocEmisor = "";
                    maindata.responsableReceptor = "";
                    maindata.numDocReceptor = "";
                    maindata.nomConductor = "";
                    maindata.numIdenConductor = "";
                    maindata.modTransp = "";
                    maindata.numIdTransp = "";
                    maindata.formaPago = "";
                    maindata.plazo = "";
                    maindata.seguro = 0.0;
                    maindata.flete = 0.0;
                    maindata.arTributos = null;
                    maindata.mostrarTributo = false;

                    ListaOLS.Add(maindata);

                    #endregion Cabecera

                    #region Contacto

                    //REGION CONTACTO
                    List<Contacto> ListaContactos = new List<Contacto>
                    {
                        new Contacto
                        {
                            whatsapp="",
                            sms="",
                            email = "",
                            telefono = ""
                        }
                    };
                    maindata.contactos = ListaContactos;

                    #endregion Contacto

                    #region Detalle

                    //DETALLE
                    DataTable DetalleNC = _nRemision.CantidadDetalle(ruta, row["Correlativo"].ToString(), fecha);

                    List<Detalle> detalleOLS = new List<Detalle>();

                    List<Unidadmedida> UnidadeMedidaOLS = new List<Unidadmedida>();

                    foreach (DataRow rowDeta in DetalleNC.Rows)
                    {
                        Detalle detalle = new Detalle();
                        detalleOLS.Add(
                            new Detalle
                            {
                                //cantidad = _nRemision.GetCantidadDetalle(ruta, row["Correlativo"].ToString(), rowDeta["idProducto"].ToString()),
                                //descripcion = rowDeta["idProducto"].ToString() + "|" + (_nRemision.GetPesoProductoDetalle(ruta, row["Correlativo"].ToString(), rowDeta["idProducto"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _nCreditos.GetNombreProducto(rowDeta["idProducto"].ToString()) + "|   |",
                                //precioUnitario = 0,
                                //ventasNoSujetas = 0,
                                //ventasExentas = 0,
                                //ventasGravadas = 0,
                                //desc = "",
                                //fecha = "",
                                //delAl = "",
                                //exportaciones = "0.0"

                                descripcion = rowDeta["idProducto"].ToString() + "|" + (_nRemision.GetPesoProductoDetalle(ruta, row["Correlativo"].ToString(), rowDeta["idProducto"].ToString()).ToString("F", CultureInfo.InvariantCulture)) + "|" + _nCreditos.GetNombreProducto(rowDeta["idProducto"].ToString()) + "|   |",
                                codTributo = null,
                                tributos = null,
                                precioUnitario = 0,
                                ventasNoSujetas = 0,
                                ivaItem = 0,
                                delAl = "",
                                exportaciones = "0.0",
                                numDocRel = "",
                                uniMedidaCodigo = _facturas.GetUnidadFacturacion(rowDeta["IdProductos"].ToString()) == 1 ? 59 : 36,
                                ventasExentas = 0,
                                fecha = "",
                                tipoItem = 2, //CONSULTAR
                                tipoDteRel = "",
                                codigoRetencionMH = "",
                                cantidad = _nRemision.GetCantidadDetalle(ruta, row["Correlativo"].ToString(), rowDeta["idProducto"].ToString()),
                                //ventasGravadas = _facturas.GetVentasGravadasDetalle(ruta, row["Numero"].ToString(), rowDeta["IdProductos"].ToString()),
                                ventasGravadas = 0,
                                ivaRetenido = 0.0,
                                desc = "",
                                descuentoItem = 0.0,
                                otroMonNoAfec = 0.0
                            });
                        maindata.detalle = detalleOLS;
                    }

                    #endregion Detalle

                    #region ENVIAR/RECEPCION DATA

                    //respuestaEnvio = EnvioDataOLS(ListaOLS, docPos, fecha);
                    respuestaOLS = EnvioDataOLS(ListaOLS, docPos, fecha, "");
                    respuestaEnvio = respuestaOLS.mensajeCompleto;

                    #endregion ENVIAR/RECEPCION DATA
                }

                //return respuestaOLS;
            }
            catch (Exception ex)
            {
                StackTrace s = new StackTrace(ex);
                Assembly thisasm = Assembly.GetExecutingAssembly();
                string methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                string errorMsj = @"Error interno:" + ex.Message.ToString() + "\n" +
                         "Metodo:" + methodname;
                GrabarErrorInternos(ruta, fecha, docPos, numFac, errorMsj);
                respuestaOLS.mensajeCompleto = errorMsj;
                respuestaOLS.ResultadoSatisfactorio = false;
            }

            return respuestaOLS;
        }

        /// <summary>
        /// ENVIA DATOS PARA ANULAR
        /// </summary>
        /// <param name="mapaAnulacion"></param>
        /// <returns></returns>
        //[WebMethod(Description ="Envia Anulacion de Documento")]
        //public RespuestaOLS AnulaDocumentos(MapaAnulacion mapaAnulacion)
        //{
        //    string fechaAnu = mapaAnulacion.fechaDoc;
        //    string[] fac_ruta = mapaAnulacion.correlativointerno.Split('|');
        //}

        /// <summary>
        /// SE ENVIAN DATOS HACIA OLS
        /// </summary>
        /// <param name="DatosRaw"></param>
        /// <param name="TipoDocEnvio"></param>
        /// <returns></returns>
        public RespuestaOLS EnvioDataOLS(List<Maindata> DatosRaw, int TipoDocEnvio, string fecha, string tokenX)
        {
            List<string> CamposVerificar = new List<string>();
            List<string> camposVacios = new List<string>();
            RespuestaOLS respuestaOLS = new RespuestaOLS();

            string NC_Only = "";

            try
            {
                int rutaTemp;
                int serieTemp2;
                string[] serieTemp3;
                string resolucionTemp;
                string facturaTemp;
                string DIC;
                string respuestaMetodo = "";
                if (TipoDocEnvio == 1)
                {
                    DIC = "F";

                    #region Obligatorios FAC

                    CamposVerificar.AddRange(new List<string>
                    {
                            "nit",
                            "nrc",
                            "idDepartamentoEmisor",
                            "idMunicipioEmisor",
                            "direccionEmisor",
                            "fechaEnvio",
                            "fechaEmision",
                            "numFactura",
                            "tipoDocumento",
                            "nitCliente",
                            "nrcCliente",
                            "nombreCliente",
                            "direccionCliente",
                            "departamento",
                            "municipio",
                            "email",
                            "idDepartamentoReceptor",
                            "idMunicipioReceptor",
                            "codigoActividadEconomica",
                            "tipoCatContribuyente",
                            "giro",
                            "codicionPago",
                            "vtaACuentaDe",
                            "notaRemision",
                            "noFecha",
                            "descripcion",
                            "precioUnitario",
                            "ventasNoSujetas",
                            "ventasExentas",
                            "tipoItem",
                            "cantidad",
                            "ventasGravadas",
                            "montoLetras",
                            "sumas",
                            "subTotalVentasExentas",
                            "subTotalVentasNoSujetas",
                            "subTotalVentasGravadas",
                            "ventasGravadas",
                            "iva",
                            "ventasExentas",
                            "ventasNoSujetas",
                            "ventaTotal",
                            "ivaPercibido1",
                            "ivaRetenido1",
                            "totalAPagar",
                            "modeloFacturacion",
                            "tipoTransmisión"
                        });

                    #endregion Obligatorios FAC
                }
                else if (TipoDocEnvio == 6)
                {
                    DIC = "C";

                    #region Obligatorios CCF

                    CamposVerificar.AddRange(new List<string>
                    {
                            "nit",
                            "idDepartamentoEmisor",
                            "idMunicipioEmisor",
                            "direccionEmisor",
                            "fechaEnvio",
                            "fechaEmision",
                            "numFactura",
                            "tipoDocumento",
                            "nitCliente",
                            "nombreCliente",
                            "direccionCliente",
                            "departamento",
                            "municipio",
                            "email",
                            "idDepartamentoReceptor",
                            "idMunicipioReceptor",
                            "codigoActividadEconomica",
                            "tipoCatContribuyente",
                            "codicionPago",
                            "vtaACuentaDe",
                            "descripcion",
                            "precioUnitario",
                            "ventasNoSujetas",
                            "ventasExentas",
                            "tipoItem",
                            "cantidad",
                            "ventasGravadas",
                            "montoLetras",
                            "sumas",
                            "subTotalVentasExentas",
                            "subTotalVentasNoSujetas",
                            "subTotalVentasGravadas",
                            "ventasGravadas",
                            "ventasExentas",
                            "ventasNoSujetas",
                            "ventaTotal",
                            "ivaRetenido1",
                            "totalAPagar",
                            "modeloFacturacion",
                            "tipoTransmisión",
                            "tipoDocumentoReceptor"
                        });

                    #endregion Obligatorios CCF
                }
                else if (TipoDocEnvio == 2)
                {
                    string[] arrayNC = tokenX.Split('|');
                    tokenX = arrayNC[0];
                    NC_Only = arrayNC[1];
                    DIC = "NC";

                    #region Obligatorios NC

                    CamposVerificar.AddRange(new List<string>
                    {
                            "nit",
                            "nrc",
                            "idDepartamentoEmisor",
                            "idMunicipioEmisor",
                            "direccionEmisor",
                            "fechaEnvio",
                            "fechaEmision",
                            "numFactura",
                            "tipoDocumento",
                            "nitCliente",
                            "nrcCliente",
                            "nombreCliente",
                            "direccionCliente",
                            "departamento",
                            "municipio",
                            "email",
                            "idDepartamentoReceptor",
                            "idMunicipioReceptor",
                            "codigoActividadEconomica",
                            "tipoCatContribuyente",
                            "giro",
                            "codicionPago",
                            "CCFAnterior",
                            "vtaACuentaDe",
                            "descripcion",
                            "precioUnitario",
                            "ventasNoSujetas",
                            "ventasExentas",
                            "tipoItem",
                            "tipoDteRel",
                            "cantidad",
                            "ventasGravadas",
                            "montoLetras",
                            "sumas",
                            "subTotalVentasExentas",
                            "subTotalVentasNoSujetas",
                            "subTotalVentasGravadas",
                            "ventasGravadas",
                            "iva",
                            "ventasExentas",
                            "ventasNoSujetas",
                            "ventaTotal",
                            "ivaPercibido1",
                            "ivaRetenido1",
                            "docRelTipo",
                            "docRelNum",
                            "docRelFecha",
                            "modeloFacturacion",
                            "tipoTransmisión"
                        });

                    #endregion Obligatorios NC
                }
                else
                {
                    DIC = "NR";

                    #region Obligatorios NR

                    CamposVerificar.AddRange(new List<string>
                    {
                            "nit",
                            "nrc",
                            "idDepartamentoEmisor",
                            "idMunicipioEmisor",
                            "direccionEmisor",
                            "fechaEnvio",
                            "fechaEmision",
                            "numFactura",
                            "tipoDocumento",
                            "nitCliente",
                            "nrcCliente",
                            "nombreCliente",
                            "direccionCliente",
                            "departamento",
                            "municipio",
                            "email",
                            "idDepartamentoReceptor",
                            "idMunicipioReceptor",
                            "codigoActividadEconomica",
                            "tipoCatContribuyente",
                            "giro",
                            "codicionPago",
                            "CCFAnterior",
                            "vtaACuentaDe",
                            "noFecha",
                            "descripcion",
                            "precioUnitario",
                            "ventasNoSujetas",
                            "ventasExentas",
                            "tipoItem",
                            "cantidad",
                            "ventasGravadas",
                            "montoLetras",
                            "sumas",
                            "subTotalVentasExentas",
                            "subTotalVentasNoSujetas",
                            "subTotalVentasGravadas",
                            "iva",
                            "ventasExentas",
                            "ventasNoSujetas",
                            "ventaTotal",
                            "modeloFacturacion",
                            "tipoTransmisión",
                            "bienTitulo",
                            "tipoDocumentoReceptor"
                        });

                    #endregion Obligatorios NR
                }

                foreach (var dato in DatosRaw)
                {
                    foreach (var campo in CamposVerificar)
                    {
                        var valor = dato.GetType().GetProperty(campo)?.GetValue(dato);

                        if (valor == null || string.IsNullOrEmpty(valor.ToString()))
                        {
                            camposVacios.Add(campo);
                        }
                    }
                }

                string jsonString = JsonConvert.SerializeObject(DatosRaw);
                string jsonCompleto = @"""{""maindata"":" + jsonString + "}";
                string jsonFinal = jsonCompleto.Substring(1);
                RestClient cliente = new RestClient(UrlJson)
                {
                    //Authenticator = new HttpBasicAuthenticator(UsuarioHead, PasswordHead),
                    Timeout = 900000
                };
                RestRequest request = new RestRequest
                {
                    Method = Method.POST
                };

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.Parameters.Clear();
                //request.AddHeader("Content-Type", "application/json; charset=utf-8");
                request.AddHeader("Authorization", tokenX);
                request.AddParameter("application/json; charset=utf-8", jsonFinal, ParameterType.RequestBody);
                IRestResponse respond = cliente.Execute(request);
                string content = respond.Content;
                HttpStatusCode httpStatusCode = respond.StatusCode;
                int numericStatusCode = (int)httpStatusCode;

                if (numericStatusCode == 200) //REVISA SU CODIGO DE ESTADO, SI ES 200 NO HAY ERROR EN EL JSON
                {
                    if (content == "null") //SERVICIO CAIDO
                    {
                        respuestaOLS.mensajeCompleto = "Servicio de OLS caido";
                        respuestaOLS.respuestaOlShttp = null;
                        respuestaOLS.numeroDocumento = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                        respuestaOLS.ResultadoSatisfactorio = false;

                        return respuestaOLS;
                    }

                    dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                    string docs = jsonRespuesta.ToString();
                    string jsonTotal = @"[" + docs + "]";
                    List<MapaResponse> jsonDocs = JsonConvert.DeserializeObject<List<MapaResponse>>(jsonTotal);
                    jsonDocs[0].JSONResultante = jsonFinal;

                    if (jsonDocs[0].message == "OK") //SI EL MENSAJE ES OK, EL JSON LLEGO A OLS
                    {
                        if (TipoDocEnvio == 1 || TipoDocEnvio == 6)
                        {
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());

                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();

                            controlOLS.CambiaEstadoFCCCF(
                                                rutaTemp,
                                                DIC,
                                                fecha,
                                                facturaTemp,
                                                jsonDocs[0].selloRecibido,
                                                jsonDocs[0].numControl,
                                                jsonDocs[0].codigoGeneracion
                                              ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO

                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]) , facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-OK", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);
                        }
                        else if (TipoDocEnvio == 2)
                        {
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            //serieTemp2 = Convert.ToInt32(DatosRaw.Select(x => x.serie).FirstOrDefault());
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            controlOLS.ActualizaEstadoNotaCredito(jsonDocs[0].numControl, jsonDocs[0].codigoGeneracion, jsonDocs[0].selloRecibido, NC_Only);
                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-OK", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);
                            //controlOLS.CambiaEstadoNC(rutaTemp, fecha, facturaTemp, jsonDocs[0].selloRecibido, jsonDocs[0].selloRecibido, NC_Only); //SE CAMBIA EL ESTADO PARA NO CONTABILIZARLA NUEVAMENTE
                        }
                        else
                        {
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            controlOLS.CambiaEstadoNR(rutaTemp, fecha, facturaTemp, jsonDocs[0].selloRecibido); //SE CAMBIA EL ESTADO PARA NO CONTABILIZARLA NUEVAMENTE
                        }

                        facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                        //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                        rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                        resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                        string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                        //controlOLS.RecLogBitacora(
                        //						1,
                        //						DIC,
                        //						Convert.ToInt32(facturaTemp),
                        //						resolucionTemp,
                        //						serieTemp,
                        //						"Documento enviado para la ruta " + rutaTemp,
                        //						numericStatusCode
                        //					  ); //SE REGISTRA EN LA BITACORA

                        respuestaMetodo = @"Documento #" + facturaTemp + "enviado!!!\n" +
                                        "Tipo documento: " + DIC + "\n" +
                                        "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                        respuestaOLS.mensajeCompleto = respuestaMetodo;
                        respuestaOLS.numeroDocumento = facturaTemp;
                        respuestaOLS.respuestaOlShttp = jsonDocs[0];
                        respuestaOLS.ResultadoSatisfactorio = true;
                        respuestaOLS.esContigencia = false;

                        if (String.IsNullOrWhiteSpace(respuestaOLS.respuestaOlShttp.selloRecibido) || respuestaOLS.respuestaOlShttp.selloRecibido == "0")
                        {
                            respuestaOLS.ResultadoSatisfactorio = false;
                            //respuestaOLS.esContigencia = true;
                        }

                        return respuestaOLS;
                    }
                    else
                    {
                        if (content == null) //SERVICIO CAIDO
                        {
                            respuestaOLS.mensajeCompleto = "Servicio caido";
                            respuestaOLS.respuestaOlShttp = null;
                            respuestaOLS.numeroDocumento = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            respuestaOLS.esContigencia = true;
                            respuestaOLS.ResultadoSatisfactorio = false;

                            return respuestaOLS;
                        }

                        if (jsonDocs[0].message.Contains("Registro existente")) //SI YA ESTA REPETIDO LE CAMBIA EL ESTADO
                        {
                            if (TipoDocEnvio == 1 || TipoDocEnvio == 6)
                            {
                                //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                                rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                                //serieTemp2 = Convert.ToInt32(DatosRaw.Select(x => x.serie).FirstOrDefault());
                                resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                                facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();

                                controlOLS.CambiaEstadoFCCCF(
                                                    rutaTemp,
                                                    DIC,
                                                    fecha,
                                                    facturaTemp,
                                                     jsonDocs[0].selloRecibido,
                                                    jsonDocs[0].numControl,
                                                    jsonDocs[0].codigoGeneracion
                                                  ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO

                                respuestaOLS.ResultadoSatisfactorio = true;

                                serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                                controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Repetido", jsonDocs[0].urlMail);
                                //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);

                                respuestaMetodo = @"Documento ya existente,se actualizan datos!!!\n";

                                respuestaOLS.mensajeCompleto = respuestaMetodo;
                                respuestaOLS.respuestaOlShttp = jsonDocs[0];
                                respuestaOLS.numeroDocumento = facturaTemp;
                                respuestaOLS.esContigencia = false;
                                respuestaOLS.ResultadoSatisfactorio = true;
                            }
                            else if (TipoDocEnvio == 2)
                            {
                                //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                                rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                                //serieTemp2 = Convert.ToInt32(DatosRaw.Select(x => x.serie).FirstOrDefault());
                                resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                                facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                                respuestaOLS.ResultadoSatisfactorio = true;
                                controlOLS.CambiaEstadoNC(rutaTemp, fecha, facturaTemp, jsonDocs[0].selloRecibido); //SE CAMBIA EL ESTADO PARA NO CONTABILIZARLA NUEVAMENTE
                                serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                                controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Repetido", jsonDocs[0].urlMail);
                                //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);
                            }
                            else
                            {
                                //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                                rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());

                                resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                                facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                                respuestaOLS.ResultadoSatisfactorio = true;
                                controlOLS.CambiaEstadoNR(rutaTemp, fecha, facturaTemp, jsonDocs[0].selloRecibido); //SE CAMBIA EL ESTADO PARA NO CONTABILIZARLA NUEVAMENTE
                            }
                        }

                        if (jsonDocs[0].message.Contains("CONTINGENCIA") || jsonDocs[0].message.Contains("NO SE HA PROPORCIONADO DATOS PARA VALIDAR")) //EL DOCUMENTO ES CONTIGENCIA
                        {
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                            respuestaMetodo = @"Documento generado como contigencia, por favor volver a enviarla en 3 minutos!!!\n";

                            if (jsonDocs[0].message.Contains("CONTINGENCIA"))
                            {
                                controlOLS.CambiaEstadoFCCCF(
                                    rutaTemp,
                                    DIC,
                                    fecha,
                                    facturaTemp,
                                    jsonDocs[0].selloRecibido,
                                    jsonDocs[0].numControl,
                                    jsonDocs[0].codigoGeneracion
                                ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO
                            }

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Contigencia", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);

                            respuestaOLS.mensajeCompleto = respuestaMetodo;
                            respuestaOLS.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS.numeroDocumento = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            respuestaOLS.esContigencia = true;
                            respuestaOLS.ResultadoSatisfactorio = false;
                        }
                        else if (!jsonDocs[0].message.Contains("CONTINGENCIA") && !jsonDocs[0].message.Contains("Registro existente"))
                        {
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            //serieTemp2 = Convert.ToInt32(DatosRaw.Select(x => x.serie).FirstOrDefault());
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                            if (camposVacios.Count > 0 && jsonDocs[0].message.Contains("FALTANTE"))
                            {
                                string camposConcatenados = string.Empty;

                                foreach (string campo in camposVacios)
                                {
                                    camposConcatenados += campo + ", ";
                                }

                                // Eliminar la última coma y espacio
                                if (!string.IsNullOrEmpty(camposConcatenados))
                                {
                                    camposConcatenados = camposConcatenados.Substring(0, camposConcatenados.Length - 2);
                                }

                                respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                                           "Tipo documento: " + DIC + "\n" +
                                           "Error:" + jsonDocs[0].message + "\n" +
                                           "Campos Obligatorios vacios:" + camposConcatenados + "\n" +
                                           "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                            }
                            else
                            {
                                respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                                            "Tipo documento: " + DIC + "\n" +
                                            "Error:" + jsonDocs[0].message + "\n" +
                                            "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                            }

                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Error", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);

                            respuestaOLS.mensajeCompleto = respuestaMetodo;
                            respuestaOLS.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS.numeroDocumento = facturaTemp;
                            respuestaOLS.esContigencia = false;
                            respuestaOLS.ResultadoSatisfactorio = false;
                        }

                        return respuestaOLS;
                    }
                }
                else
                {
                    //jsonDocs[0].JSONResultante = jsonFinal;
                    facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                    //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                    rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                    resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                    string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                    anulacion = 0;
                    //controlOLS.RecLogBitacora(
                    //							0,
                    //							DIC,
                    //							Convert.ToInt32(facturaTemp),
                    //							resolucionTemp,
                    //							serieTemp,
                    //							"BAD REQUEST: Debido a Gateway time o Error de Sintaxis en la ruta: " + DatosRaw.Select(x => x.cajaSuc).ToString(),
                    //							numericStatusCode
                    //						  ); //SE REGISTRA ERROR EN LA BITACORA
                    respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                                        "Tipo documento: " + DIC + "\n" +
                                        "Error:BAD REQUEST: Debido a Gateway time o Error de Sintaxis\n" +
                                        "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaOLS.mensajeCompleto = respuestaMetodo;
                    respuestaOLS.numeroDocumento = facturaTemp;
                    respuestaOLS.ResultadoSatisfactorio = true;

                    return respuestaOLS;
                }
            }
            catch (Exception ex)
            {
                StackTrace s = new StackTrace(ex);
                Assembly thisasm = Assembly.GetExecutingAssembly();
                string methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                respuestaOLS.mensajeCompleto = @"Error interno:" + ex.Message.ToString() + "\n" +
                         "Metodo:" + s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;

                respuestaOLS.numeroDocumento = "0";
                respuestaOLS.ResultadoSatisfactorio = false;

                return respuestaOLS;
            }
        }

        /// <summary>
        /// SE ENVIAN DATOS HACIA OLS
        /// </summary>
        /// <param name="DatosRaw"></param>
        /// <param name="TipoDocEnvio"></param>
        /// <returns></returns>
        public RespuestaOLS EnvioDataOLSSalaVentas(List<Maindata> DatosRaw, int TipoDocEnvio, string fecha, string tokenX)
        {
            List<string> CamposVerificar = new List<string>();
            List<string> camposVacios = new List<string>();
            RespuestaOLS respuestaOLS = new RespuestaOLS();

            string NC_Only = "";

            try
            {
                int rutaTemp;
                int serieTemp2;
                string[] serieTemp3;
                string resolucionTemp;
                string facturaTemp;
                string DIC;
                string respuestaMetodo = "";
                if (TipoDocEnvio == 1)
                {
                    DIC = "F";

                    #region Obligatorios FAC

                    CamposVerificar.AddRange(new List<string>
                    {
                            "nit",
                            "nrc",
                            "idDepartamentoEmisor",
                            "idMunicipioEmisor",
                            "direccionEmisor",
                            "fechaEnvio",
                            "fechaEmision",
                            "numFactura",
                            "tipoDocumento",
                            "nitCliente",
                            "nrcCliente",
                            "nombreCliente",
                            "direccionCliente",
                            "departamento",
                            "municipio",
                            "email",
                            "idDepartamentoReceptor",
                            "idMunicipioReceptor",
                            "codigoActividadEconomica",
                            "tipoCatContribuyente",
                            "giro",
                            "codicionPago",
                            "vtaACuentaDe",
                            "notaRemision",
                            "noFecha",
                            "descripcion",
                            "precioUnitario",
                            "ventasNoSujetas",
                            "ventasExentas",
                            "tipoItem",
                            "cantidad",
                            "ventasGravadas",
                            "montoLetras",
                            "sumas",
                            "subTotalVentasExentas",
                            "subTotalVentasNoSujetas",
                            "subTotalVentasGravadas",
                            "ventasGravadas",
                            "iva",
                            "ventasExentas",
                            "ventasNoSujetas",
                            "ventaTotal",
                            "ivaPercibido1",
                            "ivaRetenido1",
                            "totalAPagar",
                            "modeloFacturacion",
                            "tipoTransmisión"
                        });

                    #endregion Obligatorios FAC
                }
                else if (TipoDocEnvio == 6)
                {
                    DIC = "C";

                    #region Obligatorios CCF

                    CamposVerificar.AddRange(new List<string>
                    {
                            "nit",
                            "idDepartamentoEmisor",
                            "idMunicipioEmisor",
                            "direccionEmisor",
                            "fechaEnvio",
                            "fechaEmision",
                            "numFactura",
                            "tipoDocumento",
                            "nitCliente",
                            "nombreCliente",
                            "direccionCliente",
                            "departamento",
                            "municipio",
                            "email",
                            "idDepartamentoReceptor",
                            "idMunicipioReceptor",
                            "codigoActividadEconomica",
                            "tipoCatContribuyente",
                            "codicionPago",
                            "vtaACuentaDe",
                            "descripcion",
                            "precioUnitario",
                            "ventasNoSujetas",
                            "ventasExentas",
                            "tipoItem",
                            "cantidad",
                            "ventasGravadas",
                            "montoLetras",
                            "sumas",
                            "subTotalVentasExentas",
                            "subTotalVentasNoSujetas",
                            "subTotalVentasGravadas",
                            "ventasGravadas",
                            "ventasExentas",
                            "ventasNoSujetas",
                            "ventaTotal",
                            "ivaRetenido1",
                            "totalAPagar",
                            "modeloFacturacion",
                            "tipoTransmisión",
                            "tipoDocumentoReceptor"
                        });

                    #endregion Obligatorios CCF
                }
                else if (TipoDocEnvio == 2)
                {
                    string[] arrayNC = tokenX.Split('|');
                    tokenX = arrayNC[0];
                    NC_Only = arrayNC[1];
                    DIC = "NC";

                    #region Obligatorios NC

                    CamposVerificar.AddRange(new List<string>
                    {
                            "nit",
                            "nrc",
                            "idDepartamentoEmisor",
                            "idMunicipioEmisor",
                            "direccionEmisor",
                            "fechaEnvio",
                            "fechaEmision",
                            "numFactura",
                            "tipoDocumento",
                            "nitCliente",
                            "nrcCliente",
                            "nombreCliente",
                            "direccionCliente",
                            "departamento",
                            "municipio",
                            "email",
                            "idDepartamentoReceptor",
                            "idMunicipioReceptor",
                            "codigoActividadEconomica",
                            "tipoCatContribuyente",
                            "giro",
                            "codicionPago",
                            "CCFAnterior",
                            "vtaACuentaDe",
                            "descripcion",
                            "precioUnitario",
                            "ventasNoSujetas",
                            "ventasExentas",
                            "tipoItem",
                            "tipoDteRel",
                            "cantidad",
                            "ventasGravadas",
                            "montoLetras",
                            "sumas",
                            "subTotalVentasExentas",
                            "subTotalVentasNoSujetas",
                            "subTotalVentasGravadas",
                            "ventasGravadas",
                            "iva",
                            "ventasExentas",
                            "ventasNoSujetas",
                            "ventaTotal",
                            "ivaPercibido1",
                            "ivaRetenido1",
                            "docRelTipo",
                            "docRelNum",
                            "docRelFecha",
                            "modeloFacturacion",
                            "tipoTransmisión"
                        });

                    #endregion Obligatorios NC
                }
                else
                {
                    DIC = "NR";

                    #region Obligatorios NR

                    CamposVerificar.AddRange(new List<string>
                    {
                            "nit",
                            "nrc",
                            "idDepartamentoEmisor",
                            "idMunicipioEmisor",
                            "direccionEmisor",
                            "fechaEnvio",
                            "fechaEmision",
                            "numFactura",
                            "tipoDocumento",
                            "nitCliente",
                            "nrcCliente",
                            "nombreCliente",
                            "direccionCliente",
                            "departamento",
                            "municipio",
                            "email",
                            "idDepartamentoReceptor",
                            "idMunicipioReceptor",
                            "codigoActividadEconomica",
                            "tipoCatContribuyente",
                            "giro",
                            "codicionPago",
                            "CCFAnterior",
                            "vtaACuentaDe",
                            "noFecha",
                            "descripcion",
                            "precioUnitario",
                            "ventasNoSujetas",
                            "ventasExentas",
                            "tipoItem",
                            "cantidad",
                            "ventasGravadas",
                            "montoLetras",
                            "sumas",
                            "subTotalVentasExentas",
                            "subTotalVentasNoSujetas",
                            "subTotalVentasGravadas",
                            "iva",
                            "ventasExentas",
                            "ventasNoSujetas",
                            "ventaTotal",
                            "modeloFacturacion",
                            "tipoTransmisión",
                            "bienTitulo",
                            "tipoDocumentoReceptor"
                        });

                    #endregion Obligatorios NR
                }

                foreach (var dato in DatosRaw)
                {
                    foreach (var campo in CamposVerificar)
                    {
                        var valor = dato.GetType().GetProperty(campo)?.GetValue(dato);

                        if (valor == null || string.IsNullOrEmpty(valor.ToString()))
                        {
                            camposVacios.Add(campo);
                        }
                    }
                }

                string jsonString = JsonConvert.SerializeObject(DatosRaw);
                string jsonCompleto = @"""{""maindata"":" + jsonString + "}";
                string jsonFinal = jsonCompleto.Substring(1);
                RestClient cliente = new RestClient(UrlJson)
                {
                    //Authenticator = new HttpBasicAuthenticator(UsuarioHead, PasswordHead),
                    Timeout = 900000
                };
                RestRequest request = new RestRequest
                {
                    Method = Method.POST
                };

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.Parameters.Clear();
                //request.AddHeader("Content-Type", "application/json; charset=utf-8");
                request.AddHeader("Authorization", tokenX);
                request.AddParameter("application/json; charset=utf-8", jsonFinal, ParameterType.RequestBody);
                IRestResponse respond = cliente.Execute(request);
                string content = respond.Content;
                HttpStatusCode httpStatusCode = respond.StatusCode;
                int numericStatusCode = (int)httpStatusCode;

                if (numericStatusCode == 200) //REVISA SU CODIGO DE ESTADO, SI ES 200 NO HAY ERROR EN EL JSON
                {
                    if (content == "null") //SERVICIO CAIDO
                    {
                        respuestaOLS.mensajeCompleto = "Servicio caido";
                        respuestaOLS.respuestaOlShttp = null;
                        respuestaOLS.numeroDocumento = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                        respuestaOLS.ResultadoSatisfactorio = false;

                        return respuestaOLS;
                    }

                    dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                    string docs = jsonRespuesta.ToString();
                    string jsonTotal = @"[" + docs + "]";
                    List<MapaResponse> jsonDocs = JsonConvert.DeserializeObject<List<MapaResponse>>(jsonTotal);
                    jsonDocs[0].JSONResultante = jsonFinal;

                    if (jsonDocs[0].message == "OK") //SI EL MENSAJE ES OK, EL JSON LLEGO A OLS
                    {
                        if (TipoDocEnvio == 1 || TipoDocEnvio == 6)
                        {
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());

                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();

                            controlOLS.CambiaEstadoFCCCF_SalaVenta(
                                                rutaTemp,
                                                DIC,
                                                fecha,
                                                facturaTemp,
                                                jsonDocs[0].selloRecibido,
                                                jsonDocs[0].numControl,
                                                jsonDocs[0].codigoGeneracion
                                              ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO

                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-OK", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);
                        }
                        else if (TipoDocEnvio == 2)
                        {
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            //serieTemp2 = Convert.ToInt32(DatosRaw.Select(x => x.serie).FirstOrDefault());
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            controlOLS.ActualizaEstadoNotaCredito(jsonDocs[0].numControl, jsonDocs[0].codigoGeneracion, jsonDocs[0].selloRecibido, NC_Only);
                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-OK", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);
                            //controlOLS.CambiaEstadoNC(rutaTemp, fecha, facturaTemp, jsonDocs[0].selloRecibido, jsonDocs[0].selloRecibido, NC_Only); //SE CAMBIA EL ESTADO PARA NO CONTABILIZARLA NUEVAMENTE
                        }
                        else
                        {
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            controlOLS.CambiaEstadoNR(rutaTemp, fecha, facturaTemp, jsonDocs[0].selloRecibido); //SE CAMBIA EL ESTADO PARA NO CONTABILIZARLA NUEVAMENTE
                        }

                        facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                        //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                        rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                        resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                        string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                        //controlOLS.RecLogBitacora(
                        //						1,
                        //						DIC,
                        //						Convert.ToInt32(facturaTemp),
                        //						resolucionTemp,
                        //						serieTemp,
                        //						"Documento enviado para la ruta " + rutaTemp,
                        //						numericStatusCode
                        //					  ); //SE REGISTRA EN LA BITACORA

                        respuestaMetodo = @"Documento #" + facturaTemp + "enviado!!!\n" +
                                        "Tipo documento: " + DIC + "\n" +
                                        "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                        respuestaOLS.mensajeCompleto = respuestaMetodo;
                        respuestaOLS.numeroDocumento = facturaTemp;
                        respuestaOLS.respuestaOlShttp = jsonDocs[0];
                        respuestaOLS.ResultadoSatisfactorio = true;
                        respuestaOLS.esContigencia = false;

                        if (String.IsNullOrWhiteSpace(respuestaOLS.respuestaOlShttp.selloRecibido) || respuestaOLS.respuestaOlShttp.selloRecibido == "0")
                        {
                            respuestaOLS.ResultadoSatisfactorio = false;
                            //respuestaOLS.esContigencia = true;
                        }

                        return respuestaOLS;
                    }
                    else
                    {
                        if (content == null) //SERVICIO CAIDO
                        {
                            respuestaOLS.mensajeCompleto = "Servicio de OLS Caido!!";
                            respuestaOLS.respuestaOlShttp = null;
                            respuestaOLS.numeroDocumento = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            respuestaOLS.esContigencia = true;
                            respuestaOLS.ResultadoSatisfactorio = false;

                            return respuestaOLS;
                        }

                        if (jsonDocs[0].message.Contains("Registro existente")) //SI YA ESTA REPETIDO LE CAMBIA EL ESTADO
                        {
                            if (TipoDocEnvio == 1 || TipoDocEnvio == 6)
                            {
                                //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                                rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                                //serieTemp2 = Convert.ToInt32(DatosRaw.Select(x => x.serie).FirstOrDefault());
                                resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                                facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();

                                controlOLS.CambiaEstadoFCCCF_SalaVenta(
                                                    rutaTemp,
                                                    DIC,
                                                    fecha,
                                                    facturaTemp,
                                                     jsonDocs[0].selloRecibido,
                                                    jsonDocs[0].numControl,
                                                    jsonDocs[0].codigoGeneracion
                                                  ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO

                                respuestaOLS.ResultadoSatisfactorio = true;

                                serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                                controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Repetido", jsonDocs[0].urlMail);
                                //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);

                                respuestaMetodo = @"Documento ya existente,se actualizan datos!!!\n";

                                respuestaOLS.mensajeCompleto = respuestaMetodo;
                                respuestaOLS.respuestaOlShttp = jsonDocs[0];
                                respuestaOLS.numeroDocumento = facturaTemp;
                                respuestaOLS.esContigencia = false;
                                respuestaOLS.ResultadoSatisfactorio = true;
                            }
                            else if (TipoDocEnvio == 2)
                            {
                                //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                                rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                                //serieTemp2 = Convert.ToInt32(DatosRaw.Select(x => x.serie).FirstOrDefault());
                                resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                                facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                                respuestaOLS.ResultadoSatisfactorio = true;
                                controlOLS.CambiaEstadoNC(rutaTemp, fecha, facturaTemp, jsonDocs[0].selloRecibido); //SE CAMBIA EL ESTADO PARA NO CONTABILIZARLA NUEVAMENTE
                                serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                                controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Repetido", jsonDocs[0].urlMail);
                                //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);
                            }
                            else
                            {
                                //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                                rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());

                                resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                                facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                                respuestaOLS.ResultadoSatisfactorio = true;
                                controlOLS.CambiaEstadoNR(rutaTemp, fecha, facturaTemp, jsonDocs[0].selloRecibido); //SE CAMBIA EL ESTADO PARA NO CONTABILIZARLA NUEVAMENTE
                            }
                        }

                        if (jsonDocs[0].message.Contains("CONTINGENCIA") || jsonDocs[0].message.Contains("NO SE HA PROPORCIONADO DATOS PARA VALIDAR")) //EL DOCUMENTO ES CONTIGENCIA
                        {
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                            respuestaMetodo = @"Documento generado como contigencia, por favor volver a enviarla en 3 minutos!!!\n";

                            if (jsonDocs[0].message.Contains("CONTINGENCIA"))
                            {
                                controlOLS.CambiaEstadoFCCCF_SalaVenta(
                                    rutaTemp,
                                    DIC,
                                    fecha,
                                    facturaTemp,
                                    jsonDocs[0].selloRecibido,
                                    jsonDocs[0].numControl,
                                    jsonDocs[0].codigoGeneracion
                                ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO
                            }

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Contigencia", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);

                            respuestaOLS.mensajeCompleto = respuestaMetodo;
                            respuestaOLS.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS.numeroDocumento = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            respuestaOLS.esContigencia = true;
                            respuestaOLS.ResultadoSatisfactorio = false;
                        }
                        else if (!jsonDocs[0].message.Contains("CONTINGENCIA") && !jsonDocs[0].message.Contains("Registro existente"))
                        {
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            //serieTemp2 = Convert.ToInt32(DatosRaw.Select(x => x.serie).FirstOrDefault());
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                            if (camposVacios.Count > 0 && jsonDocs[0].message.Contains("FALTANTE"))
                            {
                                string camposConcatenados = string.Empty;

                                foreach (string campo in camposVacios)
                                {
                                    camposConcatenados += campo + ", ";
                                }

                                // Eliminar la última coma y espacio
                                if (!string.IsNullOrEmpty(camposConcatenados))
                                {
                                    camposConcatenados = camposConcatenados.Substring(0, camposConcatenados.Length - 2);
                                }

                                respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                                           "Tipo documento: " + DIC + "\n" +
                                           "Error:" + jsonDocs[0].message + "\n" +
                                           "Campos Obligatorios vacios:" + camposConcatenados + "\n" +
                                           "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                            }
                            else
                            {
                                respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                                            "Tipo documento: " + DIC + "\n" +
                                            "Error:" + jsonDocs[0].message + "\n" +
                                            "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                            }

                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');
                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Error", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);

                            respuestaOLS.mensajeCompleto = respuestaMetodo;
                            respuestaOLS.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS.numeroDocumento = facturaTemp;
                            respuestaOLS.esContigencia = false;
                            respuestaOLS.ResultadoSatisfactorio = false;
                        }

                        return respuestaOLS;
                    }
                }
                else
                {
                    //jsonDocs[0].JSONResultante = jsonFinal;
                    facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                    //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                    rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                    resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                    string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                    anulacion = 0;
                    //controlOLS.RecLogBitacora(
                    //							0,
                    //							DIC,
                    //							Convert.ToInt32(facturaTemp),
                    //							resolucionTemp,
                    //							serieTemp,
                    //							"BAD REQUEST: Debido a Gateway time o Error de Sintaxis en la ruta: " + DatosRaw.Select(x => x.cajaSuc).ToString(),
                    //							numericStatusCode
                    //						  ); //SE REGISTRA ERROR EN LA BITACORA
                    respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                                        "Tipo documento: " + DIC + "\n" +
                                        "Error:BAD REQUEST: Debido a Gateway time o Error de Sintaxis\n" +
                                        "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaOLS.mensajeCompleto = respuestaMetodo;
                    respuestaOLS.numeroDocumento = facturaTemp;
                    respuestaOLS.ResultadoSatisfactorio = true;

                    return respuestaOLS;
                }
            }
            catch (Exception ex)
            {
                StackTrace s = new StackTrace(ex);
                Assembly thisasm = Assembly.GetExecutingAssembly();
                string methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                respuestaOLS.mensajeCompleto = @"Error interno:" + ex.Message.ToString() + "\n" +
                         "Metodo:" + s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;

                respuestaOLS.numeroDocumento = "0";
                respuestaOLS.ResultadoSatisfactorio = false;

                return respuestaOLS;
            }
        }

        /// <summary>
        /// ENVIA DATOS PARA ANULAR
        /// </summary>
        /// <param name="mapaAnulacion"></param>
        /// <returns></returns>
        //[WebMethod(Description ="Envia Anulacion de Documento")]
        //public RespuestaOLS AnulaDocumentos(MapaAnulacion mapaAnulacion)
        //{
        //    string fechaAnu = mapaAnulacion.fechaDoc;
        //    string[] fac_ruta = mapaAnulacion.correlativointerno.Split('|');
        //}

        /// <summary>
        /// SE ENVIAN DATOS HACIA OLS
        /// </summary>
        /// <param name="DatosRaw"></param>
        /// <param name="TipoDocEnvio"></param>
        /// <returns></returns>
        public RespuestaOLS EnvioDataOLSPreImpresa(List<Maindata> DatosRaw, int TipoDocEnvio, string fecha, string tokenX)
        {
            RespuestaOLS respuestaOLS = new RespuestaOLS();
            try
            {
                int rutaTemp;
                string resolucionTemp;
                string facturaTemp;
                string DIC = "";
                string respuestaMetodo = "";
                string[] serieTemp3;
                if (TipoDocEnvio == 11)
                {
                    DIC = "F";
                }
                else if (TipoDocEnvio == 66)
                {
                    DIC = "C";
                }

                string jsonString = JsonConvert.SerializeObject(DatosRaw);
                string jsonCompleto = @"""{""maindata"":" + jsonString + "}";
                string jsonFinal = jsonCompleto.Substring(1);
                RestClient cliente = new RestClient(UrlJson)
                {
                    //Authenticator = new HttpBasicAuthenticator(UsuarioHead, PasswordHead),
                    Timeout = 900000
                };
                RestRequest request = new RestRequest
                {
                    Method = Method.POST
                };

                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                request.Parameters.Clear();
                //request.AddHeader("Content-Type", "application/json; charset=utf-8");
                request.AddHeader("Authorization", tokenX);
                request.AddParameter("application/json; charset=utf-8", jsonFinal, ParameterType.RequestBody);
                IRestResponse respond = cliente.Execute(request);
                string content = respond.Content;
                HttpStatusCode httpStatusCode = respond.StatusCode;
                int numericStatusCode = (int)httpStatusCode;

                if (numericStatusCode == 200) //REVISA SU CODIGO DE ESTADO, SI ES 200 NO HAY ERROR EN EL JSON
                {
                    dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                    string docs = jsonRespuesta.ToString();
                    string jsonTotal = @"[" + docs + "]";
                    List<MapaResponse> jsonDocs = JsonConvert.DeserializeObject<List<MapaResponse>>(jsonTotal);
                    jsonDocs[0].JSONResultante = jsonFinal;

                    if (jsonDocs[0].message == "OK") //SI EL MENSAJE ES OK, EL JSON LLEGO A OLS
                    {
                        if (TipoDocEnvio == 11 || TipoDocEnvio == 66)
                        {
                            //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                            rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                            resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                            facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();

                            controlOLS.CambiaEstadoFCCCFPreImpresa(
                                                rutaTemp,
                                                fecha,
                                                facturaTemp,
                                                jsonDocs[0].selloRecibido,
                                                jsonDocs[0].numControl,
                                                jsonDocs[0].codigoGeneracion
                                              ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO

                            serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-OK", jsonDocs[0].urlMail);
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);
                        }

                        facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                        //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                        rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                        resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                        string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                        //controlOLS.RecLogBitacora(
                        //						1,
                        //						DIC,
                        //						Convert.ToInt32(facturaTemp),
                        //						resolucionTemp,
                        //						serieTemp,
                        //						"Documento enviado para la ruta " + rutaTemp,
                        //						numericStatusCode
                        //					  ); //SE REGISTRA EN LA BITACORA

                        respuestaMetodo = @"Documento #" + facturaTemp + " enviado!!!\n" +
                                        "Tipo documento: " + DIC + "\n" +
                                        "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";
                        respuestaOLS.mensajeCompleto = respuestaMetodo;
                        respuestaOLS.numeroDocumento = facturaTemp;
                        respuestaOLS.respuestaOlShttp = jsonDocs[0];
                        respuestaOLS.ResultadoSatisfactorio = true;

                        return respuestaOLS;
                    }
                    else
                    {
                        if (jsonDocs[0].message.Contains("Registro existente")) //SI YA ESTA REPETIDO LE CAMBIA EL ESTADO
                        {
                            if (TipoDocEnvio == 11 || TipoDocEnvio == 66)
                            {
                                //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                                rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                                resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                                facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();

                                controlOLS.CambiaEstadoFCCCFPreImpresa(
                                                rutaTemp,
                                                fecha,
                                                facturaTemp,
                                                jsonDocs[0].selloRecibido,
                                                jsonDocs[0].numControl,
                                                jsonDocs[0].codigoGeneracion
                                              ); //SE CAMBIA EL ESTADO DE LA FACTURA SI EL ENVIO ES EXITOSO

                                serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');

                                controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Repetido", jsonDocs[0].urlMail);
                                //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);
                            }
                        }

                        facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                        //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                        rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                        resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                        string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();
                        serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');

                        anulacion = 0;
                        //controlOLS.RecLogBitacora(
                        //						0,
                        //						DIC,
                        //						Convert.ToInt32(facturaTemp),
                        //						resolucionTemp,
                        //						serieTemp,
                        //						jsonDocs[0].message + " en la ruta: " + rutaTemp,
                        //						numericStatusCode
                        //					  ); //SE REGISTRA ERROR EN LA BITACORA
                        respuestaMetodo = @"Documento #" + facturaTemp + " no fue enviado!!!\n" +
                                        "Tipo documento: " + DIC + "\n" +
                                        "Error:" + jsonDocs[0].message + "\n" +
                                        "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                        controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Error", "NO URL");
                        //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);

                        respuestaOLS.mensajeCompleto = respuestaMetodo;
                        respuestaOLS.respuestaOlShttp = jsonDocs[0];
                        respuestaOLS.numeroDocumento = facturaTemp;
                        respuestaOLS.ResultadoSatisfactorio = true;

                        return respuestaOLS;
                    }
                }
                else
                {
                    dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                    string docs = jsonRespuesta.ToString();
                    string jsonTotal = @"[" + docs + "]";
                    List<MapaResponse> jsonDocs = JsonConvert.DeserializeObject<List<MapaResponse>>(jsonTotal);
                    jsonDocs[0].JSONResultante = jsonFinal;

                    //jsonDocs[0].JSONResultante = jsonFinal;
                    facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                    //rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                    rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.terminal).FirstOrDefault());
                    resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                    string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                    anulacion = 0;
                    //controlOLS.RecLogBitacora(
                    //							0,
                    //							DIC,
                    //							Convert.ToInt32(facturaTemp),
                    //							resolucionTemp,
                    //							serieTemp,
                    //							"BAD REQUEST: Debido a Gateway time o Error de Sintaxis en la ruta: " + DatosRaw.Select(x => x.cajaSuc).ToString(),
                    //							numericStatusCode
                    //						  ); //SE REGISTRA ERROR EN LA BITACORA
                    respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                                        "Tipo documento: " + DIC + "\n" +
                                        "Error:BAD REQUEST: Debido a Gateway time o Error de Sintaxis\n" +
                                        "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaOLS.mensajeCompleto = respuestaMetodo;
                    respuestaOLS.numeroDocumento = facturaTemp;
                    respuestaOLS.ResultadoSatisfactorio = true;

                    serieTemp3 = (DatosRaw.Select(x => x.correlativoInterno).FirstOrDefault()).Split('_');

                    controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA, "Envio-Error", jsonDocs[0].urlMail);
                    //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(serieTemp3[1]), facturaTemp, jsonFinal, jsonTotal, jsonDocs[0].msgHDA);

                    return respuestaOLS;
                }
            }
            catch (Exception ex)
            {
                StackTrace s = new StackTrace(ex);
                Assembly thisasm = Assembly.GetExecutingAssembly();
                string methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                respuestaOLS.mensajeCompleto = @"Error interno:" + ex.Message.ToString() + "\n" +
                         "Metodo:" + s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;

                respuestaOLS.numeroDocumento = "0";
                respuestaOLS.ResultadoSatisfactorio = false;

                return respuestaOLS;
            }
        }

        /// <summary>
        /// ENVIA ANULACION HACIA OLS
        /// </summary>
        /// <param name="DatosRawAnulacion"></param>
        /// <param name="DatosRaw"></param>
        /// <returns></returns>
        /// [WebMethod(Description ="Envia Anulacion de Documento")]
        ///
        [WebMethod(Description = "Envia Anulacion de Documento")]
        public RespuestaOLSAnulacion EnviaDataAnulacion(string fechaDoc, string correlativoInterno, int idTipo)
        {
            Facturas _facturas = new Facturas();
            string Token = "";
            string revisaT = RevisarToken();
            int numericStatusCode = 0;
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            //RespuestaAnulacion respuestaOLS = new RespuestaAnulacion();
            RespuestaOLSAnulacion respuestaOLS1 = new RespuestaOLSAnulacion();
            MapaAnulacion DatosRawAnulacion = new MapaAnulacion();
            try
            {
                string[] fac_ruta;
                string fechaAnu = "";
                string revisarFEL = "";
                string content = "";

                int idSerieT = 0;

                if (idTipo == 1)
                {
                    fechaAnu = fechaDoc;
                    fac_ruta = correlativoInterno.Split('|');
                    idSerieT = Convert.ToInt32(fac_ruta[3]);
                    string idMotivo = fac_ruta[2]; //empleado
                    string motivoDescripcion = _facturas.ObtieneMotivoAnulacion(Convert.ToInt32(idMotivo));
                    //MapaAnulacion DatosRawAnulacion=new MapaAnulacion();
                    DatosRawAnulacion.tipoDoc = _facturas.GetTipoDoc(Convert.ToInt32(fac_ruta[1]), fechaDoc, fac_ruta[0]) == "C" ? "CCF" : "FAC";

                    //if (DatosRawAnulacion.tipoDoc == "FAC")
                    //{
                    //    idSerieT = _facturas.GetIdSerie(Convert.ToInt32(fac_ruta[1]), "F", fechaAnu, fac_ruta[0]);
                    //}
                    //else
                    //{
                    //    idSerieT = _facturas.GetIdSerie(Convert.ToInt32(fac_ruta[1]), "C", fechaAnu, fac_ruta[0]);
                    //}

                    DatosRawAnulacion.numDoc = 0;
                    DatosRawAnulacion.tipoDoc = _facturas.GetTipoDoc(Convert.ToInt32(fac_ruta[1]), fechaDoc, fac_ruta[0]) == "C" ? "CCF" : "FAC";
                    DatosRawAnulacion.codigoGeneracion = _facturas.GetCodigoGeneracion(Convert.ToInt32(fac_ruta[1]), DatosRawAnulacion.tipoDoc == "FAC" ? "F" : "C", fechaAnu, fac_ruta[0]);
                    DatosRawAnulacion.nitEmisor = "0614-130571-001-2";
                    DatosRawAnulacion.correlativoInterno = "AV_" + idSerieT + "_" + fac_ruta[0];
                    DatosRawAnulacion.fechaDoc = fechaDoc;
                    DatosRawAnulacion.fechaAnulacion = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    DatosRawAnulacion.codigoGeneracionR = "";
                    DatosRawAnulacion.nombreResponsable = "AVICOLA SALVADOREÑA";
                    DatosRawAnulacion.tipDocResponsable = "36";
                    DatosRawAnulacion.numDocResponsable = "06141305710012";
                    DatosRawAnulacion.nombreSolicita = "AVICOLA SALVADOREÑA";
                    DatosRawAnulacion.tipDocSolicita = "36";
                    DatosRawAnulacion.numDocSolicita = "06141305710012";
                    DatosRawAnulacion.tipoAnulacion = 2;
                    DatosRawAnulacion.motivoAnulacion = "Rescindir de la operación realizada";
                    revisarFEL = _facturas.GetCodigoSello(Convert.ToInt32(fac_ruta[1]), DatosRawAnulacion.tipoDoc == "FAC" ? "F" : "C", fechaAnu, fac_ruta[0]);
                }
                else
                {
                    fechaAnu = fechaDoc;
                    fac_ruta = correlativoInterno.Split('|');
                    string idMotivo = fac_ruta[2]; //empleado
                    string motivoDescripcion = _facturas.ObtieneMotivoAnulacion(Convert.ToInt32(idMotivo));
                    //idSerieT = Convert.ToInt32(fac_ruta[3]);

                    DatosRawAnulacion.numDoc = 0;
                    DatosRawAnulacion.tipoDoc = _facturas.GetTipoDocPreImpresa(Convert.ToInt32(fac_ruta[1]), fechaDoc, fac_ruta[0]) == "C" ? "CCF" : "FAC";
                    DatosRawAnulacion.codigoGeneracion = _facturas.GetCodigoGeneracionPreImpresa(Convert.ToInt32(fac_ruta[1]), fechaAnu, fac_ruta[0]);
                    DatosRawAnulacion.nitEmisor = "0614-130571-001-2";
                    DatosRawAnulacion.correlativoInterno = "AV_" + fac_ruta[0];
                    DatosRawAnulacion.fechaDoc = fechaDoc;
                    DatosRawAnulacion.fechaAnulacion = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    DatosRawAnulacion.codigoGeneracionR = "";
                    DatosRawAnulacion.nombreResponsable = "AVICOLA SALVADOREÑA";
                    DatosRawAnulacion.tipDocResponsable = "36";
                    DatosRawAnulacion.numDocResponsable = "06141305710012";
                    DatosRawAnulacion.nombreSolicita = "AVICOLA SALVADOREÑA";
                    DatosRawAnulacion.tipDocSolicita = "36";
                    DatosRawAnulacion.numDocSolicita = "06141305710012";
                    DatosRawAnulacion.tipoAnulacion = 2;
                    DatosRawAnulacion.motivoAnulacion = "Rescindir de la operación realizada";
                    revisarFEL = _facturas.GetCodigoSelloPreImpres(Convert.ToInt32(fac_ruta[1]), fechaAnu, fac_ruta[0]);
                }

                //CONVERSION A JSON Y ENVIO

                string respuestaMetodo = "";
                string jsonString = JsonConvert.SerializeObject(DatosRawAnulacion);
                string jsonFinal = jsonString;
                //string jsonFinal = jsonString.Substring(1, jsonString.Length - 2);

                if (revisarFEL != "0" && revisarFEL != null)
                {
                    RestClient cliente = new RestClient(UrlJsonAnulacion)
                    {
                        //Authenticator = new HttpBasicAuthenticator(Usuario, Password),
                        Timeout = 900000
                    };
                    RestRequest request = new RestRequest
                    {
                        Method = Method.POST
                    };
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    request.Parameters.Clear();
                    request.AddHeader("Authorization", Token);
                    request.AddParameter("application/json", jsonFinal, ParameterType.RequestBody);
                    IRestResponse respond = cliente.Execute(request);
                    content = respond.Content;
                    HttpStatusCode httpStatusCode = respond.StatusCode;
                    numericStatusCode = (int)httpStatusCode;
                }
                else
                {
                    numericStatusCode = 999;
                    string descripcion = "Documento anulado a las " + DateTime.Now.ToString();
                    controlOLS.ActualizaHH_Anulacion(descripcion, "0-NO FEL", "0-NO FEL", Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                    controlOLS.BorraReparto_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                    controlOLS.BorraDevolucion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                    controlOLS.InsertaAnulacion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], fac_ruta[2], idSerieT);
                    controlOLS.BorraPagos_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);

                    respuestaMetodo = @"Documento #" + fac_ruta[0] + " fue anulado internamente!!!\n" +
                                      "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                      "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaOLS1.mensajeCompleto = respuestaMetodo;
                    respuestaOLS1.numeroDocumento = fac_ruta[1];
                    respuestaOLS1.respuestaOlShttp = null;
                    respuestaOLS1.ResultadoSatisfactorio = true;

                    controlOLS.RecLogBitacoraFEL(Convert.ToInt32(fac_ruta[1]), Convert.ToInt32(idSerieT), fac_ruta[0], "NO JSON", "NO JSON", "ANULADO INTERNAMENTE", "Anulacion Interna", "");
                    //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);

                    return respuestaOLS1;
                }
                int rutaTemp = Convert.ToInt32(fac_ruta[1]);
                string facturaTemp = DatosRawAnulacion.correlativoInterno;

                dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                string jsontt = jsonRespuesta.result;
                //dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
                //string resultJson = jsonRespuesta2.result;
                dynamic resultObject = JsonConvert.DeserializeObject(jsontt);

                string docs = resultObject.ToString();
                string jsonTotal = @"[" + docs + "]";
                List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);

                if (numericStatusCode == 200) //REVISA SU CODIGO DE ESTADO, SI ES 200 NO HAY ERROR EN EL JSON
                {
                    //JObject jsonObjectX = JObject.Parse(content);

                    if (jsonDocs[0].status == 1 || jsonDocs[0].status == 222) //SI EL MENSAJE ES OK, EL JSON LLEGO A OLS
                    {
                        //string facturaTemp = DatosRawAnulacion.correlativoInterno;

                        if (jsonDocs[0].status == 222)
                        {
                            jsonDocs[0].selloRecibido = "0";
                        }

                        if (idTipo == 1 && jsonDocs[0].status == 1)
                        {
                            string descripcion = "Documento anulado a las " + DateTime.Now.ToString();
                            controlOLS.ActualizaHH_Anulacion(descripcion, jsonDocs[0].selloRecibido, jsonDocs[0].codigoGeneracion, Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                            controlOLS.BorraReparto_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                            controlOLS.BorraDevolucion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                            controlOLS.InsertaAnulacion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], fac_ruta[2], idSerieT);
                            controlOLS.BorraPagos_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                        }
                        if (jsonDocs[0].status == 222 && !jsonDocs[0].statusMsg.Contains("INVALIDADO"))
                        {
                            respuestaMetodo = @"Documento #" + fac_ruta[0] + " no fue enviado!!!\n" +
                                              "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                              "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                            respuestaOLS1.mensajeCompleto = respuestaMetodo;
                            respuestaOLS1.numeroDocumento = facturaTemp;
                            respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS1.ResultadoSatisfactorio = false;

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-Error", "NO URL");
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);
                        }
                        else if (jsonDocs[0].status == 222 && jsonDocs[0].statusMsg.Contains("INVALIDADO"))
                        {
                            string descripcion = "Documento anulado a las " + DateTime.Now.ToString();
                            respuestaMetodo = @"Documento #" + fac_ruta[0] + " fue enviado!!!\n" +
                                              "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                              "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                            //string descripcion = "Documento anulado a las " + DateTime.Now.ToString();
                            controlOLS.ActualizaHH_Anulacion(descripcion, jsonDocs[0].selloRecibido, jsonDocs[0].codigoGeneracion, Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                            controlOLS.BorraReparto_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                            controlOLS.BorraDevolucion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                            controlOLS.InsertaAnulacion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], fac_ruta[2], idSerieT);
                            controlOLS.BorraPagos_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);

                            respuestaOLS1.mensajeCompleto = respuestaMetodo;
                            respuestaOLS1.numeroDocumento = facturaTemp;
                            respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS1.ResultadoSatisfactorio = true;

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-Error", "NO URL");
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);
                        }
                        else if (jsonDocs[0].status == 1)
                        {
                            respuestaMetodo = @"Documento #" + fac_ruta[0] + "enviado!!!\n" +
                                              "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                              "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                            respuestaOLS1.mensajeCompleto = respuestaMetodo;
                            respuestaOLS1.numeroDocumento = facturaTemp;
                            respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS1.ResultadoSatisfactorio = true;

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-OK", "NO URL");
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);
                        }

                        //if (jsonDocs[0].status == 222)
                        //    {
                        //        respuestaMetodo = @"Documento #" + fac_ruta[0] + " no fue enviado!!!\n" +
                        //               "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                        //               "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                        //        respuestaOLS1.mensajeCompleto = respuestaMetodo;
                        //        respuestaOLS1.numeroDocumento = facturaTemp;
                        //        respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                        //        respuestaOLS1.ResultadoSatisfactorio = false;

                        //        //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-Error", "NO URL");
                        //        controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);

                        //    }

                        //respuestaOLS.res = jsonDocs[0];

                        return respuestaOLS1;
                    }
                    else
                    {
                        //string facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                        //int rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                        //string resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                        //string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                        //dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                        //dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
                        //string resultJson = jsonRespuesta2.result;
                        //dynamic resultObject = JsonConvert.DeserializeObject(resultJson);

                        //string docs = resultObject.ToString();
                        //string jsonTotal = @"[" + docs + "]";
                        //List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);

                        anulacion = 0;

                        if (jsonDocs[0].statusMsg.Contains("DOCUMENTO SE ENCUENTA ANULADO"))
                        {
                        }
                        //controlOLS.RecLogBitacora(
                        //						0,
                        //						"ANU",
                        //						Convert.ToInt32(facturaTemp),
                        //						resolucionTemp,
                        //						serieTemp,
                        //						jsonDocs[0].result + " en la ruta: " + rutaTemp,
                        //						numericStatusCode
                        //					  ); //SE REGISTRA ERROR EN LA BITACORA
                        //return respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                        //                "Tipo documento: ANU\n" +
                        //                "Error:" + jsonDocs[0].result + "\n" +
                        //                "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                        respuestaMetodo = @"Documento #" + fac_ruta[0] + " ya anulado!!!\n" +
                                       "Tipo documento: F " +
                                       "Error:" + jsonDocs[0].statusMsg + "\n" +
                                       "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                        respuestaOLS1.mensajeCompleto = respuestaMetodo;
                        respuestaOLS1.numeroDocumento = fac_ruta[0];
                        respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                        //respuestaOLS.res = jsonDocs[0];

                        controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-ANULADO ANTES", "NO URL");
                        //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);

                        return respuestaOLS1;

                        //respuestaOLS.mensajeCompleto = respuestaMetodo;
                        //respuestaOLS.respuestaOlShttp = jsonDocs[0];
                        //respuestaOLS.numeroDocumento = fac_ruta[0];
                        //respuestaOLS.ResultadoSatisfactorio = true;
                    }
                }
                else
                {
                    if (numericStatusCode == 999)
                    {
                        respuestaMetodo = @"Documento #" + fac_ruta[0] + "enviado!!!\n" +
                                      "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                      "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                        respuestaOLS1.mensajeCompleto = respuestaMetodo;
                        respuestaOLS1.numeroDocumento = fac_ruta[0];
                        respuestaOLS1.respuestaOlShttp = null;
                        //respuestaOLS.res = jsonDocs[0];

                        controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-OK", "NO URL");
                        //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);

                        return respuestaOLS1;
                    }

                    //dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                    //dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
                    //string resultJson = jsonRespuesta2.result;
                    //dynamic resultObject = JsonConvert.DeserializeObject(resultJson);

                    //string docs = resultObject.ToString();
                    //string jsonTotal = @"[" + docs + "]";
                    //List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);
                    //controlOLS.RecLogBitacora(
                    //							0,
                    //							"ANU",
                    //							Convert.ToInt32(facturaTemp),
                    //							resolucionTemp,
                    //							serieTemp,
                    //							"BAD REQUEST: Debido a Gateway time o Error de Sintaxis en la ruta: " + DatosRaw.Select(x => x.cajaSuc).ToString(),
                    //							numericStatusCode
                    //						  ); //SE REGISTRA ERROR EN LA BITACORA
                    //return respuestaMetodo = @"Documento #" + DatosRaw.Select(x => x.numFactura).ToString() + "no fue anulado!!!\n" +
                    //                        "Tipo documento: FAC/ANU\n" +
                    //                        "Error:BAD REQUEST: Debido a Gateway time o Error de Sintaxis\n" +
                    //                        "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaMetodo = @"Documento #" + fac_ruta[0] + "no fue ANULADO!!!\n" +
                                      "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                      "Error:" + jsonDocs[0].statusMsg + "\n" +
                                      "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaOLS1.mensajeCompleto = respuestaMetodo;
                    respuestaOLS1.numeroDocumento = fac_ruta[0];
                    respuestaOLS1.respuestaOlShttp = null;

                    return respuestaOLS1;
                }
            }
            catch (Exception ex)
            {
                StackTrace s = new StackTrace(ex);
                Assembly thisasm = Assembly.GetExecutingAssembly();
                string methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                respuestaOLS1.mensajeCompleto = @"Error interno:" + ex.Message.ToString() + "\n" +
                         "Metodo:" + s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;

                respuestaOLS1.numeroDocumento = "0";
                //respuestaOLS1.ResultadoSatisfactorio = false;

                return respuestaOLS1;
            }
        }

        /// <summary>
        /// ENVIA ANULACION HACIA OLS
        /// </summary>
        /// <param name="DatosRawAnulacion"></param>
        /// <param name="DatosRaw"></param>
        /// <returns></returns>
        /// [WebMethod(Description ="Envia Anulacion de Documento")]
        ///
        [WebMethod(Description = "Envia Anulacion de Documento")]
        public RespuestaOLSAnulacion EnviaDataAnulacionSalaVentas(string fechaDoc, string correlativoInterno, int idTipo)
        {
            Facturas _facturas = new Facturas();
            string Token = "";
            string revisaT = RevisarToken();
            int numericStatusCode = 0;
            if (revisaT == "0")
            {
                Token = GenerateTokenAsync(UrlToken, Usuario, Password, IdCompany, UsuarioHead, PasswordHead);
            }
            else
            {
                Token = revisaT;
            }

            //RespuestaAnulacion respuestaOLS = new RespuestaAnulacion();
            RespuestaOLSAnulacion respuestaOLS1 = new RespuestaOLSAnulacion();
            MapaAnulacion DatosRawAnulacion = new MapaAnulacion();
            try
            {
                string[] fac_ruta;
                string fechaAnu = "";
                string revisarFEL = "";
                string content = "";

                int idSerieT = 0;

                fechaAnu = fechaDoc;
                fac_ruta = correlativoInterno.Split('|');
                idSerieT = Convert.ToInt32(fac_ruta[3]);
                string idMotivo = fac_ruta[2]; //empleado
                string motivoDescripcion = _facturas.ObtieneMotivoAnulacion(Convert.ToInt32(idMotivo));

                if (idTipo == 1)
                {
                    //MapaAnulacion DatosRawAnulacion=new MapaAnulacion();
                    DatosRawAnulacion.tipoDoc = _facturasSala.GetTipoDoc(Convert.ToInt32(fac_ruta[1]), fechaDoc, fac_ruta[0]) == "C" ? "CCF" : "FAC";

                    //if (DatosRawAnulacion.tipoDoc == "FAC")
                    //{
                    //    idSerieT = _facturas.GetIdSerie(Convert.ToInt32(fac_ruta[1]), "F", fechaAnu, fac_ruta[0]);
                    //}
                    //else
                    //{
                    //    idSerieT = _facturas.GetIdSerie(Convert.ToInt32(fac_ruta[1]), "C", fechaAnu, fac_ruta[0]);
                    //}

                    DatosRawAnulacion.numDoc = 0;
                    DatosRawAnulacion.tipoDoc = _facturasSala.GetTipoDoc(Convert.ToInt32(fac_ruta[1]), fechaDoc, fac_ruta[0]) == "C" ? "CCF" : "FAC";
                    DatosRawAnulacion.codigoGeneracion = _facturasSala.GetCodigoGeneracion(Convert.ToInt32(fac_ruta[1]), DatosRawAnulacion.tipoDoc == "FAC" ? "F" : "C", fechaAnu, fac_ruta[0]);
                    DatosRawAnulacion.nitEmisor = "0614-130571-001-2";
                    DatosRawAnulacion.correlativoInterno = "AV_" + idSerieT + "_" + fac_ruta[0];
                    DatosRawAnulacion.fechaDoc = fechaDoc;
                    DatosRawAnulacion.fechaAnulacion = DateTime.Now.Date.ToString("yyyy-MM-dd");
                    DatosRawAnulacion.codigoGeneracionR = "";
                    DatosRawAnulacion.nombreResponsable = "AVICOLA SALVADOREÑA";
                    DatosRawAnulacion.tipDocResponsable = "36";
                    DatosRawAnulacion.numDocResponsable = "06141305710012";
                    DatosRawAnulacion.nombreSolicita = "AVICOLA SALVADOREÑA";
                    DatosRawAnulacion.tipDocSolicita = "36";
                    DatosRawAnulacion.numDocSolicita = "06141305710012";
                    DatosRawAnulacion.tipoAnulacion = 2;
                    DatosRawAnulacion.motivoAnulacion = "Rescindir de la operación realizada";
                    revisarFEL = _facturasSala.GetCodigoSello(Convert.ToInt32(fac_ruta[1]), DatosRawAnulacion.tipoDoc == "FAC" ? "F" : "C", fechaAnu, fac_ruta[0]);
                }

                //CONVERSION A JSON Y ENVIO

                string respuestaMetodo = "";
                string jsonString = JsonConvert.SerializeObject(DatosRawAnulacion);
                string jsonFinal = jsonString;
                //string jsonFinal = jsonString.Substring(1, jsonString.Length - 2);

                if (revisarFEL != "0" && revisarFEL != null)
                {
                    RestClient cliente = new RestClient(UrlJsonAnulacion)
                    {
                        //Authenticator = new HttpBasicAuthenticator(Usuario, Password),
                        Timeout = 900000
                    };
                    RestRequest request = new RestRequest
                    {
                        Method = Method.POST
                    };
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    request.Parameters.Clear();
                    request.AddHeader("Authorization", Token);
                    request.AddParameter("application/json", jsonFinal, ParameterType.RequestBody);
                    IRestResponse respond = cliente.Execute(request);
                    content = respond.Content;
                    HttpStatusCode httpStatusCode = respond.StatusCode;
                    numericStatusCode = (int)httpStatusCode;
                }
                else
                {
                    numericStatusCode = 999;
                    string descripcion = "Documento anulado internamente a las " + DateTime.Now.ToString();
                    controlOLS.Actualiza_AnulacionSalaVenta(fechaAnu, descripcion, "", "", Convert.ToInt32(fac_ruta[1]), fac_ruta[0]);
                    //controlOLS.ActualizaHH_Anulacion(descripcion, "0-NO FEL", "0-NO FEL", Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                    //controlOLS.BorraReparto_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                    //controlOLS.BorraDevolucion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                    //controlOLS.InsertaAnulacion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], fac_ruta[2], idSerieT);
                    //controlOLS.BorraPagos_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);

                    respuestaMetodo = @"Documento #" + fac_ruta[0] + " fue anulado internamente!!!\n" +
                                      "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                      "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaOLS1.mensajeCompleto = respuestaMetodo;
                    respuestaOLS1.numeroDocumento = fac_ruta[1];
                    respuestaOLS1.respuestaOlShttp = null;
                    respuestaOLS1.ResultadoSatisfactorio = true;

                    //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);

                    return respuestaOLS1;
                }
                int rutaTemp = Convert.ToInt32(fac_ruta[1]);
                string facturaTemp = DatosRawAnulacion.correlativoInterno;

                dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                string jsontt = jsonRespuesta.result;
                //dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
                //string resultJson = jsonRespuesta2.result;
                dynamic resultObject = JsonConvert.DeserializeObject(jsontt);

                string docs = resultObject.ToString();
                string jsonTotal = @"[" + docs + "]";
                List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);

                if (numericStatusCode == 200) //REVISA SU CODIGO DE ESTADO, SI ES 200 NO HAY ERROR EN EL JSON
                {
                    //JObject jsonObjectX = JObject.Parse(content);

                    if (jsonDocs[0].status == 1 || jsonDocs[0].status == 222) //SI EL MENSAJE ES OK, EL JSON LLEGO A OLS
                    {
                        //string facturaTemp = DatosRawAnulacion.correlativoInterno;

                        if (jsonDocs[0].status == 222)
                        {
                            jsonDocs[0].selloRecibido = "0";
                        }

                        if (idTipo == 1 && jsonDocs[0].status == 1)
                        {
                            string descripcion = "Documento anulado a las " + DateTime.Now.ToString();
                            controlOLS.Actualiza_AnulacionSalaVenta(fechaAnu, descripcion, jsonDocs[0].selloRecibido, jsonDocs[0].codigoGeneracion, Convert.ToInt32(fac_ruta[1]), fac_ruta[0]);
                            //controlOLS.BorraReparto_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                            //controlOLS.BorraDevolucion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                            //controlOLS.InsertaAnulacion_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], fac_ruta[2], idSerieT);
                            //controlOLS.BorraPagos_Anulacion(Convert.ToInt32(fac_ruta[1]), fac_ruta[0], idSerieT);
                        }

                        if (jsonDocs[0].status == 222 && !jsonDocs[0].statusMsg.Contains("INVALIDADO"))
                        {
                            respuestaMetodo = @"Documento #" + fac_ruta[0] + " no fue enviado!!!\n" +
                                   "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                   "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                            respuestaOLS1.mensajeCompleto = respuestaMetodo;
                            respuestaOLS1.numeroDocumento = facturaTemp;
                            respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS1.ResultadoSatisfactorio = false;

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-Error", "NO URL");
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);
                        }
                        else if (jsonDocs[0].status == 222 && jsonDocs[0].statusMsg.Contains("INVALIDADO"))
                        {
                            string descripcion = "Documento anulado a las " + DateTime.Now.ToString();
                            respuestaMetodo = @"Documento #" + fac_ruta[0] + " fue enviado!!!\n" +
                                              "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                              "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                            controlOLS.Actualiza_AnulacionSalaVenta(fechaAnu, descripcion, jsonDocs[0].selloRecibido, jsonDocs[0].codigoGeneracion, Convert.ToInt32(fac_ruta[1]), fac_ruta[0]);

                            respuestaOLS1.mensajeCompleto = respuestaMetodo;
                            respuestaOLS1.numeroDocumento = facturaTemp;
                            respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS1.ResultadoSatisfactorio = true;

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-Error", "NO URL");
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);
                        }
                        else if (jsonDocs[0].status == 1)
                        {
                            respuestaMetodo = @"Documento #" + fac_ruta[0] + "enviado!!!\n" +
                                   "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                   "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                            respuestaOLS1.mensajeCompleto = respuestaMetodo;
                            respuestaOLS1.numeroDocumento = facturaTemp;
                            respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                            respuestaOLS1.ResultadoSatisfactorio = true;

                            controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg,"Envio-OK", "NO URL");
                            //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);
                        }

                        //respuestaOLS.res = jsonDocs[0];

                        return respuestaOLS1;
                    }
                    else
                    {
                        //string facturaTemp = DatosRaw.Select(x => x.numFactura).FirstOrDefault();
                        //int rutaTemp = Convert.ToInt32(DatosRaw.Select(x => x.cajaSuc).FirstOrDefault());
                        //string resolucionTemp = DatosRaw.Select(x => x.resolucion).FirstOrDefault();
                        //string serieTemp = DatosRaw.Select(x => x.serie).FirstOrDefault();

                        //dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                        //dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
                        //string resultJson = jsonRespuesta2.result;
                        //dynamic resultObject = JsonConvert.DeserializeObject(resultJson);

                        //string docs = resultObject.ToString();
                        //string jsonTotal = @"[" + docs + "]";
                        //List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);

                        anulacion = 0;

                        if (jsonDocs[0].statusMsg.Contains("DOCUMENTO SE ENCUENTA ANULADO"))
                        {
                        }
                        //controlOLS.RecLogBitacora(
                        //						0,
                        //						"ANU",
                        //						Convert.ToInt32(facturaTemp),
                        //						resolucionTemp,
                        //						serieTemp,
                        //						jsonDocs[0].result + " en la ruta: " + rutaTemp,
                        //						numericStatusCode
                        //					  ); //SE REGISTRA ERROR EN LA BITACORA
                        //return respuestaMetodo = @"Documento #" + facturaTemp + "no fue enviado!!!\n" +
                        //                "Tipo documento: ANU\n" +
                        //                "Error:" + jsonDocs[0].result + "\n" +
                        //                "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                        respuestaMetodo = @"Documento #" + fac_ruta[0] + " ya anulado!!!\n" +
                                       "Tipo documento: F " +
                                       "Error:" + jsonDocs[0].statusMsg + "\n" +
                                       "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                        respuestaOLS1.mensajeCompleto = respuestaMetodo;
                        respuestaOLS1.numeroDocumento = fac_ruta[0];
                        respuestaOLS1.respuestaOlShttp = jsonDocs[0];
                        //respuestaOLS.res = jsonDocs[0];

                        controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg,"Envio-OK", "NO URL");
                        //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);

                        return respuestaOLS1;

                        //respuestaOLS.mensajeCompleto = respuestaMetodo;
                        //respuestaOLS.respuestaOlShttp = jsonDocs[0];
                        //respuestaOLS.numeroDocumento = fac_ruta[0];
                        //respuestaOLS.ResultadoSatisfactorio = true;
                    }
                }
                else
                {
                    if (numericStatusCode == 999)
                    {
                        respuestaMetodo = @"Documento #" + fac_ruta[0] + "enviado!!!\n" +
                                      "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                      "Enviado a las:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                        respuestaOLS1.mensajeCompleto = respuestaMetodo;
                        respuestaOLS1.numeroDocumento = fac_ruta[0];
                        respuestaOLS1.respuestaOlShttp = null;
                        //respuestaOLS.res = jsonDocs[0];

                        controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg, "Envio-OK", "NO URL");
                        //controlOLS.RecLogBitacoraFEL(rutaTemp, Convert.ToInt32(idSerieT), facturaTemp.Split('|')[0], jsonFinal, jsonTotal, jsonDocs[0].statusMsg);

                        return respuestaOLS1;
                    }

                    //dynamic jsonRespuesta = JsonConvert.DeserializeObject(content);
                    //dynamic jsonRespuesta2 = JsonConvert.DeserializeObject(jsonRespuesta);
                    //string resultJson = jsonRespuesta2.result;
                    //dynamic resultObject = JsonConvert.DeserializeObject(resultJson);

                    //string docs = resultObject.ToString();
                    //string jsonTotal = @"[" + docs + "]";
                    //List<RespuestaAnulacion> jsonDocs = JsonConvert.DeserializeObject<List<RespuestaAnulacion>>(jsonTotal);
                    //controlOLS.RecLogBitacora(
                    //							0,
                    //							"ANU",
                    //							Convert.ToInt32(facturaTemp),
                    //							resolucionTemp,
                    //							serieTemp,
                    //							"BAD REQUEST: Debido a Gateway time o Error de Sintaxis en la ruta: " + DatosRaw.Select(x => x.cajaSuc).ToString(),
                    //							numericStatusCode
                    //						  ); //SE REGISTRA ERROR EN LA BITACORA
                    //return respuestaMetodo = @"Documento #" + DatosRaw.Select(x => x.numFactura).ToString() + "no fue anulado!!!\n" +
                    //                        "Tipo documento: FAC/ANU\n" +
                    //                        "Error:BAD REQUEST: Debido a Gateway time o Error de Sintaxis\n" +
                    //                        "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaMetodo = @"Documento #" + fac_ruta[0] + "no fue ANULADO!!!\n" +
                                      "Tipo documento: " + DatosRawAnulacion.tipoDoc + " " +
                                      "Error:" + jsonDocs[0].statusMsg + "\n" +
                                      "Error generado:" + DateTime.Now.Hour + " horas y " + DateTime.Now.Minute + " minutos!!";

                    respuestaOLS1.mensajeCompleto = respuestaMetodo;
                    respuestaOLS1.numeroDocumento = fac_ruta[0];
                    respuestaOLS1.respuestaOlShttp = null;

                    return respuestaOLS1;
                }
            }
            catch (Exception ex)
            {
                StackTrace s = new StackTrace(ex);
                Assembly thisasm = Assembly.GetExecutingAssembly();
                string methodname = s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;
                respuestaOLS1.mensajeCompleto = @"Error interno:" + ex.Message.ToString() + "\n" +
                         "Metodo:" + s.GetFrames().Select(f => f.GetMethod()).First(m => m.Module.Assembly == thisasm).Name;

                respuestaOLS1.numeroDocumento = "0";
                //respuestaOLS1.ResultadoSatisfactorio = false;

                return respuestaOLS1;
            }
        }

        /// <summary>
        /// GRABA LOS ERRORES INTERNOS EN LA BITACORA
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="fecha"></param>
        /// <param name="docPos"></param>
        /// <param name="num"></param>
        public void GrabarErrorInternos(int ruta, string fecha, int docPos, int num, string error)
        {
            string docTemp = "";

            if (docPos == 1)
            {
                docTemp = "FC";
            }
            else if (docPos == 2)
            {
                docTemp = "NC";
            }
            else if (docPos == 3)
            {
                docTemp = "NR";
            }
            else if (docPos == 6)
            {
                docTemp = "CCF";
            }
            else if (docPos == 7)
            {
                docTemp = "ANU";
            }
            //controlOLS.RecLogBitacora(0, docTemp, num, "XX", "XX", error, 000);
        }

        public string RevisarToken()
        {
            string fechaHoy = DateTime.Now.Date.ToString("yyyy-MM-dd");
            string token = _facturas.GetTokenNow(fechaHoy);
            return token;
        }

        public string GenerateTokenAsync(string url, string usuario, string pass, string company, string userHead, string passHead)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{userHead}:{passHead}"));

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var body = new
            {
                userName = usuario,
                password = pass,
                idCompany = company
            };

            request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpClient client = new HttpClient();
            var response = client.SendAsync(request).Result;

            if (response.IsSuccessStatusCode)
            {
                var token = response.Content.ReadAsStringAsync().Result;
                var jsonResult = JsonConvert.DeserializeObject(token).ToString();
                JsonTokenOLS myDeserializedClass = JsonConvert.DeserializeObject<JsonTokenOLS>(jsonResult);
                // Extraer el valor de "message"
                //string messageValue = jsonObject.GetValue("message").ToString();

                // eliminamos los caracteres de escape de la cadena
                // Quita los caracteres de escape del string
                //JsonTokenOLS mensaje=JsonConvert.DeserializeObject<JsonTokenOLS>(token);

                // Accede a la propiedad "message"
                string message = myDeserializedClass.message;

                //var responseContent = JsonConvert.DeserializeAnonymousType(token, new { message = "" });
                _facturas.InsertaToken(message);
                return message;
            }
            else
            {
                throw new Exception($"Error al generar el token: {response.ReasonPhrase}");
            }
        }
    }
}