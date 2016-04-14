using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace MsSQLKit {
	public partial class sqlQueryGridView : UserControl {

		public sqlQueryGridView()
		{
			InitializeComponent();

			dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			dataGridView.ReadOnly = true;
			dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			dataGridView.RowTemplate.Height = 20;
			dataGridView.RowTemplate.ReadOnly = true;
			dataGridView.TabIndex = 0;

			// Custom painting for null and row numbering
			dataGridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.MyCellPainting);
			dataGridView.MouseDown += new MouseEventHandler(this.dataGridView_MouseDown);

			// Custom Header style
			
			dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			dataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
			this.dataGridView.ColumnHeadersDefaultCellStyle = Theme.dataGridViewCellStyleHeaders;
			this.dataGridView.RowHeadersDefaultCellStyle = Theme.dataGridViewCellStyleHeaders;
			this.dataGridView.ColumnHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			this.dataGridView.RowHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;


			// custom cell style
			
			dataGridView.CellBorderStyle = DataGridViewCellBorderStyle.Single;
			this.dataGridView.DefaultCellStyle = Theme.dataGridViewCellStyle;
			this.dataGridView.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;

			if (Theme.ForegroundColor != Color.Empty)
				dataGridView.GridColor = Theme.foregroundColorDark;//System.Drawing.ColorTranslator.FromHtml("#42464C");
			if (Theme.BackgroundColor != Color.Empty)
				dataGridView.BackgroundColor = Theme.BackgroundColor;//System.Drawing.ColorTranslator.FromHtml("#22282A");
			
			// Auto Resize handeling
			// dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
			dataGridView.AllowUserToResizeColumns = true;
			this.Margin = new Padding(0,0,0,20);

			dataGridView.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

		}

		public int getApproximateTotalHeight() {
			return dataGridView.Rows.GetRowsHeight(DataGridViewElementStates.None) + dataGridView.ColumnHeadersHeight;
			//return dataGridView.ColumnHeadersHeight * dataGridView.RowCount;
			
		}

		private void dataGridView_MouseDown(object sender, MouseEventArgs e)
		{
			System.Windows.Forms.DataGridView.HitTestInfo hti = dataGridView.HitTest(e.X, e.Y);
			if (hti.RowIndex == -1 && hti.ColumnIndex >= 0 && e.Button == System.Windows.Forms.MouseButtons.Right) {
			} else if (hti.ColumnIndex == -1 && hti.RowIndex >= 0) {
				// row header click
				if (dataGridView.SelectionMode != DataGridViewSelectionMode.RowHeaderSelect) {
					dataGridView.SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
				}
			} else if (hti.RowIndex == -1 && hti.ColumnIndex >= 0) {
				// column header click
				if (dataGridView.SelectionMode != DataGridViewSelectionMode.ColumnHeaderSelect) {
					dataGridView.SelectionMode = DataGridViewSelectionMode.ColumnHeaderSelect;
				}
			}
		}

		void MyCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			DataGridView dv = (DataGridView)sender;
			
			if (e.ColumnIndex == -1 && e.RowIndex != -1) {
				//e.Graphics.FillRectangle(Brushes.Blue, e.CellBounds);
				e.Paint(e.ClipBounds, (DataGridViewPaintParts.Background | DataGridViewPaintParts.Border | DataGridViewPaintParts.SelectionBackground));
				TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(), dv.RowHeadersDefaultCellStyle.Font,
									  e.CellBounds, e.CellStyle.ForeColor,
									  TextFormatFlags.PreserveGraphicsClipping |
									  TextFormatFlags.VerticalCenter |
									  TextFormatFlags.Left);

				e.Handled = true;
			}

			if (e.Value == e.CellStyle.DataSourceNullValue) {
				Color c = Color.Gray;
				e.PaintBackground(e.ClipBounds, true);
				TextRenderer.DrawText(e.Graphics, "null", dv.DefaultCellStyle.Font, e.CellBounds, c,
									  TextFormatFlags.PreserveGraphicsClipping |
									  TextFormatFlags.VerticalCenter |
									  TextFormatFlags.Left);
				e.Handled = true;
			}
		}
		public void AutoResizeRowHeader()
		{
			// ajuste la taille du rowheader selon le rowcount
			using (Graphics g = CreateGraphics()) {

				SizeF size = g.MeasureString(dataGridView.RowCount.ToString(),
											 dataGridView.DefaultCellStyle.Font,
											 495);
				dataGridView.RowHeadersWidth = (int)Math.Ceiling(size.Width);
				dataGridView.RowHeadersWidth += 5;
			}
		}
		public void SetDataSource(object ds)
		{
			dataGridView.DataSource = ds;
			dataGridView.AutoGenerateColumns = true;
			foreach (DataGridViewColumn col in dataGridView.Columns) {
				col.SortMode = DataGridViewColumnSortMode.NotSortable;
			}
			dataGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
		}

		private void dataGridView_ColumnDividerDoubleClick(object sender, DataGridViewColumnDividerDoubleClickEventArgs e)
		{
			dataGridView.AutoResizeColumn(e.ColumnIndex, DataGridViewAutoSizeColumnMode.DisplayedCells);
			e.Handled = true;
		}

	}

	static public class Util {
		static public T Find<T>(Control container) where T : Control
		{
			foreach (Control child in container.Controls)
				return (child is T ? (T)child : Find<T>(child));
			// Not found.
			return null;
		}
	}
}
