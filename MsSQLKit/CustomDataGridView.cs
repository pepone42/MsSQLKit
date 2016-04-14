using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MsSQLKit {
	class CustomDataGridView : DataGridView {
		public CustomDataGridView()
		{
			DoubleBuffered = true;
		}
	}
}
