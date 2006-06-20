using NationalInstruments;
using NationalInstruments.UI;
using NationalInstruments.UI.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using Data;
using Data.Scans;
using NationalInstruments.DAQmx;
using DAQ.Environment;
using DAQ.HAL;


namespace LaserLock
{
    public partial class MainForm : Form
    {

        private ScanMaster.Controller scanMaster;
        private ScanMaster.Analyze.GaussianFitter fitter;
        
        public MainForm()
        {
            InitializeComponent();

            // ask the remoting system for access to ScanMaster 
            RemotingConfiguration.RegisterWellKnownClientType(
                Type.GetType("ScanMaster.Controller, ScanMaster"),
                "tcp://localhost:1170/controller.rem"
                );

            scanMaster = new ScanMaster.Controller();
            fitter = new ScanMaster.Analyze.GaussianFitter();
           
        }

        
        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private double[] FitSpectrum(Scan s)
        {
            double[] xDat = s.ScanParameterArray;
            double scanStart = xDat[0];
            double scanEnd = xDat[xDat.Length - 1];
            TOF avTof = (TOF)s.GetGatedAverageOnShot(scanStart,scanEnd).TOFs[0];
            double gateStart = avTof.GateStartTime;
            double gateEnd = avTof.GateStartTime + avTof.Length*avTof.ClockPeriod;
            double[] yDat = s.GetTOFOnIntegralArray(0, gateStart, gateEnd);
            fitter.Fit(xDat, yDat, fitter.SuggestParameters(xDat, yDat, scanStart, scanEnd));
            string report = fitter.ParameterReport;
            
            string[] tokens = report.Split(' ');

            double[] fitresult = new double[4];
            for (int i = 0; i < fitresult.Length; i++) fitresult[i] = Convert.ToDouble(tokens[2 * i + 1]);
          
            return fitresult;
            
        }

        private void parkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double[] fitresult;
            try
            {
                scanMaster.AcquireAndWait(1);
                Scan scan = scanMaster.DataStore.AverageScan;
                if (scan.Points.Count != 0)
                {
                    fitresult = FitSpectrum(scan);
                    double centreVoltage = fitresult[2];
                    double scanStart = (double) scanMaster.GetOutputSetting("start");
                    double scanEnd = (double) scanMaster.GetOutputSetting("end");
                    if (centreVoltage > scanStart && centreVoltage < scanEnd)
                    {
                        if (!Environs.Debug) rampToVoltage(centreVoltage);
                        else textBox1.AppendText("Ramping to " + centreVoltage + " volts. \n");
                    }
                    else textBox1.AppendText("Failed - Unable to locate the resonance");
                }
                else textBox1.AppendText("Failed - Nothing to fit.\n");
            }
            catch (System.Net.Sockets.SocketException)
            {
                MessageBox.Show("The connection to ScanMaster was refused. \n Make sure that ScanMaster is running.", "Connection error", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
            }
        }

        private void rampToVoltage(double v)
        {
            int steps = 20;
            int delayAtEachStep = 50;
            double stepsize = v / steps;
            Task outputTask = new Task("analog output");
            AnalogOutputChannel oc =
                    (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["laser"];
            oc.AddToTask(outputTask, -10, 10);
            AnalogSingleChannelWriter writer = new AnalogSingleChannelWriter(outputTask.Stream);
            for (int i = 1; i <= steps; i++)
            {
                writer.WriteSingleSample(true, stepsize * i);
                Thread.Sleep(delayAtEachStep);
            }
            outputTask.Dispose();
            
        }
    }
}