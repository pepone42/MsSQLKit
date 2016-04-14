using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MsSQLKit {
	class SqlServer {
		private int spid_;
		private SqlConnection connection_;

		public SqlServer(string connectionString)
		{
			connection_ = new SqlConnection(connectionString);
			connection_.Open();
		}

		public DataTable QueryDataTable(string sql)
		{
			DataTable dt = new DataTable();
			SqlDataReader reader = null;
			try {
				SqlCommand cmd = new SqlCommand(sql, connection_);
				reader = cmd.ExecuteReader();
				dt.Load(reader);
				reader.Close();
			} catch (Exception ex) {
				throw ex;
			} finally {
				if (reader != null) reader.Close();
			}
			return dt;
		}
	}
}
