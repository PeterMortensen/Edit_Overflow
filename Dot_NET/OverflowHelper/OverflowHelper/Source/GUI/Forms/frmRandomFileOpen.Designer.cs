namespace OverflowHelper.Forms
{
    partial class frmRandomFileOpen
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOpenRandomFile = new System.Windows.Forms.Button();
            this.txtCurrentFile = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtInputFolder = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtFileExtensionsFilter = new System.Windows.Forms.TextBox();
            this.btnUseAudioSet = new System.Windows.Forms.Button();
            this.btnUseVideoSet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(661, 282);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(89, 28);
            this.btnOK.TabIndex = 504;
            this.btnOK.Text = "&OK";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(771, 281);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(89, 30);
            this.btnCancel.TabIndex = 505;
            this.btnCancel.Text = "&Cancel";
            // 
            // btnOpenRandomFile
            // 
            this.btnOpenRandomFile.Location = new System.Drawing.Point(691, 161);
            this.btnOpenRandomFile.Margin = new System.Windows.Forms.Padding(4);
            this.btnOpenRandomFile.Name = "btnOpenRandomFile";
            this.btnOpenRandomFile.Size = new System.Drawing.Size(165, 28);
            this.btnOpenRandomFile.TabIndex = 506;
            this.btnOpenRandomFile.Text = "Open &Random File";
            this.btnOpenRandomFile.Click += new System.EventHandler(this.btnOpenRandomFile_Click);
            // 
            // txtCurrentFile
            // 
            this.txtCurrentFile.Location = new System.Drawing.Point(108, 218);
            this.txtCurrentFile.Margin = new System.Windows.Forms.Padding(4);
            this.txtCurrentFile.Name = "txtCurrentFile";
            this.txtCurrentFile.ReadOnly = true;
            this.txtCurrentFile.Size = new System.Drawing.Size(561, 22);
            this.txtCurrentFile.TabIndex = 508;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-48, 175);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 17);
            this.label3.TabIndex = 507;
            this.label3.Text = "&Text:";
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Location = new System.Drawing.Point(691, 215);
            this.btnCopyToClipboard.Margin = new System.Windows.Forms.Padding(4);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(165, 28);
            this.btnCopyToClipboard.TabIndex = 509;
            this.btnCopyToClipboard.Text = "Co&py to Clipboard";
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 222);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 17);
            this.label1.TabIndex = 510;
            this.label1.Text = "Current &File:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 17);
            this.label2.TabIndex = 512;
            this.label2.Text = "Input &Filter:";
            // 
            // txtInputFolder
            // 
            this.txtInputFolder.Location = new System.Drawing.Point(164, 33);
            this.txtInputFolder.Margin = new System.Windows.Forms.Padding(4);
            this.txtInputFolder.Name = "txtInputFolder";
            this.txtInputFolder.Size = new System.Drawing.Size(421, 22);
            this.txtInputFolder.TabIndex = 511;
            this.txtInputFolder.Text = "C:\\VeryLarge";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 80);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(141, 17);
            this.label4.TabIndex = 514;
            this.label4.Text = "File &Extensions Filter:";
            // 
            // txtFileExtensionsFilter
            // 
            this.txtFileExtensionsFilter.Location = new System.Drawing.Point(164, 76);
            this.txtFileExtensionsFilter.Margin = new System.Windows.Forms.Padding(4);
            this.txtFileExtensionsFilter.Name = "txtFileExtensionsFilter";
            this.txtFileExtensionsFilter.Size = new System.Drawing.Size(421, 22);
            this.txtFileExtensionsFilter.TabIndex = 513;
            this.txtFileExtensionsFilter.Text = "*.mp4;*.m4v;*.ogv;*.wmv;*.wma;*.mov;*.m4a;*.flv";
            // 
            // btnUseAudioSet
            // 
            this.btnUseAudioSet.Location = new System.Drawing.Point(617, 68);
            this.btnUseAudioSet.Margin = new System.Windows.Forms.Padding(4);
            this.btnUseAudioSet.Name = "btnUseAudioSet";
            this.btnUseAudioSet.Size = new System.Drawing.Size(113, 28);
            this.btnUseAudioSet.TabIndex = 515;
            this.btnUseAudioSet.Text = "Use audio set";
            this.btnUseAudioSet.Click += new System.EventHandler(this.btnUseAudioSet_Click);
            // 
            // btnUseVideoSet
            // 
            this.btnUseVideoSet.Location = new System.Drawing.Point(617, 110);
            this.btnUseVideoSet.Margin = new System.Windows.Forms.Padding(4);
            this.btnUseVideoSet.Name = "btnUseVideoSet";
            this.btnUseVideoSet.Size = new System.Drawing.Size(113, 28);
            this.btnUseVideoSet.TabIndex = 516;
            this.btnUseVideoSet.Text = "Use video set";
            this.btnUseVideoSet.Click += new System.EventHandler(this.btnUseVideoSet_Click);
            // 
            // frmRandomFileOpen
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(876, 325);
            this.Controls.Add(this.btnUseVideoSet);
            this.Controls.Add(this.btnUseAudioSet);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtFileExtensionsFilter);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtInputFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.txtCurrentFile);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnOpenRandomFile);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmRandomFileOpen";
            this.Text = "Opening af Random File";
            this.Load += new System.EventHandler(this.frmRandomFileOpen_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Button btnOK;
        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnOpenRandomFile;
        private System.Windows.Forms.TextBox txtCurrentFile;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.Button btnCopyToClipboard;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtInputFolder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtFileExtensionsFilter;
        internal System.Windows.Forms.Button btnUseAudioSet;
        internal System.Windows.Forms.Button btnUseVideoSet;
    }
}