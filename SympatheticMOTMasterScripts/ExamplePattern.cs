using MOTMaster;
using System;

using DAQ.Pattern;
using DAQ.Analog;


public class Patterns : MOTMasterScript
{
    /// IMPORTANT NOTE ABOUT WRITING SCRIPTS: At the moment, THERE IS NO AUTOMATIC TIME ORDERING FOR ANALOG
    /// CHANNELS. IT WILL BUILD A PATTERN FOLLOWING THE ORDER IN WHICH YOU CALL AddAnalogValue / AddLinearRamp!!
    /// ---> Stick to writing out the pattern in the correct time order to avoid weirdo behaviour.


    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        // AddEdge[int channel, int time, bool value] 
        p.AddEdge(1, 0, true);
        p.AddEdge(1, 1, false);

        //Pulse(int startTime, int delay, int duration, int channel )
        //DownPulse(int startTime, int delay, int duration, int channel )
        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder(2000);

        p.AddChannel("cavity");
        p.AddChannel("laser");

        //AddAnalogValue(string channel, int time, double value)
        p.AddAnalogValue("cavity", 0, 4);
        p.AddAnalogValue("cavity", 1, 2);
        p.AddAnalogValue("cavity", 3, 4);
        p.AddAnalogValue("cavity", 4, 0);

        //AddLinearRamp(string channel, int time, int numberOfSteps, double finalValue)
        p.AddLinearRamp("cavity", 5, 5, 1);
        p.AddLinearRamp("cavity", 11, 3, 0);

        p.AddAnalogValue("laser", 1, 4);
        p.AddAnalogValue("laser", 4, 2);
        p.AddAnalogValue("laser", 5, -2);
        p.AddAnalogValue("laser", 6, 0);

        return p;
    }

}
