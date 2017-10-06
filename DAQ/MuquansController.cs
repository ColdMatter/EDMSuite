using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using NationalInstruments.DAQmx;
using System.Threading;

using DAQ.Environment;
using NationalInstruments.VisaNS;

namespace DAQ.HAL
{
    [Serializable]
    public class MuquansRS232 : RS232Instrument
    {
        string id;


        public MuquansRS232(string visaAddress, string id) : base(visaAddress)
        { 
            this.id = id;
            this.baudrate = 230400;
            
        }
        public override void Connect()
        {
            Connect(SerialTerminationMethod.TerminationCharacter);
        }
        public void Output(string message)
        {
            try
            {
                //TODO check connect and disconnect doesn't cause timing issues
                this.serial.Write(message);
                this.serial.Flush(NationalInstruments.VisaNS.BufferTypes.OutBuffer, false);
           
            }
            catch { throw new Exception("Error writing serial command: "+message); }
        }

        public bool Connected
        {
        get {  return this.connected;}
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
        private Stopwatch stopwatch;
        int serialCounter = 0;
        private bool loopMode;

       


        public MuquansController()
        {
            slaveComm = (MuquansRS232)Environs.Hardware.Instruments["muquansSlave"];
            aomComm = (MuquansRS232)Environs.Hardware.Instruments["muquansAOM"];

            //slaveDDS = new Process();
            //string path = (string)Environs.FileSystem.Paths["MuquansExePath"];
            //slaveDDS.StartInfo = ConfigureDDS(path, "slaves", 19);
            //slaveDDS.Start();
            //aomDDS = new Process();
            //aomDDS.StartInfo = ConfigureDDS(path, "aom", 21);
            //aomDDS.Start();
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
        
        /// <summary>
        /// Configures the counter task on a specified counter channel. The sample clock for this task is the PFI channel used to trigger serial communication
        /// </summary>
        public void Configure(bool loop)
        {
            loopMode = loop;
            if (runningTask == null)
            {
                counterTask = new Task();

                CounterChannel counter = (CounterChannel)Environs.Hardware.CounterChannels["Counter"];
                counterTask.CIChannels.CreateCountEdgesChannel(counter.PhysicalChannel, counter.Name, CICountEdgesActiveEdge.Rising, 0, CICountEdgesCountDirection.Up);
                counterTask.CIChannels[0].CountEdgesTerminal = (string)Environs.Hardware.Boards["analogOut"] + "/pfi1";

                counterTask.SampleClock += new SampleClockEventHandler(counterTask_Sample);

                counterTask.Timing.ConfigureSampleClock((string)Environs.Hardware.Boards["analogOut"] + "/pfi1", 10000, SampleClockActiveEdge.Rising, SampleQuantityMode.HardwareTimedSinglePoint);

                counterTask.Control(TaskAction.Verify);
            }
        }

        
        void counterTask_Sample(object sender, SampleClockEventArgs e)
        {
            lock (slaveCommands)
            {
                if (serialCounter == 0) stopwatch.Start();
                if (serialCounter < slaveCommands.Count)slaveComm.Output(slaveCommands[serialCounter]);
                Console.WriteLine(string.Format("wrote command {0}:{1} at {2}", serialCounter, slaveCommands[serialCounter],
                  stopwatch.ElapsedMilliseconds));
                if (serialCounter < aomCommands.Count) aomComm.Output(aomCommands[serialCounter]);
                Console.WriteLine(string.Format("wrote command {0}:{1} at {2}", serialCounter, aomCommands[serialCounter],
                    stopwatch.ElapsedMilliseconds));
                serialCounter++;
                if (serialCounter == slaveCommands.Count) { if (loopMode) serialCounter = 0; } //Reset to allow for loop mode
                
            }
        }

        /// <summary>
        /// Builds the string messages used for each command to the Muquans laser. These are added to a list of each DDS
        /// </summary>
        /// <param name="commands"></param>
        public void BuildCommands(List<SerialCommand> commands)
        {
            foreach (SerialCommand command in commands)
            {
                if (command.rawMessage != null)
                {
                    if (command.id == "mphi") aomCommands.Add(command.rawMessage);
                    else if (command.id == "slave0") slaveCommands.Add(command.rawMessage);
                    else throw new ArgumentException();
                    continue;
                }
                else
                    throw new NotImplementedException();
            }
        }
        public void StartOutput()
        {

            stopwatch = new Stopwatch();
            serialCounter = 0;
            if(!slaveComm.Connected)slaveComm.Connect();
            if(!aomComm.Connected) aomComm.Connect();
            //stopwatch.Start();
            if (runningTask == null) {runningTask = counterTask; }
            counterTask.Start();
            
        }

        /// <summary>
        /// Clears the output for the serial channels, but keeps the tasks  running to reduce timing jitter
        /// </summary>
        public void StopOutput()
        {
            slaveCommands = new List<string>();
            aomCommands = new List<string>();
            counterTask.Stop();
            serialCounter = 0;
            stopwatch.Restart();
        }
        
        /// <summary>
        /// Disposes of the counter task and the serial connections once the entire run has finished
        /// </summary>
        public void DisposeAll()
        {
            slaveComm.Disconnect();
            aomComm.Disconnect();
            runningTask = null;
            //counterTask.Stop();
            counterTask.Dispose();
        }
       
    }
      public enum Instruction: byte {sweep,set,phase};
    public struct SerialCommand
        { 
            public string id;
            public string rawMessage;
    
         public SerialCommand(string id,string rawMessage)
           {
               this.id = id;
               this.rawMessage = rawMessage;
           }
        }
   
}
