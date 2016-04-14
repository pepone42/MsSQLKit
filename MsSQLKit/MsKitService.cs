using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace MsSQLKit {

	/// <summary>
	/// Represent an answer for the client
	/// </summary>
	public class MSKITReturn {
		public string message { get; set; }
		public MsKitService.Error errorId { get; set; }
		public List<String> data { get; set; }
	}

	/// <summary>
	/// represent a command send by the client.
	/// </summary>
	public class MSKITCommand {
		public string commandId { get; set; }
		public int tabId { get; set; }
		public string filename { get; set; }
		public int optionalMsgCount { get; set; }
		public string connectionString { get; set; }
		public int line { get; set; }
		public int col { get; set; }
		public MSKITReturn result { get; set; }
	}

	public class MSKITAnswer {
		public int tabId { get; set; }
		public string answerId { get; set; }
		public string message { get; set; }
		public MsKitService.Error errorId { get; set; }
		public List<Dictionary<string, object>> tableResults { get; set; }
	}

	public class MSKITTheme {
		public string FontFace { get; set; }
		public int FontSize { get; set; }
		public string Background { get; set; }
		public string Foreground { get; set; }
		public string Selection { get; set; }
	}

	// Proxy to our MainForm. Just a convenience so we d'ont have to use Invoke all the time.
	public class QueryFormProxy {
		private QueryForm query_;
		private QueryForm.ExecuteQueryScalarDelegate executeQueryScalar_;
		private QueryForm.AddTabDelegate addTab_;
		private QueryForm.AttachQueryDelegate attachQuery_;
		private QueryForm.ExecuteQueryBeginDelegate executeQueryBegin_;
		private QueryForm.ExecuteQueryUpdateDelegate executeQueryUpdate_;
		private QueryForm.ExecuteQueryEndDelegate executeQueryEnd_;
		//private QueryForm.ActivateDelegate activate_;
		private QueryForm.CloseTabDelegate closeTab_;
		private QueryForm.SelectTabDelegate selectTab_;
		private QueryForm.ChangeThemeDelegate changeTheme_;

		public QueryFormProxy(QueryForm q)
		{
			query_ = q;
			this.executeQueryScalar_ = new QueryForm.ExecuteQueryScalarDelegate(query_.ExecuteQueryScalar);
			this.addTab_ = new QueryForm.AddTabDelegate(query_.AddTab);
			this.attachQuery_ = new QueryForm.AttachQueryDelegate(query_.AttachQuery);
			this.executeQueryBegin_ = new QueryForm.ExecuteQueryBeginDelegate(query_.ExecuteQueryBegin);
			this.executeQueryUpdate_ = new QueryForm.ExecuteQueryUpdateDelegate(query_.ExecuteQueryUpdate);
			this.executeQueryEnd_ = new QueryForm.ExecuteQueryEndDelegate(query_.ExecuteQueryEnd);
			this.changeTheme_ = new QueryForm.ChangeThemeDelegate(query_.ChangeTheme);
			//this.activate_ = new QueryForm.ActivateDelegate(q_.Activate);
			this.closeTab_ = new QueryForm.CloseTabDelegate(query_.CloseTab);
			this.selectTab_ = new QueryForm.SelectTabDelegate(query_.SelectTab);
		}

		public Query GetQuery(int tabId)
		{
			return query_.GetQuery(tabId);
		}

		public string ExecuteQueryScalar(int tabId, string sql)
		{
			return (string)query_.Invoke(executeQueryScalar_, new object[] { tabId, sql });
		}
		public void AddTab(int tabId, string filename)
		{
			query_.Invoke(this.addTab_, new object[] { tabId, filename });
		}
		public void AttachQuery(int tabId, Query query)
		{
			query_.Invoke(this.attachQuery_, new object[] { tabId, query });
		}
		public void ExecuteQueryEnd(int tabId)
		{
			query_.Invoke(this.executeQueryEnd_, new object[] { tabId });
		}
		public void ExecuteQueryUpdate(int tabId)
		{
			query_.Invoke(this.executeQueryUpdate_, new object[] { tabId });
		}
		public void ExecuteQueryBegin(int tabId)
		{
			query_.Invoke(this.executeQueryBegin_, new object[] { tabId });
		}
		public void CloseTab(int tabId)
		{
			query_.Invoke(this.closeTab_, new object[] { tabId });
		}
		public void SelectTab(int tabId)
		{
			query_.Invoke(this.selectTab_, new object[] { tabId });
		}
		public void ChangeTheme(string fontFace, float font_size, string backGround, string foreGround, string selection)
		{
			query_.Invoke(this.changeTheme_, new object[] { fontFace, font_size, backGround, foreGround, selection });
		}

	}

	public class MsKitService {
		public enum Error {
			SUCCESS = 0,
			SQL_CONNECTION_ERROR,
			JSON_FORMAT_ERROR,
			SQL_EXECUTION_ERROR,
			TAB_OPERATION_ERROR,
			COMPLETE_ENGINE_ERROR
		}
		static private JavaScriptSerializer serializer = new JavaScriptSerializer();
		private NamedPipeServerStream pipeServer;
		private NamedPipeClientStream pipeClient;

		// Delegate to communicate with our main form
		private QueryFormProxy q_;

		static private MSKITReturn ok = new MSKITReturn { message = "OK", errorId = Error.SUCCESS };

		public MsKitService()
		{
			pipeServer = new NamedPipeServerStream("MsKitPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message);
			pipeClient = new NamedPipeClientStream(".", "MsKitPipeAns", PipeDirection.InOut);
		}

		/// <summary>
		/// Attach a Form to our pipeServer
		/// </summary>
		/// <param name="q"></param>
		public void attachQueryForm(QueryForm q)
		{
			q_ = new QueryFormProxy(q);
		}

		private void sendAck(MSKITReturn r)
		{
			string j = serializer.Serialize(r);
			pipeServer.Write(Encoding.UTF8.GetBytes(j), 0, Encoding.UTF8.GetByteCount(j));
		}

		private MSKITReturn processCommand(MSKITCommand cmd)
		{
			// Get optional Message, if any
			List<string> optionMsg = new List<string>();
			if (cmd.optionalMsgCount > 0) {
				for (int i = 0; i < cmd.optionalMsgCount; i++) {
					sendAck(ok);
					optionMsg.Add(getMessage());
				}
			}
			// Command processing logic
			switch (cmd.commandId) {
				case "CONNECT":
					try {
						if (cmd.connectionString == null)
							throw new ArgumentNullException("connectionString");
						if (cmd.filename == null)
							throw new ArgumentNullException("filename");

						var query = new Query(cmd.connectionString, cmd.tabId);
						q_.AddTab(cmd.tabId, cmd.filename);
						q_.AttachQuery(cmd.tabId, query);
					} catch (Exception ex) {
						return new MSKITReturn { message = ex.Message, errorId = Error.SQL_CONNECTION_ERROR };
					}
					break;
				case "EXECSQL":
					Task.Run(() => {
						string message;
						try {
							if (cmd.optionalMsgCount != 1)
								throw new ArgumentException("Command expect 1 optional message");
							string sql = optionMsg[0];
							q_.ExecuteQueryBegin(cmd.tabId);
							// The sql query is executed in this thread and can be canceled from the UI thread
							message = q_.GetQuery(cmd.tabId).Execute(sql);
						} catch (Exception ex) {
							sendAnswer(new MSKITAnswer {
								answerId = "EXECRESULT",
								tabId = cmd.tabId,
								errorId = Error.SQL_EXECUTION_ERROR,
								message = ex.Message
							});

							return;// new MSKITReturn { message = ex.Message, errorId = Error.SQL_EXECUTION_ERROR };
						} finally {
							q_.ExecuteQueryEnd(cmd.tabId);
						}
						sendAnswer(new MSKITAnswer {
							answerId = "EXECRESULT",
							tabId = cmd.tabId,
							errorId = Error.SUCCESS,
							message = message
						});
					});
					break;
				case "EXECTXTSQL":
					Task.Run(() => {
						string message;
						try {
							if (cmd.optionalMsgCount != 1)
								throw new ArgumentException("Command expect 1 optional message");
							string sql = optionMsg[0];
							q_.ExecuteQueryBegin(cmd.tabId);
							// The sql query is executed in this thread and can be canceled from the UI thread
							message = q_.GetQuery(cmd.tabId).Execute(sql);
						} catch (Exception ex) {
							sendAnswer(new MSKITAnswer {
								answerId = "EXECTXTRESULT",
								tabId = cmd.tabId,
								errorId = Error.SQL_EXECUTION_ERROR,
								message = ex.Message
							});

							return;// new MSKITReturn { message = ex.Message, errorId = Error.SQL_EXECUTION_ERROR };
						} finally {
							q_.ExecuteQueryEnd(cmd.tabId);
						}
						var list = new List<Dictionary<string, object>>();

						foreach (DataRow row in q_.GetQuery(cmd.tabId).QueryResults[0].Rows) {
							var dict = new Dictionary<string, object>();

							foreach (DataColumn col in q_.GetQuery(cmd.tabId).QueryResults[0].Columns) {
								dict[col.ColumnName] = row[col];
							}
							list.Add(dict);
						}
						sendAnswer(new MSKITAnswer {
							answerId = "EXECTXTRESULT",
							tabId = cmd.tabId,
							errorId = Error.SUCCESS,
							message = message,
							tableResults = list
						});
					});
					break;
				case "EXECSCALAR":
					try {
						if (cmd.optionalMsgCount != 1)
							throw new ArgumentException("Command expect 1 optional message");
						string sql = optionMsg[0];
						string result = q_.ExecuteQueryScalar(cmd.tabId, sql);
						return new MSKITReturn { message = result, errorId = Error.SUCCESS };
					} catch (Exception ex) {
						return new MSKITReturn { message = ex.Message, errorId = Error.SQL_EXECUTION_ERROR };
					}
					break;
				case "ADDTAB":
					break;
				case "CLOSETAB":
					try {
						Debug.WriteLine("Close Tab" + cmd.tabId.ToString());
						q_.CloseTab(cmd.tabId);
					} catch (Exception ex) {
						return new MSKITReturn { message = ex.Message, errorId = Error.TAB_OPERATION_ERROR };
					}
					break;
				case "SELECTTAB":
					try {
						Debug.WriteLine("Select Tab" + cmd.tabId.ToString());
						q_.SelectTab(cmd.tabId);
					} catch (Exception ex) {
						return new MSKITReturn { message = ex.Message, errorId = Error.TAB_OPERATION_ERROR };
					}
					break;
				case "THEME":
					try {
						if (cmd.optionalMsgCount != 1)
							throw new ArgumentException("Command expect 1 optional message");
						MSKITTheme t = serializer.Deserialize<MSKITTheme>(optionMsg[0]);
						q_.ChangeTheme(t.FontFace, t.FontSize, t.Background, t.Foreground, t.Selection);
					} catch (Exception ex) {
						return new MSKITReturn { message = ex.Message, errorId = Error.TAB_OPERATION_ERROR };
					}
					break;
				case "COMPLETE":
					List<String> completeList;
					try {
						if (cmd.optionalMsgCount != 1)
							throw new ArgumentException("Command expect 1 optional message");
						string sql = optionMsg[0];
						Query q = q_.GetQuery(cmd.tabId);
						q.Complete.ParseTsql(sql);
						completeList = q.Complete.CompleteAtPorsition(cmd.line, cmd.col);
					} catch (Exception ex) {
						return new MSKITReturn { errorId = Error.COMPLETE_ENGINE_ERROR, message = ex.Message };
					}
					return new MSKITReturn { errorId = 0, message = "OK", data = completeList };

					break;
				case "RETREIVEMETADATA":
					Task.Run(() => {
						try {
							if (cmd.optionalMsgCount != 1)
								throw new ArgumentException("Command expect 1 optional message");
							string sql = optionMsg[0];
							Query q = q_.GetQuery(cmd.tabId);
							q.Complete.ParseTsql(sql);
						} catch (Exception ex) {
							sendAnswer(new MSKITAnswer {
								answerId = "RETREIVEMETADATA",
								tabId = cmd.tabId,
								errorId = Error.COMPLETE_ENGINE_ERROR,
								message = ex.Message
							});
						}
						sendAnswer(new MSKITAnswer {
							answerId = "RETREIVEMETADATA",
							tabId = cmd.tabId,
							errorId = Error.SUCCESS,
							message = "MetaData OK"
						});
					});
					break;
				case "STOP":
					break;
				default:
					break;
			}
			return ok;
		}

		/// <summary>
		/// return string reprenseting a message sent by the client
		/// </summary>
		/// <returns></returns>
		private string getMessage()
		{
			byte[] b = new byte[4096];
			StringBuilder message = new StringBuilder();
			do {
				int size = pipeServer.Read(b, 0, 4096);
				message.Append(Encoding.UTF8.GetString(b, 0, size));
			} while (pipeServer.IsMessageComplete != true);
			Debug.WriteLine(message.ToString());
			return message.ToString();
		}

		/// <summary>
		/// read a Json message representing a MSKITCommand and return it
		/// Sending a ack is the responsibility of the caller
		/// </summary>
		/// <returns></returns>
		private MSKITCommand getCommand()
		{
			return serializer.Deserialize<MSKITCommand>(getMessage());
		}

		private string sendRawMessage(string message)
		{
			byte[] b = new byte[4096];
			pipeClient.Write(Encoding.UTF8.GetBytes(message), 0, Encoding.UTF8.GetByteCount(message));
			int size = pipeClient.Read(b, 0, 4096);
			return Encoding.UTF8.GetString(b, 0, size);
		}

		private MSKITReturn sendAnswer(MSKITAnswer cmd)
		{
			var result = sendRawMessage(serializer.Serialize(cmd));
			return serializer.Deserialize<MSKITReturn>(result);
		}

		/// <summary>
		/// Main processing loop
		/// </summary>
		public void run()
		{
			MSKITReturn returnCode;


			pipeClient.Connect();

			// Signal the client that we are alive
			sendAnswer(new MSKITAnswer { answerId = "ALIVE" });

			pipeServer.WaitForConnection();

			if (q_ == null)
				throw new ArgumentException("You must attach a query before you can process command");

			while (true) {
				try {
					MSKITCommand cmd = getCommand();
					returnCode = processCommand(cmd);
					Debug.WriteLine(cmd.tabId);
					Debug.WriteLine(cmd.commandId);

					// send answer
					sendAck(returnCode);
				} catch (Exception ex) {
					Debug.WriteLine("Malformed command message " + ex.GetType().ToString() + "  " + ex.Message);
					sendAck(new MSKITReturn { errorId = Error.JSON_FORMAT_ERROR, message = ex.Message });
				}

			}
		}

		public void stop()
		{
			sendAnswer(new MSKITAnswer {
				answerId = "STOP",
				tabId = 0,
				errorId = Error.SUCCESS,
				message = "please stop"
			});
		}
	}
}
