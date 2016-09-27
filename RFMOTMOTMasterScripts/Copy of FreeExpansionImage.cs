using MOTMaster;
using MOTMaster.SnippetLibrary;

using System;
using System.Collections.Generic;

using DAQ.Pattern;
using DAQ.Analog;


//This is a release and recapture script that can be used to measure the temperature of the MOT. It switches on the MOT,
// takes an image, switches off the coils to release the MOT, switches them back on to recapture the MOT and then 
//takes another image of the cloud. Note this method requires light levels to be the same for all images taken   
public class Patterns : MOTMasterScript
{


    public Patterns()
    {
        Parameters = new Dictionary<string, object>();
        Parameters["PatternLength"] = 12000;
        Parameters["NumberOfFrames"] = 2;
        Parameters["FETOffTime"] = 7500;

        Parameters["CamTrig1Duration"] = 2;

        Parameters["CamTrig1Time"] = 7500;
        Parameters["CamTrig2Time"] = 8000;

        Parameters["MOTLoadTime"] = 7500;
        Parameters["FETStartValue"] = 5.0;
        Parameters["FETEndValue"] = 0.0;


        Parameters["CoolingLaserMOTValue"] = 0.6;

        Parameters["AOMStartValue"] = 3.9;
        Parameters["AOMEndValue"] = 4.5;

    }

    public override Dictionary<string, PatternBuilder32> GetDigitalPatterns()
    {
        Dictionary<string, PatternBuilder32> p = new Dictionary<string, PatternBuilder32>();

        p["PXI"] = new PatternBuilder32();

        p["PCI"] = new PatternBuilder32();

        //------------------TRIGGER ANALOG PATTER-----\\

        p["PXI"].AddEdge("AnalogPatternTrigger", 0, false);

        p["PXI"].AddEdge("AnalogPatternTrigger", 1, true);
        //---------------------------------------------\\


        //-------------LOAD MOT - SHOULD USE SNIPPITS--\\


        p["PXI"].AddEdge("coolaom", 0, true);

        p["PXI"].AddEdge("coolaom", 1, false);

        p["PXI"].AddEdge("coolaom", (int)Parameters["MOTLoadTime"], true);

        //---------------------------------------------\\


        //-------------------TRIGGER CAMERA-------------\\

        p["PXI"].AddEdge("camtrig", 0, false);

        p["PXI"].Pulse((int)Parameters["CamTrig1Time"], 0, 5, "camtrig");

        p["PXI"].Pulse((int)Parameters["CamTrig2Time"], 0, 5, "camtrig");


        p["PXI"].AddEdge("refaom", 0, false);

        p["PXI"].AddEdge("refaom", (int)Parameters["CamTrig1Time"], true);

        p["PXI"].AddEdge("refaom", (int)Parameters["CamTrig1Time"] + (int)Parameters["CamTrig1Duration"], false);

        p["PXI"].AddEdge("refaom", (int)Parameters["CamTrig2Time"], true);
        //-----------------------------------------------\\



        p["PCI"].AddEdge("PCIDOTest", 0, true);


        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("motfet");

        p.AddAnalogValue("motfet", 1, (double)Parameters["FETStartValue"]);

        p.AddAnalogValue("motfet", (int)Parameters["MOTLoadTime"], (double)Parameters["FETEndValue"]);

        p.AddChannel("motlightatn");

        p.AddAnalogValue("motlightatn", 0, (double)Parameters["AOMStartValue"]);

        p.AddLinearRamp("motlightatn", (int)Parameters["MOTLoadTime"], (int)Parameters["MolassesDuration"], (double)Parameters["AOMEndValue"]);

        p.AddChannel("coolsetpt");

        p.AddAnalogValue("coolsetpt", 0, (double)Parameters["CoolingLaserMOTValue"]);

        p.AddLinearRamp("coolsetpt", (int)Parameters["MOTLoadTime"], (int)Parameters["MolassesDuration"], (double)Parameters["CoolingLaserMolassesValue"]);

        p.AddAnalogValue("coolsetpt", (int)Parameters["MOTLoadTime"] + 100, (double)Parameters["CoolingLaserMOTValue"]);

        // p.SwitchAllOffAtEndOfPattern();
        return p;
    }

    public override MMAIConfiguration GetAIConfiguration()
    {

        MMAIConfiguration p = new MMAIConfiguration();

        p.AddChannel("multiDAQAI1", 0.0, 10.0);

        p.SampleRate = (int)1000;

        p.Samples = (int)100;

        return p;

    }

}
