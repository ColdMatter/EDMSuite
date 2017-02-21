using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAQ.HAL;

namespace DAQ.Pattern
{
    /// <summary>
    /// A wrapper class to define a sequence of commands for the Muquans laser. This specifies the laser to control as well as the type of the command - e.g. frequency ramp, change freq/phase
    /// </summary>
    public class MuquansBuilder
    {
        public List<MuquansCommand> commands;
        //Count the number of times a command is issued for each to check the correct number of triggers are sent
        public int slaveTrigCount;
        public int aomTrigCount;
        public int ramanTrigCount;



        public MuquansBuilder()
        {
            commands = new List<MuquansCommand>();
            slaveTrigCount = 0;
            aomTrigCount = 0;
            ramanTrigCount = 0;
        }

        public void SetFrequency(string laser, double frequency)
        {
            commands.Add(new MuquansCommand(laser, frequency, Instruction.set));
            AddToCount(laser);
        }
        public void SweepFrequency(string laser, double frequency, double time)
        {
            commands.Add(new MuquansCommand(laser, frequency, Instruction.sweep, time));
            AddToCount(laser);
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

    }
}
