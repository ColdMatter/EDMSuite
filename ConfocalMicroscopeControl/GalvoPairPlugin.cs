using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

using NationalInstruments;
using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.FakeData;
using DAQ.HAL;
using Data;
using ScanMaster.Acquire.Plugin;

namespace ConfocalMicroscopeControl
{
    class GalvoPairPlugin
    {
        #region Class members

        // Implement 
        public enum GalvoState { stopped, running};
        public GalvoState galvoState = GalvoState.stopped;

        private static GalvoPairPlugin controllerInstance;

        public static GalvoPairPlugin GetController()
        {
            if (controllerInstance == null)
            {
                controllerInstance = new GalvoPairPlugin();
            }
            return controllerInstance;
        }

        private PluginSettings settings = new PluginSettings();
        public PluginSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        private Task _galvoXInputTask;
        private Task _galvoYInputTask;
        private Task _galvoXOutputTask;
        private Task _galvoYOutputTask;

        private AnalogSingleChannelReader _galvoXreader;
        private AnalogSingleChannelReader _galvoYreader;
        private AnalogSingleChannelWriter _galvoXwriter;
        private AnalogSingleChannelWriter _galvoYwriter;

        #endregion

        #region Initialisation

        private void InitialiseSettings()
        {
            settings["GalvoXRead"] = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["GalvoX"];
            settings["GalvoXControl"] = (AnalogOutputChannel)Environs.Hardware.AnalogInputChannels["GalvoXControl"];
            settings["GalvoYRead"] = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["GalvoY"];
            settings["GalvoYControl"] = (AnalogOutputChannel)Environs.Hardware.AnalogInputChannels["GalvoYControl"];
            settings["XRangeLow"] = ((AnalogOutputChannel)settings["GalvoXControl"]).RangeLow;
            settings["XRangeHigh"] = ((AnalogOutputChannel)settings["GalvoXControl"]).RangeHigh;
            settings["YRangeLow"] = ((AnalogOutputChannel)settings["GalvoYControl"]).RangeLow;
            settings["YRangeHigh"] = ((AnalogOutputChannel)settings["GalvoYControl"]).RangeHigh;
        }

        public GalvoPairPlugin() 
        {
            InitialiseSettings();
        }

        #endregion

        public void AcquisitionStarting()
        {
            _galvoXInputTask = new Task("galvo X analog gather");
            _galvoYInputTask = new Task("galvo Y analog gather");

            _galvoXOutputTask = new Task("galvo X analog set");
            _galvoYOutputTask = new Task("galvo Y analog set");

            double XRangeLow = (double)settings["XRangeLow"]; double XRangeHigh = (double)settings["XRangeHigh"];
            double YRangeLow = (double)settings["YRangeLow"]; double YRangeHigh = (double)settings["YRangeHigh"];

            ((AnalogInputChannel)settings["GalvoX"]).AddToTask(_galvoXInputTask, XRangeLow, XRangeHigh);
            ((AnalogInputChannel)settings["GalvoY"]).AddToTask(_galvoYInputTask, YRangeLow, YRangeHigh);

            ((AnalogOutputChannel)settings["GalvoXControl"]).AddToTask(_galvoXOutputTask, XRangeLow, XRangeLow);
            ((AnalogOutputChannel)settings["GalvoYControl"]).AddToTask(_galvoYOutputTask, YRangeLow, YRangeLow);

            _galvoXInputTask.Control(TaskAction.Verify);
            _galvoYInputTask.Control(TaskAction.Verify);

            _galvoXOutputTask.Control(TaskAction.Verify);
            _galvoYOutputTask.Control(TaskAction.Verify);

            _galvoXreader = new AnalogSingleChannelReader(_galvoXInputTask.Stream);
            _galvoYreader = new AnalogSingleChannelReader(_galvoYInputTask.Stream);

            _galvoXwriter = new AnalogSingleChannelWriter(_galvoXOutputTask.Stream);
            _galvoYwriter = new AnalogSingleChannelWriter(_galvoYOutputTask.Stream);

            galvoState = GalvoState.running;
        }

        public override void AcquisitionFinished()
        {
            _galvoXInputTask.Dispose();
            _galvoYInputTask.Dispose();

            _galvoXOutputTask.Dispose();
            _galvoYOutputTask.Dispose();

            galvoState = GalvoState.stopped;
        }

        public double GetGalvoXSetpoint()
        {
            double analogData = _galvoXreader.ReadSingleSample();
            return analogData;
        }

        public double GetGalvoYSetpoint()
        {
            double analogData = _galvoYreader.ReadSingleSample();
            return analogData;
        }

        public void SetGalvoXSetpoint(double _newValue)
        {
            _galvoXwriter.WriteSingleSample(true, _newValue);
        }

        public void SetGalvoYSetpoint(double _newValue)
        {
            _galvoYwriter.WriteSingleSample(true, _newValue);
        }

        public double[,] GetSingleGalvoPairSetpoint()
        {
            if (galvoState == GalvoState.running)
            {
                MessageBox.Show("Gavlos already running!");

                // Implement exception
            }


            Task inputTask = new Task("single analog gatherer");

            double XRangeLow = (double)settings["XRangeLow"]; double XRangeHigh = (double)settings["XRangeHigh"];
            double YRangeLow = (double)settings["YRangeLow"]; double YRangeHigh = (double)settings["YRangeHigh"];

            ((AnalogInputChannel)settings["GalvoX"]).AddToTask(inputTask, XRangeLow, XRangeHigh);
            ((AnalogInputChannel)settings["GalvoY"]).AddToTask(inputTask, YRangeLow, YRangeHigh);

            inputTask.Control(TaskAction.Verify);

            AnalogMultiChannelReader reader = new AnalogMultiChannelReader(inputTask.Stream);

            double[,] analogDataIn = reader.ReadMultiSample(1);

            inputTask.Dispose();

            return analogDataIn;
        }

        public void SetSingleGalvoPairSetpoint(double[,] _newValues)
        {
            if (galvoState == GalvoState.running)
            {
                MessageBox.Show("Galvos already running!");

                // Implement exception
            }

            Task outputTask = new Task("single analog set");

            double XRangeLow = (double)settings["XRangeLow"]; double XRangeHigh = (double)settings["XRangeHigh"];
            double YRangeLow = (double)settings["YRangeLow"]; double YRangeHigh = (double)settings["YRangeHigh"];

            ((AnalogOutputChannel)settings["GalvoXControl"]).AddToTask(outputTask, XRangeLow, XRangeHigh);
            ((AnalogOutputChannel)settings["GalvoYControl"]).AddToTask(outputTask, YRangeLow, YRangeHigh);

            outputTask.Control(TaskAction.Verify);

            AnalogMultiChannelWriter writer = new AnalogMultiChannelWriter(outputTask.Stream);

            writer.WriteMultiSample(true, _newValues);

            outputTask.Dispose();
        }
    }
}
