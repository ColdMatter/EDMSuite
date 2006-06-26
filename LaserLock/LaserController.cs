using System;
using System.Collections.Generic;
using System.Text;
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
    class LaserController
    {

        private double UPPER_VOLTAGE_LIMIT = 10.0;
        private double LOWER_VOLTAGE_LIMIT = -10.0;
        
        private ScanMaster.Controller scanMaster;
        private ScanMaster.Analyze.GaussianFitter fitter;
        private MainForm ui;

        private double voltage = 0.0;

        public LaserController(MainForm form)
        {
            ui = form;
            
            // ask the remoting system for access to ScanMaster 
            RemotingConfiguration.RegisterWellKnownClientType(
                Type.GetType("ScanMaster.Controller, ScanMaster"),
                "tcp://localhost:1170/controller.rem"
                );

            scanMaster = new ScanMaster.Controller();
            fitter = new ScanMaster.Analyze.GaussianFitter();
        }
        
        public void Park()
        {
            double[] fitresult;
            ui.AddToTextBox("Attempting to park...\n");
            try
            {
                scanMaster.AcquireAndWait(1);
                Scan scan = scanMaster.DataStore.AverageScan;
                if (scan.Points.Count != 0)
                {
                    fitresult = FitSpectrum(scan);
                    double centreVoltage = fitresult[2];
                    double scanStart = (double)scanMaster.GetOutputSetting("start");
                    double scanEnd = (double)scanMaster.GetOutputSetting("end");
                    if (centreVoltage > scanStart && centreVoltage < scanEnd)
                    {
                        if (!Environs.Debug)
                        {
                            RampToVoltage(centreVoltage);
                            ui.AddToTextBox("Parked.\n");
                        }
                        else ui.AddToTextBox("Ramping to " + centreVoltage + " volts. \n");
                    }
                    else ui.AddToTextBox("Failed - Unable to locate the resonance.\n");
                }
                else ui.AddToTextBox("Failed - Nothing to fit.\n");
            }
            catch (System.Net.Sockets.SocketException)
            {
                ui.AddToTextBox("The connection to ScanMaster was refused. Make sure that ScanMaster is running.");
            }
        }

        private void RampToVoltage(double v)
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

        private double[] FitSpectrum(Scan s)
        {
            double[] xDat = s.ScanParameterArray;
            double scanStart = xDat[0];
            double scanEnd = xDat[xDat.Length - 1];
            TOF avTof = (TOF)s.GetGatedAverageOnShot(scanStart, scanEnd).TOFs[0];
            double gateStart = avTof.GateStartTime;
            double gateEnd = avTof.GateStartTime + avTof.Length * avTof.ClockPeriod;
            double[] yDat = s.GetTOFOnIntegralArray(0, gateStart, gateEnd);
            fitter.Fit(xDat, yDat, fitter.SuggestParameters(xDat, yDat, scanStart, scanEnd));
            string report = fitter.ParameterReport;

            string[] tokens = report.Split(' ');

            double[] fitresult = new double[4];
            for (int i = 0; i < fitresult.Length; i++) fitresult[i] = Convert.ToDouble(tokens[2 * i + 1]);

            return fitresult;

        }

        public double Voltage
        {
            get
            {
                return voltage;
            }
            set 
            {
                if (value >= LOWER_VOLTAGE_LIMIT && value <= UPPER_VOLTAGE_LIMIT)
                {
                    voltage = value;
                    RampToVoltage(value);
                }
                else
                {
                    // do nothing
                }
            }
        }

    }
}
