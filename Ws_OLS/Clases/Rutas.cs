using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace Ws_OLS.Clases
{
	public class Rutas
	{
		//private Database Db;

		//public Rutas()
		//{
		//	Db = DatabaseFactory.CreateDatabase();
		//}

		//public int rutaPedido(int route)
		//{
		//	StringBuilder st = new StringBuilder();

		//	st.AppendLine("SELECT 0 idRuta , 'Seleccione' Ruta, '' OficinaVenta UNION ALL  ");
		//	st.AppendLine("(SELECT CodigoCompatibilidad idRuta, CONVERT(VARCHAR(6),CodigoCompatibilidad), OficinaVenta FROM catalogos.canaldistribucion) ORDER BY idRuta  ");
		//	using (DbCommand Cm = Db.GetSqlStringCommand(st.ToString()))
		//	{
		//		Db.ExecuteNonQuery(Cm);
		//		return Db.GetParameterValue(Cm, "ruta");

		//	}
		//}
	}
}