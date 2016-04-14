using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.IO.Pipes;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace MsSQLKit {

	static class Program {
		
		public static QueryForm queryForm = null;

		/// <summary>
		/// Point d'entrée principal de l'application.
		/// </summary>
		[STAThread]
		static void Main()
		{

				Application.EnableVisualStyles();
				//Theme.applyTheme("Consolas", 9F, "#22282A", "#F1F2F3", "#4F6164");
				Application.SetCompatibleTextRenderingDefault(false);
				queryForm = new QueryForm();


				MsKitService pipeServer = new MsKitService();
				Task.Run(new Action(pipeServer.run));
				pipeServer.attachQueryForm(queryForm);

				Application.Run(queryForm);

				pipeServer.stop();
		}
	}
}
