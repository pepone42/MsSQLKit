using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MsSQLKit {
	class Theme {
		public static System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyleHeaders = new System.Windows.Forms.DataGridViewCellStyle();
		public static System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		private static Color backgroundColor;

		public static Color BackgroundColor
		{
			get { return Theme.backgroundColor; }
			set { 
				Theme.backgroundColor = value;
				Theme.backgroundColorDark = ControlPaint.Dark(Theme.backgroundColor, 0.2F);
				Theme.backgroundColorLight = ControlPaint.Light(Theme.backgroundColor, 0.2F);
			}
		}
		private static Color foregroundColor;

		public static Color ForegroundColor
		{
			get { return Theme.foregroundColor; }
			set { 
				Theme.foregroundColor = value;
				Theme.foregroundColorDark = ControlPaint.Dark(Theme.foregroundColor, 0.2F);
				Theme.foregroundColorLight = ControlPaint.Light(Theme.foregroundColor, 0.2F);
			}
		}
		private static Color selectionColor;

		public static Color SelectionColor
		{
			get { return Theme.selectionColor; }
			set { 
				Theme.selectionColor = value;
				Theme.selectionColorDark = ControlPaint.Dark(Theme.selectionColor, 0.2F);
				Theme.selectionColorLight = ControlPaint.Light(Theme.selectionColor, 0.2F);
			}
		}

		public static Color backgroundColorDark;
		public static Color foregroundColorDark;
		public static Color selectionColorDark;
		
		public static Color backgroundColorLight;
		public static Color foregroundColorLight;
		public static Color selectionColorLight;


		static Theme()
		{
			// default theme

			dataGridViewCellStyleHeaders.Font = SystemFonts.MessageBoxFont;
			dataGridViewCellStyleHeaders.NullValue = null;
			dataGridViewCellStyleHeaders.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			dataGridViewCellStyle.Font = SystemFonts.MessageBoxFont;
			dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		}

		public static void applyTheme(string fontFace, float font_size, string backGround, string foreGround, string selection)
		{
			BackgroundColor = System.Drawing.ColorTranslator.FromHtml(backGround);
			ForegroundColor = System.Drawing.ColorTranslator.FromHtml(foreGround);
			SelectionColor = System.Drawing.ColorTranslator.FromHtml(selection);

			



			dataGridViewCellStyleHeaders.Font = new System.Drawing.Font(fontFace, font_size, FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyleHeaders.NullValue = null;
			dataGridViewCellStyleHeaders.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			dataGridViewCellStyleHeaders.ForeColor = foregroundColor;
			dataGridViewCellStyleHeaders.BackColor = backgroundColorDark;
			dataGridViewCellStyleHeaders.SelectionBackColor = selectionColorDark;


			dataGridViewCellStyle.Font = new System.Drawing.Font(fontFace, font_size, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			dataGridViewCellStyle.ForeColor = foregroundColor;
			dataGridViewCellStyle.BackColor = backgroundColor;
			dataGridViewCellStyle.SelectionBackColor = selectionColor;
			dataGridViewCellStyle.SelectionForeColor = foregroundColor;

		}
	}
}
