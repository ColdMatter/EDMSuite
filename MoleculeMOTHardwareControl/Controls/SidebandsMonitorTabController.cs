using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;
using DAQ.TransferCavityLock;
using DAQ.HAL;
using DAQ.Environment;

namespace MoleculeMOTHardwareControl.Controls
{
    public class SidebandsMonitorTabController : GenericController
    {
        protected SideBandsMonitorTabView window;

        protected override GenericView CreateControl()
        {
            return new SideBandsMonitorTabView();
        }

        private TransferCavityLockable TCLHelper = new DAQMxTCLHelperSWTimed(
            "cavity", "analogTrigger3", "laser", "p2", "p1", "analogTrigger2", "cavityTriggerOut"
        );

        private static Hashtable calibrations = Environs.Hardware.Calibrations;

        public bool sidebandMonitorRunning = false;
        private string[] sidebandChannelList = { "cavityVoltage", "mot606", "mot628V1", "slowing531", "slowing628V1" };
        private Task sidebandMonitorTask = new Task("sidebandMonitor");
        private AnalogMultiChannelReader sidebandReader;
        private int sidebandMonitorSampleRate = 2500;
        private int sidebandMonitorSamplesPerChannel = 4000;
        private int waitBetweenReads = 1000;

        private bool analogsAvailable;
        private double lastCavityData;
        private double lastrefCavityData;
        private DateTime cavityTimestamp;
        private DateTime refcavityTimestamp;
        private double laserFrequencyControlVoltage;

        private Dictionary<string, Task> analogTasks = new Dictionary<string, Task>();

        private Task motAOMFreqOutputTask = new Task("MOTAOMFrequencyOutput");
        private AnalogOutputChannel motAOMFreqChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["v00Frequency"];
        private Task motAOMAmpOutputTask = new Task("MOTAOMAmplitudeOutput");
        private AnalogOutputChannel motAOMAmpChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["v00Intensity"];

        public AnalogSingleChannelWriter analogWriter;


        private AnalogInputChannel pressureSourceChamber = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["PressureSourceChamber"];
        private Task pressureMonitorTask = new Task();
        private AnalogInputChannel pressureRough = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["RoughVacuum"];
        private Task roughVacuumTask = new Task();
        private AnalogInputChannel voltageReference = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["VoltageReference"];
        private Task voltageReferenceTask = new Task();
        private AnalogInputChannel therm30KTemp = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["10KThermistor30KPlate"];
        private Task thermistor30KPlateTask = new Task();
        private AnalogInputChannel shieldTemp = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["30KShield"];
        private Task shieldTask = new Task();
        private AnalogInputChannel cellTemp = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["4KCell"];
        private Task cellTask = new Task();

        public string command;

        public double normsigGain;

        public void ConfigureCavityScan(int numberOfSteps, bool autostart)
        {
            TCLHelper.ConfigureCavityScan(numberOfSteps, autostart);
        }

        public void ConfigureReadPhotodiodes(int numberOfMeasurements, bool autostart)
        {
            TCLHelper.ConfigureReadPhotodiodes(numberOfMeasurements, autostart);
        }

        public void ConfigureSetLaserVoltage(double voltage)
        {
            TCLHelper.ConfigureSetLaserVoltage(voltage);
        }

        public void ConfigureScanTrigger()
        {
            TCLHelper.ConfigureScanTrigger();
        }

        public void ScanCavity(double[] rampVoltages, bool autostart)
        {
            TCLHelper.ScanCavity(rampVoltages, autostart);
        }

        public void StartScan()
        {
            TCLHelper.StartScan();
        }

        public void StopScan()
        {
            TCLHelper.StopScan();
        }

        public double[,] ReadPhotodiodes(int numberOfMeasurements)
        {
            return TCLHelper.ReadPhotodiodes(numberOfMeasurements);
        }

        public void SetLaserVoltage(double voltage)
        {
            TCLHelper.SetLaserVoltage(voltage);
        }

        public void ReleaseCavityHardware()
        {
            TCLHelper.ReleaseCavityHardware();
        }

        public void SendScanTriggerAndWaitUntilDone()
        {
            TCLHelper.SendScanTriggerAndWaitUntilDone();
        }
        public void ReleaseLaser()
        {
            TCLHelper.ReleaseLaser();
        }

        private double VoltageResistanceConversion(double voltage, double Vref)
        {
            return 47120 * (voltage / (Vref - voltage));
        }

        private double VoltageRoughVacuumConversion(double voltage)
        {
            return Math.Pow(10, ((voltage - 6.143) / 1.286));
        }

        private double VoltagePressureConversion(double voltage)
        {
            return Math.Pow(10, (1.667 * voltage - 11.33));
        }

        public void UpdateMonitoring()
        {
            AnalogSingleChannelReader reader3 = new AnalogSingleChannelReader(voltageReferenceTask.Stream);
            double Vref = reader3.ReadSingleSample();

            AnalogSingleChannelReader reader1 = new AnalogSingleChannelReader(pressureMonitorTask.Stream);
            double analogDataIn1 = reader1.ReadSingleSample();
            //window.monitorPressureSourceChamber.Text = VoltagePressureConversion(analogDataIn1).ToString("E02", CultureInfo.InvariantCulture);

            AnalogSingleChannelReader reader2 = new AnalogSingleChannelReader(roughVacuumTask.Stream);
            double analogDataIn2 = reader2.ReadSingleSample();
            //window.monitorRoughVacuum.Text = VoltageRoughVacuumConversion(analogDataIn2).ToString("E02", CultureInfo.InvariantCulture);


            AnalogSingleChannelReader reader4 = new AnalogSingleChannelReader(thermistor30KPlateTask.Stream);
            double analogDataIn4 = reader4.ReadSingleSample();
            //window.monitor10KTherm30KPlate.Text = VoltageResistanceConversion(analogDataIn4, Vref).ToString("E04", CultureInfo.InvariantCulture);

            AnalogSingleChannelReader reader5 = new AnalogSingleChannelReader(shieldTask.Stream);
            double analogDataIn5 = reader5.ReadSingleSample();
            //window.monitorShield.Text = VoltageResistanceConversion(analogDataIn5, Vref).ToString("E04", CultureInfo.InvariantCulture);

            AnalogSingleChannelReader reader6 = new AnalogSingleChannelReader(cellTask.Stream);
            double analogDataIn6 = reader6.ReadSingleSample();
            //window.monitorColdPlate.Text = VoltageResistanceConversion(analogDataIn6, Vref).ToString("E04", CultureInfo.InvariantCulture);
        }



        public void InitializeSidebandRead()
        {
            foreach (string channel in sidebandChannelList)
                ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                    sidebandMonitorTask,
                    0, 10);

            // internal clock, finite acquisition
            sidebandMonitorTask.Timing.ConfigureSampleClock(
                "",
                sidebandMonitorSampleRate,
                SampleClockActiveEdge.Rising,
                SampleQuantityMode.FiniteSamples,
                sidebandMonitorSamplesPerChannel);

            sidebandMonitorTask.Triggers.StartTrigger.ConfigureDigitalEdgeTrigger(
                (string)Environs.Hardware.GetInfo("usbAnalogTrigger"),
                DigitalEdgeStartTriggerEdge.Rising);

            sidebandMonitorTask.Control(TaskAction.Verify);

            sidebandReader = new AnalogMultiChannelReader(sidebandMonitorTask.Stream);
        }

        public void StartSidebandRead()
        {
            Thread readSidebandsThread = new Thread(new ThreadStart(ReadSidebands));
            readSidebandsThread.Start();
        }


        private double[,] sidebandData;
        public void ReadSidebands()
        {
            sidebandMonitorRunning = true;
            while (sidebandMonitorRunning)
            {

                sidebandMonitorTask.Start();
                sidebandData = sidebandReader.ReadMultiSample(sidebandMonitorSamplesPerChannel);
                sidebandMonitorTask.Stop();

                double[] xvals = new double[sidebandMonitorSamplesPerChannel];
                double[] yvals606 = new double[sidebandMonitorSamplesPerChannel];
                double[] yvals628V1 = new double[sidebandMonitorSamplesPerChannel];
                double[] yvals531 = new double[sidebandMonitorSamplesPerChannel];
                double[] yvals628Slowing = new double[sidebandMonitorSamplesPerChannel];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    xvals[j] = sidebandData[0, j];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    yvals606[j] = sidebandData[1, j];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    yvals628V1[j] = sidebandData[2, j];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    yvals531[j] = sidebandData[3, j];
                for (int j = 0; j < sidebandMonitorSamplesPerChannel; j++)
                    yvals628Slowing[j] = sidebandData[4, j];

                //window.displaySidebandData(window.scatterGraph1, xvals, yvals606);
                //window.displaySidebandData628V1(window.scatterGraph2, xvals, yvals628V1);
                //window.displaySidebandData531(window.scatterGraph6, xvals, yvals531);
                //window.displaySidebandData628Slowing(window.scatterGraph5, xvals, yvals628Slowing);

                Thread.Sleep(waitBetweenReads);
            }
        }

        //public void ReadFlowMeter()
        //{
        //string 
        //window.SetTextBox(window.CommandBox, value.ToString());

        //return window.CommandBox.Text;        
        //}



        public void SetFlowMeter()
        {
            //      flowMeter.SetFlow(GetCommand());
        }

    }
}
