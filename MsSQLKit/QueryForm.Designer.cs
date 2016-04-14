namespace MsSQLKit {
	partial class QueryForm {
		/// <summary>
		/// Variable nécessaire au concepteur.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Nettoyage des ressources utilisées.
		/// </summary>
		/// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form

		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryForm));
			this.toolStrip = new System.Windows.Forms.ToolStrip();
			this.cancelToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.rowCountToolStripLabel = new System.Windows.Forms.ToolStripLabel();
			this.spidToolStripLabel = new System.Windows.Forms.ToolStripLabel();
			this.filenameToolStripLabel = new System.Windows.Forms.ToolStripLabel();
			this.sqlQueryTabControl = new MsSQLKit.NCE_TabControl();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// toolStrip
			// 
			this.toolStrip.BackColor = System.Drawing.SystemColors.Control;
			this.toolStrip.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cancelToolStripButton,
            this.toolStripSeparator1,
            this.rowCountToolStripLabel,
            this.spidToolStripLabel,
            this.filenameToolStripLabel});
			this.toolStrip.Location = new System.Drawing.Point(0, 431);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(0);
			this.toolStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip.Size = new System.Drawing.Size(918, 25);
			this.toolStrip.TabIndex = 1;
			this.toolStrip.Text = "toolStrip1";
			// 
			// cancelToolStripButton
			// 
			this.cancelToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.cancelToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("cancelToolStripButton.Image")));
			this.cancelToolStripButton.Name = "cancelToolStripButton";
			this.cancelToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.cancelToolStripButton.Text = "Cancel";
			this.cancelToolStripButton.ToolTipText = "Cancel running query";
			this.cancelToolStripButton.Click += new System.EventHandler(this.cancelToolStripButton_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// rowCountToolStripLabel
			// 
			this.rowCountToolStripLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.rowCountToolStripLabel.Name = "rowCountToolStripLabel";
			this.rowCountToolStripLabel.Size = new System.Drawing.Size(59, 22);
			this.rowCountToolStripLabel.Text = "rowCount";
			// 
			// spidToolStripLabel
			// 
			this.spidToolStripLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.spidToolStripLabel.Name = "spidToolStripLabel";
			this.spidToolStripLabel.Size = new System.Drawing.Size(29, 22);
			this.spidToolStripLabel.Text = "spid";
			// 
			// filenameToolStripLabel
			// 
			this.filenameToolStripLabel.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
			this.filenameToolStripLabel.Name = "filenameToolStripLabel";
			this.filenameToolStripLabel.Size = new System.Drawing.Size(51, 22);
			this.filenameToolStripLabel.Text = "filename";
			// 
			// sqlQueryTabControl
			// 
			this.sqlQueryTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sqlQueryTabControl.Location = new System.Drawing.Point(0, 0);
			this.sqlQueryTabControl.Margin = new System.Windows.Forms.Padding(0);
			this.sqlQueryTabControl.Multiline = true;
			this.sqlQueryTabControl.Name = "sqlQueryTabControl";
			this.sqlQueryTabControl.Padding = new System.Drawing.Point(0, 0);
			this.sqlQueryTabControl.SelectedIndex = 0;
			this.sqlQueryTabControl.ShowToolTips = true;
			this.sqlQueryTabControl.Size = new System.Drawing.Size(918, 431);
			this.sqlQueryTabControl.TabIndex = 2;
			this.sqlQueryTabControl.SelectedIndexChanged += new System.EventHandler(this.sqlQueryTabControl_SelectedIndexChanged);
			// 
			// QueryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(918, 456);
			this.Controls.Add(this.sqlQueryTabControl);
			this.Controls.Add(this.toolStrip);
			this.Name = "QueryForm";
			this.Text = "MsSQLKit";
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip;
		private NCE_TabControl sqlQueryTabControl;
		private System.Windows.Forms.ToolStripButton cancelToolStripButton;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripLabel rowCountToolStripLabel;
		private System.Windows.Forms.ToolStripLabel spidToolStripLabel;
		private System.Windows.Forms.ToolStripLabel filenameToolStripLabel;
	}
}

