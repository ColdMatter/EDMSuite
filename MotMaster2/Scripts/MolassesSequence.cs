using MOTMaster2;
using MOTMaster2.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;

namespace MOTMaster2.MolassesSequence
{
    public class Patterns : MOTMasterScript
    {
        private Dictionary<string, double> SequenceEndTimes;
        public Patterns()
        {
            SequenceEndTimes = new Dictionary<string, double>();
            Parameters = new Dictionary<string, object>();
            Parameters["HSClockFrequency"] = 20000000;
            Parameters["AnalogClockFrequency"] = 100000;
            //This is the legnth of the digital pattern which is written to the HSDIO card, clocked at 20MHz
            Parameters["PatternLength"] = 46000000;
            //This is the length of the analogue pattern, clocked at 100 kHz
            Parameters["AnalogLength"] = 100000;
            Parameters["NumberOfFrames"] = 2;

            Parameters["XBias"] = 0.78;
            Parameters["YBias"] = -0.29;
            Parameters["ZBias"] = -0.4;

            Parameters["XBias2D"] = 1.7;
            Parameters["YBias2D"] = -1.55;

            //All times are in milliseconds
            Parameters["2DLoadTime"] = 200.0;
            Parameters["3DLoadTime"] = 100.0;
            Parameters["BfieldSwitchOffTime"] = (double)Parameters["2DLoadTime"] + (double)Parameters["3DLoadTime"];
            Parameters["BfieldDelayTime"] = 2.5;

            //Duration of the molasses ramp in milliseconds
            Parameters["MolassesFreqDuration"] = 2.0;
            //By default the Intensity is ramped 5ms after the molasses is switched off
            Parameters["IntensityRampTime"] = 2.0;

           
            //This is the time to image the atoms AFTER the Bfield is switched off
            Parameters["ImageStartTime"] = (double)Parameters["BfieldSwitchOffTime"] + (double)Parameters["BfieldDelayTime"] + (double)Parameters["MolassesFreqDuration"] + (double)Parameters["IntensityRampTime"];
            Parameters["ImageTime"] = 10.0;
            Parameters["ExposureTime"] = 0.3;
            Parameters["BackgroundDwellTime"] = 500.0;

            Parameters["MotPower"] = 2.0;
            Parameters["RepumpPower"] = 0.22;
            Parameters["2DBfield"] = 2.0;
            Parameters["3DBfield"] = 2.8;

            //Frequencies and attenuator voltages for the fibre AOMs
            Parameters["XAtten"] = 3.85;
            Parameters["YAtten"] = 5.4;
            Parameters["ZPAtten"] = 3.58;
            Parameters["ZMAtten"] = 3.17;


            Parameters["XFreq"] = 6.828;
            Parameters["YFreq"] = 6.927;
            Parameters["ZPFreq"] = 6.931;
            Parameters["ZMFreq"] = 7.076;

            Parameters["PushAtten"] = 5.70;
            Parameters["PushFreq"] = 7.41;
            Parameters["2DMotFreq"] = 7.35;
            Parameters["2DMotAtten"] = 5.7;

            Parameters["MOTdetuning"] = -13.5;
            Parameters["Molassesdetuning"] = -163.5;  
          
        }

        public override HSDIOPatternBuilder GetHSDIOPattern()
        {
 
            HSDIOPatternBuilder hs = new HSDIOPatternBuilder();

            SequenceStep init = new Initialize(hs, Parameters);

            SequenceStep mot2d = new Load2DMOT(hs, Parameters,init.SequenceEndTime);
            SequenceStep molasses = new Molasses(hs, Parameters,mot2d.SequenceEndTime);
            SequenceStep image = new Imaging(hs, Parameters,molasses.SequenceEndTime);
            SequenceEndTimes["init"] = init.SequenceEndTime;
            SequenceEndTimes["mot2D"] = mot2d.SequenceEndTime;
            SequenceEndTimes["molasses"] = molasses.SequenceEndTime;


            return hs;
        }

        public override AnalogPatternBuilder GetAnalogPattern()
        {
            AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["AnalogLength"]);

            SequenceStep init = new Initialize(p, Parameters);

            SequenceStep mot2d = new Load2DMOT(p, Parameters,SequenceEndTimes["init"]);
            SequenceStep molasses = new Molasses(p, Parameters,SequenceEndTimes["mot2D"]);
            SequenceStep image = new Imaging(p, Parameters,SequenceEndTimes["molasses"]);

            return p;

        }

        public override MuquansBuilder GetMuquansCommands()
        {
            MuquansBuilder mu = new MuquansBuilder();

            SequenceStep init = new Initialize(mu, Parameters);

            SequenceStep mot2d = new Load2DMOT(mu, Parameters);
            SequenceStep molasses = new Molasses(mu, Parameters);
            SequenceStep image = new Imaging(mu, Parameters);

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