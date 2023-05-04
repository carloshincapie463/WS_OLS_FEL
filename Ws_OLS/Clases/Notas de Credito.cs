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
		public DataTable CantidadNotasCredito(int ruta, string fecha, int facNum)
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
				if (facNum == -1)
				{
					sqlQuery = @"SELECT *
									FROM Reparto.DocumentosFacturasEBajada 
									WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND FELAutorizacion IS NULL";
				}
				else
				{
					sqlQuery = sqlQuery = @"SELECT *
									FROM Reparto.DocumentosFacturasEBajada 
									WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND Numero=@numero AND FELAutorizacion IS NULL";
				}

				using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
				{
					cmd.Parameters.AddWithValue("@ruta", ruta);
					cmd.Parameters.AddWithValue("@fecha", fecha);
					if (facNum != -1)
					{
						cmd.Parameters.AddWithValue("@numero", facNum);
					}
					SqlDataAdapter ds = new SqlDataAdapter(cmd);
					ds.Fill(dt);
					//dsSumario.Tables.Add(dt);
				}

				cnn.Close();
			}

			return dt;
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
		public double GetCantidadTotal(int ruta, string numero, string fecha)
		{
			string data = "";

			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				cnn.Open();
				string sqlQuery = @"SELECT SUM(Unidades)
                                    FROM Reparto.DocumentosFacturasDBajada 
                                    WHERE idRuta=@ruta AND Numero=@numero AND CAST(Fecha AS DATE)=@fecha";

				using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
				{
					cmd.Parameters.AddWithValue("@ruta", ruta);
					cmd.Parameters.AddWithValue("@numero", numero);
					cmd.Parameters.AddWithValue("@fecha", fecha);
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
				string sqlQuery = @"SELECT IdProductos
                                    FROM Reparto.DocumentosFacturasDBajada  
                                    WHERE idRuta=@ruta AND CAST(Fecha AS DATE)=@fecha AND Numero=@numero ";

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
	}
}