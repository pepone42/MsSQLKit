using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MsSQLKit {



	public partial class QueryForm : Form {
		Dictionary<int, Query> querys_;
		public delegate void ExecuteQueryDelegate(int tabId, string sql);
		public delegate string ExecuteQueryScalarDelegate(int tabId, string sql);
		public delegate void AddTabDelegate(int tabId,string filename);
		public delegate void AttachQueryDelegate(int tabId, Query query);
		public delegate void CloseTabDelegate(int tabId);
		public delegate void SelectTabDelegate(int tabId);
		public delegate void ActivateDelegate();
		public delegate void ExecuteQueryUpdateDelegate(int tabId);
		public delegate void ExecuteQueryEndDelegate(int tabId);
		public delegate void ExecuteQueryBeginDelegate(int tabId);
		public delegate void ChangeThemeDelegate(string fontFace, float font_size, string backGround, string foreGround, string selection);

		public QueryForm()
		{
			InitializeComponent();

			querys_ = new Dictionary<int, Query>();

			spidToolStripLabel.Text = "";
			filenameToolStripLabel.Text = "";
			rowCountToolStripLabel.Text = "";
			cancelToolStripButton.Enabled = false;
			
		}

		public void AddTab(int tabId,string filename)
		{
			if (!sqlQueryTabControl.TabPages.ContainsKey(tabId.ToString())) {
				sqlQueryTabControl.TabPages.Add(tabId.ToString(), Path.GetFileName(filename));

				var panel = new TableLayoutPanel();
				panel.BackColor = Theme.BackgroundColor;
				panel.Dock = DockStyle.Fill;
				panel.AutoScroll = true;

				panel.Margin = new System.Windows.Forms.Padding(0);
				panel.Padding = panel.Margin;
				sqlQueryTabControl.TabPages[tabId.ToString()].Controls.Add(panel);
			}
		}
		public void AttachQuery(int tabId, Query query)
		{
			if (sqlQueryTabControl.TabPages[tabId.ToString()].Tag != null) {
				// close current connection
				Query q = (Query)sqlQueryTabControl.TabPages[tabId.ToString()].Tag;
				q.Dispose();
			}
			spidToolStripLabel.Text = "(" + query.OwnerSpid.ToString() + ")";
			filenameToolStripLabel.Text = query.TabId.ToString();
			querys_[tabId] = query;
			sqlQueryTabControl.TabPages[tabId.ToString()].Tag = query;
			sqlQueryTabControl.SelectTab(tabId.ToString());
		}

		public void TabExecuteQuery(int tabId, string sql)
		{
			sqlQueryTabControl.TabPages[tabId.ToString()].Select();

			cancelToolStripButton.Enabled = true;
			querys_[tabId].Execute(sql);
		}

		public Query GetQuery(int tabId)
		{
			return querys_[tabId];
		}
		public void ExecuteQueryEnd(int tabId)
		{
			ExecuteQueryUpdate(tabId);
			cancelToolStripButton.Enabled = false;
			rowCountToolStripLabel.Text = "RowCount: " + querys_[tabId].TotalRowCount.ToString();
		}
		public void ExecuteQueryUpdate(int tabId)
		{
			if (!querys_.ContainsKey(tabId)) {
				throw new Exception("Tab " + tabId.ToString() + " has no DB connection attached");
			}
			if (!sqlQueryTabControl.TabPages.ContainsKey(tabId.ToString())) {
				throw new Exception("Tab " + tabId.ToString() + " does not exist");
			}
			Query q = querys_[tabId];

			Panel panel = (TableLayoutPanel)sqlQueryTabControl.TabPages[q.TabId.ToString()].Controls[0];

			foreach (var dt in q.QueryResults) {
				sqlQueryGridView grid = new sqlQueryGridView();
				panel.Controls.Add(grid);
				grid.SetDataSource(dt);
				if (q.QueryResults.Count == 1) {
					grid.Dock = DockStyle.Fill;
					grid.Margin = new Padding(0);
				} else {


					if (grid.getApproximateTotalHeight() < 500)
						grid.Height = grid.getApproximateTotalHeight() + 20;
					grid.Dock = DockStyle.Top;
				}
			}
		}
		public void ExecuteQueryBegin(int tabId)
		{
			if (!querys_.ContainsKey(tabId)) {
				throw new Exception("Tab " + tabId.ToString() + " has no DB connection attached");
			}
			if (!sqlQueryTabControl.TabPages.ContainsKey(tabId.ToString())) {
				throw new Exception("Tab " + tabId.ToString() + " does not exist");
			}
			sqlQueryTabControl.SelectTab(tabId.ToString());
			cancelToolStripButton.Enabled = true;
			Panel panel = (TableLayoutPanel)sqlQueryTabControl.TabPages[tabId.ToString()].Controls[0];
			panel.Controls.Clear();


		}
		public void ExecuteQuery(int tabId, string sql)
		{
			if (!querys_.ContainsKey(tabId)) {
				throw new Exception("Tab " + tabId.ToString() + " has no DB connection attached");
			}
			if (!sqlQueryTabControl.TabPages.ContainsKey(tabId.ToString())) {
				throw new Exception("Tab " + tabId.ToString() + " does not exist");
			}

			TabExecuteQuery(tabId, sql);
			sqlQueryTabControl.SelectTab(tabId.ToString());
		}
		public void CloseTab(int tabId)
		{
			Debug.WriteLine("Close tab " + tabId.ToString());
			if (sqlQueryTabControl.TabPages.ContainsKey(tabId.ToString())) {
				//throw new Exception("Tab " + filename + " does not exist");

				if (querys_.ContainsKey(tabId)) {
					querys_[tabId].Dispose();
					querys_.Remove(tabId);
				}
				sqlQueryTabControl.TabPages.RemoveByKey(tabId.ToString());
			}
		}
		public void SelectTab(int tabId)
		{
			if (sqlQueryTabControl.TabPages.ContainsKey(tabId.ToString())) {
				//throw new Exception("Tab " + filename + " does not exist");
				sqlQueryTabControl.SelectTab(tabId.ToString());
			}

		}


		private void sqlQueryTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			TabControl t = (TabControl)sender;
			if (t.SelectedTab != null) {
				Query q = (Query)t.SelectedTab.Tag;
				spidToolStripLabel.Text = q.OwnerSpid.ToString();
				filenameToolStripLabel.Text = q.TabId.ToString();
				cancelToolStripButton.Enabled = q.Running;
			} else {
				spidToolStripLabel.Text = "";
				filenameToolStripLabel.Text = "";
			}
		}

		public string ExecuteQueryScalar(int tabId, string sql)
		{
			if (!querys_.ContainsKey(tabId)) {
				throw new Exception("Tab " + tabId.ToString() + " has no DB connection attached");
			}
			if (!sqlQueryTabControl.TabPages.ContainsKey(tabId.ToString())) {
				throw new Exception("Tab " + tabId.ToString() + " does not exist");
			}
			object o = querys_[tabId].ExecuteScalar(sql);
			if (o != null && o.GetType() != typeof(DBNull))
				return o.ToString();
			else
				return "";
		}

		public void ChangeTheme(string fontFace, float font_size, string backGround, string foreGround, string selection)
		{
			Theme.applyTheme(fontFace, font_size, backGround, foreGround, selection);
			//toolStrip.BackColor = Theme.BackgroundColor;
			//toolStrip.ForeColor = Theme.ForegroundColor;

			//toolStrip.RenderMode = ToolStripRenderMode.ManagerRenderMode;
			//if (toolStrip.Renderer == null)
			//	toolStrip.Renderer = new CustomRenderer();
			//sqlQueryTabControl.DrawMode = TabDrawMode.OwnerDrawFixed;
		}


		private void cancelToolStripButton_Click(object sender, EventArgs e)
		{
			if (sqlQueryTabControl.SelectedTab != null) {
				Query q = (Query)sqlQueryTabControl.SelectedTab.Tag;
				if (q.Running) q.QueryCancel();
			}
		}

		//private void sqlQueryTabControl_DrawItem(object sender, DrawItemEventArgs e)
		//{

		//}

	}
}
