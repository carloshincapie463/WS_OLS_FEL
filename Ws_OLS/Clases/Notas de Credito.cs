using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Ws_OLS.Clases
{
    public class Notas_de_Credito
    {
        string connectionString = ConfigurationManager.ConnectionStrings["IPES"].ConnectionString;

        //OBTIENE CANTIDAD DE FILAS POR RUTA Y FECHA DONDE SEA FACTURA
        public DataTable CantidadNotasCredito(int ruta, string fecha)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series 
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                //string sqlQuery = @"SELECT *
                //					FROM Reparto.DocumentosFacturasEBajada 
                //					WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha";

                string sqlQuery;
                sqlQuery = @"SELECT *
                                FROM SAP.Liquidacion 
                                WHERE ZOPERAC IN ('01', '04', '05', '24', '74')
                                AND CAST(Fecha AS DATE) =@fecha AND idRuta =@ruta AND ZFE_CLAVE IS NULL";
                //       if (facNum == -1)
                //       {
                //           sqlQuery = @"SELECT *
                //                       FROM SAP.Liquidacion 
                //                       WHERE ZOPERAC IN ('01', '04', '05')
                //                       AND CAST(Fecha AS DATE) =@fecha AND idRuta =@ruta AND ZFE_CLAVE IS NULL";
                //       }
                //       else
                //       {
                //           sqlQuery = sqlQuery = @"SELECT *
                //FROM Reparto.DocumentosFacturasEBajada 
                //WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND Numero=@numero AND FELAutorizacion IS NULL";
                //       }

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                   
                    SqlDataAdapter ds = new SqlDataAdapter(cmd);
                    ds.Fill(dt);
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }

            return dt;
        }

        public string GetResolucionNC(int ruta, string ope)
        {
            string resolucion = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {

                string sqlQuery = "";
                cnn.Open();
                if (ope == "01" || ope=="04" || ope=="74")
                {
                    sqlQuery = @"SELECT S.resolucion 
                                    FROM Facturacion.Series S
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Serie=S.numeroSerie
                                    WHERE E.idFactura=@factura AND estado ='ACT'";
                }
                else
                {
                    sqlQuery = @"SELECT S.resolucion 
                                    FROM Facturacion.Series S
                                    INNER JOIN Liquidaciones.MermasextraE E ON E.Serie=S.numeroSerie
                                    WHERE E.idFactura=@factura AND estado ='ACT'";
                }
                

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        resolucion = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return resolucion;
        }

        //OBTIENE RESTINICIO
        public string GetResInicioNC(int ruta)
        {
            string resInicio = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT numeroDel 
                                    FROM Facturacion.Series 
                                    WHERE idRuta=@ruta AND WHERE idTipoSerie=2 AND estado ='ACT'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        resInicio = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return resInicio;
        }

        //OBTIENE RESTFIN
        public string GetResFinNC(int ruta)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT numeroAl 
                                    FROM Facturacion.Series 
                                    WHERE idRuta=@ruta AND WHERE idTipoSerie=2 AND estado ='ACT'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        //OBTIENE RESTFIN
        public string GetRestFechaNC(int ruta)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT FechaAutorizacion 
                                    FROM Facturacion.Series 
                                    WHERE idRuta=@ruta  AND WHERE idTipoSerie=2 AND estado ='ACT'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        //OBTIENE NUMERO SERIES
        public string GetNumSerieNC(int ruta)
        {
            string data = "";
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT numeroSerie 
                                    FROM Facturacion.Series S
                                    WHERE idRuta=@ruta  AND WHERE idTipoSerie=2 AND estado ='ACT'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        public string GetCorrelativoInterno(string idFactura, string ope)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();
                if (ope == "01" || ope == "04" || ope=="24" || ope == "74")
                {
                    sqlQuery = @"SELECT NumeroFormulario 
                                 FROM Liquidaciones.NotasEncabezado 
                                 WHERE idFactura IN (@idFactura)'";
                }
                else
                {
                    sqlQuery = @"SELECT NumeroFormulario 
                                 FROM Liquidaciones.mermasextrae 
                                 WHERE idFactura IN (@idFactura)'";
                }
                  

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", idFactura);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        public string GetClienteNC(string idFactura, string ope)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();
                if (ope == "01" || ope == "04" || ope=="24" || ope == "74")
                {
                    sqlQuery = @"SELECT idCliente 
                                    FROM Liquidaciones.NotasEncabezado 
                                    WHERE idFactura IN (@idFactura)'";

                }
                else
                {
                    sqlQuery = @"SELECT idCliente 
                                    FROM Liquidaciones.mermasextrae 
                                    WHERE idFactura IN (@idFactura)'";
                }

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", idFactura);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        public string GetTotalNc(string idFactura, string ope)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();
                if (ope == "01" || ope == "04" || ope=="24" || ope == "74")
                {
                    sqlQuery = @"
                                    SELECT Total 
                                    FROM Liquidaciones.NotasEncabezado 
                                    WHERE idFactura IN (@idFactura)'";
                }
                else
                {
                    sqlQuery = @"
                                    SELECT Total 
                                    FROM Liquidaciones.mermasextrae 
                                    WHERE idFactura IN (@idFactura)'";
                }
                    

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", idFactura);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        public string GetSubTotalNc(string idFactura, string ope)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();
                if (ope == "01" || ope == "04" || ope=="24" || ope == "74")
                {
                    sqlQuery = @"
                                    SELECT SubTotal 
                                    FROM Liquidaciones.NotasEncabezado 
                                    WHERE idFactura IN (@idFactura)'";
                }
                else
                {
                    sqlQuery = @"
                                    SELECT SubTotal 
                                    FROM Liquidaciones.mermasextrae 
                                    WHERE idFactura IN (@idFactura)'";
                }

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", idFactura);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        public string GetIvaNc(string idFactura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"
                                    SELECT IVA 
                                    FROM Liquidaciones.NotasEncabezado 
                                    WHERE idFactura IN (@idFactura)'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", idFactura);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        public string GetPercepcionNc(string idFactura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"
                                    SELECT Percepcion 
                                    FROM Liquidaciones.NotasEncabezado 
                                    WHERE idFactura IN (@idFactura)'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", idFactura);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        public string GetCCFAnteriorNC(string idFactura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"
                                    SELECT Factura 
                                    FROM Liquidaciones.FacturasE 
                                    WHERE idFactura IN (@idFactura)'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", idFactura);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        //CAMPO FEL-OBTIENE CODIGO DE GENERACION
        public string GetCodigoGeneracion(int ruta, string fecha, string numero)
        {
            string data = "";
            //using (SqlConnection cnn = new SqlConnection(connectionString)) using (SqlConnection cnn = new SqlConnection(connectionString))
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT FELSerie 
                                    FROM Reparto.DocumentosFacturasEBajada
                                    WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha and Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }


        //OBTIENE NUIT CLIENTE
        public string GetNITCliente(int ruta, string numero)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT CD.DocumentoCliente 
									FROM  Reparto.DocumentosFacturasEBajada FEB
									INNER JOIN Clientes.Documentos CD ON FEB.IdCliente = CD.IdCliente 
									INNER JOIN Clientes.TiposDocumentos CT ON CT.IdTiposDocumento = CD.IdTiposDocumento 
									WHERE CD.IdTiposDocumento =1 AND idRuta=@ruta and Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        //OBTIENE DUI CLIENTE
        public string GetDUI(int ruta, string numero)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT CD.DocumentoCliente 
									FROM Reparto.DocumentosFacturasEBajada FEB
									INNER JOIN Clientes.Documentos CD ON FEB.IdCliente = CD.IdCliente 
									INNER JOIN Clientes.TiposDocumentos CT ON CT.IdTiposDocumento = CD.IdTiposDocumento 
									WHERE CD.IdTiposDocumento =2 AND idRuta=@ruta AND Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }

        //OBTIENE NRC CLIENTE
        public string GetNRC(int ruta, string fecha, string numero)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT CD.DocumentoCliente 
									FROM Reparto.DocumentosFacturasEBajada FEB
									INNER JOIN Clientes.Documentos CD ON FEB.IdCliente = CD.IdCliente 
									INNER JOIN Clientes.TiposDocumentos CT ON CT.IdTiposDocumento = CD.IdTiposDocumento 
									WHERE CD.IdTiposDocumento =3 AND idRuta=@ruta AND Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }


        //OBTIENE CANTIDAD TOTAL DE UNIDADES
        public double GetCantidadTotal(string idFactura)
        {
            string data = "";

            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT SUM(Unidades)
                                    FROM Liquidaciones.NotasEncabezado  
                                    WHERE idFactura IN (@idFactura)'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", idFactura);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return Convert.ToDouble(data);
        }

        //OBTIENE EL CCF ANTERIOR
        public string GetCCFAnterior(string ruta, string fecha, string numero)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                //string sqlQuery = @"SELECT idSerie
                //                                FROM Facturacion.Series 
                //                                WHERE idRuta=@ruta AND CAST(FechaIngreso AS DATE)=@fecha AND idTipoSerie=@idTipo ";
                string sqlQuery = @"SELECT idFacturaOriginal
									FROM Reparto.DocumentosFacturasEBajada 
									WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND Numero=@numero";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }
        /**********************DETALLE***********************/
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ruta"></param>
        /// <param name="numero"></param>
        /// <param name="fecha"></param>
        /// <returns></returns>

        //OBTIENE DETALLE POR FACTURAS
        public DataTable CantidadDetalle(int ruta, string numero, string fecha)
        {
            DataTable dt = new DataTable();

            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT MATNR
                                    FROM SAP.Liquidacion 
                                    WHERE ZOPERAC IN ('01', '04', '05')
                                    AND CAST(Fecha AS DATE) =@fecha AND idRuta =@ruta AND ZFE_CLAVE IS NULL ";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    SqlDataAdapter ds = new SqlDataAdapter(cmd);
                    ds.Fill(dt);
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }

            return dt;
        }


        //OBTIENE CANTIDAD DE UNIDADES POR CADA DETALLE
        public double GetCantidadDetalle(int ruta, string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT Unidades 
                                    FROM Reparto.DocumentosFacturasDBajada 
                                    WHERE idRuta=@ruta AND Numero=@numero AND IdProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@producto", producto);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return Convert.ToDouble(data);
        }


        //OBTIENE NOMBRE DEL PRODUCTO
        public string GetNombreProducto(string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT NombreCompleto 
                                    FROM SAP.Productos 
                                    WHERE idProducto = @producto";


                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@producto", producto);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return data;
        }


        //OBTIENE PESO DEL PRODUCTO 
        public double GetPesoProductoDetalle(int ruta, string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT Peso 
                                    FROM Reparto.DocumentosFacturasDBajada
                                    WHERE idRuta=@ruta AND Numero=@numero AND IdProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@producto", producto);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return Convert.ToDouble(data);
        }


        //OBTIENE PRECIO UNITARIO DEL DETALLE
        public decimal GetPrecioUnitarioDetalle(int ruta, string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT Precio 
                                    FROM Reparto.DocumentosFacturasDBajada 
                                    WHERE idRuta=@ruta AND Numero=@numero AND IdProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@producto", producto);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return Convert.ToDecimal(data);
        }

        //OBTIENE VENTAS GRAVADAS DETALLE
        public double GetVentasGravadasDetalle(int ruta, string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT SubTotal 
                                    FROM Reparto.DocumentosFacturasDBajada
                                    WHERE idRuta=@ruta AND Numero=@numero AND IdProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@producto", producto);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }

                cnn.Close();
            }

            return Convert.ToDouble(data);
        }

        //CAMPO FEL-OBTIENE IVA DE LA LINEA
        public decimal GetIVALineaFac(int ruta, string fecha, string numero, string producto)
        {
            string data = "";
            //using (SqlConnection cnn = new SqlConnection(connectionString)) using (SqlConnection cnn = new SqlConnection(connectionString))
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT ISNULL(Iva, 0) Iva
                                    FROM Reparto.DocumentosFacturasDBajada
                                    WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha and Numero=@numero AND IdProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    cmd.Parameters.AddWithValue("@numero", numero);
                    cmd.Parameters.AddWithValue("@producto", producto);
                    SqlDataReader dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        data = dr[0].ToString();
                    }
                }



                cnn.Close();
            }

            return Convert.ToDecimal(data);
        }
    }
}