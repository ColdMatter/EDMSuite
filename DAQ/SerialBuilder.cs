using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.HAL;

namespace DAQ.Pattern
{
    //TODO Change this to a more general serial command builder
    /// <summary>
    /// A wrapper class to define a sequence of commands for the Muquans laser. This specifies the laser to control as well as the type of the command - e.g. frequency ramp, change freq/phase
    /// </summary>
    [Serializable]
    public class SerialBuilder
    {
        public List<SerialCommand> commands;
        //Count the number of times a command is issued for each to check the correct number of triggers are sent
        public int slaveTrigCount;
        public int aomTrigCount;
        public int ramanTrigCount;

        public SerialBuilder()
        {
            commands = new List<SerialCommand>();
            slaveTrigCount = 0;
            aomTrigCount = 0;
            ramanTrigCount = 0;
        }

        public void AddToCount(string laser)
        {
            if (laser == "mphi")
                aomTrigCount += 1;
            else if (laser == "raman")
                ramanTrigCount += 1;
            else
                slaveTrigCount += 1;

        }

        public void AddCommand(string id, string message)
        {
            if (id == "slave0" || id == "mphi") { commands.Add(new SerialCommand(id, message)); AddToCount(id); }
            else throw new ArgumentException();
        }
    }
}
