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
        Parameters["PatternLength"] = 5500;
        Parameters["NumberOfFrames"] = 2;

        Parameters["CamTrig1Time"] = 1;
        Parameters["CamTrig2Time"] = 5070;

        Parameters["MOTFETVoltage"] = 5.0;
        Parameters["BiasFETVoltage"] = 0.1;
        Parameters["BiasOffTime"] = 5000;
      //  Parameters["ProbeLightOn"] = 5000;
    }

    public override Dictionary<string, PatternBuilder32> GetDigitalPatterns()
    {
        Dictionary<string, PatternBuilder32> p = new Dictionary<string,PatternBuilder32>();

        p["PXI"] = new PatternBuilder32();

        p["PCI"] = new PatternBuilder32();

        //------------------TRIGGER ANALOG PATTER-----\\

        p["PXI"].AddEdge("AnalogPatternTrigger", 0, false);

        p["PXI"].AddEdge("AnalogPatternTrigger",1,true);
        //---------------------------------------------\\


        //-------------LOAD MOT - SHOULD USE SNIPPITS--\\


        p["PXI"].AddEdge("coolaom", 0, true);

        p["PXI"].AddEdge("coolaom", 1, false);
        
        //---------------------------------------------\\

        //===testlength of sequence


     //   p["PXI"].AddEdge("sequencelen", 0, true);
      //  p["PXI"].AddEdge("sequencelen", (int)Parameters["PatternLength"]-1, false);

        //-------------------TRIGGER CAMERA-------------\\

        p["PXI"].AddEdge("camtrig", 0, false);

        p["PXI"].Pulse((int)Parameters["CamTrig1Time"],0,5,"camtrig");

        p["PXI"].Pulse((int)Parameters["CamTrig2Time"], 0, 5, "camtrig");
        
        p["PXI"].AddEdge("refaom", 0, false);

        p["PXI"].Pulse((int)Parameters["CamTrig1Time"], 0, 2, "refaom");

        p["PXI"].Pulse((int)Parameters["CamTrig2Time"], 0, 2, "refaom");
        //-----------------------------------------------\\



        p["PCI"].AddEdge("PCIDOTest",0,true);
        

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("motfet");

        p.AddAnalogValue("motfet", 0, (double)0.0);

        p.AddAnalogValue("motfet", 5, (double)Parameters["MOTFETVoltage"]);

        p.AddChannel("zbias");

        p.AddAnalogValue("zbias", 0, (double)Parameters["BiasFETVoltage"]);

        p.AddAnalogValue("zbias", (int)Parameters["BiasOffTime"], (double)0.0);

       // p.SwitchAllOffAtEndOfPattern();
        return p;
    }

    public override MMAIConfiguration GetAIConfiguration()
    {

        MMAIConfiguration p = new MMAIConfiguration();

        p.AddChannel("MOTFluoresence", 0.0, 10.0);

        p.AddChannel("MOTCoilSense", 0.0, 10.0);

        p.SampleRate = (int)1000;

        p.Samples = (int)1200;
   
        return p;

    }

}
