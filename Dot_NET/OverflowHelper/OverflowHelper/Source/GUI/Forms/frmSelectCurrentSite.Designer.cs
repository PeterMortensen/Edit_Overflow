namespace OverflowHelper
{
    partial class frmSelectCurrentSite
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbSites = new System.Windows.Forms.ComboBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblNote_HiddenControls_HIDDEN = new System.Windows.Forms.Label();
            this.btnExportLlistToClipboard = new System.Windows.Forms.Button();
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cbSites
            // 
            this.cbSites.FormattingEnabled = true;
            this.cbSites.ItemHeight = 16;
            this.cbSites.Location = new System.Drawing.Point(109, 52);
            this.cbSites.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbSites.MaxDropDownItems = 30;
            this.cbSites.Name = "cbSites";
            this.cbSites.Size = new System.Drawing.Size(461, 24);
            this.cbSites.TabIndex = 0;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(457, 107);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 28);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(557, 107);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 28);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(36, 55);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "Site:";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(664, 41);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 4;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(699, 92);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "label2";
            // 
            // lblNote_HiddenControls_HIDDEN
            // 
            this.lblNote_HiddenControls_HIDDEN.AutoSize = true;
            this.lblNote_HiddenControls_HIDDEN.BackColor = System.Drawing.Color.Red;
            this.lblNote_HiddenControls_HIDDEN.Location = new System.Drawing.Point(135, 11);
            this.lblNote_HiddenControls_HIDDEN.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNote_HiddenControls_HIDDEN.Name = "lblNote_HiddenControls_HIDDEN";
            this.lblNote_HiddenControls_HIDDEN.Size = new System.Drawing.Size(310, 17);
            this.lblNote_HiddenControls_HIDDEN.TabIndex = 6;
            this.lblNote_HiddenControls_HIDDEN.Text = "Note 1: There are hidden controls on the right...";
            this.lblNote_HiddenControls_HIDDEN.Visible = false;
            // 
            // btnExportLlistToClipboard
            // 
            this.btnExportLlistToClipboard.Location = new System.Drawing.Point(196, 107);
            this.btnExportLlistToClipboard.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnExportLlistToClipboard.Name = "btnExportLlistToClipboard";
            this.btnExportLlistToClipboard.Size = new System.Drawing.Size(187, 28);
            this.btnExportLlistToClipboard.TabIndex = 7;
            this.btnExportLlistToClipboard.Text = "Export list to cli&pboard";
            this.btnExportLlistToClipboard.UseVisualStyleBackColor = true;
            this.btnExportLlistToClipboard.Click += new System.EventHandler(this.btnExportLlistToClipboard_Click);
            // 
            // lblNote_NoSitesDirectlyInDesigner_HIDDEN
            // 
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.AutoSize = true;
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.BackColor = System.Drawing.Color.Red;
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.Location = new System.Drawing.Point(135, 31);
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.Name = "lblNote_NoSitesDirectlyInDesigner_HIDDEN";
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.Size = new System.Drawing.Size(540, 17);
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.TabIndex = 8;
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.Text = "Note 2: Control \'cbSites\' is now filled in programmatically, not directly in the " +
    "designer.";
            this.lblNote_NoSitesDirectlyInDesigner_HIDDEN.Visible = false;
            // 
            // frmSelectCurrentSite
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(653, 150);
            this.Controls.Add(this.lblNote_NoSitesDirectlyInDesigner_HIDDEN);
            this.Controls.Add(this.btnExportLlistToClipboard);
            this.Controls.Add(this.lblNote_HiddenControls_HIDDEN);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.cbSites);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmSelectCurrentSite";
            this.Text = "Select Current Site";
            this.Load += new System.EventHandler(this.frmSelectCurrentSite_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbSites;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblNote_HiddenControls_HIDDEN;
        private System.Windows.Forms.Button btnExportLlistToClipboard;
        private System.Windows.Forms.Label lblNote_NoSitesDirectlyInDesigner_HIDDEN;
    }
}