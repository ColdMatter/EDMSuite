using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

using System.IO;
using DAQ;
using DAQ.Environment;
using DAQ.HAL;


namespace WaveMeter
{
    public partial class ControlWindow : Form
    {
        public ControlWindow()
        {
            InitializeComponent();
        }

        private ControlWindow window;
        public Controller controller;

        public void addDataToGraph(double[] xdata, double[] ydata)
        {
            scatterPlot1.PlotXY(xdata, ydata);

        }
        public void addextradata(double[] xdata, double[] ydata)
        {
            scatterPlot2.PlotXY(xdata, ydata);
        }
        public void addmoredata(double[] xdata, double[] ydata)
        {
            scatterPlot3.PlotXY(xdata, ydata);
        }



         
        #region Camera Control

        private void button1_Click(object sender, EventArgs e)
        {   
            
            controller.CameraSnapshot();
            controller.displyData();
            
        }


        private void button2_Click(object sender, EventArgs e)
        {
            controller.CameraStream();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            controller.StopCameraStream();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            
            controller.Displyfit();
        }

        

        private void ControlWindow_Load(object sender, EventArgs e)
        {
           
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
         #endregion

        private void scatterGraph1_PlotDataChanged_1(object sender, EventArgs e)
        {
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            double [] distance = new double[2000];
            for (int i = 0; i < 20; i++)
            {
             

                controller.CameraSnapshot();
                controller.displyData();

                distance[i]=controller.Getthedistance();

                
            }
            controller.calculation(distance);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void scatterGraph1_PlotDataChanged_1(object sender, NationalInstruments.UI.XYPlotDataChangedEventArgs e)
        {

        }





    }
    
}
