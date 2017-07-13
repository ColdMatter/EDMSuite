using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using NationalInstruments.DAQmx;
using System.Threading;

using DAQ.Environment;

namespace DAQ.HAL
{
    [Serializable]
    public class MuquansRS232 : RS232Instrument
    {
        string id;


        public MuquansRS232(string visaAddress, string id) : base(visaAddress)
        { 
            this.id = id;
            this.baudrate = 2304000;

        }

        public void Output(string message)
        {
            this.serial.Write(message);
            Console.WriteLine(message);
            
        }

       
        
    }
    /// <summary>
    /// An interface to control the muquans laser using serial communication to onboard DDS. These are used to program the frequency/phase of each laser 
    /// </summary>
    public class MuquansController :MarshalByRefObject
    {
        public MuquansRS232 slaveComm;
        public MuquansRS232 aomComm;
        private Process slaveDDS;
        private Process aomDDS;
        private Task counterTask;
        private Task runningTask;
        private List<string> slaveCommands;
        private List<string> aomCommands;
        private int counts;
        int serialCounter = 0;

       


        public MuquansController()
        {
            slaveComm = (MuquansRS232)Environs.Hardware.Instruments["muquansSlave"];
            aomComm = (MuquansRS232)Environs.Hardware.Instruments["muquansAOM"];

            slaveDDS = new Process();
            string path = (string)Environs.FileSystem.Paths["MuquansExePath"];
            slaveDDS.StartInfo = ConfigureDDS(path, "slaves", 19);
            slaveDDS.Start();
            aomDDS = new Process();
            aomDDS.StartInfo = ConfigureDDS(path, "aom", 21);
            aomDDS.Start();
            slaveCommands = new List<string>();
            aomCommands = new List<string>();

        }
        public ProcessStartInfo ConfigureDDS(string path,string id, int port)
        {
            ///<summary>
            ///Configures the starting parameters for a dds process
            ///id - The identifier for the DDS. This is either "slave" or "aom"
            ///port - The port number used to communicate to the DDS. The default values are 18 and 20
            /// </summary>

            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = path + "ukus_dds_" + id + "_conf.txt comm " + port;
            info.FileName = path + "serial_to_dds_gw.exe";
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = false;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;
            info.ErrorDialog = true;
            return info;

        }
        #region Command Building
        public string BuildLaserString(string laser, string command)
        {
            string msg;
            if (laser == "mphi")
                msg = command + " mphi ";
            else
            {
                if (laser == "raman")
                    msg = "set_dds dds_raman; " + command + " ";
                else
                    msg = "set_dds ddsq; " + command + " " + laser + " ";
            }
            return msg;
        }

        public string SetFrequency(string laser, double val)
        {
            string msg = BuildLaserString(laser, "set_freq");
            msg += val + "e6; ext_update\n";
            return msg;
        }

        public string SetPhase(string laser, double val)
        {
            string msg = BuildLaserString(laser, "set_phase");
            msg += val + "; ext_update\n";
            return msg;
        }
        //Sweeps the frequency of a laser. The frequency value is a DDS frequency in MHz and the time is in ms.
        public string SweepFrequency(string laser, double freqTo, double time)
        {
            string msg = BuildLaserString(laser, "sweep_to");
            msg += freqTo + "e6 " + time + "e-3; ext_update\n";
            return msg;
        }

        public void SetMOTFrequency(double freq)
        {
            //ddsVal = 88.8125 - 0.0625 * freq;
            slaveCommands.Add(SetFrequency("slave0", freq));
        }
        public void SetMPhiFrequency(double freq)
        {
           //double ddsVal = 107.975 + 0.25 * freq;
            aomCommands.Add(SetFrequency("mphi", freq));
        }

        public void SweepMOTFrequency(double freq, double time)
        {
            //double ddsVal = 88.8125 - 0.0625 * freq;
            slaveCommands.Add(SweepFrequency("slave0", freq, time));
        }

        public void SweepMphiFrequency(double freq, double time)
        {
            //double ddsVal = 107.975 + 0.25 * freq;
            aomCommands.Add(SweepFrequency("mphi", freq, time));
        }
        #endregion
        /// <summary>
        /// Configures the counter task on a specified counter channel. The sample clock for this task is the PFI channel used to trigger serial communication
        /// </summary>
        public void Configure()
        {
           
            counterTask = new Task();

          
            CounterChannel counter = (CounterChannel)Environs.Hardware.CounterChannels["Counter"];
            counterTask.CIChannels.CreateCountEdgesChannel(counter.PhysicalChannel, counter.Name, CICountEdgesActiveEdge.Rising, 0, CICountEdgesCountDirection.Up);
            counterTask.CIChannels[0].CountEdgesTerminal = (string)Environs.Hardware.Boards["analogOut"] + "/pfi1";

            counterTask.SampleClock += new SampleClockEventHandler(counterTask_Sample);
        
            counterTask.Timing.ConfigureSampleClock((string)Environs.Hardware.Boards["analogOut"] + "/pfi1", 100000, SampleClockActiveEdge.Rising, SampleQuantityMode.HardwareTimedSinglePoint);

            counterTask.Control(TaskAction.Verify);
           
      
            
        }

        
        void counterTask_Sample(object sender, SampleClockEventArgs e)
        {
            if (serialCounter < slaveCommands.Count)
                slaveComm.Output(slaveCommands[serialCounter]);
            if (serialCounter < aomCommands.Count)
                aomComm.Output(aomCommands[serialCounter]);
            serialCounter++;
           
            
        }

        /// <summary>
        /// Builds the string messages used for each command to the Muquans laser. These are added to a list of each DDS
        /// </summary>
        /// <param name="commands"></param>
        public void BuildCommands(List<MuquansCommand> commands)
        {
            foreach (MuquansCommand command in commands)
            {
                if (command.rawMessage != null)
                {
                    if (command.laser == "mphi") aomCommands.Add(command.rawMessage);
                    else if (command.laser == "slave0") slaveCommands.Add(command.rawMessage);
                    else throw new ArgumentException();
                    continue;
                }
                if (command.instruction == Instruction.set)
                {
                    if (command.laser == "mphi")
                        SetMPhiFrequency(command.frequency);
                    else if (command.laser == "raman")
                        throw new NotImplementedException();
                    else
                        SetMOTFrequency(command.frequency);
                }
                else if (command.instruction == Instruction.sweep)
                {
                    if (command.laser == "mphi")
                        SweepMphiFrequency(command.frequency, command.sweeptime);
                    else if (command.laser == "raman")
                        throw new NotImplementedException();
                    else
                        SweepMOTFrequency(command.frequency, command.sweeptime);
                }
                else
                    throw new NotImplementedException();
            }
        }
        public void StartOutput()
        {

            
            serialCounter = 0;
            slaveComm.Connect();
            aomComm.Connect();
            //Outputs the first commands then waits 10ms before returning
            aomComm.Output(aomCommands[serialCounter]);
            slaveComm.Output(slaveCommands[serialCounter]);
            serialCounter++;
            Thread.Sleep(10);
            counterTask.Start();
            
        }

        public void StopOutput()
        {
            counterTask.Dispose();
            slaveComm.Disconnect();
            aomComm.Disconnect();
            slaveCommands = new List<string>();
            aomCommands = new List<string>();

        }
      
       
    }
      public enum Instruction: byte {sweep,set,phase};
    public struct MuquansCommand
        { 
            public string laser;
            public double frequency;
            public double sweeptime;
            public Instruction instruction;
            public string rawMessage;
     
            public MuquansCommand(string laser,double frequency, Instruction type, double time)
                {
                    this.laser = laser;
                    this.frequency = frequency;
                    this.sweeptime = time;
                    this.instruction = type;
                    this.rawMessage = null;
                   
                }
           public MuquansCommand(string laser,double frequency, Instruction type)
            {
                this.laser = laser;
                this.frequency = frequency;
                this.sweeptime = 0.0;
                this.instruction = type;
                this.rawMessage = null;
            }
         public MuquansCommand(string laser,string rawMessage)
           {
               this.laser = laser;
               this.frequency = 0.0;
               this.sweeptime = 0.0;
               this.instruction = Instruction.set;
               this.rawMessage = rawMessage;
           }
        }
   
}
