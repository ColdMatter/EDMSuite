using System;
using System.Xml.Serialization;

using Wolfram.NETLink;

using DAQ.Environment;
using DAQ.Pattern;
using DAQ.Mathematica;
using DecelerationConfig;
using ScanMaster.Acquire.Patterns;
using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
	/// <summary>
	/// A plugin for deceleration patterns. Make sure that the sequenceLength setting is always a multiple of 2 (see 
    /// documentation for PumpProbePatternPlugin to find out why).
    ///
	/// </summary>
	[Serializable]
	public class DecelerationPatternPlugin : SupersonicPGPluginBase
	{
		[NonSerialized]
		private DecelerationPatternBuilder decelPatternBuilder;
		private TimingSequence decelSequence = null;

		protected override void InitialiseCustomSettings()
		{
			settings["delayToDeceleration"] = 1000;
			settings["molecule"] = "YbF";
			settings["voltage"] = 10;
			settings["initspeed"] = 337;
			settings["initposition"] = 0;
			settings["onposition"] = 16.0;
			settings["offposition"] = 28.0;
			settings["numberOfStages"] = 12;
            settings["sequenceLength"] = 2;
            settings["resonanceOrder"] = 1;
            settings["decelOnStart"] = 300;
            settings["decelOnDuration"] = 800;
            settings["modulationMode"] = "BurstAndOff";
		}


		protected override void DoAcquisitionStarting()
		{
			decelPatternBuilder = new DecelerationPatternBuilder();

			// prepare the deceleration sequence
			buildDecelerationSequence(
				(string)settings["molecule"],
				(int)settings["voltage"],
				(int)settings["initspeed"],
				(int)settings["initposition"],
				(double)settings["onposition"],
				(double)settings["offposition"],
				(int)settings["numberOfStages"],
                (int)settings["resonanceOrder"]
				);
		}

		protected override IPatternSource GetScanPattern()
		{
			decelPatternBuilder.Clear();
			decelPatternBuilder.ShotSequence(
				0,
				(int)settings["sequenceLength"],
				(int)settings["padShots"], 
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
				(int)config.shotGathererPlugin.Settings["gateStartTime"],
				(int)settings["delayToDeceleration"],
				decelSequence,
                (string)settings["modulationMode"],
                (int)settings["decelOnStart"],
                (int)settings["decelOnDuration"],
				(bool)config.switchPlugin.Settings["switchActive"]
				);
			decelPatternBuilder.BuildPattern(2 * ((int)settings["padShots"] + 1) * (int)settings["sequenceLength"]
				* (int)settings["flashlampPulseInterval"]);

			return decelPatternBuilder;
		}


		private void buildDecelerationSequence(String molecule, int voltage,
			int initspeed, int initposition, double onposition, double offposition, int numberOfStages, int resonanceOrder)
		{
			IKernelLink ml = MathematicaService.GetKernel();
			MathematicaService.LoadPackage((String)Environs.Info["SwitchSequenceCode"], false);
			if (ml != null) 
			{
				//Here's the call to the Mathematica makeSequence function
				String argString = "makeSequence[" + molecule + ", " + voltage + ", " + initspeed + ", " + initposition + ", " +
					onposition + ", " + offposition + ", " + numberOfStages + ", " + resonanceOrder + "]";
				ml.Evaluate(argString);
				ml.WaitForAnswer();
				double[] timesdouble = (double[]) ml.GetArray(typeof(double),1); // get the list of switch times
				// The list of times is in seconds. Convert to the units of the clockFrequency and round to the nearest unit
				int[] times = new int[timesdouble.Length];
				for(int i = 0; i < timesdouble.Length; i++)
				{
					times[i] = (int)Math.Round((int)settings["clockFrequency"]*timesdouble[i]);
				}
				//Console.WriteLine("Generated Timing Sequence");
				decelSequence = new TimingSequence();
				
				ml.Evaluate("getVersion[]");
				ml.WaitForAnswer();
				decelSequence.Version = ml.GetString();
				
				ml.Evaluate("getDeceleratorName[]");
				ml.WaitForAnswer();
				decelSequence.Name = ml.GetString();

				// Get the decelerator structure
				ml.Evaluate("getStructure[" + resonanceOrder + "]");
				ml.WaitForAnswer();
				int[] structure = (int[])ml.GetArray(typeof(int),1);
				// keep track of the state of the horizontal and vertical electrodes
				bool[] states = {false, false}; //{horizontal, vertical}
				
				// The timing sequence is built within this for loop. The structure of the decelerator is encoded
				// as follows: 0 means everything off, 1 means V on, H off, 2 means H on V off and 3 means both on.
				for (int i = 0; i < times.Length && i < structure.Length; i++)
				{
					if (structure[i] == 0) //horizontal = false, vertical = false
					{
						if (states[0] != false)
						{
							decelSequence.Add("decelhplus", times[i], false);
							decelSequence.Add("decelhminus", times[i], false);
							states[0] = false;
						}
						if (states[1] != false)
						{
							decelSequence.Add("decelvplus", times[i], false);
							decelSequence.Add("decelvminus", times[i], false);
							states[1] = false;
						}
					}

					if (structure[i] == 1) //horizontal = false, vertical = true
					{
						if (states[0] != false)
						{
							decelSequence.Add("decelhplus", times[i], false);
							decelSequence.Add("decelhminus", times[i], false);
							states[0] = false;
						}
						if (states[1] != true)
						{
							decelSequence.Add("decelvplus", times[i], true);
							decelSequence.Add("decelvminus", times[i], true);
							states[1] = true;
						}
					}

					if (structure[i] == 2) //horizontal = true, vertical = false
					{
						if (states[0] != true)
						{
							decelSequence.Add("decelhplus", times[i], true);
							decelSequence.Add("decelhminus", times[i], true);
							states[0] = true;
						}
						if (states[1] != false)
						{
							decelSequence.Add("decelvplus", times[i], false);
							decelSequence.Add("decelvminus", times[i], false);
							states[1] = false;
						}
					}

					if (structure[i] == 3) //horizontal = true, vertical = true
					{
						if (states[0] != true)
						{
							decelSequence.Add("decelhplus", times[i], true);
							decelSequence.Add("decelhminus", times[i], true);
							states[0] = true;
						}
						if (states[1] != true)
						{
							decelSequence.Add("decelvplus", times[i], true);
							decelSequence.Add("decelvminus", times[i], true);
							states[1] = true;
						}
					}
					
				}
        
				//Console.WriteLine(decelSequence.ToString());
			}
		}
	
		
	}
}
