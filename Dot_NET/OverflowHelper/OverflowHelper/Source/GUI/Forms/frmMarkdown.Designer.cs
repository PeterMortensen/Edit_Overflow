namespace OverflowHelper.Forms
{
    partial class frmMarkdown
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
            this.label2 = new System.Windows.Forms.Label();
            this.txtLinkText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtURL = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtInlineMarkdown = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtShortMarkdown_text = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtShortMarkdown_reference = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtRefNumber = new System.Windows.Forms.TextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbnClipboardHTML = new System.Windows.Forms.RadioButton();
            this.rbnClipboardDontChange = new System.Windows.Forms.RadioButton();
            this.rbnClipboardInline = new System.Windows.Forms.RadioButton();
            this.rbnClipboardShort = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.txtHTML = new System.Windows.Forms.TextBox();
            this.chkEmphasise = new System.Windows.Forms.CheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportInlineFormat = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSelectLinkTextField = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSelectURLField = new System.Windows.Forms.ToolStripMenuItem();
            this.label8 = new System.Windows.Forms.Label();
            this.txtMediaWikiLink = new System.Windows.Forms.TextBox();
            this.lblMediaWikiLink_External = new System.Windows.Forms.Label();
            this.txtMediaWikiLink_External = new System.Windows.Forms.TextBox();
            this.lblWikipediaFamilyCrosslink = new System.Windows.Forms.Label();
            this.txtWikipediaFamilyCrosslink = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 54);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 17);
            this.label2.TabIndex = 200;
            this.label2.Text = "Lin&k text:";
            // 
            // txtLinkText
            // 
            this.txtLinkText.Location = new System.Drawing.Point(145, 50);
            this.txtLinkText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtLinkText.Name = "txtLinkText";
            this.txtLinkText.Size = new System.Drawing.Size(655, 22);
            this.txtLinkText.TabIndex = 201;
            this.txtLinkText.Text = "Iron";
            this.txtLinkText.TextChanged += new System.EventHandler(this.txtLinkText_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 94);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 17);
            this.label1.TabIndex = 202;
            this.label1.Text = "&URL:";
            // 
            // txtURL
            // 
            this.txtURL.Location = new System.Drawing.Point(145, 90);
            this.txtURL.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtURL.Name = "txtURL";
            this.txtURL.Size = new System.Drawing.Size(655, 22);
            this.txtURL.TabIndex = 203;
            this.txtURL.Text = "http://en.wikipedia.org/wiki/Iron";
            this.txtURL.TextChanged += new System.EventHandler(this.txtURL_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 350);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 17);
            this.label4.TabIndex = 306;
            this.label4.Text = "Inline &Markdown:";
            // 
            // txtInlineMarkdown
            // 
            this.txtInlineMarkdown.Location = new System.Drawing.Point(165, 346);
            this.txtInlineMarkdown.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtInlineMarkdown.Name = "txtInlineMarkdown";
            this.txtInlineMarkdown.ReadOnly = true;
            this.txtInlineMarkdown.Size = new System.Drawing.Size(635, 22);
            this.txtInlineMarkdown.TabIndex = 307;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 33);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(39, 17);
            this.label3.TabIndex = 302;
            this.label3.Text = "&Text:";
            // 
            // txtShortMarkdown_text
            // 
            this.txtShortMarkdown_text.Location = new System.Drawing.Point(124, 30);
            this.txtShortMarkdown_text.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtShortMarkdown_text.Name = "txtShortMarkdown_text";
            this.txtShortMarkdown_text.ReadOnly = true;
            this.txtShortMarkdown_text.Size = new System.Drawing.Size(655, 22);
            this.txtShortMarkdown_text.TabIndex = 303;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 84);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 17);
            this.label5.TabIndex = 304;
            this.label5.Text = "Re&ference:";
            // 
            // txtShortMarkdown_reference
            // 
            this.txtShortMarkdown_reference.Location = new System.Drawing.Point(124, 80);
            this.txtShortMarkdown_reference.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtShortMarkdown_reference.Name = "txtShortMarkdown_reference";
            this.txtShortMarkdown_reference.ReadOnly = true;
            this.txtShortMarkdown_reference.Size = new System.Drawing.Size(655, 22);
            this.txtShortMarkdown_reference.TabIndex = 305;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtShortMarkdown_reference);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.txtShortMarkdown_text);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(21, 203);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(801, 123);
            this.groupBox1.TabIndex = 301;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Short Markdown";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(733, 538);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(89, 30);
            this.btnCancel.TabIndex = 503;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(624, 539);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(89, 28);
            this.btnOK.TabIndex = 501;
            this.btnOK.Text = "&OK";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(20, 150);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(90, 17);
            this.label6.TabIndex = 204;
            this.label6.Text = "&Ref. number:";
            // 
            // txtRefNumber
            // 
            this.txtRefNumber.Location = new System.Drawing.Point(145, 146);
            this.txtRefNumber.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtRefNumber.Name = "txtRefNumber";
            this.txtRefNumber.Size = new System.Drawing.Size(33, 22);
            this.txtRefNumber.TabIndex = 205;
            this.txtRefNumber.Text = "1";
            this.txtRefNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(707, 144);
            this.btnGenerate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(95, 28);
            this.btnGenerate.TabIndex = 250;
            this.btnGenerate.Text = "&Generate";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbnClipboardHTML);
            this.groupBox2.Controls.Add(this.rbnClipboardDontChange);
            this.groupBox2.Controls.Add(this.rbnClipboardInline);
            this.groupBox2.Controls.Add(this.rbnClipboardShort);
            this.groupBox2.Location = new System.Drawing.Point(301, 127);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(389, 52);
            this.groupBox2.TabIndex = 207;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Change clipboard";
            // 
            // rbnClipboardHTML
            // 
            this.rbnClipboardHTML.AutoSize = true;
            this.rbnClipboardHTML.Location = new System.Drawing.Point(304, 21);
            this.rbnClipboardHTML.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbnClipboardHTML.Name = "rbnClipboardHTML";
            this.rbnClipboardHTML.Size = new System.Drawing.Size(67, 21);
            this.rbnClipboardHTML.TabIndex = 214;
            this.rbnClipboardHTML.Text = "HTM&L";
            this.rbnClipboardHTML.UseVisualStyleBackColor = true;
            // 
            // rbnClipboardDontChange
            // 
            this.rbnClipboardDontChange.AutoSize = true;
            this.rbnClipboardDontChange.Location = new System.Drawing.Point(20, 21);
            this.rbnClipboardDontChange.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbnClipboardDontChange.Name = "rbnClipboardDontChange";
            this.rbnClipboardDontChange.Size = new System.Drawing.Size(113, 21);
            this.rbnClipboardDontChange.TabIndex = 209;
            this.rbnClipboardDontChange.Text = "&Don\'t change";
            this.rbnClipboardDontChange.UseVisualStyleBackColor = true;
            // 
            // rbnClipboardInline
            // 
            this.rbnClipboardInline.AutoSize = true;
            this.rbnClipboardInline.Location = new System.Drawing.Point(229, 21);
            this.rbnClipboardInline.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbnClipboardInline.Name = "rbnClipboardInline";
            this.rbnClipboardInline.Size = new System.Drawing.Size(62, 21);
            this.rbnClipboardInline.TabIndex = 213;
            this.rbnClipboardInline.Text = "&Inline";
            this.rbnClipboardInline.UseVisualStyleBackColor = true;
            // 
            // rbnClipboardShort
            // 
            this.rbnClipboardShort.AutoSize = true;
            this.rbnClipboardShort.Checked = true;
            this.rbnClipboardShort.Location = new System.Drawing.Point(147, 21);
            this.rbnClipboardShort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbnClipboardShort.Name = "rbnClipboardShort";
            this.rbnClipboardShort.Size = new System.Drawing.Size(63, 21);
            this.rbnClipboardShort.TabIndex = 211;
            this.rbnClipboardShort.TabStop = true;
            this.rbnClipboardShort.Text = "&Short";
            this.rbnClipboardShort.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(20, 385);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.label7.Size = new System.Drawing.Size(50, 17);
            this.label7.TabIndex = 308;
            this.label7.Text = "&HTML:";
            // 
            // txtHTML
            // 
            this.txtHTML.Location = new System.Drawing.Point(165, 382);
            this.txtHTML.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtHTML.Name = "txtHTML";
            this.txtHTML.ReadOnly = true;
            this.txtHTML.Size = new System.Drawing.Size(635, 22);
            this.txtHTML.TabIndex = 309;
            // 
            // chkEmphasise
            // 
            this.chkEmphasise.AutoSize = true;
            this.chkEmphasise.Location = new System.Drawing.Point(195, 149);
            this.chkEmphasise.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkEmphasise.Name = "chkEmphasise";
            this.chkEmphasise.Size = new System.Drawing.Size(99, 21);
            this.chkEmphasise.TabIndex = 206;
            this.chkEmphasise.Text = "Em&phasise";
            this.chkEmphasise.UseVisualStyleBackColor = true;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(8, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(839, 28);
            this.menuStrip1.TabIndex = 505;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuImportInlineFormat,
            this.mnuExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // mnuImportInlineFormat
            // 
            this.mnuImportInlineFormat.Name = "mnuImportInlineFormat";
            this.mnuImportInlineFormat.Size = new System.Drawing.Size(212, 24);
            this.mnuImportInlineFormat.Text = "Import &inline format";
            this.mnuImportInlineFormat.Click += new System.EventHandler(this.mnuImportInlineFormat_Click);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(212, 24);
            this.mnuExit.Text = "Exit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSelectLinkTextField,
            this.mnuSelectURLField});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(47, 24);
            this.viewToolStripMenuItem.Text = "&Edit";
            // 
            // mnuSelectLinkTextField
            // 
            this.mnuSelectLinkTextField.Name = "mnuSelectLinkTextField";
            this.mnuSelectLinkTextField.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            this.mnuSelectLinkTextField.Size = new System.Drawing.Size(262, 24);
            this.mnuSelectLinkTextField.Text = "Select Link Text field";
            this.mnuSelectLinkTextField.Click += new System.EventHandler(this.mnuSelectLinkTextField_Click);
            // 
            // mnuSelectURLField
            // 
            this.mnuSelectURLField.Name = "mnuSelectURLField";
            this.mnuSelectURLField.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.U)));
            this.mnuSelectURLField.Size = new System.Drawing.Size(262, 24);
            this.mnuSelectURLField.Text = "Select URL field";
            this.mnuSelectURLField.Click += new System.EventHandler(this.mnuSelectURLField_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 421);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 17);
            this.label8.TabIndex = 310;
            this.label8.Text = "Media&Wiki:";
            // 
            // txtMediaWikiLink
            // 
            this.txtMediaWikiLink.Location = new System.Drawing.Point(165, 417);
            this.txtMediaWikiLink.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtMediaWikiLink.Name = "txtMediaWikiLink";
            this.txtMediaWikiLink.ReadOnly = true;
            this.txtMediaWikiLink.Size = new System.Drawing.Size(635, 22);
            this.txtMediaWikiLink.TabIndex = 311;
            // 
            // lblMediaWikiLink_External
            // 
            this.lblMediaWikiLink_External.AutoSize = true;
            this.lblMediaWikiLink_External.Location = new System.Drawing.Point(20, 457);
            this.lblMediaWikiLink_External.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMediaWikiLink_External.Name = "lblMediaWikiLink_External";
            this.lblMediaWikiLink_External.Size = new System.Drawing.Size(134, 17);
            this.lblMediaWikiLink_External.TabIndex = 506;
            this.lblMediaWikiLink_External.Text = "MediaWiki, e&xternal:";
            // 
            // txtMediaWikiLink_External
            // 
            this.txtMediaWikiLink_External.Location = new System.Drawing.Point(165, 453);
            this.txtMediaWikiLink_External.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtMediaWikiLink_External.Name = "txtMediaWikiLink_External";
            this.txtMediaWikiLink_External.ReadOnly = true;
            this.txtMediaWikiLink_External.Size = new System.Drawing.Size(635, 22);
            this.txtMediaWikiLink_External.TabIndex = 313;
            // 
            // lblWikipediaFamilyCrosslink
            // 
            this.lblWikipediaFamilyCrosslink.Location = new System.Drawing.Point(20, 482);
            this.lblWikipediaFamilyCrosslink.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWikipediaFamilyCrosslink.Name = "lblWikipediaFamilyCrosslink";
            this.lblWikipediaFamilyCrosslink.Size = new System.Drawing.Size(117, 38);
            this.lblWikipediaFamilyCrosslink.TabIndex = 508;
            this.lblWikipediaFamilyCrosslink.Text = "Wikipedia &family crosslink:";
            // 
            // txtWikipediaFamilyCrosslink
            // 
            this.txtWikipediaFamilyCrosslink.Location = new System.Drawing.Point(165, 489);
            this.txtWikipediaFamilyCrosslink.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtWikipediaFamilyCrosslink.Name = "txtWikipediaFamilyCrosslink";
            this.txtWikipediaFamilyCrosslink.ReadOnly = true;
            this.txtWikipediaFamilyCrosslink.Size = new System.Drawing.Size(635, 22);
            this.txtWikipediaFamilyCrosslink.TabIndex = 315;
            // 
            // frmMarkdown
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(839, 582);
            this.Controls.Add(this.lblWikipediaFamilyCrosslink);
            this.Controls.Add(this.txtWikipediaFamilyCrosslink);
            this.Controls.Add(this.lblMediaWikiLink_External);
            this.Controls.Add(this.txtMediaWikiLink_External);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtMediaWikiLink);
            this.Controls.Add(this.chkEmphasise);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtHTML);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtRefNumber);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtInlineMarkdown);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtURL);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLinkText);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmMarkdown";
            this.Text = "Markdown";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLinkText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtURL;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtInlineMarkdown;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtShortMarkdown_text;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtShortMarkdown_reference;
        private System.Windows.Forms.GroupBox groupBox1;
        internal System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtRefNumber;
        internal System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbnClipboardDontChange;
        private System.Windows.Forms.RadioButton rbnClipboardInline;
        private System.Windows.Forms.RadioButton rbnClipboardShort;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtHTML;
        private System.Windows.Forms.RadioButton rbnClipboardHTML;
        private System.Windows.Forms.CheckBox chkEmphasise;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.ToolStripMenuItem mnuImportInlineFormat;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectLinkTextField;
        private System.Windows.Forms.ToolStripMenuItem mnuSelectURLField;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtMediaWikiLink;
        private System.Windows.Forms.Label lblMediaWikiLink_External;
        private System.Windows.Forms.TextBox txtMediaWikiLink_External;
        private System.Windows.Forms.Label lblWikipediaFamilyCrosslink;
        private System.Windows.Forms.TextBox txtWikipediaFamilyCrosslink;
    }
}