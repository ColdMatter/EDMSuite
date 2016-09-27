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
        Parameters["PatternLength"] = 100000;
        Parameters["NumberOfFrames"] = 2;

        Parameters["CamTrigDuration"] = 700;

        Parameters["CamTrig1Time"] = 700;
        Parameters["CamTrig2Time"] = 99500;

        Parameters["FETStartValue"] = 5.0;
        Parameters["FETEndValue"] = 0.0;

        Parameters["MolassesDuration"] = 200;
        Parameters["CoolingLaserMOTValue"] = 0.6;
        Parameters["CoolingLaserMolassesValue"] = 0.4;

        Parameters["AOMStartValue"] = 2.0;
        Parameters["AOMEndValue"] = 6.0;

        Parameters["FeedForwardStartValue"] = 0.0;
        Parameters["FeedForwardEndValue"] = -0.196;
        Parameters["FeedForwardRampDuration"] = 100;
        Parameters["FeedForwardHoldDuration"] = 5;

        Parameters["AOM1On"] = true;
        Parameters["AOM2On"] = false;
        
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
        //Cool AOM is on when DO is false. Cool AOM 2 is on when DO is true.

        p["PXI"].AddEdge("coolaom", 0, true);
         
        p["PXI"].AddEdge("coolaom", 1, false);

        p["PXI"].AddEdge("coolaom", (int)Parameters["MolassesDuration"] , true);

        if ((bool)Parameters["AOM1On"])
        {
            p["PXI"].AddEdge("coolaom", (int)Parameters["MolassesDuration"] + 10, false);
            p["PXI"].AddEdge("coolaom", (int)Parameters["CamTrig1Time"], true);
        }
        

       
        
        p["PXI"].AddEdge("coolaom2", 0, false);

        if ((bool)Parameters["AOM2On"])
        {
            p["PXI"].AddEdge("coolaom2", (int)Parameters["MolassesDuration"] + 10, true);
            p["PXI"].AddEdge("coolaom2", (int)Parameters["CamTrig1Time"], false);
        }
        
        

        //---------------------------------------------\\


        //-------------------TRIGGER CAMERA-------------\\

        p["PXI"].AddEdge("camtrig", 0, false);

        p["PXI"].Pulse((int)Parameters["CamTrig1Time"],0,5,"camtrig");

        p["PXI"].Pulse((int)Parameters["CamTrig2Time"], 0, 5, "camtrig");
       
     
        p["PXI"].AddEdge("refaom", 0, false);

        p["PXI"].AddEdge("refaom", (int)Parameters["CamTrig1Time"] , true);

        p["PXI"].AddEdge("refaom", (int)Parameters["CamTrig1Time"] + (int)Parameters["CamTrigDuration"], false);

        p["PXI"].AddEdge("refaom", (int)Parameters["CamTrig2Time"], true);
        //-----------------------------------------------\\



        p["PCI"].AddEdge("PCIDOTest",0,true);
        

        return p;
    }

    public override AnalogPatternBuilder GetAnalogPattern()
    {
        AnalogPatternBuilder p = new AnalogPatternBuilder((int)Parameters["PatternLength"]);

        p.AddChannel("motfet");

        p.AddAnalogValue("motfet", 0, (double)Parameters["FETEndValue"]);

        p.AddAnalogValue("motfet", (int)Parameters["MolassesDuration"]+1, (double)Parameters["FETStartValue"]);


        p.AddChannel("motlightatn");

        p.AddAnalogValue("motlightatn", 0, (double)Parameters["AOMStartValue"]);

        p.AddLinearRamp("motlightatn",(int)1,(int)Parameters["MolassesDuration"],(double)Parameters["AOMEndValue"]);

        p.AddAnalogValue("motlightatn", (int)Parameters["MolassesDuration"]+(int)1, (double)Parameters["AOMStartValue"]);
        
        
        
        p.AddChannel("coolingfeedfwd");

        p.AddAnalogValue("coolingfeedfwd",0,(double)Parameters["FeedForwardStartValue"]);

        p.AddLinearRamp("coolingfeedfwd", 1, (int)Parameters["MolassesDuration"], (double)Parameters["FeedForwardStartValue"] - 0.05);

        p.AddAnalogValue("coolingfeedfwd", (int)Parameters["MolassesDuration"] + 1, (double)Parameters["FeedForwardEndValue"]);

        p.AddAnalogValue("coolingfeedfwd", 99999, (double)Parameters["FeedForwardStartValue"]);
       // p.SwitchAllOffAtEndOfPattern();
        return p;
    }

    public override MMAIConfiguration GetAIConfiguration()
    {

        MMAIConfiguration p = new MMAIConfiguration();

        p.AddChannel("MOTFluoresence", -10.0, 10.0);

        p.AddChannel("coolerrsig", -10.0, 10.0);

        p.AddChannel("MOTCoilSense", -10.0, 10.0);
        
        p.SampleRate = (int)5000;

        p.Samples = (int)5000;
   
        return p;

    }

}
