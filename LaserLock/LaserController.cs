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
using DAQ.Remoting;
using System.Windows.Forms;

namespace LaserLock
{
    public class LaserController : MarshalByRefObject
    {

        private const double UPPER_VOLTAGE_LIMIT = 10.0;
        private const double LOWER_VOLTAGE_LIMIT = -10.0;
        private const int SAMPLES_PER_READ = 10;
        private const int SLEEPING_TIME = 500;
        private const int LATENCY = 10000;
        
        private ScanMaster.Controller scanMaster;
        private ScanMaster.Analyze.GaussianFitter fitter;
        private DecelerationHardwareControl.Controller hardwareControl;
        
        private MainForm ui;

        private Task outputTask;
        private AnalogOutputChannel laserChannel;
        private AnalogSingleChannelWriter laserWriter;

        private Task inputTask;
        private AnalogInputChannel cavityChannel;
        private AnalogSingleChannelReader cavityReader;

        public enum ControllerState { free, busy, stopping };
        private ControllerState status = ControllerState.free;

        private double voltage = 0.0;
        private double[] latestData;

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public void Start()
        {
            ui = new MainForm();
            ui.controller = this;

            // get access to ScanMaster and the DecelerationHardwareController
            RemotingHelper.ConnectScanMaster();
            RemotingHelper.ConnectDecelerationHardwareControl();

            scanMaster = new ScanMaster.Controller();
            hardwareControl = new DecelerationHardwareControl.Controller();
            fitter = new ScanMaster.Analyze.GaussianFitter();

           
            if (!Environs.Debug)
            {
                outputTask = new Task("LaserControllerOutput");
                laserChannel =
                        (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["laser"];
                laserChannel.AddToTask(outputTask, -10, 10);
                outputTask.Control(TaskAction.Verify);
                laserWriter = new AnalogSingleChannelWriter(outputTask.Stream);

                inputTask = new Task("LaserControllerInput");
                cavityChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["lock"];
                cavityChannel.AddToTask(inputTask, -10, 10);
                cavityReader = new AnalogSingleChannelReader(inputTask.Stream);
            }

            Application.Run(ui);
        }

        public ControllerState Status
        {
            get { return status; }
            set { status = value; }
        }
        
        public void Park()
        {
            double[] fitresult;
            ui.AddToTextBox("Attempting to park..." + Environment.NewLine);
            status = ControllerState.busy;
            try
            {
                scanMaster.AcquireAndWait(1);
                voltage = 0.0;
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
                            ui.AddToTextBox("Parked at " + centreVoltage + " volts." + Environment.NewLine);
                        }
                        else ui.AddToTextBox("Ramping to " + centreVoltage + " volts. \n");
                        ui.SetOutputVoltageNumericEditorValue(centreVoltage);
                    }
                    else ui.AddToTextBox("Failed - Unable to locate the resonance." + Environment.NewLine);
                }
                else ui.AddToTextBox("Failed - Nothing to fit." + Environment.NewLine);
            }
            catch (System.Net.Sockets.SocketException)
            {
                ui.AddToTextBox("The connection to ScanMaster was refused. Make sure that ScanMaster is running." + Environment.NewLine);
            }
            status = ControllerState.free;
        }

        public void Lock()
        {
            double averageValue = 0;
            Random r = new Random();
            status = ControllerState.busy;
            
            hardwareControl.LaserLocked = true;
            while (status == ControllerState.busy)
            {
                if (!Environs.Debug)
                {
                    laserChannel.Blocked = true;
                    if (hardwareControl.AnalogInputsAvailable)
                    {
                        inputTask.Start();
                        latestData = cavityReader.ReadMultiSample(SAMPLES_PER_READ);
                        inputTask.Stop();
                        foreach (double d in latestData) averageValue += d;
                        averageValue = averageValue / SAMPLES_PER_READ;
                        hardwareControl.UpdateLockCavityData(averageValue);
                    }
                    else
                    {
                        averageValue = hardwareControl.LastCavityData;
                    }
                }            
                else
                {
                    averageValue = r.NextDouble();
                    hardwareControl.UpdateLockCavityData(averageValue);
                }
                if (hardwareControl.TimeSinceLastCavityRead < LATENCY) ui.AddToTextBox(averageValue + Environment.NewLine);    
                Thread.Sleep(SLEEPING_TIME);
            }
            status = ControllerState.free;
            hardwareControl.LaserLocked = false;
            if (!Environs.Debug) laserChannel.Blocked = false;
        }

        private void RampToVoltage(double v)
        {
            int steps = 20;
            int delayAtEachStep = 50;
            double stepsize = (v-voltage) / steps;
           
            for (int i = 1; i <= steps; i++)
            {
                voltage += stepsize;
                laserWriter.WriteSingleSample(true, voltage);
                Thread.Sleep(delayAtEachStep);
            }
            outputTask.Control(TaskAction.Unreserve);
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
                    if (!Environs.Debug)
                    {

                        voltage = value;
                        laserWriter.WriteSingleSample(true, voltage);
                        outputTask.Control(TaskAction.Unreserve);
                    }
                    else
                    {
                        ui.AddToTextBox("Voltage changed to " + ui.OutputVoltageNumericEditorValue + Environment.NewLine);
                    }

                }
                else
                {
                    // do nothing
                }
            }
        }

    }
}
