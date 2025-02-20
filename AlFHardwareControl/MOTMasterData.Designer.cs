﻿
namespace AlFHardwareControl
{
    partial class MOTMasterData
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.sourceEnable = new System.Windows.Forms.CheckBox();
            this.RejectVal = new System.Windows.Forms.TextBox();
            this.RejectCondPicker = new System.Windows.Forms.ComboBox();
            this.RejectEnable = new System.Windows.Forms.CheckBox();
            this.NormSource = new System.Windows.Forms.ComboBox();
            this.Normalise = new System.Windows.Forms.CheckBox();
            this.fixY = new System.Windows.Forms.CheckBox();
            this.fixX = new System.Windows.Forms.CheckBox();
            this.dataGraph = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot3 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.scatterPlot4 = new NationalInstruments.UI.ScatterPlot();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.sourceEnable);
            this.groupBox1.Controls.Add(this.RejectVal);
            this.groupBox1.Controls.Add(this.RejectCondPicker);
            this.groupBox1.Controls.Add(this.RejectEnable);
            this.groupBox1.Controls.Add(this.NormSource);
            this.groupBox1.Controls.Add(this.Normalise);
            this.groupBox1.Controls.Add(this.fixY);
            this.groupBox1.Controls.Add(this.fixX);
            this.groupBox1.Location = new System.Drawing.Point(4, 270);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(589, 105);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Settings";
            // 
            // sourceEnable
            // 
            this.sourceEnable.AutoSize = true;
            this.sourceEnable.Location = new System.Drawing.Point(273, 43);
            this.sourceEnable.Name = "sourceEnable";
            this.sourceEnable.Size = new System.Drawing.Size(96, 17);
            this.sourceEnable.TabIndex = 7;
            this.sourceEnable.Text = "Enable Source";
            this.sourceEnable.UseVisualStyleBackColor = true;
            this.sourceEnable.CheckedChanged += new System.EventHandler(this.sourceEnable_CheckedChanged);
            // 
            // RejectVal
            // 
            this.RejectVal.Location = new System.Drawing.Point(509, 16);
            this.RejectVal.Name = "RejectVal";
            this.RejectVal.Size = new System.Drawing.Size(74, 20);
            this.RejectVal.TabIndex = 6;
            // 
            // RejectCondPicker
            // 
            this.RejectCondPicker.FormattingEnabled = true;
            this.RejectCondPicker.Location = new System.Drawing.Point(381, 16);
            this.RejectCondPicker.Name = "RejectCondPicker";
            this.RejectCondPicker.Size = new System.Drawing.Size(121, 21);
            this.RejectCondPicker.TabIndex = 5;
            // 
            // RejectEnable
            // 
            this.RejectEnable.AutoSize = true;
            this.RejectEnable.Location = new System.Drawing.Point(273, 20);
            this.RejectEnable.Name = "RejectEnable";
            this.RejectEnable.Size = new System.Drawing.Size(102, 17);
            this.RejectEnable.TabIndex = 4;
            this.RejectEnable.Text = "Enable rejection";
            this.RejectEnable.UseVisualStyleBackColor = true;
            this.RejectEnable.CheckedChanged += new System.EventHandler(this.RejectEnable_CheckedChanged);
            // 
            // NormSource
            // 
            this.NormSource.FormattingEnabled = true;
            this.NormSource.Location = new System.Drawing.Point(99, 63);
            this.NormSource.Name = "NormSource";
            this.NormSource.Size = new System.Drawing.Size(121, 21);
            this.NormSource.TabIndex = 3;
            this.NormSource.SelectedIndexChanged += new System.EventHandler(this.NormSource_SelectedIndexChanged);
            // 
            // Normalise
            // 
            this.Normalise.AutoSize = true;
            this.Normalise.Location = new System.Drawing.Point(7, 67);
            this.Normalise.Name = "Normalise";
            this.Normalise.Size = new System.Drawing.Size(72, 17);
            this.Normalise.TabIndex = 2;
            this.Normalise.Text = "Normalise";
            this.Normalise.UseVisualStyleBackColor = true;
            this.Normalise.CheckedChanged += new System.EventHandler(this.Normalise_CheckedChanged);
            // 
            // fixY
            // 
            this.fixY.AutoSize = true;
            this.fixY.Location = new System.Drawing.Point(7, 43);
            this.fixY.Name = "fixY";
            this.fixY.Size = new System.Drawing.Size(79, 17);
            this.fixY.TabIndex = 1;
            this.fixY.Text = "Fix Y range";
            this.fixY.UseVisualStyleBackColor = true;
            this.fixY.CheckedChanged += new System.EventHandler(this.fixY_CheckedChanged);
            // 
            // fixX
            // 
            this.fixX.AutoSize = true;
            this.fixX.Location = new System.Drawing.Point(7, 20);
            this.fixX.Name = "fixX";
            this.fixX.Size = new System.Drawing.Size(79, 17);
            this.fixX.TabIndex = 0;
            this.fixX.Text = "Fix X range";
            this.fixX.UseVisualStyleBackColor = true;
            this.fixX.CheckedChanged += new System.EventHandler(this.fixX_CheckedChanged);
            // 
            // dataGraph
            // 
            this.dataGraph.InteractionMode = ((NationalInstruments.UI.GraphInteractionModes)((((((((NationalInstruments.UI.GraphInteractionModes.ZoomX | NationalInstruments.UI.GraphInteractionModes.ZoomY) 
            | NationalInstruments.UI.GraphInteractionModes.ZoomAroundPoint) 
            | NationalInstruments.UI.GraphInteractionModes.PanX) 
            | NationalInstruments.UI.GraphInteractionModes.PanY) 
            | NationalInstruments.UI.GraphInteractionModes.DragCursor) 
            | NationalInstruments.UI.GraphInteractionModes.DragAnnotationCaption) 
            | NationalInstruments.UI.GraphInteractionModes.EditRange)));
            this.dataGraph.Location = new System.Drawing.Point(4, 4);
            this.dataGraph.Name = "dataGraph";
            this.dataGraph.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot3,
            this.scatterPlot4});
            this.dataGraph.Size = new System.Drawing.Size(589, 260);
            this.dataGraph.TabIndex = 2;
            this.dataGraph.UseColorGenerator = true;
            this.dataGraph.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.dataGraph.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            // 
            // scatterPlot3
            // 
            this.scatterPlot3.LineStyle = NationalInstruments.UI.LineStyle.Dot;
            this.scatterPlot3.PointColor = System.Drawing.Color.Lime;
            this.scatterPlot3.XAxis = this.xAxis1;
            this.scatterPlot3.YAxis = this.yAxis1;
            // 
            // xAxis1
            // 
            this.xAxis1.Caption = "Time [ms]";
            // 
            // yAxis1
            // 
            this.yAxis1.Caption = "Voltage [V]";
            // 
            // scatterPlot4
            // 
            this.scatterPlot4.LineColor = System.Drawing.Color.Lime;
            this.scatterPlot4.LineColorPrecedence = NationalInstruments.UI.ColorPrecedence.UserDefinedColor;
            this.scatterPlot4.XAxis = this.xAxis1;
            this.scatterPlot4.YAxis = this.yAxis1;
            // 
            // MOTMasterData
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGraph);
            this.Controls.Add(this.groupBox1);
            this.Name = "MOTMasterData";
            this.Size = new System.Drawing.Size(596, 378);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGraph)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox fixX;
        private NationalInstruments.UI.WindowsForms.ScatterGraph dataGraph;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.CheckBox fixY;
        private System.Windows.Forms.ComboBox NormSource;
        private System.Windows.Forms.CheckBox Normalise;
        private System.Windows.Forms.TextBox RejectVal;
        private System.Windows.Forms.ComboBox RejectCondPicker;
        private System.Windows.Forms.CheckBox RejectEnable;
        public System.Windows.Forms.CheckBox sourceEnable;
        private NationalInstruments.UI.ScatterPlot scatterPlot3;
        private NationalInstruments.UI.ScatterPlot scatterPlot4;
    }
}
