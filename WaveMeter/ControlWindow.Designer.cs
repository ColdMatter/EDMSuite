namespace WaveMeter
{
    partial class ControlWindow
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.scatterGraph1 = new NationalInstruments.UI.WindowsForms.ScatterGraph();
            this.scatterPlot1 = new NationalInstruments.UI.ScatterPlot();
            this.xAxis1 = new NationalInstruments.UI.XAxis();
            this.yAxis1 = new NationalInstruments.UI.YAxis();
            this.scatterPlot2 = new NationalInstruments.UI.ScatterPlot();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.button6 = new System.Windows.Forms.Button();
            this.scatterPlot3 = new NationalInstruments.UI.ScatterPlot();
            ((System.ComponentModel.ISupportInitialize)(this.scatterGraph1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(984, 13);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(87, 47);
            this.button1.TabIndex = 1;
            this.button1.Text = "Snap";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(984, 76);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(87, 47);
            this.button2.TabIndex = 3;
            this.button2.Text = "Stream";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(984, 142);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(87, 47);
            this.button3.TabIndex = 4;
            this.button3.Text = "Stop";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // scatterGraph1
            // 
            this.scatterGraph1.Location = new System.Drawing.Point(12, 0);
            this.scatterGraph1.Name = "scatterGraph1";
            this.scatterGraph1.Plots.AddRange(new NationalInstruments.UI.ScatterPlot[] {
            this.scatterPlot1,
            this.scatterPlot2,
            this.scatterPlot3});
            this.scatterGraph1.Size = new System.Drawing.Size(946, 638);
            this.scatterGraph1.TabIndex = 5;
            this.scatterGraph1.UseColorGenerator = true;
            this.scatterGraph1.XAxes.AddRange(new NationalInstruments.UI.XAxis[] {
            this.xAxis1});
            this.scatterGraph1.YAxes.AddRange(new NationalInstruments.UI.YAxis[] {
            this.yAxis1});
            this.scatterGraph1.PlotDataChanged += new NationalInstruments.UI.XYPlotDataChangedEventHandler(this.scatterGraph1_PlotDataChanged_1);
            // 
            // scatterPlot1
            // 
            this.scatterPlot1.XAxis = this.xAxis1;
            this.scatterPlot1.YAxis = this.yAxis1;
            // 
            // scatterPlot2
            // 
            this.scatterPlot2.XAxis = this.xAxis1;
            this.scatterPlot2.YAxis = this.yAxis1;
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(984, 205);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(87, 47);
            this.button4.TabIndex = 6;
            this.button4.Text = "Fit";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(984, 271);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(87, 47);
            this.button5.TabIndex = 7;
            this.button5.Text = "Test";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(984, 340);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(87, 47);
            this.button6.TabIndex = 8;
            this.button6.Text = "Exit";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // scatterPlot3
            // 
            this.scatterPlot3.XAxis = this.xAxis1;
            this.scatterPlot3.YAxis = this.yAxis1;
            // 
            // ControlWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1083, 687);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.scatterGraph1);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "ControlWindow";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.ControlWindow_Load);
            ((System.ComponentModel.ISupportInitialize)(this.scatterGraph1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private NationalInstruments.UI.WindowsForms.ScatterGraph scatterGraph1;
        private NationalInstruments.UI.ScatterPlot scatterPlot1;
        private NationalInstruments.UI.XAxis xAxis1;
        private NationalInstruments.UI.YAxis yAxis1;
        private System.Windows.Forms.Button button4;
        private NationalInstruments.UI.ScatterPlot scatterPlot2;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.Button button6;
        private NationalInstruments.UI.ScatterPlot scatterPlot3;
    }
}

