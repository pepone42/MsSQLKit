using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MsSQLKit {
	/**
	 * Prototype to use a custom themed tabcontrol.
	 * Not used for now.
	 */
	public partial class NCE_TabControl : TabControl {
		Rectangle TabBoundary;
		RectangleF TabTextBoundary;
		StringFormat format = new StringFormat(); //for tab header text

		public NCE_TabControl()
		{
			
			//this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
			//this.DrawMode = TabDrawMode.OwnerDrawFixed;


			this.format.Alignment = StringAlignment.Center;
			this.format.LineAlignment = StringAlignment.Center;
			this.Margin = new Padding(0);
			this.Padding = new Point(0);
			this.Multiline = false;
		}

		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			Graphics g = pevent.Graphics;
			g.FillRectangle(new SolidBrush(Theme.backgroundColorDark), 0, 0, this.Size.Width, this.Size.Height);
			g.FillRectangle(new SolidBrush(Theme.BackgroundColor), this.DisplayRectangle);
			g.DrawRectangle(new Pen(Theme.foregroundColorDark, 2), this.DisplayRectangle.X - 1, this.DisplayRectangle.Y - 1
				, this.DisplayRectangle.Width + 2, this.DisplayRectangle.Height +2);

			foreach (TabPage tp in this.TabPages) {
				//drawItem
				int index = this.TabPages.IndexOf(tp);

				this.TabBoundary = this.GetTabRect(index);
				this.TabTextBoundary = (RectangleF)this.GetTabRect(index);
				if (this.SelectedIndex == index)
					g.FillRectangle(new SolidBrush(Theme.SelectionColor), this.TabBoundary);
				else
					g.FillRectangle(new SolidBrush(Theme.BackgroundColor), this.TabBoundary);
				g.DrawRectangle(new Pen(Theme.foregroundColorDark), this.TabBoundary);
				g.DrawString(tp.Text, this.Font, new SolidBrush(Theme.ForegroundColor), this.TabTextBoundary, format);
			}
		}
	}
}
