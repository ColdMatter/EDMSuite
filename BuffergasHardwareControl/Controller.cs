using System;
using System.Collections;
using System.Text;
using System.Windows.Forms;
using DAQ.Environment;
using DAQ.HAL;
using NationalInstruments.DAQmx;

namespace BuffergasHardwareControl
{


    public class Controller : MarshalByRefObject
    {
        ControlWindow window;

        public double flowControlVoltage;
        private Task outputTask = new Task("FlowControllerOutput");
        private AnalogOutputChannel flowChannel = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels["laser"];
        public AnalogSingleChannelWriter flowWriter;

        public double flowInputVoltage;
        private Task inputTask = new Task("FlowMeterInput");
        private AnalogInputChannel flowmeterChannel = (AnalogInputChannel)Environs.Hardware.AnalogInputChannels["pressure1"];
        public AnalogSingleChannelReader flowReader;




        public void Start()
        {


            flowChannel.AddToTask(outputTask, 0, 5);
            outputTask.Control(TaskAction.Verify);
            flowWriter = new AnalogSingleChannelWriter(outputTask.Stream);

            flowmeterChannel.AddToTask(inputTask, 0, 5);
            inputTask.Control(TaskAction.Verify);
            flowReader = new AnalogSingleChannelReader(inputTask.Stream);





            // make the control window
            window = new ControlWindow();
            window.controller = this;



            Application.Run(window);

        }


       public double FlowControlVoltage
        {
            get { return flowControlVoltage; }
            set { flowControlVoltage = value;
            flowWriter.WriteSingleSample(true, value);
            outputTask.Control(TaskAction.Unreserve);
            }
        }


       public double FlowInputVoltage
       {
           get
           {
              flowInputVoltage = flowReader.ReadSingleSample();
              inputTask.Control(TaskAction.Unreserve);
              return flowInputVoltage;
           }
           
       }

     

    }



    }


    

