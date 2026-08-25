using System;
using System.Collections.Generic;
/*using System.Linq;
using System.Text;
using System.Threading.Tasks;*/
using DAQ.Pattern;
using DAQ.Analog;
using DAQ;

namespace MOTMaster.SnippetLibrary
{
    public class CaFBECLoadMOT : MOTMasterScriptSnippet
    {

        public CaFBECLoadMOT(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
            AddDigitalSnippet(p, Parameters);
        }
        public CaFBECLoadMOT(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            AddAnalogSnippet(p, Parameters);
        }


        public void AddDigitalSnippet(PatternBuilder32 p, Dictionary<String, Object> Parameters)
        {
            int slowingAOMStart = (int)Parameters["BXAOMFreeFlightDuration"];
            int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
            int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

            p.Pulse((int)Parameters["PatternStart"], 0, 10, "analogPatternTrigger");
            p.EnforceTimeOrdering(false);

            p.Pulse(
                (int)Parameters["PatternStart"],
                0,
                (int)Parameters["TCLBlockLength"],
                "blockTCL"
            );

            p.Pulse(
                (int)Parameters["PatternStart"],
                -(int)Parameters["FlashToQ"],
                (int)Parameters["QSwitchPulseDuration"],
                "flash"
            );
            

            if ((double)Parameters["yagONorOFF"] > 5.0)
            {
                p.Pulse(
                    (int)Parameters["PatternStart"], 
                    0, 
                    (int)Parameters["QSwitchPulseDuration"], 
                    "q"
                    );
            }

            if ((double)Parameters["SlowingONorOFF"] > 5.0)
            {
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    slowingAOMStart,
                    slowingChirpStop - slowingAOMStart,
                    "BXAOM"
                );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    slowingAOMStart,
                    slowingChirpStop - slowingAOMStart,
                    "BXAOM2"
                );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    10,
                    5000,
                    "BXSidebands"
                );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    0,
                    slowingChirpStop,
                    "RepumpAOM"
                );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    0,
                    slowingChirpStop,
                    "RepumpBroadening"
                );
            }

            if ((double)Parameters["MOTONorOFF"] > 5.0)
            {
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    (int)Parameters["V00AOMONStartTime"],
                    (int)Parameters["MOTLoadingDuration"],
                    "V00R0AOM"
                    );
                p.Pulse(
                    (int)Parameters["PatternStart"],
                    (int)Parameters["V00AOMONStartTime"],
                    (int)Parameters["MOTLoadingDuration"],
                    "V00R1plusAOMredMOT"
                );
            }
        }

        public void AddAnalogSnippet(AnalogPatternBuilder p, Dictionary<String, Object> Parameters)
        {
            p.AddChannel("BXChirp");
            p.AddChannel("SlowingBField");
            p.AddChannel("motCoils");
            p.AddChannel("ShimCoilX");
            p.AddChannel("ShimCoilY");
            p.AddChannel("ShimCoilZ");
            p.AddChannel("V00R0AOMVCOFreq");
            p.AddChannel("V00R0AOMVCOAmp");
            p.AddChannel("V00R1plusAOMAmp");
            p.AddChannel("V00R0EOMAmp");

            int slowingAOMStart = (int)Parameters["BXAOMFreeFlightDuration"];
            int slowingChirpStart = slowingAOMStart + (int)Parameters["BXAOMPostBunchingDuration"];
            int slowingChirpStop = slowingChirpStart + (int)Parameters["BXAOMChirpDuration"];

            if ((double)Parameters["SlowingONorOFF"] > 5.0)
            {
                p.AddLinearRamp(
                    "BXChirp",
                    slowingChirpStart,
                    slowingChirpStop - slowingChirpStart,
                    (double)Parameters["BXChirpMHzSpan"] / (double)Parameters["BXMHzPerVolt"]
                );
                p.AddLinearRamp(
                    "BXChirp",
                    slowingChirpStop + 5000,
                    1000,
                    (double)Parameters["BXChirpStartValue"]
                );
                p.AddAnalogValue(
                    "SlowingBField",
                    0,
                    (double)Parameters["SlowingCoilFieldValue"]
                );
                p.AddAnalogValue(
                    "SlowingBField",
                    slowingChirpStop + 400,
                    0.0
                );
            }

            if ((double)Parameters["MOTONorOFF"] > 5.0)
            {
                p.AddAnalogValue(
                    "motCoils",
                    (int)Parameters["MOTCoilsStartTime"],
                    (double)Parameters["MOTLoadingFieldValue"]
                );
                p.AddAnalogValue(
                    "ShimCoilX",
                    slowingChirpStop,
                    (double)Parameters["XShimCoilsMOTValue"]
                );
                p.AddAnalogValue(
                    "ShimCoilY",
                    slowingChirpStop,
                    (double)Parameters["YShimCoilsMOTValue"]
                );
                p.AddAnalogValue(
                    "ShimCoilZ",
                    slowingChirpStop,
                    (double)Parameters["ZShimCoilsMOTValue"]
                );
                p.AddAnalogValue(
                    "V00R0AOMVCOFreq",
                    0,
                    (double)Parameters["V00R0AOMVCOFreqMOTLoading"]
                );
                p.AddAnalogValue(
                    "V00R0AOMVCOAmp",
                    0,
                    (double)Parameters["V00R0AOMVCOAmpMOTLoading"]
                );
                p.AddAnalogValue(
                   "V00R1plusAOMAmp",
                   0,
                   (double)Parameters["V00R1plusAOMAmpMOTLoading"]
                );
                p.AddAnalogValue(
                   "V00R0EOMAmp",
                   0,
                   (double)Parameters["V00R0EOMAmpMOTValue"]
                );
            }
        }
    }
}
