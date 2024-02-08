using System;
using System.Collections.Generic;
using System.Linq;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

namespace DigitalTransferCavityLock
{
    public class CounterReaderHelper
    {

        public string counterChannel;
        public string samplingChannel;
        public string refClockChannel;
        public double refClockFreq;
        public string syncChannel;

        public CounterChannel CounterChannel
        {
            get
            {
                return (CounterChannel)Environs.Hardware.CounterChannels[counterChannel];
            }
        }

        public CounterChannel SamplingChannel
        {
            get
            {
                return (CounterChannel)Environs.Hardware.CounterChannels[samplingChannel];
            }
        }

        /*public CounterChannel SyncChannel
        {
            get
            {
                return (CounterChannel)Environs.Hardware.CounterChannels[syncChannel];
            }
        }*/

        public CounterReaderHelper(string counter, string samplingClock, string refClock, double _refClockFreq, string sync)
        {
            counterChannel = counter;
            samplingChannel = samplingClock;
            refClockChannel = refClock;
            refClockFreq = _refClockFreq;
            syncChannel = sync;
        }

        private Task counterTask;
        private CounterSingleChannelReader dataReader;

        public void SetUpTask(double periodMS)
        {
            counterTask = new Task("Counter Read task for" + this.samplingChannel);

            CIChannel MainCounter = counterTask.CIChannels.CreateCountEdgesChannel(CounterChannel.PhysicalChannel, CounterChannel.Name, CICountEdgesActiveEdge.Rising, 0, CICountEdgesCountDirection.Up);
            MainCounter.CountEdgesTerminal = refClockChannel;
            MainCounter.CountEdgesCountResetEnable = true;
            MainCounter.CountEdgesCountResetTerminal = syncChannel;
            MainCounter.CountEdgesCountResetActiveEdge = CICountEdgesCountResetActiveEdge.Rising;
            MainCounter.CountEdgesCountResetResetCount = 0;

            counterTask.Timing.ReferenceClockRate = 10e6;
            counterTask.Timing.ConfigureSampleClock(SamplingChannel.PhysicalChannel, 10000000, SampleClockActiveEdge.Rising, SampleQuantityMode.FiniteSamples, 1);
            counterTask.Stream.Buffer.InputBufferSize = 2;
            counterTask.Triggers.ArmStartTrigger.ConfigureDigitalEdgeTrigger(syncChannel, DigitalEdgeArmStartTriggerEdge.Rising);
            //counterTask.Triggers.ArmStartTrigger.ConfigureNone();
            counterTask.Stream.Timeout = (int)Math.Ceiling(periodMS / 2);
            //counterTask.SampleComplete += (object ob, SampleCompleteEventArgs e) => { ReadCallback(); };
            //counterTask.SampleClock += (object ob, SampleClockEventArgs e) => { ReadCallback(); };

            counterTask.Control(TaskAction.Verify);
            counterTask.Control(TaskAction.Commit);
            dataReader = new CounterSingleChannelReader(counterTask.Stream);
            //counterTask.Start();

        }

        public double dataMS = 0;
        public bool fail = false;
        public bool ready = false;

        public void InitiateRead()
        {
            fail = false;
            ready = false;
            dataReader.BeginReadSingleSampleUInt32(new AsyncCallback(ReadCallback), null);
        }

        public void ReadCallback(IAsyncResult res)
        {
            try
            {
                dataMS = 1000 * dataReader.EndReadSingleSampleUInt32(res) / refClockFreq;
            }
            catch (DaqException e)
            {
                fail = true;
            }
            ready = true;
        }

        public void DismissTask()
        {
            counterTask.Dispose();
        }

    }
}
