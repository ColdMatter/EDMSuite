using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
using DAQ.HAL;
using DAQ.Environment;

namespace TriggeredShapedPulses
{

    public class Controller : MarshalByRefObject
    {
        NIRfsg _rfsgSession;

        const double rfCarrierFreq = 170e6;
        const double samplingRate = 100e6;
        
        Dictionary<int, ShapedPulse> pulses = new Dictionary<int, ShapedPulse>();
        Dictionary<int, string> RfChoice = new Dictionary<int, string>();

        private int topRfChoice;
        private int botRfChoice;

        public Controller() { }

        public int TopRfChoice
        {
            get { return topRfChoice; }
            set { topRfChoice = value; }
        }
        public int BotRfChoice
        {
            get { return botRfChoice; }
            set { botRfChoice = value; }
        }

        public void LoadPulses(double[][] pulseParams)
        {
            pulses.Add(1, new ShapedPulse());
            pulses.Add(2, new ShapedPulse());

            foreach (int key in pulses.Keys)
            {
                Console.WriteLine("Loading pulse " + key + ": " + pulseParams[key - 1][0] + "/" + pulseParams[key - 1][1] + "/" + pulseParams[key - 1][2]);
                Console.WriteLine("Pulse frequency (in Hz): " + pulseParams[key - 1][3]);
                Console.WriteLine("Phase offset (in rad): " + pulseParams[key - 1][4]);
                pulses[key].Param0 = pulseParams[key - 1][0];
                pulses[key].Param1 = pulseParams[key - 1][1];
                pulses[key].Param2 = pulseParams[key - 1][2];
                pulses[key].Freq = pulseParams[key - 1][3];
                pulses[key].Phase = pulseParams[key - 1][4];
            }

            Console.WriteLine("Pulses loaded successfully.");
        }
        public void Initialise()
        {
            RfChoice.Add(1, "pulse1");
            RfChoice.Add(2, "pulse2");

            topRfChoice = 1;
            botRfChoice = 1;

            string resourceName = (String)Environs.Hardware.Boards["rfAWG"]; 
            double ArbPreFilterGain = -2;
            int waveformRepeatCount = 1;

            double power = 5;
            double pulseLength = 1e-6;
            double deadTime = 3e-7; // The generated pulses seem to cut off at some point if I use
                                    // all the sampling points available, hence the need for some
                                    // 'dead time' at the end where the output is all zeros.

            int numberOfSamples, numberOfDeadPoints;
            int waveformQuantum;
            double actualIQRate, actualPulseLength;
            double bandwidth;

            try
            {
                Console.WriteLine("Initialising..");

                // Initialise the NIRfsg session
                _rfsgSession = new NIRfsg(resourceName, true, false);

                // Subscribe to warnings
                _rfsgSession.DriverOperation.Warning += new EventHandler<RfsgWarningEventArgs>(DriverOperation_Warning);

                // Configure instrument
                _rfsgSession.RF.Configure(rfCarrierFreq, power);
                _rfsgSession.Arb.GenerationMode = RfsgWaveformGenerationMode.Script;
                _rfsgSession.Arb.IQRate = samplingRate;
                _rfsgSession.Arb.PreFilterGain = ArbPreFilterGain;
                _rfsgSession.RF.PowerLevelType = RfsgRFPowerLevelType.PeakPower;

                // Enable finite generation
                _rfsgSession.Arb.IsWaveformRepeatCountFinite = true;
                _rfsgSession.Arb.WaveformRepeatCount = waveformRepeatCount;

                // Get actual IQ rate
                actualIQRate = _rfsgSession.Arb.IQRate;
                Console.WriteLine("Sampling rate: " + actualIQRate);

                // Calculate waveform quantum based on pulse length
                waveformQuantum = _rfsgSession.Arb.WaveformCapabilities.WaveformQuantum;
                numberOfSamples = CoerceToQuantum(actualIQRate * pulseLength, waveformQuantum);
                Console.WriteLine("No. of samples: " + numberOfSamples);

                // Get actual pulse length
                actualPulseLength = numberOfSamples / actualIQRate;
                Console.WriteLine("Pulse length: " + actualPulseLength + " s");

                // Configure bandwidth
                bandwidth = 2 / actualPulseLength;
                _rfsgSession.Arb.SignalBandwidth = bandwidth;

                // Get number of dead points at the end
                numberOfDeadPoints = Convert.ToInt32(actualIQRate * deadTime);
                Console.WriteLine("No. of dead points: " + numberOfDeadPoints);

                // Initialise waveforms and write waveforms to device
                double[] initialParams = new double[3] { 1, 0, 0 };
                double[] blankData = Enumerable.Repeat(0.0, numberOfSamples).ToArray();
                double[][] writeData;

                foreach(int key in pulses.Keys)
                {
                    writeData = MakePulse(pulses[key], numberOfSamples, numberOfDeadPoints);
                    _rfsgSession.Arb.WriteWaveform(RfChoice[key], writeData[0], writeData[1]);
                }
                _rfsgSession.Arb.WriteWaveform("allZeros", blankData, blankData);

            }

            catch (Exception ex)
            {
                ShowError("Initialise()", ex);
            }

        }

        public void StartGeneration()
        {
            string BotRFPulse = RfChoice[botRfChoice];
            string TopRFPulse = RfChoice[topRfChoice];

            string script = @"script myScript" + Environment.NewLine +
                "   Repeat forever" + Environment.NewLine +
                "      Generate allZeros" + Environment.NewLine +
                "      Clear scriptTrigger0" + Environment.NewLine +
                "      Wait until scriptTrigger0" + Environment.NewLine +
                "      Clear scriptTrigger1" + Environment.NewLine +
                "      Repeat until scriptTrigger1" + Environment.NewLine +
                "         Generate " + BotRFPulse + Environment.NewLine +
                "      end repeat" + Environment.NewLine +
                "      Clear scriptTrigger1" + Environment.NewLine +
                "      Repeat until scriptTrigger1" + Environment.NewLine +
                "         Generate " + TopRFPulse + Environment.NewLine +
                "      end repeat" + Environment.NewLine +
                "   end repeat" + Environment.NewLine +
                "end script";

            RfsgScriptTriggerType triggerType1;
            RfsgScriptTriggerType triggerType2;
            string triggerSource1;
            string triggerSource2;

            triggerType1 = RfsgScriptTriggerType.DigitalEdge;
            triggerType2 = RfsgScriptTriggerType.DigitalEdge;
            triggerSource1 = RfsgDigitalEdgeScriptTriggerSource.Pfi0;
            triggerSource2 = RfsgDigitalEdgeScriptTriggerSource.Pfi1;

            try
            {

                Console.WriteLine("Starting generation of pulses..");

                // Configure scriptTrigger0 (refer to the script) 
                if (triggerType1 == RfsgScriptTriggerType.DigitalEdge)
                {
                    _rfsgSession.Triggers.ScriptTriggers[0].DigitalEdge.Configure(triggerSource1, RfsgTriggerEdge.RisingEdge);
                }
                else if (triggerType1 == RfsgScriptTriggerType.DigitalLevel)
                {
                    _rfsgSession.Triggers.ScriptTriggers[0].DigitalLevel.Configure(triggerSource1, RfsgTriggerLevel.ActiveHigh);
                }
                else
                {
                    _rfsgSession.Triggers.ScriptTriggers[0].Disable();
                }

                // Configure scriptTrigger1 (refer to the script) 
                if (triggerType2 == RfsgScriptTriggerType.DigitalEdge)
                {
                    _rfsgSession.Triggers.ScriptTriggers[1].DigitalEdge.Configure(triggerSource2, RfsgTriggerEdge.RisingEdge);
                }
                else if (triggerType2 == RfsgScriptTriggerType.DigitalLevel)
                {
                    _rfsgSession.Triggers.ScriptTriggers[1].DigitalLevel.Configure(triggerSource2, RfsgTriggerLevel.ActiveHigh);
                }
                else
                {
                    _rfsgSession.Triggers.ScriptTriggers[1].Disable();
                }

                // Write the script 
                _rfsgSession.Arb.Scripting.WriteScript(script);

                // Initiate Generation 
                _rfsgSession.Initiate();
            }

            catch (Exception ex)
            {
                ShowError("StartGeneration()", ex);
            }

        }
        public void PauseGeneration()
        {
            try
            {
                // Abort current generation
                _rfsgSession.Abort();
            }
            catch (Exception ex)
            {
                ShowError("PauseGeneration()", ex);
            }
        }
        public void StopGeneration()
        {
            try
            {
                if (_rfsgSession != null)
                {
                    // Disable the output
                    _rfsgSession.RF.OutputEnabled = false;

                    // Unsubscribe from warning events
                    _rfsgSession.DriverOperation.Warning -= DriverOperation_Warning;

                    // Close the RFSG NIRfsg session
                    _rfsgSession.Close();
                }

                _rfsgSession = null;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in StopGeneration(): " + ex.Message);
            }
        }

        private void ShowError(string functionName, Exception exception)
        {
            StopGeneration();
            Console.WriteLine("Error in " + functionName + ": " + exception.Message);
        }
        private void DriverOperation_Warning(object sender, RfsgWarningEventArgs e)
        {
            // Display the rfsg warning
            Console.WriteLine(e.Message);
        }
        private static int CoerceToQuantum(double numberOfSamples, int waveformQuantum)
        {
            double smallestNumberOfSamples;

            if (waveformQuantum <= 0)
                return -1;

            if (numberOfSamples >= waveformQuantum)
                smallestNumberOfSamples = numberOfSamples;
            else
                smallestNumberOfSamples = waveformQuantum;

            return Convert.ToInt32(smallestNumberOfSamples / waveformQuantum) * waveformQuantum;
        }
        private static double[][] MakePulse(ShapedPulse pulse, int noSamples, int noDeadPts)
        {
            double[][] iqArray = new double[2][]
            {
                new double[noSamples],
                new double[noSamples]
            };
            double[] amplitude = new double[noSamples];
            double[] cosArray = new double[noSamples];
            double[] sinArray = new double[noSamples];

            for (int i = 0; i < (noSamples - noDeadPts); i++)
            {
                amplitude[i] = (pulse.Param0 + pulse.Param1 * Math.Sin(Math.PI * i / (noSamples - noDeadPts)) + pulse.Param2 * Math.Sin(2 * Math.PI * i / (noSamples - noDeadPts)));
                cosArray[i] = Math.Cos(2 * Math.PI * (pulse.Freq - samplingRate) * i + pulse.Phase);
                sinArray[i] = Math.Sin(2 * Math.PI * (pulse.Freq - samplingRate) * i + pulse.Phase);
                iqArray[0][i] = amplitude[i] * cosArray[i];
                iqArray[1][i] = amplitude[i] * sinArray[i];
            }

            for (int i = noSamples - noDeadPts + 1; i < noSamples; i++)
            {
                iqArray[0][i] = 0;
                iqArray[1][i] = 0;
            }

            return iqArray;
        }
    }
}
