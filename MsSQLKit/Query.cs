using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MsSQLKit {

	public class binary {
		public string to_string()
		{
			return "varbinary";
		}
	}

	public class Query : IDisposable {
		private int ownerSpid_;

		public int OwnerSpid
		{
			get { return ownerSpid_; }
		}
		private int tabId_;

		public int TabId
		{
			get { return tabId_; }
		}
		//private string name_;

		//public string Name
		//{
		//	get { return name_; }
		//	//set { name_ = value; }
		//}
		//private DataTable queryResult_;
		private List<DataTable> queryResult_;

		public DataTable QueryResult
		{
			get { return queryResult_[0]; }
		}

		public List<DataTable> QueryResults
		{
			get { return queryResult_; }
		}

		public int TotalRowCount
		{
			get
			{
				int totalCount = 0;
				foreach (var dt in queryResult_) {
					totalCount += dt.Rows.Count;
				}
				return totalCount;
			}
		}
		private SqlConnection connection_;
		private bool running_ = false;
		private bool cancelPending_ = false;
		private CompleteEngine complete;

		internal CompleteEngine Complete
		{
			get { return complete; }
			//set { complete = value; }
		}


		public bool Running
		{
			get { return running_; }
		}

		string sqlMessage_;

		private int getSpid()
		{
			var cmd = new SqlCommand("select cast(@@spid as int)", connection_);
			int? id = (int?)cmd.ExecuteScalar();
			if (id.HasValue) {
				return (int)id;
			} else {
				return -1;
			}
		}

		public Query(string connectionString, int tabId)
		{
			connection_ = new SqlConnection(connectionString);
			connection_.InfoMessage += connection_InfoMessage;
			connection_.Open();
			ownerSpid_ = getSpid();
			tabId_ = tabId;
			//if (tabId_ != null)
			//	name_ = Path.GetFileName(tabId_);
			complete = new CompleteEngine(connection_);
			//queryResult_ = new List<DataTable>();
		}

		void connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			// Todo: use a string builder
			sqlMessage_ += "\n" + e.Message;
		}


		public string Execute(String sql)
		{
			sqlMessage_="";
			queryResult_ = null;
			queryResult_ = new List<DataTable>();

			running_ = true;
			List<string> sql_batch=new List<string>();
			string last_batch=null;
			var batches = Regex.Split(sql, @"^\s*(GO(?:\s+[0-9]+)?)\s*(?:--.*)?$", RegexOptions.Multiline | RegexOptions.IgnoreCase );
			foreach (var batch in batches) {
				// Execute the last batch exactly one time
				if (String.Compare(batch, "GO", true) == 0) {
					if (!String.IsNullOrEmpty(last_batch))
						sql_batch.Add(last_batch);
					continue;
				} else {
					var match=Regex.Match(batch,@"^GO\s+([0-9]+)$",RegexOptions.IgnoreCase);
					// Execute the last batch exactly N times
					if (match.Success) {
						for(int i=0;i<Int32.Parse(match.Groups[1].Value);i++)
							sql_batch.Add(last_batch);
					} else {
						last_batch = batch;
					}
				}
			}
			if (!String.IsNullOrEmpty(last_batch))
				sql_batch.Add(last_batch);

			foreach (var s in sql_batch) {
				SqlCommand cmd = new SqlCommand(s, connection_);
				cmd.CommandTimeout = 0;

				using (var reader = cmd.ExecuteReader()) {
					do {
						DataTable dt = new DataTable();
						List<DataColumn> listCols = new List<DataColumn>();
						List<String> colsName = new List<string>();
						DataTable dtSchema;
						dtSchema = reader.GetSchemaTable();
						// create our schemas
						if (dtSchema != null) {
							foreach (DataRow drow in dtSchema.Rows) {
								string columnName = System.Convert.ToString(drow["ColumnName"]);
								int i = 1;
								while (colsName.Contains(columnName) == true) {
									columnName = System.Convert.ToString(drow["ColumnName"]) + i.ToString();
									i++;
								}
								colsName.Add(columnName);
								Type t = (Type)(drow["DataType"]);
								if (t == System.Type.GetType("System.Boolean"))
									t = System.Type.GetType("System.Int32");
								if (t == System.Type.GetType("System.Byte[]"))
									t = System.Type.GetType("System.String");
								DataColumn column = new DataColumn(columnName, t);
								column.Unique = (bool)drow["IsUnique"];
								column.AllowDBNull = (bool)drow["AllowDBNull"];
								column.AutoIncrement = (bool)drow["IsAutoIncrement"];
								listCols.Add(column);
								dt.Columns.Add(column);
							}
						}
						// Read rows from DataReader and populate the DataTable
						while (reader.Read()) {
							DataRow dataRow = dt.NewRow();
							for (int i = 0; i < listCols.Count; i++) {
								Type t = reader[i].GetType();
								if (t == System.Type.GetType("System.Byte[]"))
									dataRow[((DataColumn)listCols[i])] = "0x"+BitConverter.ToString((byte[])reader[i]).Replace("-",string.Empty);
								else
									dataRow[((DataColumn)listCols[i])] = reader[i];
							}
							dt.Rows.Add(dataRow);
							// todo
							if (cancelPending_ == true) {

								cmd.Cancel();
								cmd.Dispose();
								cancelPending_ = false;
								running_ = false;
								return "Canceld";
							}
						}
						if (dt.Columns.Count>0)
							queryResult_.Add(dt);
					} while (reader.NextResult());
					cmd.Dispose();
				}
				cancelPending_ = false;
				running_ = false;
			}
			return sqlMessage_;
		}

		public void Dispose()
		{
			if (connection_.State != ConnectionState.Closed)
				connection_.Close();
			connection_.Dispose();
			//queryResult_.Dispose();
		}

		public object ExecuteScalar(string sql)
		{
			SqlCommand cmd = new SqlCommand(sql, connection_);
			return cmd.ExecuteScalar();

		}

		/* Cancel la query en cours */
		public void QueryCancel()
		{
			cancelPending_ = true;

		}
	}
}
