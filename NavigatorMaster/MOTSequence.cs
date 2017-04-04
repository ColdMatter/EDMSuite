using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

namespace NavigatorMaster
{
    public class Patterns : MOTMasterScript
    {
        public Patterns()
        {
            Parameters = new Dictionary<string, object>();
            //Digital clock frequency divided by analogue clock frequency
            Parameters["ScaleFactor"] = 20000000 / 100000;
            //This is the legnth of the digital pattern which is written to the HSDIO card, clocked at 20MHz
            Parameters["PatternLength"] = 46000000;
            //This is the length of the analogue pattern, clocked at 100 kHz
            Parameters["AnalogLength"] = 100000;
            Parameters["NumberOfFrames"] = 2;

            Parameters["XBias"] = 0.0;
            Parameters["YBias"] = 1.2;
            Parameters["ZBias"] = 0.9;

            //All times are in milliseconds and then multiplied by the 100 KHz clock frequency
            Parameters["2DLoadTime"] = 500 * 100;
            
            Parameters["3DLoadTime"] = 100 * 100;
            Parameters["BfieldSwitchOffTime"] = (int)Parameters["2DLoadTime"] + (int)Parameters["3DLoadTime"];
            Parameters["BfieldDelayTime"] = 25 * 10;
            
            Parameters["ImageTime"] = 0;
            Parameters["ExposureTime"] = 1 * 100;
            Parameters["BackgroundDwellTime"] = 1000 * 100;

            Parameters["MotPower"] = 2.0;
            Parameters["RepumpPower"] = 0.26;
            Parameters["2DBfield"] = 2.0;
            Parameters["3DBfield"] = 3.3;

            //Frequencies and attenuator voltages for the fibre AOMs
            Parameters["XAtten"] = 3.66;
            Parameters["YAtten"] = 4.36;
            Parameters["ZPAtten"] = 3.38;
            Parameters["ZMAtten"] = 3.05;

            Parameters["XFreq"] = 6.84;
            Parameters["YFreq"] = 6.95;
            Parameters["ZPFreq"] = 6.95;
            Parameters["ZMFreq"] = 7.090;

            Parameters["PushAtten"] = 5.75;
            Parameters["PushFreq"] = 7.41;
            Parameters["2DMotFreq"] = 7.35;
            Parameters["2DMotAtten"] = 5.7;

            Parameters["MOTdetuning"] = -13.5;
            Parameters["Molassesdetuning"] = -163.5;
        }

        public override HSDIOPatternBuilder GetHSDIOPattern()
        {
 
            HSDIOPatternBuilder hs = new HSDIOPatternBuilder();

                MOTMasterScriptSnippet init = new Initialize(hs, Parameters);

                MOTMasterScriptSnippet mot2d = new Load2DMOT(hs, Parameters);

                MOTMasterScriptSnippet image = new Imaging(hs, Parameters);

            return hs;
        }

        public override AnalogPatternBuilder GetAnalogPattern()
        {
            AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["AnalogLength"]);

            MOTMasterScriptSnippet init = new Initialize(p, Parameters);

            MOTMasterScriptSnippet mot2d = new Load2DMOT(p, Parameters);

            MOTMasterScriptSnippet image = new Imaging(p, Parameters);

            return p;

        }

        public override MuquansBuilder GetMuquansCommands()
        {
            MuquansBuilder mu = new MuquansBuilder();
     
            MOTMasterScriptSnippet init = new Initialize(mu, Parameters);

            MOTMasterScriptSnippet mot2d = new Load2DMOT(mu, Parameters);

            MOTMasterScriptSnippet image = new Imaging(mu, Parameters);

            return mu;

        }

        public override MMAIConfiguration GetAIConfiguration()
        {
            return null;
        }
        public override PatternBuilder32 GetDigitalPattern()
        {
            throw new NotImplementedException();
        }
    }

}