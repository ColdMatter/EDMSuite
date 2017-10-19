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
//using ScanMaster.Acquire.Plugin;

namespace ConfocalControl
{
    public class GalvoPairPlugin
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

        private PluginSettings settings = new PluginSettings("galvoPair");
        public PluginSettings Settings
        {
            get { return settings; }
            set { settings = value; }
        }

        private AnalogInputChannel _galvoXReadChannel;
        private AnalogOutputChannel _galvoXControlChannel;
        private AnalogInputChannel _galvoYReadChannel;
        private AnalogOutputChannel _galvoYControlChannel;

        private double XRangeLow;
        private double XRangeHigh;
        private double YRangeLow;
        private double YRangeHigh;

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
            //settings.LoadSettings();
            if (settings.Keys.Count == 0)
            {
                settings["GalvoXInit"] = "0.5";
                settings["GalvoYInit"] = "0.5";
                settings["GalvoXControl"] = "AO0";
                settings["GalvoYControl"] = "AO1";
                settings["GalvoXRead"] = "AI0";
                settings["GalvoYRead"] = "AI1";
                settings["pointsPerVolt"] = 100;
                return;
            }
        }

        public bool IsRunning()
        {
            return galvoState == GalvoState.running;
        }

        public GalvoPairPlugin() 
        {
            InitialiseSettings();

            _galvoXReadChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[(string)settings["GalvoXRead"]];
            _galvoXControlChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[(string)settings["GalvoXControl"]];
            _galvoYReadChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels[(string)settings["GalvoYRead"]];
            _galvoYControlChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[(string)settings["GalvoYControl"]];

            XRangeLow = _galvoXControlChannel.RangeLow;
            XRangeHigh = _galvoXControlChannel.RangeHigh;
            YRangeLow = _galvoYControlChannel.RangeLow;
            YRangeHigh = _galvoYControlChannel.RangeHigh;

            _galvoXInputTask = null;
            _galvoYInputTask = null;
            _galvoXOutputTask = null;
            _galvoYOutputTask = null;

            _galvoXreader = null;
            _galvoYreader = null;
            _galvoXwriter = null;
            _galvoYwriter = null;
        }

        #endregion

        public void AcquisitionStarting()
        {
            if (IsRunning())
            {
                throw new DaqException("Counter already running");
            }

            _galvoXInputTask = new Task("galvo X analog gather");
            _galvoYInputTask = new Task("galvo Y analog gather");

            _galvoXOutputTask = new Task("galvo X analog set");
            _galvoYOutputTask = new Task("galvo Y analog set");

            _galvoXReadChannel.AddToTask(_galvoXInputTask, XRangeLow, XRangeHigh);
            _galvoYReadChannel.AddToTask(_galvoYInputTask, YRangeLow, YRangeHigh);

            _galvoXControlChannel.AddToTask(_galvoXOutputTask, XRangeLow, XRangeHigh);
            _galvoYControlChannel.AddToTask(_galvoYOutputTask, YRangeLow, YRangeHigh);

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

        public void AcquisitionFinished()
        {
                _galvoXInputTask.Dispose();
                _galvoYInputTask.Dispose();
                _galvoXOutputTask.Dispose();
                _galvoYOutputTask.Dispose();

                _galvoXInputTask = null;
                _galvoYInputTask = null;
                _galvoXOutputTask = null;
                _galvoYOutputTask = null;

                _galvoXreader = null;
                _galvoYreader = null;
                _galvoXwriter = null;
                _galvoYwriter = null;

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

        public void MoveOnlyAcquisitionStarting()
        {
            if (IsRunning())
            {
                throw new DaqException("Galvo already running");
            }

            _galvoXOutputTask = new Task("galvo X analog set");
            _galvoYOutputTask = new Task("galvo Y analog set");

            _galvoXControlChannel.AddToTask(_galvoXOutputTask, XRangeLow, XRangeHigh);
            _galvoYControlChannel.AddToTask(_galvoYOutputTask, YRangeLow, YRangeHigh);

            _galvoXOutputTask.Control(TaskAction.Verify);
            _galvoYOutputTask.Control(TaskAction.Verify);

            _galvoXwriter = new AnalogSingleChannelWriter(_galvoXOutputTask.Stream);
            _galvoYwriter = new AnalogSingleChannelWriter(_galvoYOutputTask.Stream);

            galvoState = GalvoState.running;
        }

        public void MoveOnlyAcquisitionFinished()
        {
                _galvoXOutputTask.Dispose();
                _galvoYOutputTask.Dispose();

                _galvoXOutputTask = null;
                _galvoYOutputTask = null;

                _galvoXwriter = null;
                _galvoYwriter = null;

                galvoState = GalvoState.stopped;
        }

        public void  SetGalvoXSetpointAndWait(double _newValue, AsyncCallback callBackFunction, Object stateObject)
        {
            IAsyncResult ar = _galvoXwriter.BeginWriteSingleSample(true, _newValue, callBackFunction, stateObject);
            _galvoXwriter.EndWrite(ar);
        }

        public void SetGalvoYSetpointAndWait(double _newValue, AsyncCallback callBackFunction, Object stateObject)
        {
            IAsyncResult ar = _galvoYwriter.BeginWriteSingleSample(true, _newValue, callBackFunction, stateObject);
            _galvoYwriter.EndWrite(ar);
        }
    }
}
