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
			settings["delayToDeceleration"] = 0;
			settings["molecule"] = "YbF";
			settings["voltage"] = 10.0;
			settings["initspeed"] = 337;
			settings["initposition"] = 0;
			settings["onposition"] = 24.0;
			settings["offposition"] = 32.0;
			settings["numberOfStages"] = 21;
            settings["sequenceLength"] = 2;
            settings["resonanceOrder"] = 1;
            settings["decelOnStart"] = 300;
            settings["decelOnDuration"] = 800;
            settings["modulationMode"] = "BurstAndOff";
            settings["jState"] = 0;
            settings["mState"] = 0;
		}


		protected override void DoAcquisitionStarting()
		{
			decelPatternBuilder = new DecelerationPatternBuilder();

			// prepare the deceleration sequence
			buildDecelerationSequence();
		}

		protected override IPatternSource GetScanPattern()
		{
			decelPatternBuilder.Clear();
			decelPatternBuilder.ShotSequence(
				(int)settings["padStart"],
				(int)settings["sequenceLength"],
				(int)settings["padShots"], 
				(int)settings["flashlampPulseInterval"],
				(int)settings["valvePulseLength"],
				(int)settings["valveToQ"],
				(int)settings["flashToQ"],
                GateStartTimePGUnits,
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

        private void buildDecelerationSequence()
        {
            //get the settings we need
            double voltage = (double)settings["voltage"];
            int initspeed = (int)settings["initspeed"];
            double onposition = (double)settings["onposition"] / 1000;
            double offposition = (double)settings["offposition"] / 1000;
            int numberOfStages = (int)settings["numberOfStages"];
            int resonanceOrder = (int)settings["resonanceOrder"];
            int jState = (int)settings["jState"];
            int mState = (int)settings["mState"];

            // make the FieldMap
            FieldMap map = new FieldMap((string)Environs.FileSystem.Paths["decelerationUtilitiesPath"] +
                (string)Environs.Hardware.GetInfo("deceleratorFieldMap"),
                (int)Environs.Hardware.GetInfo("mapPoints"),
                (double)Environs.Hardware.GetInfo("mapStartPoint"),
                (double)Environs.Hardware.GetInfo("mapResolution"));
            //make the molecule
            Molecule mol = new Molecule((string)Environs.Hardware.GetInfo("moleculeName"),
                (double)Environs.Hardware.GetInfo("moleculeMass"),
                (double)Environs.Hardware.GetInfo("moleculeRotationalConstant"),
                (double)Environs.Hardware.GetInfo("moleculeDipoleMoment"));
            //make the decelerator and the experiment
            Decelerator decel = new Decelerator();
            DecelerationExperiment experiment = new DecelerationExperiment();
            //assign a map and a lens spacing to the decelerator
            decel.Map = map;
            decel.LensSpacing = (double)Environs.Hardware.GetInfo("deceleratorLensSpacing");
            //assign decelerator, molecule, quantum state and switch structure to the experiment
            experiment.Decelerator = decel;
            experiment.Molecule = mol;
            experiment.QuantumState = new int[] { jState, mState };
            experiment.Structure = (DecelerationExperiment.SwitchStructure)Environs.Hardware.GetInfo("deceleratorStructure");
            //get the timing sequence
            double[] timesdouble = experiment.GetTimingSequence(voltage, onposition, offposition, initspeed, numberOfStages, resonanceOrder);

            // The list of times is in seconds and is measured from the moment when the synchronous molecule 
            // reaches the decelerator. Add on the amount of time it takes to reach this point.
            // Then convert to the units of the clockFrequency and round to the nearest unit.
            double nominalTimeToDecelerator = (double)Environs.Hardware.GetInfo("sourceToSoftwareDecelerator") / initspeed;
            int[] times = new int[timesdouble.Length];
            for (int i = 0; i < timesdouble.Length; i++)
            {
                times[i] = (int)Math.Round((int)settings["clockFrequency"] * (nominalTimeToDecelerator + timesdouble[i]));
            }

            int[] structure = new int[times.Length];

            // the last switch must send all electrodes to ground
            structure[structure.Length - 1] = 0;
            // build the rest of the structure
            int k = 0;
            switch (experiment.Structure)
            {
                case DecelerationExperiment.SwitchStructure.H_Off_V_Off:
                    while (k < structure.Length - 1)
                    {
                        if (k < structure.Length - 1) { structure[k] = 2; k++; }
                        if (k < structure.Length - 1) { structure[k] = 0; k++; }
                        if (k < structure.Length - 1) { structure[k] = 1; k++; }
                        if (k < structure.Length - 1) { structure[k] = 0; k++; }
                    }
                    break;
                case DecelerationExperiment.SwitchStructure.V_Off_H_Off:
                    while (k < structure.Length - 1)
                    {
                        if (k < structure.Length - 1) { structure[k] = 1; k++; }
                        if (k < structure.Length - 1) { structure[k] = 0; k++; }
                        if (k < structure.Length - 1) { structure[k] = 2; k++; }
                        if (k < structure.Length - 1) { structure[k] = 0; k++; }
                    }
                    break;
                case DecelerationExperiment.SwitchStructure.H_V:
                    while (k < structure.Length - 1)
                    {
                        if (k < structure.Length - 1) { structure[k] = 2; k++; }
                        if (k < structure.Length - 1) { structure[k] = 1; k++; }
                    }
                    break;
                case DecelerationExperiment.SwitchStructure.V_H:
                    while (k < structure.Length - 1)
                    {
                        if (k < structure.Length - 1) { structure[k] = 1; k++; }
                        if (k < structure.Length - 1) { structure[k] = 2; k++; }
                    }
                    break;
            }
            // keep track of the state of the horizontal and vertical electrodes
            bool[] states = { false, false }; //{horizontal, vertical}
            decelSequence = new TimingSequence();
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
            Console.WriteLine(decelSequence.ToString());
        }

        //// This is old code that uses a Mathematica package to generate the timing sequence
        //// Keep it here for now, so that we can make sure the two codes do the same thing.
        //private void buildDecelerationSequence()
        //{
        //    String molecule = (string)settings["molecule"];
        //    double voltage = (double)settings["voltage"];
        //    int initspeed = (int)settings["initspeed"];
        //    int initposition = (int)settings["initposition"];
        //    double onposition = (double)settings["onposition"];
        //    double offposition = (double)settings["offposition"];
        //    int numberOfStages = (int)settings["numberOfStages"];
        //    int resonanceOrder = (int)settings["resonanceOrder"];

        //    IKernelLink ml = MathematicaService.GetKernel();
        //    MathematicaService.LoadPackage((String)Environs.Info["SwitchSequenceCode"], false);
        //    if (ml != null) 
        //    {
        //        //Here's the call to the Mathematica makeSequence function
        //        String argString = "makeSequence[" + molecule + ", " + voltage + ", " + initspeed + ", " + initposition + ", " +
        //            onposition + ", " + offposition + ", " + numberOfStages + ", " + resonanceOrder + "]";
        //        ml.Evaluate(argString);
        //        ml.WaitForAnswer();
        //        double[] timesdouble = (double[]) ml.GetArray(typeof(double),1); // get the list of switch times

        //        // The list of times is in seconds and is measured from the moment when the synchronous molecule 
        //        // reaches the decelerator. Add on the amount of time it takes to reach this point.
        //        // Then convert to the units of the clockFrequency and round to the nearest unit.
        //        double nominalTimeToDecelerator = (double)Environs.Hardware.GetInfo("sourceToSoftwareDecelerator") / initspeed;
        //        int[] times = new int[timesdouble.Length];
        //        for(int i = 0; i < timesdouble.Length; i++)
        //        {
        //            times[i] = (int)Math.Round((int)settings["clockFrequency"]*(nominalTimeToDecelerator + timesdouble[i]));
        //        }
        //        //Console.WriteLine("Generated Timing Sequence");
        //        decelSequence = new TimingSequence();
				
        //        ml.Evaluate("getVersion[]");
        //        ml.WaitForAnswer();
        //        decelSequence.Version = ml.GetString();
				
        //        ml.Evaluate("getDeceleratorName[]");
        //        ml.WaitForAnswer();
        //        decelSequence.Name = ml.GetString();

        //        // Get the decelerator structure
        //        ml.Evaluate("getStructure[" + resonanceOrder + "]");
        //        ml.WaitForAnswer();
        //        int[] structure = (int[])ml.GetArray(typeof(int),1);
        //        // keep track of the state of the horizontal and vertical electrodes
        //        bool[] states = {false, false}; //{horizontal, vertical}
				
        //        // The timing sequence is built within this for loop. The structure of the decelerator is encoded
        //        // as follows: 0 means everything off, 1 means V on, H off, 2 means H on V off and 3 means both on.
        //        for (int i = 0; i < times.Length && i < structure.Length; i++)
        //        {
        //            if (structure[i] == 0) //horizontal = false, vertical = false
        //            {
        //                if (states[0] != false)
        //                {
        //                    decelSequence.Add("decelhplus", times[i], false);
        //                    decelSequence.Add("decelhminus", times[i], false);
        //                    states[0] = false;
        //                }
        //                if (states[1] != false)
        //                {
        //                    decelSequence.Add("decelvplus", times[i], false);
        //                    decelSequence.Add("decelvminus", times[i], false);
        //                    states[1] = false;
        //                }
        //            }

        //            if (structure[i] == 1) //horizontal = false, vertical = true
        //            {
        //                if (states[0] != false)
        //                {
        //                    decelSequence.Add("decelhplus", times[i], false);
        //                    decelSequence.Add("decelhminus", times[i], false);
        //                    states[0] = false;
        //                }
        //                if (states[1] != true)
        //                {
        //                    decelSequence.Add("decelvplus", times[i], true);
        //                    decelSequence.Add("decelvminus", times[i], true);
        //                    states[1] = true;
        //                }
        //            }

        //            if (structure[i] == 2) //horizontal = true, vertical = false
        //            {
        //                if (states[0] != true)
        //                {
        //                    decelSequence.Add("decelhplus", times[i], true);
        //                    decelSequence.Add("decelhminus", times[i], true);
        //                    states[0] = true;
        //                }
        //                if (states[1] != false)
        //                {
        //                    decelSequence.Add("decelvplus", times[i], false);
        //                    decelSequence.Add("decelvminus", times[i], false);
        //                    states[1] = false;
        //                }
        //            }

        //            if (structure[i] == 3) //horizontal = true, vertical = true
        //            {
        //                if (states[0] != true)
        //                {
        //                    decelSequence.Add("decelhplus", times[i], true);
        //                    decelSequence.Add("decelhminus", times[i], true);
        //                    states[0] = true;
        //                }
        //                if (states[1] != true)
        //                {
        //                    decelSequence.Add("decelvplus", times[i], true);
        //                    decelSequence.Add("decelvminus", times[i], true);
        //                    states[1] = true;
        //                }
        //            }
					
        //        }
        
        //        Console.WriteLine(decelSequence.ToString());
        //    }
        //}
	
		
	}
}
