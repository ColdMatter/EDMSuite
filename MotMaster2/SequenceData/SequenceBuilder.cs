using System;
using System.Collections.Generic;
using System.Linq;
using DAQ.Environment;
using DAQ.Analog;
using DAQ.Pattern;
using DataStructures;
using dotMath;

namespace MOTMaster2.SequenceData
{
    //Builds a list of SequenceSteps into a MOTMasterSequence. Inherits from MOTMaster script so that it can function as one
    class SequenceBuilder : MOTMasterScript
    {
        private MOTMasterSequence sequence;
        private List<SequenceStep> sequenceSteps;
        private AnalogPatternBuilder analogPB;
        private HSDIOPatternBuilder hsPB;
        private MuquansBuilder muPB;
        private double currentTime;

        public SequenceBuilder(List<SequenceStep> steps, Dictionary<string,object> prms)
        {
            sequence = new MOTMasterSequence();
            sequenceSteps = steps;
            Parameters = prms;
        }

        public SequenceBuilder(Sequence sequenceData)
        {
            sequence = new MOTMasterSequence();
            sequenceSteps = sequenceData.Steps;
            Parameters = sequenceData.CreateParameterDictionary();
        }

        public void CreatePatternBuilders()
        {
           
            analogPB = new AnalogPatternBuilder((int)Parameters["AnalogLength"]);
            hsPB = new HSDIOPatternBuilder();  
            muPB = new MuquansBuilder();
        }
        //Builds a MOTMasterSequence using a list of SequenceSteps
        public void BuildSequence()
        {
            CreatePatternBuilders();
            foreach (string channel in Environs.Hardware.AnalogOutputChannels.Keys) analogPB.AddChannel(channel);
            double timeMultiplier = 1.0;
            int analogClock = (int)Parameters["AnalogClockFrequency"];
            int digitalClock = (int)Parameters["HSClockFrequency"];
            //These hardcoded times are used to specify a pre-trigger time for both the trigger to send the serial command and the trigger to start the laser frequency ramp.
            int serialPreTrigger = ConvertToSampleTime(4, digitalClock);
            int serialWait = ConvertToSampleTime(2, digitalClock);
            SequenceStep previousStep = null;
            foreach (SequenceStep step in sequenceSteps)
            {
               
                if (!step.Enabled) continue;
                if (step.Timebase == TimebaseUnits.ms) timeMultiplier = 1.0;
                else if (step.Timebase == TimebaseUnits.us) timeMultiplier = 0.001;
                else if (step.Timebase == TimebaseUnits.s) timeMultiplier = 1000.0;

                //TODO Include a method for delaying a digital edge
                int digitalStartTime = ConvertToSampleTime(currentTime,digitalClock);

                
                foreach (string analogChannel in step.GetUsedAnalogChannels())
                {
                  AddAnalogChannelStep(timeMultiplier, analogClock, step, analogChannel);

                }
                //Adds the edges for each digital channel
                foreach (string digitalChannel in step.GetUsedDigitalChannels(previousStep))
                {
                    AddDigitalChannelStep(step, digitalStartTime, digitalChannel);
                }
                //Adds the Muquans string commands as well as the required serial pulses
                if (step.RS232Commands)
                {
                    string laserID = "";
                    //TODO Fix the sequence parser to make it work with more generic serial commands
                    foreach (SerialItem serialCommand in step.GetSerialData())
                    {
                        
                        if (serialCommand.Name == "Slave") laserID = "slave0";
                        else if (serialCommand.Name == "AOM") laserID = "mphi";
                        string[] valueArr = serialCommand.Value.Split(';');
                        if (valueArr[0] == "Set") muPB.SetFrequency(laserID, SequenceParser.ParseOrGetParameter(valueArr[1]));
                        else if (valueArr[0] == "Sweep") muPB.SweepFrequency(laserID, SequenceParser.ParseOrGetParameter(valueArr[1]), SequenceParser.ParseOrGetParameter(valueArr[2]));
                        //TODO Fix TimeOrder issues with the serial triggers
                        hsPB.Pulse(digitalStartTime, -(serialPreTrigger+serialWait), 200, "serialPreTrigger");
                        hsPB.Pulse(digitalStartTime, serialWait, 200, "slaveDDSTrig");
                        hsPB.Pulse(digitalStartTime, serialWait, 200, "aomDDSTrig");
                    }
                }

                
                //Adds the time of the sequence step to the total running time
                currentTime += step.Duration * timeMultiplier;
                previousStep = step;

            }
        }

        public override AnalogPatternBuilder GetAnalogPattern()
        {
            return analogPB;
        }

        public override HSDIOPatternBuilder GetHSDIOPattern()
        {
            return hsPB;
        }

        public override MuquansBuilder GetMuquansCommands()
        {
            return muPB;
        }

        public override PatternBuilder32 GetDigitalPattern()
        {
            throw new NotImplementedException();
        }
        public override MMAIConfiguration GetAIConfiguration()
        {
            return null;
        }


        private void AddDigitalChannelStep(SequenceStep step, int digitalStartTime, string digitalChannel)
        {
                            hsPB.AddEdge(digitalChannel, digitalStartTime, step.GetDigitalData(digitalChannel));
        }

        private void AddAnalogChannelStep(double timeMultiplier, int analogClock, SequenceStep step, string analogChannel)
        {
                            AnalogChannelSelector channelType = step.GetAnalogChannelType(analogChannel);
            //Does not try to add anything if the channel does not do anything during this time
                            if (channelType == AnalogChannelSelector.Continue) return;
                            double startTime = step.GetAnalogStartTime(analogChannel);
                            int analogStartTime = ConvertToSampleTime(currentTime+startTime,analogClock);
                            double value = 0.0;
                            if (channelType != AnalogChannelSelector.Function && channelType != AnalogChannelSelector.XYPairs) value = step.GetAnalogValue(analogChannel);
                            int duration;
                            switch (channelType)
                            {
                                case AnalogChannelSelector.Continue:
                                    break;
                                case AnalogChannelSelector.SingleValue:
                                    analogPB.AddAnalogValue(analogChannel,analogStartTime,value);
                                    break;
                                case AnalogChannelSelector.LinearRamp:
                                    duration = ConvertToSampleTime(step.GetAnalogDuration(analogChannel) * timeMultiplier, analogClock);
                                    analogPB.AddLinearRamp(analogChannel,analogStartTime,duration,value);
                                    break;
                                case AnalogChannelSelector.Pulse:
                                    duration = ConvertToSampleTime(step.GetAnalogDuration(analogChannel)*timeMultiplier,analogClock);
                                    double finalValue = step.GetAnalogFinalValue(analogChannel);
                                    analogPB.AddAnalogPulse(analogChannel, analogStartTime, duration, value, finalValue);
                                    break;
                                case AnalogChannelSelector.Function:
                                    string analogFunction = step.GetFunction(analogChannel);
                                    duration = ConvertToSampleTime(step.GetAnalogDuration(analogChannel)*timeMultiplier,analogClock);
                                    CompileAnalogFunction(analogChannel, analogFunction, startTime, analogStartTime,analogClock,duration);
                                    break;
                                case AnalogChannelSelector.XYPairs:
                                    List<double[]> xypairs = step.GetXYPairs(analogChannel);
                                    string interpolationType = step.GetInterpolationType(analogChannel);
                                    double[] xvals = xypairs[0];
                                    double[] yvals = xypairs[1];
                                    if (interpolationType == "Step")
                                    {
                                        for (int i = 0; i < xvals.Length; i++)
                                        {
                                            int valTime = analogStartTime + ConvertToSampleTime(xvals[i] * timeMultiplier, analogClock);
                                            analogPB.AddAnalogValue(analogChannel, valTime, yvals[i]);
                                        }
                                    }
                                    else if (interpolationType == "Piecewise Linear")
                                    {
                                        int nClockCycles;
                                        for (int i = 0; i < xvals.Length-1; i++)
                                        {
                                            nClockCycles = ConvertToSampleTime((xvals[i + 1] - xvals[i]) * timeMultiplier, analogClock);
                                            analogPB.AddLinearRamp(analogChannel, analogStartTime, nClockCycles, yvals[i]);
                                            analogStartTime += nClockCycles;
                                        }
                                    }
                                    else throw new Exception("Specified Interpolation type unsupported. Redefine it as an equation.");
                                    break;
                            }
}

        /// <summary>
        /// Converts a time from milliseconds into number of samples
        /// </summary>
        /// <param name="time"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        public int ConvertToSampleTime(double time, int frequency)
        {
            return (int)(time * frequency / 1000);
        }
        public double ConvertToRealTime(int sampleTime, int frequency)
        {
            return sampleTime * 1000.0 / frequency;
        }

       
        /// <summary>
        /// Compiles a function string for a channel and adds its values to the analog pattern.
        /// </summary>
        /// <param name="analogChannel">name of the analog channel</param>
        /// <param name="function">function string to compile</param>
        /// <param name="startTime">start time of the function relative to the start of the sequence step (in units of the timebase)</param>
        /// <param name="analogStartTime">start time of the sequence step relative to the start of the whole pattern (in units of samples output by the card)</param>
        /// <param name="analogClock">Clock frequency of the card</param>
        /// <param name="duration">Duration of the function output in units of samples</param>
        void CompileAnalogFunction(string analogChannel, string function, double startTime, int analogStartTime, int analogClock, int duration)
        {
            EqCompiler compiler = new EqCompiler(function, true);
            compiler.Compile();
            double funcValue;
            bool timeFunc = false;
            //Checks all variables to use values in parameter dictionary
            foreach (string variable in compiler.GetVariableList())
            {
                if (variable == "t")
                {
                    timeFunc = true;
                }
                else if (Parameters.Keys.Contains(variable))
                {
                    compiler.SetVariable(variable, (double)Parameters[variable]);
                }
            }
            if (timeFunc)
            {
                for (int i = 0; i < duration; i++)
                {
                    compiler.SetVariable("t", startTime);
                    funcValue = compiler.Calculate();
                    analogPB.AddAnalogValue(analogChannel, analogStartTime + i, funcValue);
                    startTime += 1.0 / analogClock;
                }
            }
            else
            {
                //If the function evaluates to a single value, just this is used
                funcValue = compiler.Calculate();
                analogPB.AddAnalogValue(analogChannel, analogStartTime, funcValue);
            }
            
        }
    }

}
