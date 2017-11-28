using System;
using System.Collections.Generic;
using System.Linq;
using DAQ.Environment;
using DAQ.Analog;
using DAQ.Pattern;
using DataStructures;
using DAQ;
using dotMath;
using System.Collections.ObjectModel;

namespace MOTMaster2.SequenceData
{
    //Builds a list of SequenceSteps into a MOTMasterSequence. Inherits from MOTMaster script so that it can function as one
    class SequenceBuilder : MOTMasterScript
    {
        private MOTMasterSequence sequence;
        private ObservableCollection<SequenceStep> sequenceSteps;
        private AnalogPatternBuilder analogPB;
        private PatternBuilder32 digitalPB;
        private SerialBuilder muPB;
        private SequenceStep _sequenceStep;
        private bool staticSequence;
        private int analogClock;
        private int digitalClock;
        private int[] digitalSampleTimes;
        private double[] currentTimes;

        /*
        public SequenceBuilder(List<SequenceStep> steps, Dictionary<string, object> prms)
        {
            sequence = new MOTMasterSequence();
            sequenceSteps = steps;
            Parameters = prms;

        }
        */
        public SequenceBuilder(Sequence sequenceData)
        {
            sequence = new MOTMasterSequence();
            sequenceSteps = sequenceData.Steps;
            Parameters = sequenceData.CreateParameterDictionary();
            foreach (string entry in new List<string>() { "AnalogLength", "HSClockFrequency", "AnalogClockFrequency" })
            {
                if (!Parameters.ContainsKey(entry) && !Controller.config.Debug) throw new Exception(string.Format("Sequence does not contain the required parameter {0}", entry));
            }
        }

        public void CreatePatternBuilders()
        {
            analogClock = Controller.config.AnalogPatternClockFrequency;
            if (Controller.config.HSDIOCard) digitalClock = Controller.config.DigitalPatternClockFrequency;
            else digitalClock = (int)Parameters["PGClockFrequency"];

            GetPatternLengthsFromSteps();
            analogPB = new AnalogPatternBuilder((int)Parameters["AnalogLength"]);
            if (Controller.config.HSDIOCard) digitalPB = new HSDIOPatternBuilder();
            else digitalPB = new PatternBuilder32();
            if (Controller.config.UseMuquans) muPB = new SerialBuilder();
        }

        private void GetPatternLengthsFromSteps()
        {
            int index = 0;
            double currentTime = 0.0;
            int digitalSample = 0;
            digitalSampleTimes = new int[sequenceSteps.Where(t=>t.Enabled).Count()];
            currentTimes = new double[sequenceSteps.Where(t => t.Enabled).Count()];
            foreach (SequenceStep step in sequenceSteps)
            {

                double duration = 0.0;
                double timeMultiplier = 1.0;
               
                //TODO Is there a better way to reference parameters for the step duration?
                if (step.Duration is string)
                {
                    if (Double.TryParse((string)step.Duration, out duration)) ;
                    else if (Parameters.ContainsKey((string)step.Duration)) duration = Convert.ToDouble(Parameters[(string)step.Duration]);
                    else duration = CompileTimeEquation((string)step.Duration);
                }
                else duration = (double)step.Duration;
                if (!step.Enabled || duration - 0.0 < 1e-15) continue;
                if (step.Timebase == TimebaseUnits.ms) timeMultiplier = 1.0;
                else if (step.Timebase == TimebaseUnits.us) timeMultiplier = 0.001;
                else if (step.Timebase == TimebaseUnits.s) timeMultiplier = 1000.0;

                //Adds the time of the sequence step to the total running time

                //if (index == 0) { digitalSampleTimes[0] = ConvertToSampleTime(duration * timeMultiplier, digitalClock); }
                //else
                //{
                //    digitalSampleTimes[index] = digitalSampleTimes[index - 1] + ConvertToSampleTime(duration * timeMultiplier, digitalClock);
                    
                //}
                digitalSampleTimes[index] = digitalSample;
                digitalSample += ConvertToSampleTime(duration * timeMultiplier, digitalClock);
                currentTimes[index] = currentTime;
                currentTime += duration * timeMultiplier;
                index++;
            }
            //Sets the Analog and Digital pattern lengths
            int analogLength = ConvertToSampleTime(currentTime, analogClock);
            int digitalLength = ConvertToSampleTime(currentTime, digitalClock);
            Parameters["AnalogLength"] = analogLength;
            Parameters["PatternLength"] = digitalLength;

        }
        //Builds a MOTMasterSequence using a list of SequenceSteps
        public void BuildSequence()
        {
            //List of digital channels which are reserved for trigger pulses. These are excluded when adding the edges for digital channels
            List<string> digitalChannelExcludeList = new List<string>() { "serialPreTrigger" };
            CreatePatternBuilders();

            foreach (string channel in Environs.Hardware.AnalogOutputChannels.Keys) analogPB.AddChannel(channel);

            int index = 0;
            //These hardcoded times are used to specify a pre-trigger time for both the trigger to send the serial command and the trigger to start the laser frequency ramp.
            int serialPreTrigger = ConvertToSampleTime(2, digitalClock);
            int serialWait = ConvertToSampleTime(2, digitalClock);

            SequenceStep previousStep = null;
            foreach (SequenceStep step in sequenceSteps)
            {
                _sequenceStep = step;
                if (!step.Enabled) continue;
                int digitalSample = digitalSampleTimes[index];
                double currentTime = currentTimes[index];
                double timeMultiplier = 0.0;
                if (step.Timebase == TimebaseUnits.ms) timeMultiplier = 1.0;
                else if (step.Timebase == TimebaseUnits.us) timeMultiplier = 0.001;
                else if (step.Timebase == TimebaseUnits.s) timeMultiplier = 1000.0;

                foreach (string analogChannel in step.GetUsedAnalogChannels())
                {
                    try
                    {
                        AddAnalogChannelStep(timeMultiplier, analogClock,currentTime, step, analogChannel);
                    }
                    catch
                    {
                       throw new Exception(string.Format("Failed to add analog data for Channel:{0} Step:{1}",analogChannel,step.Name));
                    }
                }
                //Adds the Muquans string commands as well as the required serial pulses before digital pulses to prevent time order exceptions
                if (step.RS232Commands)
                {
                    //TODO Fix the sequence parser to make it work with more generic serial commands
                    foreach (SerialItem serialCommand in step.GetSerialData())
                    {
                        AddSerialCommand(serialCommand);
                    }
                    try
                    {
                        digitalPB.Pulse(digitalSample - (serialPreTrigger + serialWait), 0, 200, "serialPreTrigger");
                    }
                    catch
                    {
                        double trigTime = (digitalSample - (serialPreTrigger + serialWait))/digitalClock;
                        throw new Exception(string.Format("Failed to add serial pre-trigger in step {0}. Expected at time {1}.", step.Name, trigTime));
                    }
                }
                //Adds the edges for each digital channel
                foreach (string digitalChannel in step.GetUsedDigitalChannels(previousStep))
                {
                    if (digitalChannelExcludeList.Contains(digitalChannel)) continue;
                    try
                    {
                        AddDigitalChannelStep(step, digitalSample, digitalChannel);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("Error adding digital value. Step: {0} Channel: {1}", step.Name, digitalChannel));
                    }
                }
                previousStep = step;
                index++;

            }
           
        }

        private void AddSerialCommand(SerialItem serialCommand)
        {

            if (serialCommand.Value.Contains("\\n")) throw new Exception(string.Format("Serial command {0} contains an escape command. This is not necessary",serialCommand.Value));
            string[] valueArr = serialCommand.Value.Split(' ');
            string val;
            for (int i = 0; i < valueArr.Length; i++)
            {
                val = (valueArr[i].EndsWith(";")) ? valueArr[i].TrimEnd(';') : valueArr[i];
                if (Parameters.ContainsKey(val)) valueArr[i] = (valueArr[i].EndsWith(";")) ? Parameters[val].ToString() + ";" : Parameters[val].ToString();
            }

            string command = string.Join(" ", valueArr);
            //TODO Make this work for general serial devices
            if (serialCommand.Name == "muquansSlave") muPB.AddCommand("slave0", command);
            else if (serialCommand.Name == "muquansAOM") muPB.AddCommand("mphi", command);
        }


        public override AnalogPatternBuilder GetAnalogPattern()
        {
            return analogPB;
        }

        public override HSDIOPatternBuilder GetHSDIOPattern()
        {
            return (HSDIOPatternBuilder)digitalPB;
        }

        public override SerialBuilder GetMuquansCommands()
        {
            return muPB;
        }

        public override PatternBuilder32 GetDigitalPattern()
        {
            return digitalPB;
        }
        public override MMAIConfiguration GetAIConfiguration()
        {
            MMAIConfiguration mmaiConfig = new MMAIConfiguration();
            try
            {
                mmaiConfig.AddChannel("accelerometer", -3.0, 3.0);
                mmaiConfig.AddChannel("photodiode", -3.0, 3.0);
            }
            catch (Exception e)
            {
                ErrorManager.ErrorMgr.errorMsg("Could not add analog input channels. Are accelerometer and photodiode channels defined in hardware class?", -4);
            }
            mmaiConfig.SampleRate = Controller.ExpData.SampleRate;
            mmaiConfig.Samples = Controller.ExpData.NSamples;
            return mmaiConfig;
        }


        private void AddDigitalChannelStep(SequenceStep step, int digitalStartTime, string digitalChannel)
        {
            if (digitalStartTime % 2 != 0) { Console.WriteLine(string.Format("Error {0}"), digitalStartTime); }
            digitalPB.AddEdge(digitalChannel, digitalStartTime, step.GetDigitalData(digitalChannel));
        }

        private void AddAnalogChannelStep(double timeMultiplier, int analogClock,double currentTime, SequenceStep step, string analogChannel)
        {
            AnalogChannelSelector channelType = step.GetAnalogChannelType(analogChannel);
            //Does not try to add anything if the channel does not do anything during this time
            if (channelType == AnalogChannelSelector.Continue) return;
            double startTime = step.GetAnalogStartTime(analogChannel);
            int analogStartTime = ConvertToSampleTime(currentTime + startTime, analogClock);
            double value = 0.0;
            try
            {
                if (channelType != AnalogChannelSelector.Function && channelType != AnalogChannelSelector.XYPairs) value = step.GetAnalogValue(analogChannel);
            }
            catch (Exception e)
            {
                throw new Exception(string.Format("Could not parse argument type {0}. Step: {1} Channel: {2}", channelType.ToString(), step.Name, analogChannel));
            }
            int duration;
            try
            {
                switch (channelType)
                {
                    case AnalogChannelSelector.Continue:
                        break;
                    case AnalogChannelSelector.SingleValue:
                        analogPB.AddAnalogValue(analogChannel, analogStartTime, value);
                        break;
                    case AnalogChannelSelector.LinearRamp:
                        duration = ConvertToSampleTime(step.GetAnalogDuration(analogChannel) * timeMultiplier, analogClock);
                        analogPB.AddLinearRamp(analogChannel, analogStartTime, duration, value);
                        break;
                    case AnalogChannelSelector.Pulse:
                        duration = ConvertToSampleTime(step.GetAnalogDuration(analogChannel) * timeMultiplier, analogClock);
                        double finalValue = step.GetAnalogFinalValue(analogChannel);
                        analogPB.AddAnalogPulse(analogChannel, analogStartTime, duration, value, finalValue);
                        break;
                    case AnalogChannelSelector.Function:
                        string analogFunction = step.GetFunction(analogChannel);
                        duration = ConvertToSampleTime(step.GetAnalogDuration(analogChannel) * timeMultiplier, analogClock);
                        CompileAnalogFunction(analogChannel, analogFunction, startTime, analogStartTime, analogClock, duration);
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
                            for (int i = 0; i < xvals.Length - 1; i++)
                            {
                                nClockCycles = ConvertToSampleTime((xvals[i + 1] - xvals[i]) * timeMultiplier, analogClock);
                                analogPB.AddLinearRamp(analogChannel, analogStartTime, nClockCycles, yvals[i + 1]);
                                if (i == xvals.Length - 2) analogPB.AddAnalogValue(analogChannel, analogStartTime, yvals[i + 1]);
                                analogStartTime += nClockCycles;
                            }
                        }
                        else throw new Exception("Specified interpolation type unsupported. Redefine it as an equation.");
                        break;
                }
            }
            catch (DAQ.Analog.AnalogPatternBuilder.ConflictInPatternException e)
            {
                throw new Exception(string.Format("Pattern conflict error. Step: {0} Channel: {1}", step.Name, analogChannel));
            }
            catch (DAQ.Analog.AnalogPatternBuilder.InsufficientPatternLengthException e)
            {
                throw new Exception(string.Format("Insufficient length for pattern. Step: {0} Channel: {1} Start time: {2}", step.Name, analogChannel, analogStartTime));
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
            if (function == "") throw new Exception(string.Format("Expected function for Channel: {0} in Step: {1}",analogChannel,_sequenceStep.Name));
            if (Parameters.Keys.Contains(function))
            {
                analogPB.AddAnalogValue(analogChannel, analogStartTime, (double)Parameters[function]);
                return;
            }
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
                else throw new Exception(string.Format("Variable {0} not found in parameters. Required for channel {1} in step {2}", variable, analogChannel, _sequenceStep.Name));
            }
            if (timeFunc)
            {
                if (duration == 0) throw new Exception(string.Format("Duration not set for function {0} in step {1}", analogChannel, _sequenceStep.Name));
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

        double CompileTimeEquation(string function)
        {
            EqCompiler compiler = new EqCompiler(function, true);
            compiler.Compile();

            //Checks all variables to use values in parameter dictionary
            foreach (string variable in compiler.GetVariableList())
            {
                if (Parameters.Keys.Contains(variable))
                {
                    compiler.SetVariable(variable, (double)Parameters[variable]);
                }
                else throw new Exception(string.Format("Variable {0} not found in parameters. Required for duration in step {1}", variable, _sequenceStep.Name));
            }

            return compiler.Calculate();
        }
    }

}
