using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Ws_OLS.Clases
{
	public class Notas_de_Remision
	{


		string connectionString = ConfigurationManager.ConnectionStrings["IPES"].ConnectionString;

		//OBTIENE CANTIDAD DE FILAS POR RUTA Y FECHA DONDE SEA FACTURA
		public DataTable CantidadNotasRemision(int ruta, string fecha, int facNum)
		{
			DataTable dt = new DataTable();

			using (SqlConnection cnn = new SqlConnection(connectionString))
			{

				//string sqlQuery = @"SELECT TOP 1(IdRuta) AS Ruta, IdSerie, Correlativo, idProducto, UnidadesTotal, PesoTotal,
				//						   FechaGeneracion, FechaDescarga, FELAutorizacion
				//					FROM HandHeld.NotaRemisionBajada 
				//					WHERE idRuta=@ruta AND CAST(FechaGeneracion AS DATE)=@fecha AND FELAutorizacion IS NULL
				//					GROUP BY IdRuta, IdSerie, Correlativo, idProducto, UnidadesTotal, PesoTotal,
				//						   FechaGeneracion, FechaDescarga, FELAutorizacion";

				cnn.Open();
				string sqlQuery;
				if (facNum == -1)
				{
					sqlQuery = @"SELECT 
									DISTINCT (Correlativo),
									IdRuta,
									IdSerie
								FROM HandHeld.NotaRemisionBajada
								WHERE idRuta=@ruta AND CAST(FechaGeneracion AS DATE)=@fecha AND FELAutorizacion IS NULL";
				}
				else
				{
					sqlQuery = @"SELECT 
									DISTINCT (Correlativo),
									IdRuta,
									IdSerie
								FROM HandHeld.NotaRemisionBajada
								WHERE idRuta=@ruta AND CAST(FechaGeneracion AS DATE)=@fecha AND Correlativo=@correlativo AND FELAutorizacion IS NULL";
				}

				using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
				{
					cmd.Parameters.AddWithValue("@ruta", ruta);
					cmd.Parameters.AddWithValue("@fecha", fecha);
					if (facNum != -1)
					{
						cmd.Parameters.AddWithValue("@correlativo", facNum);
					}
					SqlDataAdapter ds = new SqlDataAdapter(cmd);
					ds.Fill(dt);
					//dsSumario.Tables.Add(dt);
				}

				cnn.Close();
			}

			return dt;
		}


		//OBTIENE CANTIDAD TOTAL DE UNIDADES
		public double GetCantidadTotal(int ruta, string numero, string fecha)
		{
			string data = "";

			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				cnn.Open();
				string sqlQuery = @"SELECT SUM(UnidadesTotal)
                                    FROM HandHeld.NotaRemisionBajada  
                                    WHERE idRuta=@ruta AND Correlativo=@numero AND CAST(FechaGeneracion AS DATE)=@fecha";

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


		/**********DETALLE********************/

		//OBTIENE CANTIDAD DE LINEAS DE DETALLE
		public DataTable CantidadDetalle(int ruta, string corr, string fecha)
		{
			DataTable dt = new DataTable();

			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				cnn.Open();
				string sqlQuery = @"SELECT idProducto
                                    FROM HandHeld.NotaRemisionBajada 
                                    WHERE idRuta=@ruta AND CAST(FechaGeneracion AS DATE)=@fecha AND Correlativo=@corr";

				using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
				{
					cmd.Parameters.AddWithValue("@ruta", ruta);
					cmd.Parameters.AddWithValue("@fecha", fecha);
					cmd.Parameters.AddWithValue("@corr", corr);
					SqlDataAdapter ds = new SqlDataAdapter(cmd);
					ds.Fill(dt);
					//dsSumario.Tables.Add(dt);
				}

				cnn.Close();
			}

			return dt;
		}

		//OBTIENE CANTIDAD DE UNIDADES POR CADA DETALLE
		public double GetCantidadDetalle(int ruta, string corr, string producto)
		{
			string data = "";
			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				cnn.Open();
				string sqlQuery = @"SELECT UnidadesTotal 
                                    FROM HandHeld.NotaRemisionBajada 
                                    WHERE idRuta=@ruta AND Correlativo=@corr AND idProducto=@producto";

				using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
				{
					cmd.Parameters.AddWithValue("@ruta", ruta);
					cmd.Parameters.AddWithValue("@corr", corr);
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


		//OBTIENE PESO DEL PRODUCTO 
		public double GetPesoProductoDetalle(int ruta, string corr, string producto)
		{
			string data = "";
			using (SqlConnection cnn = new SqlConnection(connectionString))
			{
				cnn.Open();
				string sqlQuery = @"SELECT PesoTotal
                                    FROM HandHeld.NotaRemisionBajada
                                    WHERE idRuta=@ruta AND Correlativo=@corr AND idProducto=@producto";

				using (SqlCommand cmd = new SqlCommand(sqlQuery, cnn))
				{
					cmd.Parameters.AddWithValue("@ruta", ruta);
					cmd.Parameters.AddWithValue("@corr", corr);
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