using System;
using System.Collections;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
//using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.DAQmx;
using DAQ.HAL;
using DAQ.Environment;

namespace LatticeHardwareControl
{
    public class Program : MarshalByRefObject
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        //Task cryoCoolertask;
        private static double pressure = 0;

        public Task pressuretask;

        AlicatFlowController FlowControllers = (AlicatFlowController)Environs.Hardware.Instruments["FlowControllers"];
        BigSkyYAG LatticeYAG = (BigSkyYAG)Environs.Hardware.Instruments["LatticeYAG"];

        Hashtable digitalTasks = new Hashtable();
        Form1 form;

        private void CreateDigitalTask(String name)
        {
            //if (!Environs.Debug)
            //{
            Task digitalTask = new Task(name);
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels[name]).AddToTask(digitalTask);
            digitalTask.Control(TaskAction.Verify);
            digitalTasks.Add(name, digitalTask);
            //}
        }


        private void SetDigitalLine(string name, bool value)
        {
            //if (!Environs.Debug)
            //{
            Task digitalTask1 = ((Task)digitalTasks[name]);
            DigitalSingleChannelWriter writer = new DigitalSingleChannelWriter(digitalTask1.Stream);
            writer.WriteSingleSampleSingleLine(true, value);
            digitalTask1.Control(TaskAction.Unreserve);
            //}
        }
        private double ReadAnalogInput(Task task)
        {
            AnalogSingleChannelReader reader = new AnalogSingleChannelReader(task.Stream);

            double val;
            Random rnd = new Random();
            //if (!Environs.Debug)
            //{
            val = reader.ReadSingleSample();
            task.Control(TaskAction.Unreserve);
            //}
            //else

            //{
            //    val = rnd.NextDouble();
            //}
            return val;
        }
        private Task CreateAnalogInputTask(string channel)
        {
            Task task = new Task("EDMHCIn" + channel);
            //if (!Environs.Debug)
            //{
            ((AnalogInputChannel)Environs.Hardware.AnalogInputChannels[channel]).AddToTask(
                task,
                0,
                10
            );
            task.Control(TaskAction.Verify);
            //}
            return task;
        }



        public void Start()
        {
            CreateDigitalTask("cryoCooler");
            pressuretask = CreateAnalogInputTask("pressure");
            // make the control window
            form = new Form1();
            form.controller = this;
            Application.Run(form);
          }


        public void activatecryo()
        {
            SetDigitalLine("cryoCooler", true);
        }
        public void deactivatecryo()
        {
            SetDigitalLine("cryoCooler", false);
        }
        public void pressurevalue()
        {
            pressure=ReadAnalogInput(pressuretask);
            form.SetTextBox(form.tbHeliumFlowSetpoint, (pressure).ToString());
        }

        #region Helium Flow Controller

        private double newHeliumFlowSetpoint;
        private double newSF6FlowSetpoint;
        private string lastHeliumFlowAct;
        private string heliumFlowActSeries = "Helium Flow";
        private double heliumFlowUpperLimit = 10.0; // Maximum neon flow that the MKS PR4000B flow controller is capable of.
        private double heliumFlowLowerLimit = 0.0; // Minimum neon flow that the MKS PR4000B flow controller is capable of.
        private double SF6FlowUpperLimit = 4.0; // Maximum neon flow that the MKS PR4000B flow controller is capable of.
        private double SF6FlowLowerLimit = 0.0; // Minimum neon flow that the MKS PR4000B flow controller is capable of.

        public void SetSetpointHelium()
        {
            //if (!Environs.Debug)
            //{
            string flowrate = form.tbNewHeliumFlowSetPoint.Text.ToString();
            string output = FlowControllers.SetSetpoint("a", flowrate);
            string[] outputs = output.Split(); //the ouput is always a series of info in the form: unitID/AbsPress/Temp/VolumetricFlow/Stand.MassFlow/Setpoint/Gas
            form.SetTextBox(form.tbHeliumFlowSetpoint, outputs[5]); //We split the string and output the setpoint
            //}
        }


        public void GetDataHelium()
        {
            //if (!Environs.Debug)
            //{
           
            string output = FlowControllers.QueryData("a");
            form.SetTextBox(form.tbHeliumFlowSetpoint, output);
            //}
        }

        public string GetFlowHelium()
        {
            //if (!Environs.Debug)
            //{

            string Heliumdata = FlowControllers.QueryFlow("a");
            return Heliumdata;

            //}
        }


        public void UpdateHeliumFlowActMonitor()
        {
            //sample the neon flow (actual)
            lastHeliumFlowAct = FlowControllers.QueryFlow("a");

            //update text boxes
            form.SetTextBox(form.tbHeliumFlowActual, lastHeliumFlowAct);
        }

        public void UpdateHeliumFlowSetpointMonitor()
        {
            //sample the neon flow (actual)
            string lastHeliumFlowSetpoint = FlowControllers.QuerySetpoint("a");

            //update text boxes
            form.SetTextBox(form.tbHeliumFlowSetpoint, lastHeliumFlowSetpoint);
        }

        public void UpdateSF6FlowActMonitor()
        {
            //sample the neon flow (actual)
            string lastSF6FlowAct = FlowControllers.QueryFlow("b");

            //update text boxes
            form.SetTextBox(form.tbSF6FlowActual, lastSF6FlowAct);
        }

        public void UpdateSF6FlowSetpointMonitor()
        {
            //sample the neon flow (actual)
            string lastSF6FlowSetpoint = FlowControllers.QuerySetpoint("b");

            //update text boxes
            form.SetTextBox(form.tbHeliumFlowSetpoint, lastSF6FlowSetpoint);
        }

        private Thread FlowMonitorPollThread;
        private int FlowMonitorPollPeriod = 1000;
        private bool FlowMonitorFlag;
        private bool HeliumFlowSetPointFlag;
        private bool SF6FlowSetPointFlag;
        private Object FlowMonitorLock;
        internal void StartFlowMonitorPoll()
        {
            FlowMonitorPollThread = new Thread(new ThreadStart(FlowActMonitorPollWorker));
            FlowMonitorPollThread.IsBackground = true; // When the application is closed, this thread will also immediately stop. This is lazy coding, but it works and shouldnn't cause any problems. This means it is a background thread of the main (UI) thread, so it will end with the main thread.
            FlowMonitorPollPeriod = Int32.Parse(form.tbFlowActPollPeriod.Text);
            form.EnableControl(form.btStartFlowActMonitor, false);
            form.EnableControl(form.btStopFlowActMonitor, true);
            form.EnableControl(form.tbNewHeliumFlowSetPoint, true);
            form.EnableControl(form.btSetNewHeliumFlowSetpoint, true);
            form.EnableControl(form.tbNewSF6FlowSetPoint, true);
            form.EnableControl(form.btSetNewSF6FlowSetpoint, true);
            FlowMonitorLock = new Object();
            FlowMonitorFlag = false;
            HeliumFlowSetPointFlag = false;
            SF6FlowSetPointFlag = false;
            FlowMonitorPollThread.Start();
        }

        internal void StopFlowMonitorPoll()
        {
            FlowMonitorFlag = true;
        }

        public void SetHeliumFlowSetpoint()
        {
            if (Double.TryParse(form.tbNewHeliumFlowSetPoint.Text, out newHeliumFlowSetpoint))
            {
                if (newHeliumFlowSetpoint <= heliumFlowUpperLimit & heliumFlowLowerLimit <= newHeliumFlowSetpoint)
                {
                    HeliumFlowSetPointFlag = true; // set flag that will trigger the setpoint to be changed in NeonFlowActMonitorPollWorker()
                }
                else MessageBox.Show("Setpoint request is outside of the Alicat flow range (" + heliumFlowLowerLimit.ToString() + " - " + heliumFlowUpperLimit.ToString() + " SCCM)", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse setpoint string. Ensure that a number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);

        }

        public void SetSF6FlowSetpoint()
        {
            if (Double.TryParse(form.tbNewSF6FlowSetPoint.Text, out newSF6FlowSetpoint))
            {
                if (newSF6FlowSetpoint <= SF6FlowUpperLimit & SF6FlowLowerLimit <= newSF6FlowSetpoint)
                {
                    SF6FlowSetPointFlag = true; // set flag that will trigger the setpoint to be changed in NeonFlowActMonitorPollWorker()
                }
                else MessageBox.Show("Setpoint request is outside of the Alicat flow range (" + SF6FlowLowerLimit.ToString() + " - " + SF6FlowUpperLimit.ToString() + " SCCM)", "User input exception", MessageBoxButtons.OK);
            }
            else MessageBox.Show("Unable to parse setpoint string. Ensure that a number has been written, with no additional non-numeric characters.", "", MessageBoxButtons.OK);

        }

        public void SetHeliumFlow(double flowrate)
        {
            FlowControllers.SetSetpoint("a", flowrate.ToString());
        }

        private void FlowActMonitorPollWorker()
        {
            for (; ; )// for (; ; ) is an infinite loop, equivalent to while(true)
            {
                Thread.Sleep(FlowMonitorPollPeriod);
                lock (FlowMonitorLock)
                {
                    UpdateHeliumFlowActMonitor();
                    PlotLastHeliumFlowAct();
                    UpdateHeliumFlowSetpointMonitor();
                    if (HeliumFlowSetPointFlag)
                    {
                        FlowControllers.SetSetpoint("a", newHeliumFlowSetpoint.ToString());
                        HeliumFlowSetPointFlag = false;
                    }
                    if (SF6FlowSetPointFlag)
                    {
                        FlowControllers.SetSetpoint("b", newSF6FlowSetpoint.ToString());
                        SF6FlowSetPointFlag = false;
                    }
                    if (FlowMonitorFlag)
                    {
                        FlowMonitorFlag = false;
                        break;
                    }
                }
            }
            form.EnableControl(form.btStartFlowActMonitor, true);
            form.EnableControl(form.btStopFlowActMonitor, false);
            form.EnableControl(form.tbNewHeliumFlowSetPoint, false);
            form.EnableControl(form.btSetNewHeliumFlowSetpoint, false);
            form.EnableControl(form.tbNewSF6FlowSetPoint, false);
            form.EnableControl(form.btSetNewSF6FlowSetpoint, false);
        }

        public void PlotLastHeliumFlowAct()
        {
            DateTime localDate = DateTime.Now;

            //plot the most recent sample
            form.AddPointToChart(form.chartHeliumFlow, heliumFlowActSeries, localDate, Convert.ToDouble(lastHeliumFlowAct));
        }

        #endregion 

        #region
        //YAG Commands
        public void CheckYAGTemp()
        {
            //if (!Environs.Debug)
            //{

            string output = LatticeYAG.CheckTemperature();
            form.SetTextBox(form.tbHeliumFlowSetpoint, output);
            //}
        }

        #endregion

    }
}
