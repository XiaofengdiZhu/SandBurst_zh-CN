namespace SandBurst
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.historyButton = new System.Windows.Forms.Button();
            this.windowTextBox = new System.Windows.Forms.TextBox();
            this.windowSelectButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.settingListBox = new System.Windows.Forms.ListBox();
            this.rightClickMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemAdd = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ratioRadioButton = new System.Windows.Forms.RadioButton();
            this.widthRadioButton = new System.Windows.Forms.RadioButton();
            this.scaleRadioButton = new System.Windows.Forms.RadioButton();
            this.scalePanel3 = new System.Windows.Forms.Panel();
            this.ratioBar = new System.Windows.Forms.TrackBar();
            this.ratioLabel = new System.Windows.Forms.Label();
            this.scalePanel2 = new System.Windows.Forms.Panel();
            this.widthLabel = new System.Windows.Forms.Label();
            this.widthTextBox = new System.Windows.Forms.TextBox();
            this.scalePanel1 = new System.Windows.Forms.Panel();
            this.scaleButton5 = new System.Windows.Forms.Button();
            this.scaleButton4 = new System.Windows.Forms.Button();
            this.scaleButton3 = new System.Windows.Forms.Button();
            this.scaleButton2 = new System.Windows.Forms.Button();
            this.scaleButton1 = new System.Windows.Forms.Button();
            this.scaleBar = new System.Windows.Forms.TrackBar();
            this.scaleLabel = new System.Windows.Forms.Label();
            this.executeButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.windowMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.historyMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.rightClickMenu.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.scalePanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ratioBar)).BeginInit();
            this.scalePanel2.SuspendLayout();
            this.scalePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleBar)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            resources.ApplyResources(this.panel1, "panel1");
            this.panel1.BackColor = System.Drawing.SystemColors.Control;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.historyButton);
            this.panel1.Controls.Add(this.windowTextBox);
            this.panel1.Controls.Add(this.windowSelectButton);
            this.panel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel1.Name = "panel1";
            // 
            // historyButton
            // 
            resources.ApplyResources(this.historyButton, "historyButton");
            this.historyButton.Name = "historyButton";
            this.historyButton.UseVisualStyleBackColor = true;
            this.historyButton.Click += new System.EventHandler(this.HistoryButton_Click);
            // 
            // windowTextBox
            // 
            resources.ApplyResources(this.windowTextBox, "windowTextBox");
            this.windowTextBox.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.windowTextBox.Name = "windowTextBox";
            this.windowTextBox.ReadOnly = true;
            this.windowTextBox.TabStop = false;
            // 
            // windowSelectButton
            // 
            resources.ApplyResources(this.windowSelectButton, "windowSelectButton");
            this.windowSelectButton.Name = "windowSelectButton";
            this.windowSelectButton.UseVisualStyleBackColor = true;
            this.windowSelectButton.Click += new System.EventHandler(this.WindowSelectButton_Click);
            // 
            // groupBox1
            // 
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.BackColor = System.Drawing.SystemColors.Control;
            this.groupBox1.Controls.Add(this.settingListBox);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // settingListBox
            // 
            resources.ApplyResources(this.settingListBox, "settingListBox");
            this.settingListBox.ContextMenuStrip = this.rightClickMenu;
            this.settingListBox.FormattingEnabled = true;
            this.settingListBox.Name = "settingListBox";
            this.settingListBox.Click += new System.EventHandler(this.SettingListBox_Click);
            this.settingListBox.DoubleClick += new System.EventHandler(this.SettingListBox_DoubleClick);
            // 
            // rightClickMenu
            // 
            resources.ApplyResources(this.rightClickMenu, "rightClickMenu");
            this.rightClickMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.rightClickMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemAdd,
            this.MenuItemEdit,
            this.MenuItemDelete});
            this.rightClickMenu.Name = "rightClickMenu";
            // 
            // MenuItemAdd
            // 
            resources.ApplyResources(this.MenuItemAdd, "MenuItemAdd");
            this.MenuItemAdd.Name = "MenuItemAdd";
            this.MenuItemAdd.Click += new System.EventHandler(this.MenuItemAdd_Click);
            // 
            // MenuItemEdit
            // 
            resources.ApplyResources(this.MenuItemEdit, "MenuItemEdit");
            this.MenuItemEdit.Name = "MenuItemEdit";
            this.MenuItemEdit.Click += new System.EventHandler(this.MenuItemEdit_Click);
            // 
            // MenuItemDelete
            // 
            resources.ApplyResources(this.MenuItemDelete, "MenuItemDelete");
            this.MenuItemDelete.Name = "MenuItemDelete";
            this.MenuItemDelete.Click += new System.EventHandler(this.MenuItemDelete_Click);
            // 
            // groupBox2
            // 
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Controls.Add(this.ratioRadioButton);
            this.groupBox2.Controls.Add(this.widthRadioButton);
            this.groupBox2.Controls.Add(this.scaleRadioButton);
            this.groupBox2.Controls.Add(this.scalePanel3);
            this.groupBox2.Controls.Add(this.scalePanel2);
            this.groupBox2.Controls.Add(this.scalePanel1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // ratioRadioButton
            // 
            resources.ApplyResources(this.ratioRadioButton, "ratioRadioButton");
            this.ratioRadioButton.Name = "ratioRadioButton";
            this.ratioRadioButton.UseVisualStyleBackColor = true;
            this.ratioRadioButton.CheckedChanged += new System.EventHandler(this.ScaleRadioButton_Click);
            // 
            // widthRadioButton
            // 
            resources.ApplyResources(this.widthRadioButton, "widthRadioButton");
            this.widthRadioButton.Name = "widthRadioButton";
            this.widthRadioButton.UseVisualStyleBackColor = true;
            this.widthRadioButton.CheckedChanged += new System.EventHandler(this.ScaleRadioButton_Click);
            // 
            // scaleRadioButton
            // 
            resources.ApplyResources(this.scaleRadioButton, "scaleRadioButton");
            this.scaleRadioButton.Checked = true;
            this.scaleRadioButton.Name = "scaleRadioButton";
            this.scaleRadioButton.TabStop = true;
            this.scaleRadioButton.UseVisualStyleBackColor = true;
            this.scaleRadioButton.Click += new System.EventHandler(this.ScaleRadioButton_Click);
            // 
            // scalePanel3
            // 
            resources.ApplyResources(this.scalePanel3, "scalePanel3");
            this.scalePanel3.Controls.Add(this.ratioBar);
            this.scalePanel3.Controls.Add(this.ratioLabel);
            this.scalePanel3.Name = "scalePanel3";
            // 
            // ratioBar
            // 
            resources.ApplyResources(this.ratioBar, "ratioBar");
            this.ratioBar.Maximum = 100;
            this.ratioBar.Minimum = 1;
            this.ratioBar.Name = "ratioBar";
            this.ratioBar.Value = 100;
            this.ratioBar.Scroll += new System.EventHandler(this.RatioBar_Scroll);
            // 
            // ratioLabel
            // 
            resources.ApplyResources(this.ratioLabel, "ratioLabel");
            this.ratioLabel.Name = "ratioLabel";
            // 
            // scalePanel2
            // 
            resources.ApplyResources(this.scalePanel2, "scalePanel2");
            this.scalePanel2.Controls.Add(this.widthLabel);
            this.scalePanel2.Controls.Add(this.widthTextBox);
            this.scalePanel2.Name = "scalePanel2";
            // 
            // widthLabel
            // 
            resources.ApplyResources(this.widthLabel, "widthLabel");
            this.widthLabel.Name = "widthLabel";
            // 
            // widthTextBox
            // 
            resources.ApplyResources(this.widthTextBox, "widthTextBox");
            this.widthTextBox.Name = "widthTextBox";
            this.widthTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.WidthTextBox_KeyPress);
            this.widthTextBox.Leave += new System.EventHandler(this.WidthTextBox_Leave);
            // 
            // scalePanel1
            // 
            resources.ApplyResources(this.scalePanel1, "scalePanel1");
            this.scalePanel1.Controls.Add(this.scaleButton5);
            this.scalePanel1.Controls.Add(this.scaleButton4);
            this.scalePanel1.Controls.Add(this.scaleButton3);
            this.scalePanel1.Controls.Add(this.scaleButton2);
            this.scalePanel1.Controls.Add(this.scaleButton1);
            this.scalePanel1.Controls.Add(this.scaleBar);
            this.scalePanel1.Controls.Add(this.scaleLabel);
            this.scalePanel1.Name = "scalePanel1";
            // 
            // scaleButton5
            // 
            resources.ApplyResources(this.scaleButton5, "scaleButton5");
            this.scaleButton5.Name = "scaleButton5";
            this.scaleButton5.UseVisualStyleBackColor = true;
            this.scaleButton5.Click += new System.EventHandler(this.ScaleButton_Click);
            // 
            // scaleButton4
            // 
            resources.ApplyResources(this.scaleButton4, "scaleButton4");
            this.scaleButton4.Name = "scaleButton4";
            this.scaleButton4.UseVisualStyleBackColor = true;
            this.scaleButton4.Click += new System.EventHandler(this.ScaleButton_Click);
            // 
            // scaleButton3
            // 
            resources.ApplyResources(this.scaleButton3, "scaleButton3");
            this.scaleButton3.Name = "scaleButton3";
            this.scaleButton3.UseVisualStyleBackColor = true;
            this.scaleButton3.Click += new System.EventHandler(this.ScaleButton_Click);
            // 
            // scaleButton2
            // 
            resources.ApplyResources(this.scaleButton2, "scaleButton2");
            this.scaleButton2.Name = "scaleButton2";
            this.scaleButton2.UseVisualStyleBackColor = true;
            this.scaleButton2.Click += new System.EventHandler(this.ScaleButton_Click);
            // 
            // scaleButton1
            // 
            resources.ApplyResources(this.scaleButton1, "scaleButton1");
            this.scaleButton1.Name = "scaleButton1";
            this.scaleButton1.UseVisualStyleBackColor = true;
            this.scaleButton1.Click += new System.EventHandler(this.ScaleButton_Click);
            // 
            // scaleBar
            // 
            resources.ApplyResources(this.scaleBar, "scaleBar");
            this.scaleBar.Maximum = 400;
            this.scaleBar.Minimum = 100;
            this.scaleBar.Name = "scaleBar";
            this.scaleBar.Value = 100;
            this.scaleBar.Scroll += new System.EventHandler(this.ScaleBar_Scroll);
            // 
            // scaleLabel
            // 
            resources.ApplyResources(this.scaleLabel, "scaleLabel");
            this.scaleLabel.Name = "scaleLabel";
            // 
            // executeButton
            // 
            resources.ApplyResources(this.executeButton, "executeButton");
            this.executeButton.Name = "executeButton";
            this.executeButton.UseVisualStyleBackColor = true;
            this.executeButton.Click += new System.EventHandler(this.ExecuteButton_Click);
            // 
            // stopButton
            // 
            resources.ApplyResources(this.stopButton, "stopButton");
            this.stopButton.Name = "stopButton";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // windowMenu
            // 
            resources.ApplyResources(this.windowMenu, "windowMenu");
            this.windowMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.windowMenu.Name = "windowMenu";
            // 
            // historyMenu
            // 
            resources.ApplyResources(this.historyMenu, "historyMenu");
            this.historyMenu.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.historyMenu.Name = "historyMenu";
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.executeButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.rightClickMenu.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.scalePanel3.ResumeLayout(false);
            this.scalePanel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ratioBar)).EndInit();
            this.scalePanel2.ResumeLayout(false);
            this.scalePanel2.PerformLayout();
            this.scalePanel1.ResumeLayout(false);
            this.scalePanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scaleBar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox windowTextBox;
        private System.Windows.Forms.Button windowSelectButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox settingListBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton widthRadioButton;
        private System.Windows.Forms.RadioButton scaleRadioButton;
        private System.Windows.Forms.Button executeButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.ContextMenuStrip windowMenu;
        private System.Windows.Forms.Panel scalePanel1;
        private System.Windows.Forms.Button scaleButton5;
        private System.Windows.Forms.Button scaleButton4;
        private System.Windows.Forms.Button scaleButton3;
        private System.Windows.Forms.Button scaleButton2;
        private System.Windows.Forms.Button scaleButton1;
        private System.Windows.Forms.TrackBar scaleBar;
        private System.Windows.Forms.Label scaleLabel;
        private System.Windows.Forms.Panel scalePanel2;
        private System.Windows.Forms.Label widthLabel;
        private System.Windows.Forms.TextBox widthTextBox;
        private System.Windows.Forms.ContextMenuStrip rightClickMenu;
        private System.Windows.Forms.ToolStripMenuItem MenuItemAdd;
        private System.Windows.Forms.ToolStripMenuItem MenuItemEdit;
        private System.Windows.Forms.ToolStripMenuItem MenuItemDelete;
        private System.Windows.Forms.Button historyButton;
        private System.Windows.Forms.ContextMenuStrip historyMenu;
        private System.Windows.Forms.RadioButton ratioRadioButton;
        private System.Windows.Forms.Panel scalePanel3;
        private System.Windows.Forms.TrackBar ratioBar;
        private System.Windows.Forms.Label ratioLabel;
    }
}

