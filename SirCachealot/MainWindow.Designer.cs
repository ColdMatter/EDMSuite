namespace SirCachealot
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addBlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.databaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.test1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.test2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadGateSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveGateConfigSetToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.statsTextBox = new System.Windows.Forms.TextBox();
            this.errorLogTextBox = new System.Windows.Forms.TextBox();
            this.gateConfigSelectionComboBox = new System.Windows.Forms.ComboBox();
            this.gateListDataView = new System.Windows.Forms.DataGridView();
            this.updateGatesButton = new System.Windows.Forms.Button();
            this.currentGateConfigNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.newGateConfigButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.addGatedBlockToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gateListDataView)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.databaseToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.gatesToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(970, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addBlockToolStripMenuItem,
            this.addGatedBlockToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // addBlockToolStripMenuItem
            // 
            this.addBlockToolStripMenuItem.Name = "addBlockToolStripMenuItem";
            this.addBlockToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addBlockToolStripMenuItem.Text = "Add block";
            this.addBlockToolStripMenuItem.Click += new System.EventHandler(this.addBlockToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // databaseToolStripMenuItem
            // 
            this.databaseToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectToolStripMenuItem,
            this.createToolStripMenuItem});
            this.databaseToolStripMenuItem.Name = "databaseToolStripMenuItem";
            this.databaseToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.databaseToolStripMenuItem.Text = "Database";
            // 
            // selectToolStripMenuItem
            // 
            this.selectToolStripMenuItem.Name = "selectToolStripMenuItem";
            this.selectToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.selectToolStripMenuItem.Text = "Select ...";
            this.selectToolStripMenuItem.Click += new System.EventHandler(this.selectToolStripMenuItem_Click);
            // 
            // createToolStripMenuItem
            // 
            this.createToolStripMenuItem.Name = "createToolStripMenuItem";
            this.createToolStripMenuItem.Size = new System.Drawing.Size(120, 22);
            this.createToolStripMenuItem.Text = "Create ...";
            this.createToolStripMenuItem.Click += new System.EventHandler(this.createToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.test1ToolStripMenuItem,
            this.test2ToolStripMenuItem});
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // test1ToolStripMenuItem
            // 
            this.test1ToolStripMenuItem.Name = "test1ToolStripMenuItem";
            this.test1ToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.test1ToolStripMenuItem.Text = "Test1";
            this.test1ToolStripMenuItem.Click += new System.EventHandler(this.test1ToolStripMenuItem_Click);
            // 
            // test2ToolStripMenuItem
            // 
            this.test2ToolStripMenuItem.Name = "test2ToolStripMenuItem";
            this.test2ToolStripMenuItem.Size = new System.Drawing.Size(101, 22);
            this.test2ToolStripMenuItem.Text = "Test2";
            this.test2ToolStripMenuItem.Click += new System.EventHandler(this.test2ToolStripMenuItem_Click);
            // 
            // gatesToolStripMenuItem
            // 
            this.gatesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadGateSetToolStripMenuItem,
            this.saveGateConfigSetToolStripMenuItem});
            this.gatesToolStripMenuItem.Name = "gatesToolStripMenuItem";
            this.gatesToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
            this.gatesToolStripMenuItem.Text = "Gates";
            // 
            // loadGateSetToolStripMenuItem
            // 
            this.loadGateSetToolStripMenuItem.Name = "loadGateSetToolStripMenuItem";
            this.loadGateSetToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.loadGateSetToolStripMenuItem.Text = "Load gate config set";
            this.loadGateSetToolStripMenuItem.Click += new System.EventHandler(this.loadGateSetToolStripMenuItem_Click);
            // 
            // saveGateConfigSetToolStripMenuItem
            // 
            this.saveGateConfigSetToolStripMenuItem.Name = "saveGateConfigSetToolStripMenuItem";
            this.saveGateConfigSetToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.saveGateConfigSetToolStripMenuItem.Text = "Save gate config set";
            this.saveGateConfigSetToolStripMenuItem.Click += new System.EventHandler(this.saveGateConfigSetToolStripMenuItem_Click);
            // 
            // logTextBox
            // 
            this.logTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.logTextBox.Location = new System.Drawing.Point(274, 27);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logTextBox.Size = new System.Drawing.Size(233, 199);
            this.logTextBox.TabIndex = 2;
            // 
            // statsTextBox
            // 
            this.statsTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.statsTextBox.Location = new System.Drawing.Point(12, 27);
            this.statsTextBox.Multiline = true;
            this.statsTextBox.Name = "statsTextBox";
            this.statsTextBox.ReadOnly = true;
            this.statsTextBox.Size = new System.Drawing.Size(256, 199);
            this.statsTextBox.TabIndex = 3;
            // 
            // errorLogTextBox
            // 
            this.errorLogTextBox.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.errorLogTextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.errorLogTextBox.ForeColor = System.Drawing.Color.Red;
            this.errorLogTextBox.Location = new System.Drawing.Point(12, 232);
            this.errorLogTextBox.Multiline = true;
            this.errorLogTextBox.Name = "errorLogTextBox";
            this.errorLogTextBox.ReadOnly = true;
            this.errorLogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.errorLogTextBox.Size = new System.Drawing.Size(947, 132);
            this.errorLogTextBox.TabIndex = 4;
            this.errorLogTextBox.Text = "Error log";
            // 
            // gateConfigSelectionComboBox
            // 
            this.gateConfigSelectionComboBox.FormattingEnabled = true;
            this.gateConfigSelectionComboBox.Location = new System.Drawing.Point(629, 28);
            this.gateConfigSelectionComboBox.Name = "gateConfigSelectionComboBox";
            this.gateConfigSelectionComboBox.Size = new System.Drawing.Size(120, 21);
            this.gateConfigSelectionComboBox.TabIndex = 6;
            this.gateConfigSelectionComboBox.SelectedIndexChanged += new System.EventHandler(this.gateConfigSelectionComboBox_SelectedIndexChanged);
            // 
            // gateListDataView
            // 
            this.gateListDataView.AllowUserToResizeColumns = false;
            this.gateListDataView.AllowUserToResizeRows = false;
            this.gateListDataView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gateListDataView.Location = new System.Drawing.Point(513, 78);
            this.gateListDataView.Name = "gateListDataView";
            this.gateListDataView.Size = new System.Drawing.Size(445, 148);
            this.gateListDataView.TabIndex = 7;
            // 
            // updateGatesButton
            // 
            this.updateGatesButton.Location = new System.Drawing.Point(842, 53);
            this.updateGatesButton.Name = "updateGatesButton";
            this.updateGatesButton.Size = new System.Drawing.Size(117, 23);
            this.updateGatesButton.TabIndex = 8;
            this.updateGatesButton.Text = "Update gate config";
            this.updateGatesButton.UseVisualStyleBackColor = true;
            this.updateGatesButton.Click += new System.EventHandler(this.updateGatesButton_Click);
            // 
            // currentGateConfigNameTextBox
            // 
            this.currentGateConfigNameTextBox.Location = new System.Drawing.Point(629, 55);
            this.currentGateConfigNameTextBox.Name = "currentGateConfigNameTextBox";
            this.currentGateConfigNameTextBox.Size = new System.Drawing.Size(120, 20);
            this.currentGateConfigNameTextBox.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(513, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Gate config name:";
            // 
            // newGateConfigButton
            // 
            this.newGateConfigButton.Location = new System.Drawing.Point(842, 27);
            this.newGateConfigButton.Name = "newGateConfigButton";
            this.newGateConfigButton.Size = new System.Drawing.Size(117, 23);
            this.newGateConfigButton.TabIndex = 11;
            this.newGateConfigButton.Text = "New gate config";
            this.newGateConfigButton.UseVisualStyleBackColor = true;
            this.newGateConfigButton.Click += new System.EventHandler(this.newGateConfigButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(513, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Gate config selection:";
            // 
            // addGatedBlockToolStripMenuItem
            // 
            this.addGatedBlockToolStripMenuItem.Name = "addGatedBlockToolStripMenuItem";
            this.addGatedBlockToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addGatedBlockToolStripMenuItem.Text = "Add gated block";
            this.addGatedBlockToolStripMenuItem.Click += new System.EventHandler(this.addGatedBlockToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 372);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.newGateConfigButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.currentGateConfigNameTextBox);
            this.Controls.Add(this.updateGatesButton);
            this.Controls.Add(this.gateListDataView);
            this.Controls.Add(this.gateConfigSelectionComboBox);
            this.Controls.Add(this.errorLogTextBox);
            this.Controls.Add(this.statsTextBox);
            this.Controls.Add(this.logTextBox);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainWindow";
            this.Text = "SirCachealot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formCloseHandler);
            this.Load += new System.EventHandler(this.formLoadHandler);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gateListDataView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem databaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createToolStripMenuItem;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.TextBox statsTextBox;
        private System.Windows.Forms.ToolStripMenuItem selectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem test1ToolStripMenuItem;
        private System.Windows.Forms.TextBox errorLogTextBox;
        private System.Windows.Forms.ToolStripMenuItem test2ToolStripMenuItem;
        private System.Windows.Forms.ComboBox gateConfigSelectionComboBox;
        private System.Windows.Forms.ToolStripMenuItem gatesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadGateSetToolStripMenuItem;
        private System.Windows.Forms.DataGridView gateListDataView;
        private System.Windows.Forms.Button updateGatesButton;
        private System.Windows.Forms.TextBox currentGateConfigNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button newGateConfigButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem saveGateConfigSetToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addBlockToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addGatedBlockToolStripMenuItem;
    }
}
