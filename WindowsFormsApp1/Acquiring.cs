using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NationalInstruments.DAQmx;
using System.Windows.Forms;

namespace PaddlePolStabiliser
{
    

    class Acquiring
    {
        public Task AcqTask;
        private AnalogSingleChannelReader reader;
        
        private AsyncCallback readCallback;

        private static Acquiring acquiringInstance;
        private static Controller controller;

        public static Acquiring GetAcquiring()
        {
            if (acquiringInstance == null)
            {
                acquiringInstance = new Acquiring();
            }
            return acquiringInstance;
        }

        public void SetupTask()
        {
            Controller controller = Controller.GetController();

            acquiringInstance.AcqTask = new Task();

            double sampleRate = 10;
            double min = -10;
            double max = 10;

            acquiringInstance.AcqTask.AIChannels.CreateVoltageChannel(controller.progSettings["DetectorChannel"], "Detector",
                (AITerminalConfiguration)(-1), min, max, AIVoltageUnits.Volts);
            acquiringInstance.AcqTask.Timing.ConfigureSampleClock("", sampleRate, SampleClockActiveEdge.Rising,
                SampleQuantityMode.ContinuousSamples, 1);
            ///AcqTask.Triggers.ReferenceTrigger.ConfigureDigitalEdgeTrigger("internal",
            ///    DigitalEdgeReferenceTriggerEdge.Rising,0);
            acquiringInstance.AcqTask.Control(TaskAction.Verify);

            acquiringInstance.reader = new AnalogSingleChannelReader(AcqTask.Stream);

            acquiringInstance.AcqTask.Start();
        }

        public void EndTask()
        {
            acquiringInstance.AcqTask.Stop();
            acquiringInstance.AcqTask.Dispose();
        }

        public double ReadPoint()
        {
            //This is asychronous readout from the DAQ card this seems overkill for this
            //program so I am not going to use it, leave here for future
            //readCallback = new AsyncCallback(ReadInCallback);
            //reader.SynchronizeCallbacks = true;
            //reader.BeginReadMultiSample(1,readCallback,AcqTask);
            
            double value = acquiringInstance.reader.ReadSingleSample();
            return value;
        }

        //private void ReadInCallback(IAsyncResult asyncResult)
        //{
        //    double[] readdata = reader.EndReadMultiSample(asyncResult);
        //    Controller.GetController().StoredData.Enqueue(readdata[readdata.Length -1]);

        //}
        
    }
}
