using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Ws_OLS.Clases
{
    public class ControlDatosOLS
    {
        //CADENA DE CONEXION
        private string connectionString = ConfigurationManager.ConnectionStrings["IPES"].ConnectionString;

        //string connectionStringH = ConfigurationManager.ConnectionStrings["IPESH"].ConnectionString;

        //OBTIENE CANTIDAD DE FILAS POR RUTA Y FECHA DONDE SEA FACTURA
        public void CambiaEstadoFCCCF(int ruta, string FC, string fecha, string numero, string sello, string control, string generacion)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //
                //      WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string selloTemp = "";

                if (!String.IsNullOrWhiteSpace(sello) && sello != "0" && sello.Length>10)
                {
                    selloTemp = "FELAutorizacion = @sello,";
                }


                string sqlQuery = @"UPDATE HandHeld.FacturaEBajada
									SET "+ selloTemp +" FELSerie=@generacion, FELNumero=@control "+
									"WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND TipoDocumento=@FC AND Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@FC", FC);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    if (!String.IsNullOrWhiteSpace(sello) && sello!="0")
                    {
                        cmd.Parameters.AddWithValue("@sello", sello);
                    }
                    

                    cmd.Parameters.AddWithValue("@generacion", generacion);
                    cmd.Parameters.AddWithValue("@control", control);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        public void CambiaEstadoSello(int ruta, string FC, string fecha, string numero, string sello)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"UPDATE HandHeld.FacturaEBajada
									SET FELAutorizacion=@sello
									WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND TipoDocumento=@FC AND Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@FC", FC);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@sello", sello);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        //OBTIENE CANTIDAD DE FILAS POR RUTA Y FECHA DONDE SEA FACTURA
        public void CambiaEstadoFCCCFPreImpresa(int ruta, string fecha, string numero, string sello, string control, string generacion)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"UPDATE Facturacion.FacturaE
									SET FELAutorizacion=@Sello, FELNumero=@NumControl, FELSerie=@Generacion
									WHERE idRutaReparto=@ruta AND CAST(FechaFactura AS DATE)=@fecha AND idFactura=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@Sello", sello);
                    cmd.Parameters.AddWithValue("@NumControl", control);
                    cmd.Parameters.AddWithValue("@Generacion", generacion);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        //CAMBIA ESTADO FACTURA ANULADA
        public void CambiaEstadoFANU(int ruta, string FC, string fecha, string numero, string sello)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"UPDATE HandHeld.FacturaEBajada
									SET FELAutorizacion=@sello, FeLAnulacionNumero=1
									WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND TipoDocumento=@FC AND Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@FC", FC);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@sello", sello);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        //OBTIENE CANTIDAD DE FILAS POR RUTA Y FECHA DONDE SEA NOTA DE CREDITO
        public void CambiaEstadoNC(int ruta, string fecha, string numero, string sello)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"UPDATE Reparto.DocumentosFacturasEBajada
									SET FELAutorizacion=@sello
									WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@sello", sello);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        //OBTIENE CANTIDAD DE FILAS POR RUTA Y FECHA DONDE SEA NOTA DE REMISION
        public void CambiaEstadoNR(int ruta, string fecha, string corr, string sello)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"UPDATE HandHeld.NotaRemisionBajada
									SET FELAutorizacion=@sello
									WHERE idRuta=@ruta AND CAST(FechaDescarga AS DATE)=@fecha AND Correlativo=@corr";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@corr", corr);
                    cmd.Parameters.AddWithValue("@sello", sello);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        //INGRESA DATO EN BITACORA
        public void RecLogBitacora(int est, string TipoDoc, int NumDoc, string Resolucion, string Serie, string Mensaje, int Status)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                DateTime fechaActual = new DateTime();
                fechaActual = DateTime.Now;
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"INSERT INTO dbo.BitacorasOLS
									VALUES(@est, @TipoDoc, @NumDoc, @Resolucion, @Serie, @Mensaje, @Status, @FechaHora)";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@est", est);
                    cmd.Parameters.AddWithValue("@TipoDoc", TipoDoc);
                    cmd.Parameters.AddWithValue("@NumDoc", NumDoc);
                    cmd.Parameters.AddWithValue("@Resolucion", Resolucion);
                    cmd.Parameters.AddWithValue("@Serie", Serie);
                    cmd.Parameters.AddWithValue("@Mensaje", Mensaje);
                    cmd.Parameters.AddWithValue("@Status", Status);
                    cmd.Parameters.AddWithValue("@FechaHora", fechaActual);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        //INGRESA DATO EN BITACORA FEL NUEVA
        public void RecLogBitacoraFEL(int idRuta, int idSerie, string NumDoc, string jsonGenerado, string jsonResultante, string MensajeOLS)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string[] numeroDoc=NumDoc.Split('_');
        
                DateTime fechaActual = new DateTime();
                fechaActual = DateTime.Now;
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"INSERT INTO vendemas.logFEL
									VALUES(@idRuta, @idSerie, @NumDoc, @jsonGenerado, @jsonResultante, @MensajeOLS)";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idRuta", idRuta);
                    cmd.Parameters.AddWithValue("@idSerie", idSerie);
                    if (NumDoc.Contains("_"))
                    {
                        cmd.Parameters.AddWithValue("@NumDoc", numeroDoc[1]);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@NumDoc", NumDoc);
                    }
                    
                    cmd.Parameters.AddWithValue("@jsonGenerado", jsonGenerado);
                    cmd.Parameters.AddWithValue("@jsonResultante", jsonResultante);
                    cmd.Parameters.AddWithValue("@MensajeOLS", MensajeOLS);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        /***************************************/
        /***************ANULACIONES*************/
        /***************************************/

        public void ActualizaHH_Anulacion(string descripcion, string Numero, string serie, int ruta, string idNumero, int serieID)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                DateTime fechaActual = new DateTime();
                fechaActual = DateTime.Now;
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"UPDATE HandHeld.FacturaEBajada SET estado ='ANU',FELDescripcion=@Fel_Descripcion,
                                    FelAnulacionNumero=@FelAnulacionNumero,FELAnulacionSerie=@FEL_AnulacionSerie,StatusEntregado ='N',StatusImpresion ='A'
                                    WHERE  idruta=@idruta AND numero=@numero AND idSerie=@serieID ";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@Fel_Descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@FelAnulacionNumero", Numero);
                    cmd.Parameters.AddWithValue("@FEL_AnulacionSerie", serie);
                    cmd.Parameters.AddWithValue("@idruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", idNumero);
                    cmd.Parameters.AddWithValue("@serieID", serieID);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        public void BorraReparto_Anulacion(int ruta, string idNumero, int idSerie)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                DateTime fechaActual = new DateTime();
                fechaActual = DateTime.Now;
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"DELETE FROM Reparto.DevolucionPedidoBajada
                                    WHERE idruta=@idruta
                                    AND idfactura IN (SELECT idpedidoifx FROM HandHeld.FacturaEBajada WHERE idruta=@idruta AND numero=@numero and idSerie=@idSerie)";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", idNumero);
                    cmd.Parameters.AddWithValue("@idSerie", idSerie);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        public void BorraDevolucion_Anulacion(int ruta, string idNumero, int idSerie)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                DateTime fechaActual = new DateTime();
                fechaActual = DateTime.Now;
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"DELETE FROM HandHeld.DevolucionPedido
                                    WHERE idruta=@idruta 
                                    AND idPedidoIfx IN (SELECT idpedidoifx FROM HandHeld.FacturaEBajada WHERE idruta=@idruta AND numero=@numero and idSerie=@idSerie)";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", idNumero);
                    cmd.Parameters.AddWithValue("@idSerie", idSerie);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        public void InsertaAnulacion_Anulacion(int ruta, string idNumero, string motivo, int idSerie)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                DateTime fechaActual = new DateTime();
                fechaActual = DateTime.Now;
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"INSERT INTO  [reparto].[FacturasAnuladasBajada] ([IdRuta],[SubRuta],[numero],[fechaHoraAnulacion],[motivo],[fecha], [idSerie]) 
                                    VALUES(@idruta,'A',@numero,GETDATE(),@motivo,CONVERT(DATE,GETDATE()), @idSerie)";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", idNumero);
                    cmd.Parameters.AddWithValue("@motivo", motivo);
                    cmd.Parameters.AddWithValue("@idSerie", idSerie);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        public void BorraPagos_Anulacion(int ruta, string idNumero, int idSerie)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                DateTime fechaActual = new DateTime();
                fechaActual = DateTime.Now;
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"DELETE FROM reparto.PagosBajadaGPRS WHERE IdRuta = @idruta AND Factura = @numero and idSerie=@idSerie;
                                    DELETE FROM reparto.PagosBajada WHERE IdRuta = @idruta AND Factura = @numero and idSerie=@idSerie";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", idNumero);
                    cmd.Parameters.AddWithValue("@idSerie", idSerie);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }

        //NTC SGR
        public void ActualizaEstadoNotaCredito(string numero, string generacion, string sello, string factura)
        {
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                //string sqlQuery = @"UPDATE SAP.Liquidacion
                //                        SET ZFE_NUMERO =@numero, ZFE_CLAVE =@sello, ZZBKTXT = @generacion
                //                        WHERE ZNROCF=@numeroID";

                string sqlQuery = @"UPDATE LiquidacioneS.NotasEncabezado 
                                    SET FELSerie=@generacion,FELNUMERO=@numero,FELAutorizacion=@sello
                                    WHERE  serie + RIGHT('00000000' + LTRIM(RTRIM(STR(numeroformulario))), 8)=@numeroID";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@sello", sello);
                    cmd.Parameters.AddWithValue("@generacion", generacion);
                    cmd.Parameters.AddWithValue("@numeroID", factura);
                    cmd.ExecuteNonQuery();
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }
        }
    }
}