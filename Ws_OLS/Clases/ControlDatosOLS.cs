using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace Ws_OLS.Clases
{
    public class ControlDatosOLS
    {

        //CADENA DE CONEXION
        string connectionString = ConfigurationManager.ConnectionStrings["IPES"].ConnectionString;
        //string connectionStringH = ConfigurationManager.ConnectionStrings["IPESH"].ConnectionString;


        //OBTIENE CANTIDAD DE FILAS POR RUTA Y FECHA DONDE SEA FACTURA
        public void CambiaEstadoFCCCF(int ruta, string FC, string fecha, string numero, string sello)
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
    }
}