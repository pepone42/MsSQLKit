using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.SmoMetadataProvider;
using Microsoft.SqlServer.Management.SqlParser;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.SqlServer.Management.SqlParser.Binder;
using Microsoft.SqlServer.Management.SqlParser.MetadataProvider;
using Microsoft.SqlServer.Management.SqlParser.Intellisense;
using System.Diagnostics;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using System.Threading.Tasks;
using Microsoft.SqlServer.Management.Smo;

namespace MsSQLKit {
	/**
	 * POC implementation of a complete engine using the SqlServer Intellisense Parser.
	 * needs Microsoft.SqlServer.Management.SmoMetadataProvider and Microsoft.SqlServer.Management.SqlParser assembly
	 * */
	class CompleteEngine {
		private class CustomMetadataDisplayInfoProvider : IMetadataDisplayInfoProvider {
			public CustomMetadataDisplayInfoProvider()
			{
			}

			public CasingStyle BuiltInCasing { get; set; }

			public string CollectionToString<T>(IMetadataCollection<T> metadataCollection, bool singleLine) where T : class, IMetadataObject
			{
				throw new NotImplementedException();
			}
			public string CollectionToString<T>(IMetadataOrderedCollection<T> metadataCollection, bool singleLine) where T : class, IMetadataObject
			{
				throw new NotImplementedException();
			}
			public string GetDatabaseQualifiedName(IMetadataObject metadataObject)
			{
				throw new NotImplementedException();
			}
			public string GetDescription(IMetadataObject metadataObject)
			{
				return string.Format("{0}\t{1}",metadataObject.Name ,metadataObject.GetType().Name);
			}
			public string GetDisplayName(IMetadataObject metadataObject)
			{
				return metadataObject.Name;
			}
			public string ObjectToString(IMetadataObject metadataObject)
			{
				throw new NotImplementedException();
			}
		}

		private class CompleteServer {
			public ServerConnection Connection { get; set; }
			public SmoMetadataProvider MetaDataProvider { get; set; }
			public IBinder InterfaceBinder { get; set; }


			public CompleteServer(SqlConnection connection)
			{
				Connection = new ServerConnection(new SqlConnection(connection.ConnectionString));
				Server s = new Server(Connection);
				//MetaDataProvider = SmoMetadataProvider.CreateConnectedProvider(Connection);
				MetaDataProvider = SmoMetadataProvider.CreateDisconnectedProvider(s);
				InterfaceBinder = BinderProvider.CreateBinder(MetaDataProvider);
			}
		}

		ParseResult pResult;

		static private Dictionary<string, CompleteServer> server = new Dictionary<string, CompleteServer>();
		static CustomMetadataDisplayInfoProvider metadataDisplayInfoProvider = new CustomMetadataDisplayInfoProvider();

		CompleteServer bindingServer;
		bool isBinding = false;

		public CompleteEngine(SqlConnection sql)
		{
			Debug.WriteLine("New Complete server " + sql.ConnectionString);
			if (!server.ContainsKey(sql.ConnectionString)) {
				server[sql.ConnectionString] = new CompleteServer(sql);
			}
			bindingServer = server[sql.ConnectionString];

		}

		private void parse_and_bind(string sql)
		{
			if (!isBinding) {
				isBinding = true;
				try {
					Debug.WriteLine("Begin Full Parse" + DateTime.Now.ToString("HH:mm:ss ff"));
					pResult = Parser.IncrementalParse(sql, pResult);
					Debug.WriteLine("End Full Parse, begin Bind" + DateTime.Now.ToString("HH:mm:ss ff"));
					bindingServer.InterfaceBinder.Bind(new List<ParseResult>() { pResult }, bindingServer.Connection.DatabaseName, BindMode.Batch);
					Debug.WriteLine("Bind" + DateTime.Now.ToString("HH:mm:ss ff"));
				} catch (Exception ex) {
					Debug.WriteLine("Exception " + ex.Message);
				} finally {
					isBinding = false;
				}
			}
		}
		private void bind()
		{
			if (!isBinding) {
				isBinding = true;
				try {
					bindingServer.InterfaceBinder.Bind(new List<ParseResult>() { pResult }, bindingServer.Connection.DatabaseName, BindMode.Batch);
					Debug.WriteLine("End Bind " + DateTime.Now.ToString("HH:mm:ss ff"));
				} catch (Exception ex) {
					Debug.WriteLine("Exception " + ex.Message);
				} finally {
					isBinding = false;
				}
			}
		}
		
		public List<string> ParseTsql(string sql)
		{
			Debug.WriteLine("Begin Full Parse " + DateTime.Now.ToString("HH:mm:ss ff"));
			pResult = Parser.IncrementalParse(sql, pResult);
			Debug.WriteLine("End Full Parse, begin Bind " + DateTime.Now.ToString("HH:mm:ss ff"));

			bind();
			
			var err = new List<string>();
			foreach (var error in pResult.Errors) {
				err.Add(String.Format("{0}¤{1}¤{2}¤{3}", error.Message, error.Start.LineNumber,error.Start.ColumnNumber,error.IsWarning));
			}
			return err;

		}
		public List<string> CompleteAtPorsition(int line, int col)
		{
			if (isBinding || pResult == null) return null;

			Debug.WriteLine("Begin Completion " + DateTime.Now.ToString("HH:mm:ss ff"));
			Debug.WriteLine("Token "+pResult.GetTokenNumber(line,col).ToString());
			var declarations = Resolver.FindCompletions(pResult, line, col, metadataDisplayInfoProvider);
			Debug.WriteLine("End Completion " + DateTime.Now.ToString("HH:mm:ss ff"));
			Debug.WriteLine("Found " + declarations.Count + " possible declaration");
			List<string> r = new List<string>();
			foreach (var d in declarations) {
				r.Add(d.Description);
			}

			Debug.WriteLine("return " + r.Count + " possible declaration");
			return r;
		}
	}
}
