using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace EDMLeakageMonitor
{
	/// <summary>
	/// Summary description for TextReadout.
	/// </summary>
	public class TextReadoutDialog : System.Windows.Forms.Form
	{
		//Arraylists for rolling average
		private ArrayList masterArrayList;
		private int pointsToAverage;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.GroupBox groupBoxCurrentTextBoxes;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBoxMonitor1;
		private System.Windows.Forms.TextBox textBoxMonitor2;
		private System.Windows.Forms.TextBox textBoxMonitor4;
		private System.Windows.Forms.TextBox textBoxMonitor3;
		private System.Windows.Forms.Label monitorLabel1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox textBoxAverage4;
		private System.Windows.Forms.TextBox textBoxAverage3;
		private System.Windows.Forms.TextBox textBoxAverage2;
		private System.Windows.Forms.TextBox textBoxAverage1;
		private System.Windows.Forms.TextBox textBoxPointsToAverage;
		private System.Windows.Forms.GroupBox groupBoxPointsToAverage;
		private System.Windows.Forms.Button buttonSet;
		private System.Windows.Forms.Button buttonHelp;
		private System.Windows.Forms.Label na1;
		private System.Windows.Forms.Label na2;
		private System.Windows.Forms.Label na4;
		private System.Windows.Forms.Label na3;
		private System.Windows.Forms.Label na6;
		private System.Windows.Forms.Label na5;
		private System.Windows.Forms.Label na8;
		private System.Windows.Forms.Label na7;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public TextReadoutDialog()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			
			//The masterArrayList contains an Arraylist for each leakage monitor.
			//This ArrayList is averaged to get the moving average.
			masterArrayList = new ArrayList();
			for(int i = 0;i<4;i++)
			{
				masterArrayList.Add(new ArrayList());
			}

			handleAvgTBxMsg("Wait...");		
			//Default pointsToAverage;
			pointsToAverage = 10;
			textBoxPointsToAverage.Text = pointsToAverage.ToString();


		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.groupBoxCurrentTextBoxes = new System.Windows.Forms.GroupBox();
			this.na1 = new System.Windows.Forms.Label();
			this.textBoxAverage4 = new System.Windows.Forms.TextBox();
			this.textBoxAverage3 = new System.Windows.Forms.TextBox();
			this.textBoxAverage2 = new System.Windows.Forms.TextBox();
			this.textBoxAverage1 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.monitorLabel1 = new System.Windows.Forms.Label();
			this.textBoxMonitor4 = new System.Windows.Forms.TextBox();
			this.textBoxMonitor3 = new System.Windows.Forms.TextBox();
			this.textBoxMonitor2 = new System.Windows.Forms.TextBox();
			this.textBoxMonitor1 = new System.Windows.Forms.TextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.textBoxPointsToAverage = new System.Windows.Forms.TextBox();
			this.groupBoxPointsToAverage = new System.Windows.Forms.GroupBox();
			this.buttonSet = new System.Windows.Forms.Button();
			this.buttonHelp = new System.Windows.Forms.Button();
			this.na2 = new System.Windows.Forms.Label();
			this.na4 = new System.Windows.Forms.Label();
			this.na3 = new System.Windows.Forms.Label();
			this.na6 = new System.Windows.Forms.Label();
			this.na5 = new System.Windows.Forms.Label();
			this.na8 = new System.Windows.Forms.Label();
			this.na7 = new System.Windows.Forms.Label();
			this.groupBoxCurrentTextBoxes.SuspendLayout();
			this.groupBoxPointsToAverage.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(96, 24);
			this.textBox1.Name = "textBox1";
			this.textBox1.TabIndex = 0;
			this.textBox1.Text = "textBox1";
			// 
			// groupBoxCurrentTextBoxes
			// 
			this.groupBoxCurrentTextBoxes.BackColor = System.Drawing.SystemColors.Control;
			this.groupBoxCurrentTextBoxes.Controls.Add(this.na6);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.na5);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.na8);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.na7);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.na4);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.na3);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.na2);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.na1);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.textBoxAverage4);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.textBoxAverage3);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.textBoxAverage2);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.textBoxAverage1);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.label3);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.label2);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.label1);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.monitorLabel1);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.textBoxMonitor4);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.textBoxMonitor3);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.textBoxMonitor2);
			this.groupBoxCurrentTextBoxes.Controls.Add(this.textBoxMonitor1);
			this.groupBoxCurrentTextBoxes.Location = new System.Drawing.Point(8, 8);
			this.groupBoxCurrentTextBoxes.Name = "groupBoxCurrentTextBoxes";
			this.groupBoxCurrentTextBoxes.Size = new System.Drawing.Size(280, 240);
			this.groupBoxCurrentTextBoxes.TabIndex = 1;
			this.groupBoxCurrentTextBoxes.TabStop = false;
			this.groupBoxCurrentTextBoxes.Text = "Current Leakage";
			// 
			// na1
			// 
			this.na1.Location = new System.Drawing.Point(248, 168);
			this.na1.Name = "na1";
			this.na1.Size = new System.Drawing.Size(24, 23);
			this.na1.TabIndex = 12;
			this.na1.Text = "nA";
			// 
			// textBoxAverage4
			// 
			this.textBoxAverage4.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxAverage4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxAverage4.ForeColor = System.Drawing.Color.RoyalBlue;
			this.textBoxAverage4.Location = new System.Drawing.Point(144, 200);
			this.textBoxAverage4.Name = "textBoxAverage4";
			this.textBoxAverage4.ReadOnly = true;
			this.textBoxAverage4.Size = new System.Drawing.Size(96, 29);
			this.textBoxAverage4.TabIndex = 11;
			this.textBoxAverage4.Text = "";
			// 
			// textBoxAverage3
			// 
			this.textBoxAverage3.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxAverage3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxAverage3.ForeColor = System.Drawing.Color.RoyalBlue;
			this.textBoxAverage3.Location = new System.Drawing.Point(144, 88);
			this.textBoxAverage3.Name = "textBoxAverage3";
			this.textBoxAverage3.ReadOnly = true;
			this.textBoxAverage3.Size = new System.Drawing.Size(96, 29);
			this.textBoxAverage3.TabIndex = 10;
			this.textBoxAverage3.Text = "";
			this.textBoxAverage3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBoxAverage2
			// 
			this.textBoxAverage2.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxAverage2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxAverage2.ForeColor = System.Drawing.Color.RoyalBlue;
			this.textBoxAverage2.Location = new System.Drawing.Point(8, 200);
			this.textBoxAverage2.Name = "textBoxAverage2";
			this.textBoxAverage2.ReadOnly = true;
			this.textBoxAverage2.TabIndex = 9;
			this.textBoxAverage2.Text = "";
			this.textBoxAverage2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBoxAverage1
			// 
			this.textBoxAverage1.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxAverage1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxAverage1.ForeColor = System.Drawing.Color.RoyalBlue;
			this.textBoxAverage1.Location = new System.Drawing.Point(8, 88);
			this.textBoxAverage1.Name = "textBoxAverage1";
			this.textBoxAverage1.ReadOnly = true;
			this.textBoxAverage1.TabIndex = 8;
			this.textBoxAverage1.Text = "";
			this.textBoxAverage1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(8, 136);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(96, 24);
			this.label3.TabIndex = 7;
			this.label3.Text = "North G";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(144, 136);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(96, 24);
			this.label2.TabIndex = 6;
			this.label2.Text = "South G";
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(144, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(96, 24);
			this.label1.TabIndex = 5;
			this.label1.Text = "South C";
			// 
			// monitorLabel1
			// 
			this.monitorLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.monitorLabel1.Location = new System.Drawing.Point(8, 24);
			this.monitorLabel1.Name = "monitorLabel1";
			this.monitorLabel1.Size = new System.Drawing.Size(96, 24);
			this.monitorLabel1.TabIndex = 4;
			this.monitorLabel1.Text = "North C";
			// 
			// textBoxMonitor4
			// 
			this.textBoxMonitor4.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxMonitor4.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxMonitor4.ForeColor = System.Drawing.Color.Black;
			this.textBoxMonitor4.Location = new System.Drawing.Point(144, 160);
			this.textBoxMonitor4.Name = "textBoxMonitor4";
			this.textBoxMonitor4.ReadOnly = true;
			this.textBoxMonitor4.Size = new System.Drawing.Size(96, 29);
			this.textBoxMonitor4.TabIndex = 3;
			this.textBoxMonitor4.Text = "";
			this.textBoxMonitor4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBoxMonitor3
			// 
			this.textBoxMonitor3.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxMonitor3.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxMonitor3.ForeColor = System.Drawing.Color.Black;
			this.textBoxMonitor3.Location = new System.Drawing.Point(144, 48);
			this.textBoxMonitor3.Name = "textBoxMonitor3";
			this.textBoxMonitor3.ReadOnly = true;
			this.textBoxMonitor3.Size = new System.Drawing.Size(96, 29);
			this.textBoxMonitor3.TabIndex = 2;
			this.textBoxMonitor3.Text = "";
			this.textBoxMonitor3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBoxMonitor2
			// 
			this.textBoxMonitor2.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxMonitor2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxMonitor2.ForeColor = System.Drawing.Color.Black;
			this.textBoxMonitor2.Location = new System.Drawing.Point(8, 160);
			this.textBoxMonitor2.Name = "textBoxMonitor2";
			this.textBoxMonitor2.ReadOnly = true;
			this.textBoxMonitor2.TabIndex = 1;
			this.textBoxMonitor2.Text = "";
			this.textBoxMonitor2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// textBoxMonitor1
			// 
			this.textBoxMonitor1.BackColor = System.Drawing.SystemColors.Control;
			this.textBoxMonitor1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxMonitor1.ForeColor = System.Drawing.Color.Black;
			this.textBoxMonitor1.Location = new System.Drawing.Point(8, 48);
			this.textBoxMonitor1.Name = "textBoxMonitor1";
			this.textBoxMonitor1.ReadOnly = true;
			this.textBoxMonitor1.TabIndex = 0;
			this.textBoxMonitor1.Text = "";
			this.textBoxMonitor1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(8, 312);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(152, 24);
			this.button1.TabIndex = 2;
			this.button1.Text = "Close";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// textBoxPointsToAverage
			// 
			this.textBoxPointsToAverage.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.textBoxPointsToAverage.Location = new System.Drawing.Point(16, 16);
			this.textBoxPointsToAverage.Name = "textBoxPointsToAverage";
			this.textBoxPointsToAverage.TabIndex = 13;
			this.textBoxPointsToAverage.Text = "";
			// 
			// groupBoxPointsToAverage
			// 
			this.groupBoxPointsToAverage.Controls.Add(this.buttonSet);
			this.groupBoxPointsToAverage.Controls.Add(this.textBoxPointsToAverage);
			this.groupBoxPointsToAverage.Location = new System.Drawing.Point(8, 256);
			this.groupBoxPointsToAverage.Name = "groupBoxPointsToAverage";
			this.groupBoxPointsToAverage.Size = new System.Drawing.Size(280, 48);
			this.groupBoxPointsToAverage.TabIndex = 13;
			this.groupBoxPointsToAverage.TabStop = false;
			this.groupBoxPointsToAverage.Text = "PointsToAverage";
			// 
			// buttonSet
			// 
			this.buttonSet.Location = new System.Drawing.Point(168, 16);
			this.buttonSet.Name = "buttonSet";
			this.buttonSet.Size = new System.Drawing.Size(104, 24);
			this.buttonSet.TabIndex = 13;
			this.buttonSet.Text = "Set";
			this.buttonSet.Click += new System.EventHandler(this.buttonSet_Click);
			// 
			// buttonHelp
			// 
			this.buttonHelp.Location = new System.Drawing.Point(176, 312);
			this.buttonHelp.Name = "buttonHelp";
			this.buttonHelp.Size = new System.Drawing.Size(104, 24);
			this.buttonHelp.TabIndex = 14;
			this.buttonHelp.Text = "Help";
			this.buttonHelp.Click += new System.EventHandler(this.buttonHelp_Click);
			// 
			// na2
			// 
			this.na2.Location = new System.Drawing.Point(248, 208);
			this.na2.Name = "na2";
			this.na2.Size = new System.Drawing.Size(24, 23);
			this.na2.TabIndex = 13;
			this.na2.Text = "nA";
			// 
			// na4
			// 
			this.na4.Location = new System.Drawing.Point(248, 96);
			this.na4.Name = "na4";
			this.na4.Size = new System.Drawing.Size(24, 23);
			this.na4.TabIndex = 15;
			this.na4.Text = "nA";
			// 
			// na3
			// 
			this.na3.Location = new System.Drawing.Point(248, 56);
			this.na3.Name = "na3";
			this.na3.Size = new System.Drawing.Size(24, 23);
			this.na3.TabIndex = 14;
			this.na3.Text = "nA";
			// 
			// na6
			// 
			this.na6.Location = new System.Drawing.Point(112, 96);
			this.na6.Name = "na6";
			this.na6.Size = new System.Drawing.Size(24, 23);
			this.na6.TabIndex = 19;
			this.na6.Text = "nA";
			// 
			// na5
			// 
			this.na5.Location = new System.Drawing.Point(112, 56);
			this.na5.Name = "na5";
			this.na5.Size = new System.Drawing.Size(24, 23);
			this.na5.TabIndex = 18;
			this.na5.Text = "nA";
			// 
			// na8
			// 
			this.na8.Location = new System.Drawing.Point(112, 208);
			this.na8.Name = "na8";
			this.na8.Size = new System.Drawing.Size(24, 23);
			this.na8.TabIndex = 17;
			this.na8.Text = "nA";
			// 
			// na7
			// 
			this.na7.Location = new System.Drawing.Point(112, 168);
			this.na7.Name = "na7";
			this.na7.Size = new System.Drawing.Size(24, 23);
			this.na7.TabIndex = 16;
			this.na7.Text = "nA";
			// 
			// TextReadoutDialog
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(298, 344);
			this.ControlBox = false;
			this.Controls.Add(this.buttonHelp);
			this.Controls.Add(this.groupBoxPointsToAverage);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.groupBoxCurrentTextBoxes);
			this.Controls.Add(this.textBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "TextReadoutDialog";
			this.Text = "Leakage Current Monitor - Text Readout";
			this.groupBoxCurrentTextBoxes.ResumeLayout(false);
			this.groupBoxPointsToAverage.ResumeLayout(false);
			this.ResumeLayout(false);

		}


		#endregion

		
		public void populateTextBoxes(ArrayList data)
		{
			
			textBoxMonitor1.Text=((double)data[0]).ToString();
			textBoxMonitor2.Text=((double)data[1]).ToString();
			textBoxMonitor3.Text=((double)data[2]).ToString();
			textBoxMonitor4.Text=((double)data[3]).ToString();
			
			for(int i=0;i<4;i++)
			{
				(masterArrayList[i])=manageArrayListForAverage(((ArrayList)masterArrayList[i]),(double)data[i]);

			}
			
			if(((ArrayList)masterArrayList[1]).Count>=pointsToAverage)
			{
				textBoxAverage1.Text=movingAverage((ArrayList)masterArrayList[0]).ToString();
				textBoxAverage2.Text=movingAverage((ArrayList)masterArrayList[1]).ToString();
				textBoxAverage3.Text=movingAverage((ArrayList)masterArrayList[2]).ToString();
				textBoxAverage4.Text=movingAverage((ArrayList)masterArrayList[3]).ToString();
			}

		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.Hide();		
		}

		private ArrayList manageArrayListForAverage(ArrayList relevantAL2, double newCurrent)
		{
			if(relevantAL2.Count>=pointsToAverage)
			{
				relevantAL2.RemoveAt(0);
			}
			relevantAL2.Add(newCurrent);
			return relevantAL2;
		}

		private double movingAverage(ArrayList relevantAl)
		{
			double sum = new double();
			foreach(double d in relevantAl)
			{
				sum=sum+d;
			}
			return (sum/(Convert.ToDouble(relevantAl.Count)));
		}

		private void buttonSet_Click(object sender, System.EventArgs e)
		{ 
			masterArrayList.Clear();
			masterArrayList = new ArrayList();
			for(int i = 0;i<4;i++)
			{
				masterArrayList.Add(new ArrayList());
			}
			
			try
			{
				pointsToAverage = Convert.ToInt32(textBoxPointsToAverage.Text);
			}
			catch
			{
				pointsToAverage=1;
				textBoxPointsToAverage.Text="1";
			}

			handleAvgTBxMsg("Wait...");

			


		}

		private void handleAvgTBxMsg(string message)
		{
			textBoxAverage1.TextAlign = HorizontalAlignment.Center;  
			textBoxAverage2.TextAlign = HorizontalAlignment.Center;  
			textBoxAverage3.TextAlign = HorizontalAlignment.Center;  
			textBoxAverage4.TextAlign = HorizontalAlignment.Center;  
			textBoxAverage1.Text = message;
			textBoxAverage2.Text = message;
			textBoxAverage3.Text = message;
			textBoxAverage4.Text = message;
			textBoxAverage1.TextAlign = HorizontalAlignment.Left;  
			textBoxAverage2.TextAlign = HorizontalAlignment.Left;  
			textBoxAverage3.TextAlign = HorizontalAlignment.Left;  
			textBoxAverage4.TextAlign = HorizontalAlignment.Left;
		}

		private void buttonHelp_Click(object sender, System.EventArgs e)
		{
			MessageBox.Show ("The light blue numbers are the averaged currents. Enter a number of Points to average to control precision. Pressing set will reset the averaging, regardless as to whether the number of points to average has changed.", "Leakage Current Monitor", 
				MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

		}



		



		

	}
}
