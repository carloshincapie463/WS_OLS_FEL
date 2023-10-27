using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Ws_OLS.Clases
{
    public class Notas_de_Credito
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["IPES"].ConnectionString;

        //OBTIENE CANTIDAD DE FILAS POR RUTA Y FECHA DONDE SEA FACTURA
        public DataTable CantidadNotasCredito(int ruta, string fecha, string facnum)
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
                //sqlQuery = @"SELECT *
                //                FROM SAP.Liquidacion
                //                WHERE ZOPERAC IN ('01', '04', '05', '24', '74')
                //                AND CAST(Fecha AS DATE) =@fecha AND idRuta =@ruta AND ZFE_CLAVE IS NULL";
                if (facnum == "-1")
                {
                    sqlQuery = @"SELECT *
                                FROM SAP.Liquidacion
                                WHERE ZOPERAC IN ('01', '04', '05', '24', '74')
                                AND CAST(Fecha AS DATE) =@fecha AND idRuta =@ruta AND ZFE_CLAVE IS NULL";
                }
                else
                {
                    //sqlQuery = @"SELECT *
                    //            FROM SAP.Liquidacion
                    //            WHERE ZOPERAC IN ('01', '04', '05', '24', '74')
                    //            //AND CAST(Fecha AS DATE) =@fecha AND idRuta =@ruta AND ZNROCF=@factnum AND ZFE_CLAVE IS NULL";

                    sqlQuery = @"SELECT  serie + RIGHT('00000000' + LTRIM(RTRIM(STR(numeroformulario))), 8) as ZNROCF,
                               IdFactura AS VBELNF, 
                               IdCliente AS Vendedor, *
                               FROM  Liquidaciones.NotasEncabezado e
                                WHERE CAST(e.Fecha AS DATE) =@fecha AND e.Ruta =@ruta and MARCAFEL=1 AND FELAutorizacion IS NULL
                                AND  serie + RIGHT('00000000' + LTRIM(RTRIM(STR(numeroformulario))), 8)=@factnum";
                }

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@ruta", ruta);
                    cmd.Parameters.AddWithValue("@fecha", fecha);
                    if (facnum != "-1")
                    {
                        cmd.Parameters.AddWithValue("@factnum", facnum);
                    }

                    SqlDataAdapter ds = new SqlDataAdapter(cmd);
                    ds.Fill(dt);
                    //dsSumario.Tables.Add(dt);
                }

                cnn.Close();
            }

            return dt;
        }

        public string GetResolucionNC(string factura)
        {
            string resolucion = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();

                sqlQuery = @"SELECT resolucion
                                    FROM Facturacion.Series
                                    WHERE numeroSerie=@factura AND estado ='ACT'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@factura", factura);
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
        public string GetResInicioNC(string factura)
        {
            string resInicio = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT numeroDel
                                    FROM Facturacion.Series
                                    WHERE numeroSerie=@factura AND estado ='ACT'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@factura", factura);
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
        public string GetResFinNC(string factura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT numeroAl
                                    FROM Facturacion.Series
                                    WHERE numeroSerie=@factura AND estado ='ACT'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@factura", factura);
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
        public string GetRestFechaNC(string factura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT FechaAutorizacion
                                    FROM Facturacion.Series
                                    WHERE numeroSerie=@factura AND estado ='ACT'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@factura", factura);
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
        public string GetNumSerieNC(string factura)
        {
            string data = "";
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT Serie
                                    FROM Liquidaciones.NotasEncabezado
                                    WHERE Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(NumeroFormulario))), 8)='@factura'";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@factura", factura);
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

        //OBTIENE NOMBRE DEL USUARIO
        public string GetNombreUsuario(string id)
        {
            string data = "";
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT nombreEmpleado
									FROM EmpleadosHH
									WHERE CodigoAs=@id";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
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

        public string GetCorrelativoInterno(string idFactura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();

                sqlQuery = @"SELECT NumeroFormulario
                                 FROM Liquidaciones.NotasEncabezado
                                 WHERE Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(NumeroFormulario))), 8)=@idFactura";

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

        public string GetClienteNC(string idFactura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();

                sqlQuery = @"SELECT ISNULL(fe.idClienteDestinatario ,n.idCliente )
                            FROM Liquidaciones.NotasEncabezado N 
                            LEFT JOIN Liquidaciones.FacturasE fe  on n.idFactura =fe.idFactura
                            WHERE n.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(NumeroFormulario))), 8)=@idFactura";

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

        public string GetTotalNc(string idFactura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();
                sqlQuery = @"
                                    SELECT Total
                                    FROM Liquidaciones.NotasEncabezado
                                    WHERE Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(NumeroFormulario))), 8)=@idFactura";

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

        public string GetSubTotalNc(string idFactura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                string sqlQuery = "";
                cnn.Open();

                sqlQuery = @"
                                    SELECT SubTotal
                                    FROM Liquidaciones.NotasEncabezado
                                    WHERE Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(NumeroFormulario))), 8)=@idFactura";

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
                                    WHERE Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(NumeroFormulario))), 8)=@idFactura";

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

        public decimal GetIVALineaNc(string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"
                                    SELECT D.IVA
                                    FROM Liquidaciones.NotasDetalle D
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Numero = D.Numero
                                    WHERE E.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(E.NumeroFormulario))), 8)=@idFactura AND D.idProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", numero);
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
                                    WHERE Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(NumeroFormulario))), 8)=@idFactura";

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
                string sqlQuery = @"SELECT FF.FELSerie
                                    FROM Facturacion.FacturaE FF
                                    INNER JOIN Liquidaciones.FacturasE LE ON CONVERT(BIGINT,FF.FACTURA)=CONVERT(BIGINT,LE.FACTURA)
                                    INNER JOIN Liquidaciones.NotasEncabezado N ON N.idFactura = LE.idFactura
                                    AND FF.idCliente=LE.idClienteDestinatario
                                    AND LE.idRutaReparto=FF.idRutaReparto
                                    AND N.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(N.NumeroFormulario))), 8)=@idFactura
                                    UNION ALL
                                    SELECT FF.FELSerie
                                    FROM handheld.FacturaEBajada FF
                                    INNER JOIN Liquidaciones.FacturasE LE ON CONVERT(BIGINT,FF.numero)=CONVERT(BIGINT,LE.FACTURA)
                                    INNER JOIN Liquidaciones.NotasEncabezado N ON N.idFactura = LE.idFactura
                                    AND FF.idCliente=LE.idClienteDestinatario
                                    AND LE.idRutaReparto=FF.IdRuta
                                    AND N.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(N.NumeroFormulario))), 8)=@idFactura";

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

        public DateTime GetDocfec(string idFactura)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT CONVERT(DATE,FF.FechaFactura) Fecha
                                    FROM Facturacion.FacturaE FF
                                    INNER JOIN Liquidaciones.FacturasE LE ON CONVERT(BIGINT,FF.FACTURA)=CONVERT(BIGINT,LE.FACTURA)
                                    INNER JOIN Liquidaciones.NotasEncabezado N ON N.idFactura = LE.idFactura
                                    AND FF.idCliente=LE.idClienteDestinatario
                                    AND LE.idRutaReparto=FF.idRutaReparto
                                    AND N.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(N.NumeroFormulario))), 8)=@idfactura
                                    --and n.Fecha=converT(Date,getdate())
                                    union
                                    SELECT CONVERT(DATE,FF.Fecha) Fecha
                                    FROM handheld.FacturaEBajada FF
                                    INNER JOIN Liquidaciones.FacturasE LE ON CONVERT(BIGINT,FF.numero)=CONVERT(BIGINT,LE.FACTURA)
                                    INNER JOIN Liquidaciones.NotasEncabezado N ON N.idFactura = LE.idFactura
                                    AND FF.idCliente=LE.idClienteDestinatario
                                    AND LE.idRutaReparto=FF.IdRuta
                                    AND N.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(N.NumeroFormulario))), 8)=@idfactura";

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

            return Convert.ToDateTime(data);
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
                string sqlQuery = @"SELECT SUM(D.Unidades)
                                    FROM Liquidaciones.NotasDetalle D
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Numero = D.Numero
                                    WHERE E.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(E.NumeroFormulario))), 8)=@idFactura";

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
                //string sqlQuery = @"SELECT MATNR
                //                    FROM SAP.Liquidacion
                //                    WHERE ZOPERAC IN ('01', '04', '05', '24', '74')
                //                    AND CAST(Fecha AS DATE) =@fecha AND idRuta =@ruta AND ZNROCF=@numero AND ZFE_CLAVE IS NULL ";

                string sqlQuery = @"SELECT idProductos AS MATNR
                                   FROM  Liquidaciones.NotasEncabezado e  INNER JOIN Liquidaciones.NotasDetalle D ON E.Numero=D.Numero
                                   WHERE CAST(e.Fecha AS DATE) =@fecha AND e.Ruta =@ruta and MARCAFEL=1 AND FELAutorizacion IS NULL
                                   AND serie + RIGHT('00000000' + LTRIM(RTRIM(STR(numeroformulario))), 8)=@numero";

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
        public double GetCantidadDetalle(string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT D.Unidades
                                    FROM Liquidaciones.NotasDetalle D
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Numero = D.Numero
                                    WHERE E.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(E.NumeroFormulario))), 8)=@idFactura AND D.idProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", numero);
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
        public double GetPesoProductoDetalle(string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT D.Peso
                                    FROM Liquidaciones.NotasDetalle D
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Numero = D.Numero
                                    WHERE E.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(E.NumeroFormulario))), 8)=@idFactura AND D.idProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", numero);
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

        public double GetUnidadesProductoDetalle(string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT D.Unidades
                                    FROM Liquidaciones.NotasDetalle D
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Numero = D.Numero
                                    WHERE E.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(E.NumeroFormulario))), 8)=@idFactura AND D.idProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", numero);
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
        public decimal GetPrecioUnitarioDetalle(string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT D.Precio
                                    FROM Liquidaciones.NotasDetalle D
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Numero = D.Numero
                                    WHERE E.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(E.NumeroFormulario))), 8)=@idFactura AND D.idProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", numero);
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
        public double GetVentasGravadasDetalle(string numero, string producto)
        {
            string data = "";
            using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT D.SubTotal
                                    FROM Liquidaciones.NotasDetalle D
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Numero = D.Numero
                                    WHERE E.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(E.NumeroFormulario))), 8)=@idFactura AND D.idProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", numero);
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
        public decimal GetIVALineaFac(string numero, string producto)
        {
            string data = "";
            //using (SqlConnection cnn = new SqlConnection(connectionString)) using (SqlConnection cnn = new SqlConnection(connectionString))
            using (SqlConnection cnn = new SqlConnection(connectionString))
            //using (SqlConnection cnn = new SqlConnection(connectionString))
            {
                cnn.Open();
                string sqlQuery = @"SELECT ISNULL(IVA, 0) IVA
                                    FROM Liquidaciones.NotasDetalle D
                                    INNER JOIN Liquidaciones.NotasEncabezado E ON E.Numero = D.Numero
                                    WHERE E.Serie  + RIGHT('00000000' + LTRIM(RTRIM(STR(E.NumeroFormulario))), 8)=@idFactura AND D.idProductos=@producto";

                using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
                {
                    cmd.Parameters.AddWithValue("@idFactura", numero);
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