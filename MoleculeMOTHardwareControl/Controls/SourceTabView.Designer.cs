namespace MoleculeMOTHardwareControl.Controls
{
    partial class SourceTabView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tempGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.cryoGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.cryoSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.cryoLED = new NationalInstruments.UI.WindowsForms.Led();
            this.heaterGroup = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.heaterSwitch = new NationalInstruments.UI.WindowsForms.Switch();
            this.heaterLED = new NationalInstruments.UI.WindowsForms.Led();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.cycleButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.readButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tempGraph)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.cryoGroup.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cryoSwitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cryoLED)).BeginInit();
            this.heaterGroup.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.heaterSwitch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.heaterLED)).BeginInit();
            this.tableLayoutPanel5.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 77.64706F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22.35294F));
            this.tableLayoutPanel1.Controls.Add(this.tempGraph, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel6, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 81.97531F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 18.02469F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(680, 810);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tempGraph
            // 
            this.tempGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tempGraph.Location = new System.Drawing.Point(3, 3);
            this.tempGraph.Name = "tempGraph";
            this.tempGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1});
            this.tempGraph.Size = new System.Drawing.Size(521, 658);
            this.tempGraph.TabIndex = 0;
            this.tempGraph.UseColorGenerator = true;
            this.tempGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.tempGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Caption = "Time";
            // 
            // yAxis1
            // 
            this.yAxis1.Caption = "Temperature (°C) ";
            this.yAxis1.Mode = NationalInstruments.UI.AxisMode.AutoScaleVisibleLoose;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.cryoGroup, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.heaterGroup, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.tableLayoutPanel5, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.readButton, 0, 3);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(530, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 4;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(147, 658);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // cryoGroup
            // 
            this.cryoGroup.Controls.Add(this.tableLayoutPanel4);
            this.cryoGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cryoGroup.Location = new System.Drawing.Point(3, 167);
            this.cryoGroup.Name = "cryoGroup";
            this.cryoGroup.Size = new System.Drawing.Size(141, 158);
            this.cryoGroup.TabIndex = 1;
            this.cryoGroup.TabStop = false;
            this.cryoGroup.Text = "Cryo-cooler";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.cryoSwitch, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.cryoLED, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(135, 139);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // cryoSwitch
            // 
            this.cryoSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoSwitch.Location = new System.Drawing.Point(38, 58);
            this.cryoSwitch.Name = "cryoSwitch";
            this.cryoSwitch.Size = new System.Drawing.Size(59, 78);
            this.cryoSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.cryoSwitch.TabIndex = 0;
            // 
            // cryoLED
            // 
            this.cryoLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cryoLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.cryoLED.Location = new System.Drawing.Point(43, 3);
            this.cryoLED.Name = "cryoLED";
            this.cryoLED.Size = new System.Drawing.Size(49, 49);
            this.cryoLED.TabIndex = 1;
            // 
            // heaterGroup
            // 
            this.heaterGroup.Controls.Add(this.tableLayoutPanel3);
            this.heaterGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.heaterGroup.Location = new System.Drawing.Point(3, 3);
            this.heaterGroup.Name = "heaterGroup";
            this.heaterGroup.Size = new System.Drawing.Size(141, 158);
            this.heaterGroup.TabIndex = 0;
            this.heaterGroup.TabStop = false;
            this.heaterGroup.Text = "Heater";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.heaterSwitch, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.heaterLED, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 60F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(135, 139);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // heaterSwitch
            // 
            this.heaterSwitch.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterSwitch.Location = new System.Drawing.Point(38, 58);
            this.heaterSwitch.Name = "heaterSwitch";
            this.heaterSwitch.Size = new System.Drawing.Size(59, 78);
            this.heaterSwitch.SwitchStyle = NationalInstruments.UI.SwitchStyle.VerticalToggle3D;
            this.heaterSwitch.TabIndex = 0;
            // 
            // heaterLED
            // 
            this.heaterLED.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.heaterLED.LedStyle = NationalInstruments.UI.LedStyle.Round3D;
            this.heaterLED.Location = new System.Drawing.Point(43, 3);
            this.heaterLED.Name = "heaterLED";
            this.heaterLED.Size = new System.Drawing.Size(49, 49);
            this.heaterLED.TabIndex = 1;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Controls.Add(this.cycleButton, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.groupBox1, 0, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 331);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(141, 158);
            this.tableLayoutPanel5.TabIndex = 2;
            // 
            // cycleButton
            // 
            this.cycleButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cycleButton.Location = new System.Drawing.Point(33, 96);
            this.cycleButton.Name = "cycleButton";
            this.cycleButton.Size = new System.Drawing.Size(75, 44);
            this.cycleButton.TabIndex = 2;
            this.cycleButton.Text = "Cycle Source";
            this.cycleButton.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(135, 73);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Cycle Max Temp (°C)";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.numericUpDown1.Location = new System.Drawing.Point(6, 19);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
            this.numericUpDown1.TabIndex = 3;
            this.numericUpDown1.Value = new decimal(new int[] {
            40,
            0,
            0,
            0});
            // 
            // readButton
            // 
            this.readButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.readButton.Location = new System.Drawing.Point(36, 552);
            this.readButton.Name = "readButton";
            this.readButton.Size = new System.Drawing.Size(75, 46);
            this.readButton.TabIndex = 3;
            this.readButton.Text = "Start Reading";
            this.readButton.UseVisualStyleBackColor = true;
            this.readButton.Click += new System.EventHandler(this.toggleReading);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 667);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(521, 140);
            this.tableLayoutPanel6.TabIndex = 2;
            // 
            // SourceTabView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "SourceTabView";
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.tempGraph)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.cryoGroup.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cryoSwitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cryoLED)).EndInit();
            this.heaterGroup.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.heaterSwitch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.heaterLED)).EndInit();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private NationalInstruments.UI.WindowsForms.ScatterGraph tempGraph;
        private NationalInstruments.UI.ScatterPlot scatterPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox cryoGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private NationalInstruments.UI.WindowsForms.Switch cryoSwitch;
        private NationalInstruments.UI.WindowsForms.Led cryoLED;
        private System.Windows.Forms.GroupBox heaterGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private NationalInstruments.UI.WindowsForms.Switch heaterSwitch;
        private NationalInstruments.UI.WindowsForms.Led heaterLED;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.Button cycleButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Button readButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;

    }
}
