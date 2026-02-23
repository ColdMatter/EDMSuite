using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

// This script is for just using the Q1 on the slowing axis to deplete the TOF and probe with the Q1 in the MOT chamber. "Pump" refers to the VECSEL 2 Q(1) line, which is actually the probe. Slower is the VECSEL 3 on the Q(1) line

public class Patterns : MOTMasterScript
{
    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 100000;
        Parameters["Void"] = 0;
        Parameters["t0"] = 10000;
        Parameters["flash"] = true;

        Parameters["CameraDelay"] = -2200;

        Parameters["SlowerDelay"] = 300;
        Parameters["SlowerDelaySwitch"] = 0;
        Parameters["SlowerWidth"] = 100;
        Parameters["SlowerWidthNeg"] = 0;
        Parameters["v1SlowerOverhead"] = 1000;
        Parameters["MOTTime"] = 6000;

        Parameters["ProbeWidth"] = 2500;
        Parameters["ProbeDelay"] = 0;

        Parameters["v1ShutterDelay"] = 10000;
        Parameters["v1Switch"] = true;
        Parameters["v0Switch"] = true;

        Parameters["HeShutterOn"] = -500;
        Parameters["HeShutterOff"] = 200;

        //1V -> 800 MHz (in blue) VECSEL3 !!!OLD AMP (220V/10V)
        //0.5V/10ms -> ~80 MHz/ms 

        Parameters["v0_block_on"] = -5000;
        Parameters["v0_block_off"] = 20000;
        Parameters["v0_amp"] = 0.45;
        Parameters["v0_offset"] = 1.0;
        Parameters["v0_chirp_len"] = 300*2;

        //1V -> ~200 MHz (in IR) Vecsel 1
        //2V -> ~400 MHz (in IR) Vecsel 1
        Parameters["v1_block_on"] = -7500;
        Parameters["v1_block_off"] = 22500;
        Parameters["v1_amp"] = -0.65;
        Parameters["v1_amp"] = -0.65*100/110;
        Parameters["v1_offset"] = 1.0;
        Parameters["v1_hold_time"] = 6000;
        Parameters["v1_chirp_len"] = 330;
        Parameters["v1_chirp_len"] = 300*2;

        Parameters["amp_scaling"] = 1.0;


        //this.switchConfiguration = new Dictionary<string, List<object>>
        //    {
        //        {"v0Switch", new List<object>{true, true, false, false } },
        //        {"v1Switch", new List<object>{true, false, true, false } }
        //    };

        //this.switchConfiguration = new Dictionary<string, List<object>>
        //    {
        //        {"v0Switch", new List<object>{true, false} },
        //        {"v1Switch", new List<object>{true, true} }
        //    };

        //this.switchConfiguration = new Dictionary<string, List<object>>
        //    {
        //        {"v0Switch", new List<object>{true, false, true} },
        //        {"v1Switch", new List<object>{true, true, true} },
        //        {"SlowerDelay", new List<object>{100, 100, 300} },
        //        {"amp_scaling", new List<object>{0.0, 0.0, 0.0} },
        //        {"flash", new List<object>{true, true, true} }
        //    };

        this.switchConfiguration = new Dictionary<string, List<object>>
            {
                {"v0Switch", new List<object>{true, false, true} },
                {"v1Switch", new List<object>{false, false, false} },
                {"SlowerDelay", new List<object>{100, 100, 300} },
                {"amp_scaling", new List<object>{0.0, 0.0, 0.0} },
                {"flash", new List<object>{true, true, true} }
            };

        //this.switchConfiguration = new Dictionary<string, List<object>>
        //    {
        //        {"v0Switch", new List<object>{true, true} },
        //        {"v1Switch", new List<object>{true, false} }
        //    };

        //this.switchConfiguration = new Dictionary<string, List<object>>
        //    {
        //        {"v0Switch", new List<object>{true} },
        //        {"v1Switch", new List<object>{true} }
        //    };
    }

    private int iparam(string param_name)
    {
        return Convert.ToInt32(Parameters[param_name]);
    }

    private double dparam(string param_name)
    {
        return Convert.ToDouble(Parameters[param_name]);
    }

    public override PatternBuilder32 GetDigitalPattern()
    {
        PatternBuilder32 p = new PatternBuilder32();
        int cameraDelay = Convert.ToInt32(Parameters["CameraDelay"]);
        int shutterDelay = Convert.ToInt32(Parameters["v1ShutterDelay"]);
        int t0 = Convert.ToInt32(Parameters["t0"]);
        int HeShutterOn = Convert.ToInt32(Parameters["HeShutterOn"]);
        int HeShutterOff = Convert.ToInt32(Parameters["HeShutterOff"]);


        //MOTMasterScriptSnippet lm = new LoadMoleculeMOT(p, Parameters); // This is how you load "preset" patterns.          
        //   p.AddEdge("v00Shutter", 0, true);

        //Shutter takes ~40 ms +- 10 ms to start opening and closing. Shutter delay of 100 ms ensures shutter is fully open. Add shutter delay to all timings so that shutter opening is the first event in the pattern

        p.AddEdge("q",t0 ,true);
        p.AddEdge("q",10 + t0, false);

        if ((bool)Parameters["flash"])
        {
            p.AddEdge("flash", t0 - 14, true);
            p.AddEdge("flash", t0 + 0, false);
        }

        p.AddEdge("detector", 2000, true);
        p.AddEdge("detector", 2000 + 10, false);

        p.AddEdge("detector", t0 + cameraDelay, true);
        p.AddEdge("detector", t0 + cameraDelay + 10, false);

        p.AddEdge("He_Shutter", t0 + HeShutterOn, true);
        p.AddEdge("He_Shutter", t0 + HeShutterOff, false);

        //Shutter begins closed
        //p.AddEdge("VECSEL2_Shutter", 0, true);
        if ((bool)Parameters["v1Switch"])
        {
            p.AddEdge("VECSEL2_Shutter", 0, true);
            //Close shutter after 200 ms after experiment is complete to limit repump hitting cell
            p.AddEdge("VECSEL2_Shutter", t0 + shutterDelay, false);

            p.AddEdge("VECSEL1_Block", t0 + iparam("v1_block_on"), true);
            p.AddEdge("VECSEL1_Block", t0 + iparam("v1_block_off"), false);

        }

        if ((bool)Parameters["v0Switch"])
        {
            p.AddEdge("VECSEL3_Block", t0 + iparam("v0_block_on"), true);
            p.AddEdge("VECSEL3_Block", t0 + iparam("v0_block_off"), false);
        }


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {

        int t0 = Convert.ToInt32(Parameters["t0"]);
        int slower_delay = Convert.ToInt32(Parameters["SlowerDelay"]) + Convert.ToInt32(Parameters["SlowerDelaySwitch"]) - iparam("SlowerWidthNeg");
        int slower_width = Convert.ToInt32(Parameters["SlowerWidth"]) + iparam("SlowerWidthNeg");
        int probe_delay = Convert.ToInt32(Parameters["ProbeDelay"]);
        int probe_width = Convert.ToInt32(Parameters["ProbeWidth"]);
        int v1_overhead = Convert.ToInt32(Parameters["v1SlowerOverhead"]);
        int MOT_time = Convert.ToInt32(Parameters["MOTTime"]);
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);
        p.AddChannel("AOM1_VCA");
        p.AddChannel("AOM2_VCA");


        if (slower_delay + t0 != 0)
        {
            p.AddAnalogValue("AOM1_VCA", 0, 0);
            p.AddAnalogValue("AOM2_VCA", 0, 0);
        }

        p.AddAnalogValue("AOM1_VCA", t0 + slower_delay, 0);
        p.AddAnalogValue("AOM1_VCA", t0 + slower_delay + slower_width + v1_overhead, 10);
        p.AddAnalogValue("AOM1_VCA", t0 + slower_delay + slower_width + v1_overhead + MOT_time, 0);

        if ((bool)Parameters["v0Switch"])
        {
            p.AddAnalogValue("AOM2_VCA", t0 + slower_delay, 10);
            p.AddAnalogValue("AOM2_VCA", t0 + slower_delay + slower_width, 0);
        }

        p.AddChannel("VECSEL1_CHIRP");

        int chirpLen_v1 = Convert.ToInt32(Parameters["v1_chirp_len"]);
        int holdTime_v1 = Convert.ToInt32(Parameters["v1_hold_time"]);
        
        p.AddAnalogValue("VECSEL1_CHIRP", 0, (double)Parameters["v1_offset"]);
        if ((bool)Parameters["v1Switch"])
        {
            p.AddLinearRamp("VECSEL1_CHIRP", t0 + slower_delay, chirpLen_v1, (double)Parameters["v1_offset"] + Convert.ToDouble(Parameters["v1_amp"])*dparam("amp_scaling"));
            p.AddLinearRamp("VECSEL1_CHIRP", t0 + slower_delay + chirpLen_v1 + 2 + holdTime_v1, chirpLen_v1, (double)Parameters["v1_offset"]);
        }


        int chirpLen_v0 = Convert.ToInt32(Parameters["v0_chirp_len"]);

        p.AddChannel("VECSEL3_CHIRP");

        p.AddAnalogValue("VECSEL3_CHIRP", 0, (double)Parameters["v0_offset"]);
        if ((bool)Parameters["v0Switch"])
        {
            p.AddLinearRamp("VECSEL3_CHIRP", t0 + slower_delay, chirpLen_v0, (double)Parameters["v0_offset"] + Convert.ToDouble(Parameters["v0_amp"]) * dparam("amp_scaling"));
            p.AddLinearRamp("VECSEL3_CHIRP", t0 + slower_delay + chirpLen_v0 + 2, chirpLen_v0, (double)Parameters["v0_offset"]);
        }

        return p;
   }

}
