namespace RecoveryStar
{
    partial class MainForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.menuStrip = new System.Windows.Forms.MenuStrip();
			this.����ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.�����ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.�����������ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.������������������ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.���������ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.���������������ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.�����������������ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.�������ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.������������ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.separatorToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
			this.����������ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.coderConfigGroupBox = new System.Windows.Forms.GroupBox();
			this.redundancyGroupBox = new System.Windows.Forms.GroupBox();
			this.redundancyMacTrackBar = new EConTech.Windows.MACUI.MACTrackBar();
			this.allVolCountGroupBox = new System.Windows.Forms.GroupBox();
			this.allVolCountMacTrackBar = new EConTech.Windows.MACUI.MACTrackBar();
			this.toolTip = new System.Windows.Forms.ToolTip(this.components);
			this.testButton = new System.Windows.Forms.Button();
			this.repairButton = new System.Windows.Forms.Button();
			this.recoverButton = new System.Windows.Forms.Button();
			this.protectButton = new System.Windows.Forms.Button();
			this.browser = new FileBrowser.Browser();
			this.menuStrip.SuspendLayout();
			this.coderConfigGroupBox.SuspendLayout();
			this.redundancyGroupBox.SuspendLayout();
			this.allVolCountGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip
			// 
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.����ToolStripMenuItem,
            this.�����������ToolStripMenuItem,
            this.���������ToolStripMenuItem,
            this.�������ToolStripMenuItem});
			this.menuStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Size = new System.Drawing.Size(986, 23);
			this.menuStrip.TabIndex = 0;
			this.menuStrip.Text = "menuStrip";
			// 
			// ����ToolStripMenuItem
			// 
			this.����ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.�����ToolStripMenuItem});
			this.����ToolStripMenuItem.Name = "����ToolStripMenuItem";
			this.����ToolStripMenuItem.Size = new System.Drawing.Size(48, 19);
			this.����ToolStripMenuItem.Text = "����";
			// 
			// �����ToolStripMenuItem
			// 
			this.�����ToolStripMenuItem.Image = global::RecoveryStar.Properties.Resources.Exit;
			this.�����ToolStripMenuItem.Name = "�����ToolStripMenuItem";
			this.�����ToolStripMenuItem.Size = new System.Drawing.Size(108, 22);
			this.�����ToolStripMenuItem.Text = "�����";
			this.�����ToolStripMenuItem.Click += new System.EventHandler(this.�����ToolStripMenuItem_Click);
			// 
			// �����������ToolStripMenuItem
			// 
			this.�����������ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.������������������ToolStripMenuItem});
			this.�����������ToolStripMenuItem.Name = "�����������ToolStripMenuItem";
			this.�����������ToolStripMenuItem.Size = new System.Drawing.Size(95, 19);
			this.�����������ToolStripMenuItem.Text = "�����������";
			// 
			// ������������������ToolStripMenuItem
			// 
			this.������������������ToolStripMenuItem.Image = global::RecoveryStar.Properties.Resources.StartBenchmark;
			this.������������������ToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.������������������ToolStripMenuItem.Name = "������������������ToolStripMenuItem";
			this.������������������ToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
			this.������������������ToolStripMenuItem.Text = "���� ��������������";
			this.������������������ToolStripMenuItem.Click += new System.EventHandler(this.������������������ToolStripMenuItem_Click);
			// 
			// ���������ToolStripMenuItem
			// 
			this.���������ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.���������������ToolStripMenuItem,
            this.�����������������ToolStripMenuItem});
			this.���������ToolStripMenuItem.Name = "���������ToolStripMenuItem";
			this.���������ToolStripMenuItem.Size = new System.Drawing.Size(79, 19);
			this.���������ToolStripMenuItem.Text = "���������";
			// 
			// ���������������ToolStripMenuItem
			// 
			this.���������������ToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.���������������ToolStripMenuItem.Name = "���������������ToolStripMenuItem";
			this.���������������ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.���������������ToolStripMenuItem.Text = "��������� ������";
			this.���������������ToolStripMenuItem.Click += new System.EventHandler(this.���������������ToolStripMenuItem_Click);
			// 
			// �����������������ToolStripMenuItem
			// 
			this.�����������������ToolStripMenuItem.Name = "�����������������ToolStripMenuItem";
			this.�����������������ToolStripMenuItem.Size = new System.Drawing.Size(193, 22);
			this.�����������������ToolStripMenuItem.Text = "������� ����������";
			this.�����������������ToolStripMenuItem.Click += new System.EventHandler(this.�����������������ToolStripMenuItem_Click);
			// 
			// �������ToolStripMenuItem
			// 
			this.�������ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.������������ToolStripMenuItem,
            this.separatorToolStripMenuItem,
            this.����������ToolStripMenuItem});
			this.�������ToolStripMenuItem.Name = "�������ToolStripMenuItem";
			this.�������ToolStripMenuItem.Size = new System.Drawing.Size(65, 19);
			this.�������ToolStripMenuItem.Text = "�������";
			// 
			// ������������ToolStripMenuItem
			// 
			this.������������ToolStripMenuItem.Image = global::RecoveryStar.Properties.Resources.Help;
			this.������������ToolStripMenuItem.Name = "������������ToolStripMenuItem";
			this.������������ToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
			this.������������ToolStripMenuItem.Text = "����� �������";
			this.������������ToolStripMenuItem.Click += new System.EventHandler(this.������������ToolStripMenuItem_Click);
			// 
			// separatorToolStripMenuItem
			// 
			this.separatorToolStripMenuItem.Name = "separatorToolStripMenuItem";
			this.separatorToolStripMenuItem.Size = new System.Drawing.Size(155, 6);
			// 
			// ����������ToolStripMenuItem
			// 
			this.����������ToolStripMenuItem.Name = "����������ToolStripMenuItem";
			this.����������ToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
			this.����������ToolStripMenuItem.Text = "� ���������...";
			this.����������ToolStripMenuItem.Click += new System.EventHandler(this.����������ToolStripMenuItem_Click);
			// 
			// coderConfigGroupBox
			// 
			this.coderConfigGroupBox.Controls.Add(this.redundancyGroupBox);
			this.coderConfigGroupBox.Controls.Add(this.allVolCountGroupBox);
			this.coderConfigGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.coderConfigGroupBox.Location = new System.Drawing.Point(414, 26);
			this.coderConfigGroupBox.Name = "coderConfigGroupBox";
			this.coderConfigGroupBox.Size = new System.Drawing.Size(561, 98);
			this.coderConfigGroupBox.TabIndex = 5;
			this.coderConfigGroupBox.TabStop = false;
			this.coderConfigGroupBox.Text = "������������ ������ (�������� �����: ...; ����� ��� ��������������: ...; ����� ��" +
    "����: ...)";
			// 
			// redundancyGroupBox
			// 
			this.redundancyGroupBox.Controls.Add(this.redundancyMacTrackBar);
			this.redundancyGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.redundancyGroupBox.Location = new System.Drawing.Point(286, 21);
			this.redundancyGroupBox.Name = "redundancyGroupBox";
			this.redundancyGroupBox.Size = new System.Drawing.Size(264, 65);
			this.redundancyGroupBox.TabIndex = 4;
			this.redundancyGroupBox.TabStop = false;
			this.redundancyGroupBox.Text = "������������ �����������:";
			// 
			// redundancyMacTrackBar
			// 
			this.redundancyMacTrackBar.BackColor = System.Drawing.Color.Transparent;
			this.redundancyMacTrackBar.BorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.redundancyMacTrackBar.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.redundancyMacTrackBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(123)))), ((int)(((byte)(125)))), ((int)(((byte)(123)))));
			this.redundancyMacTrackBar.IndentHeight = 6;
			this.redundancyMacTrackBar.Location = new System.Drawing.Point(6, 24);
			this.redundancyMacTrackBar.Maximum = 199;
			this.redundancyMacTrackBar.Minimum = 0;
			this.redundancyMacTrackBar.Name = "redundancyMacTrackBar";
			this.redundancyMacTrackBar.Size = new System.Drawing.Size(252, 28);
			this.redundancyMacTrackBar.TabIndex = 6;
			this.redundancyMacTrackBar.TextTickStyle = System.Windows.Forms.TickStyle.None;
			this.redundancyMacTrackBar.TickColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(146)))), ((int)(((byte)(148)))));
			this.redundancyMacTrackBar.TickHeight = 4;
			this.redundancyMacTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.toolTip.SetToolTip(this.redundancyMacTrackBar, "��� ������ ������������ ����������� - ��� ���� ������������������, �� ������ ����" +
        "� ����������� ������ �����");
			this.redundancyMacTrackBar.TrackerColor = System.Drawing.Color.ForestGreen;
			this.redundancyMacTrackBar.TrackerSize = new System.Drawing.Size(16, 16);
			this.redundancyMacTrackBar.TrackLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(93)))), ((int)(((byte)(90)))));
			this.redundancyMacTrackBar.TrackLineHeight = 3;
			this.redundancyMacTrackBar.Value = 19;
			this.redundancyMacTrackBar.ValueChanged += new EConTech.Windows.MACUI.ValueChangedHandler(this.redundancyMacTrackBar_ValueChanged);
			// 
			// allVolCountGroupBox
			// 
			this.allVolCountGroupBox.Controls.Add(this.allVolCountMacTrackBar);
			this.allVolCountGroupBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.allVolCountGroupBox.Location = new System.Drawing.Point(12, 21);
			this.allVolCountGroupBox.Name = "allVolCountGroupBox";
			this.allVolCountGroupBox.Size = new System.Drawing.Size(264, 65);
			this.allVolCountGroupBox.TabIndex = 3;
			this.allVolCountGroupBox.TabStop = false;
			this.allVolCountGroupBox.Text = "����� ���������� �����:";
			// 
			// allVolCountMacTrackBar
			// 
			this.allVolCountMacTrackBar.BackColor = System.Drawing.Color.Transparent;
			this.allVolCountMacTrackBar.BorderColor = System.Drawing.SystemColors.ActiveBorder;
			this.allVolCountMacTrackBar.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.allVolCountMacTrackBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(123)))), ((int)(((byte)(125)))), ((int)(((byte)(123)))));
			this.allVolCountMacTrackBar.IndentHeight = 6;
			this.allVolCountMacTrackBar.Location = new System.Drawing.Point(6, 24);
			this.allVolCountMacTrackBar.Maximum = 24;
			this.allVolCountMacTrackBar.Minimum = 0;
			this.allVolCountMacTrackBar.Name = "allVolCountMacTrackBar";
			this.allVolCountMacTrackBar.Size = new System.Drawing.Size(252, 28);
			this.allVolCountMacTrackBar.TabIndex = 5;
			this.allVolCountMacTrackBar.TextTickStyle = System.Windows.Forms.TickStyle.None;
			this.allVolCountMacTrackBar.TickColor = System.Drawing.Color.FromArgb(((int)(((byte)(148)))), ((int)(((byte)(146)))), ((int)(((byte)(148)))));
			this.allVolCountMacTrackBar.TickHeight = 4;
			this.allVolCountMacTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.toolTip.SetToolTip(this.allVolCountMacTrackBar, "��� ������ ����� - ��� ��������� ��������� � ���� ������������������");
			this.allVolCountMacTrackBar.TrackerColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(130)))), ((int)(((byte)(198)))));
			this.allVolCountMacTrackBar.TrackerSize = new System.Drawing.Size(16, 16);
			this.allVolCountMacTrackBar.TrackLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(93)))), ((int)(((byte)(90)))));
			this.allVolCountMacTrackBar.TrackLineHeight = 3;
			this.allVolCountMacTrackBar.Value = 18;
			this.allVolCountMacTrackBar.ValueChanged += new EConTech.Windows.MACUI.ValueChangedHandler(this.allVolCountMacTrackBar_ValueChanged);
			// 
			// toolTip
			// 
			this.toolTip.AutomaticDelay = 2000;
			this.toolTip.AutoPopDelay = 20000;
			this.toolTip.InitialDelay = 2000;
			this.toolTip.ReshowDelay = 1000;
			// 
			// testButton
			// 
			this.testButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.testButton.Image = global::RecoveryStar.Properties.Resources.Test;
			this.testButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.testButton.Location = new System.Drawing.Point(309, 27);
			this.testButton.Name = "testButton";
			this.testButton.Size = new System.Drawing.Size(100, 97);
			this.testButton.TabIndex = 4;
			this.testButton.Text = "��������������";
			this.testButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolTip.SetToolTip(this.testButton, "�������������� ���������������� ����� �����");
			this.testButton.UseVisualStyleBackColor = true;
			this.testButton.Click += new System.EventHandler(this.testButton_Click);
			// 
			// repairButton
			// 
			this.repairButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.repairButton.Image = global::RecoveryStar.Properties.Resources.Repair;
			this.repairButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.repairButton.Location = new System.Drawing.Point(210, 27);
			this.repairButton.Name = "repairButton";
			this.repairButton.Size = new System.Drawing.Size(100, 97);
			this.repairButton.TabIndex = 3;
			this.repairButton.Text = "��������";
			this.repairButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolTip.SetToolTip(this.repairButton, "������������ ����������� ����������������� ������ �����");
			this.repairButton.UseVisualStyleBackColor = true;
			this.repairButton.Click += new System.EventHandler(this.repairButton_Click);
			// 
			// recoverButton
			// 
			this.recoverButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.recoverButton.Image = global::RecoveryStar.Properties.Resources.Recover;
			this.recoverButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.recoverButton.Location = new System.Drawing.Point(111, 27);
			this.recoverButton.Name = "recoverButton";
			this.recoverButton.Size = new System.Drawing.Size(100, 97);
			this.recoverButton.TabIndex = 2;
			this.recoverButton.Text = "�������";
			this.recoverButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolTip.SetToolTip(this.recoverButton, "������� ����� �� ������ ����� � ���������� ������");
			this.recoverButton.UseVisualStyleBackColor = true;
			this.recoverButton.Click += new System.EventHandler(this.recoverButton_Click);
			// 
			// protectButton
			// 
			this.protectButton.BackColor = System.Drawing.SystemColors.Control;
			this.protectButton.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.protectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
			this.protectButton.Image = global::RecoveryStar.Properties.Resources.Protect;
			this.protectButton.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
			this.protectButton.Location = new System.Drawing.Point(12, 27);
			this.protectButton.Name = "protectButton";
			this.protectButton.Size = new System.Drawing.Size(100, 97);
			this.protectButton.TabIndex = 1;
			this.protectButton.Text = "��������";
			this.protectButton.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.toolTip.SetToolTip(this.protectButton, "������������ ����� � ���������������� ������� � ������ ����������");
			this.protectButton.UseVisualStyleBackColor = false;
			this.protectButton.Click += new System.EventHandler(this.protectButton_Click);
			// 
			// browser
			// 
			this.browser.Location = new System.Drawing.Point(12, 131);
			this.browser.Name = "browser";
			this.browser.SelectedNode = null;
			this.browser.Size = new System.Drawing.Size(962, 432);
			this.browser.StartUpDirectory = FileBrowser.SpecialFolders.DekstopDir;
			this.browser.TabIndex = 0;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(986, 576);
			this.Controls.Add(this.coderConfigGroupBox);
			this.Controls.Add(this.testButton);
			this.Controls.Add(this.repairButton);
			this.Controls.Add(this.recoverButton);
			this.Controls.Add(this.protectButton);
			this.Controls.Add(this.browser);
			this.Controls.Add(this.menuStrip);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.menuStrip;
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Recovery Star 2.22 (RUS)";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.coderConfigGroupBox.ResumeLayout(false);
			this.redundancyGroupBox.ResumeLayout(false);
			this.redundancyGroupBox.PerformLayout();
			this.allVolCountGroupBox.ResumeLayout(false);
			this.allVolCountGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem ����ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem �������ToolStripMenuItem;
        private System.Windows.Forms.Button protectButton;
        private System.Windows.Forms.Button recoverButton;
        private System.Windows.Forms.ToolStripMenuItem �����ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ������������ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator separatorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ����������ToolStripMenuItem;
        private System.Windows.Forms.Button repairButton;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.GroupBox coderConfigGroupBox;
        private System.Windows.Forms.GroupBox redundancyGroupBox;
        private System.Windows.Forms.GroupBox allVolCountGroupBox;
        private EConTech.Windows.MACUI.MACTrackBar allVolCountMacTrackBar;
        private EConTech.Windows.MACUI.MACTrackBar redundancyMacTrackBar;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.ToolStripMenuItem �����������ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ������������������ToolStripMenuItem;
        internal FileBrowser.Browser browser;
        private System.Windows.Forms.ToolStripMenuItem ���������ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ���������������ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem �����������������ToolStripMenuItem;
    }
}

