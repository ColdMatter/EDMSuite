using System;
using System.Globalization;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;
using NationalInstruments;

namespace MicrocavityHardwareControl
{
    public class Controller : MarshalByRefObject
    {
        // This is the controller for the Hardware Controller that can also connect to the hardware
        // at the same time as ScanMaster and lets one manually change settings and observe signals
        // etc.
        
        #region Constants
        
        //constants go here

        #endregion

        ControlWindow window;

        #region tasks
        
        //set up a task for an analog output and analog input channel
        //The hardware class automatically gets the correct hardware 
        //because environs gets the computer name and applies the hardware
        //settings in DAQ.
        private AnalogInputChannel uCavityReflectionECDLChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["uCavityReflectionECDL"];
        private AnalogInputChannel uCavityReflectionTiSapphChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["uCavityReflectionTiSapph"];
        private AnalogOutputChannel uCavityScanChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["uCavityControl"]; 
        private Task ReflectionChannelTask = new Task();

        #endregion

        public void Start()
        {
            //You are here need to assign channel to task
            uCavityReflectionECDLChannel.AddToTask(ReflectionChannelTask, -10, 10);
            uCavityReflectionTiSapphChannel.AddToTask(ReflectionChannelTask, -10, 10);
            // Not sure if I need to add the output channels to the task
            //uCavityScanChannel.AddToTask(OutputChannelTask, -10, 10);

            //Verify the Task
            ReflectionChannelTask.Control(TaskAction.Verify); 
            
            //open the GUI
            window = new ControlWindow();
            window.controller = this;
            Application.Run(window);
        }

        public void UpdateMonitoring()
        {
            AnalogMultiChannelReader testReader = new AnalogMultiChannelReader(ReflectionChannelTask.Stream);
            double [] testValue = testReader.ReadSingleSample();
            window.uCavityReflectionECDL.Text = testValue[0].ToString("E04", CultureInfo.InvariantCulture);
            window.uCavityReflectionTiSapph.Text = testValue[1].ToString("E04", CultureInfo.InvariantCulture);
        }
    }
}