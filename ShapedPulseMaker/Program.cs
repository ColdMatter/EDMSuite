using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;

namespace TriggeredShapedPulses
{
    class Program
    {
        NIRfsg _rfsgSession;

        const double ArbPreFilterGain = -2;

        static void Main(string[] args)
        {
            Program newSession = new Program();

            newSession.MakePulses();

            Console.WriteLine("Pulses generated and written to device.");

            int indicator = 0;
            int j = 0;
            string BottomRFPulse = " ";
            string TopRFPulse = " ";
            int BottomChoice = 0;
            int TopChoice = 0;

            while (j == 0)
            {
                indicator = 0;

                while (indicator == 0)
                {
                    Console.WriteLine("Choose the bottom RF pulse (1/2):");

                    if (int.TryParse(Console.ReadLine(), out BottomChoice))
                    {
                        if (BottomChoice == 1)
                        {
                            BottomRFPulse = "Pulse1";
                            indicator = 1;
                        }

                        else if (BottomChoice == 2)
                        {
                            BottomRFPulse = "Pulse2";
                            indicator = 1;
                        }
                        else
                            Console.WriteLine("Please stay within the choices given.");

                    }

                    else
                        Console.WriteLine("Please stay within the choices given.");

                }

                indicator = 0;

                while (indicator == 0)
                {
                    Console.WriteLine("Choose the top RF pulse (1/2):");

                    if (int.TryParse(Console.ReadLine(), out TopChoice))
                    {
                        if (TopChoice == 1)
                        {
                            TopRFPulse = "Pulse1";
                            indicator = 1;
                        }

                        else if (TopChoice == 2)
                        {
                            TopRFPulse = "Pulse2";
                            indicator = 1;
                        }
                        else
                            Console.WriteLine("Please stay within the choices given.");

                    }

                    else
                        Console.WriteLine("Please stay within the choices given.");

                }

                Console.WriteLine("You have chosen " + BottomRFPulse + " for the first pulse and " + TopRFPulse + " for the second pulse.");
                newSession.StartGeneration(BottomRFPulse, TopRFPulse);

                indicator = 0;

                while (indicator == 0)
                {
                    Console.WriteLine("Device now ready to receive triggers.\nReselect pulses (type again) or stop generation (type stop)");
                    string command = Console.ReadLine();

                    switch(command)
                    {
                        case "again":
                            newSession.UpdateGeneration();
                            indicator = 1;
                            break;
                        case "stop":
                            newSession.StopGeneration();
                            j = 1;
                            indicator = 1;
                            break;
                        default:
                        Console.WriteLine("Command not recognised!");
                        break;
                    }
                }
            }




        }

        void MakePulses()
        {
            ShapedPulse pulse1, pulse2;
            double[] pulsedata1, pulsedata2;
            double[] blankData;

            string resourceName = "Dev2";
            int numberOfSamples, numberOfDeadPoints;
            int waveformQuantum;

            double frequency = 170000000;
            double power = 5;
            double iqRate = 100e6;
            double actualIQRate;
            double pulseLength = 1e-6;
            double bandwidth = 2 / pulseLength;
            double actualPulseLength;
            double deadTime = 3e-7;
            int waveformRepeatCount = 1;

            try
            {
                 // Initialise the NIRfsg session
                _rfsgSession = new NIRfsg(resourceName, true, false);

                // Subscribe to warnings
                _rfsgSession.DriverOperation.Warning += new EventHandler<RfsgWarningEventArgs>(DriverOperation_Warning);

                // Configure instrument
                _rfsgSession.RF.Configure(frequency, power);
                _rfsgSession.Arb.GenerationMode = RfsgWaveformGenerationMode.Script;
                _rfsgSession.Arb.IQRate = iqRate;
                _rfsgSession.Arb.PreFilterGain = ArbPreFilterGain;
                _rfsgSession.RF.PowerLevelType = RfsgRFPowerLevelType.PeakPower;

                // Enable finite generation
                _rfsgSession.Arb.IsWaveformRepeatCountFinite = true;
                _rfsgSession.Arb.WaveformRepeatCount = waveformRepeatCount;

                // Configure bandwidth
                _rfsgSession.Arb.SignalBandwidth = bandwidth;

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

                // Get number of dead points at the end
                numberOfDeadPoints = Convert.ToInt32(actualIQRate * deadTime);
                Console.WriteLine("No. of dead points: " + numberOfDeadPoints);

                // Generate waveforms
                pulse1 = new ShapedPulse(0, 0.9, 0.1, numberOfSamples, numberOfDeadPoints);
                pulse2 = new ShapedPulse(1, 0, 0, numberOfSamples, numberOfDeadPoints);
                pulsedata1 = pulse1.MakePulse();
                pulsedata2 = pulse2.MakePulse();
                blankData = Enumerable.Repeat(0.0, numberOfSamples).ToArray();

                // Write waveforms to device
                _rfsgSession.Arb.WriteWaveform("Pulse1", pulsedata1, blankData);
                _rfsgSession.Arb.WriteWaveform("Pulse2", pulsedata2, blankData);
                _rfsgSession.Arb.WriteWaveform("allZeros", blankData, blankData);

            }

            catch (Exception ex)
            {
                ShowError("MakePulses()", ex);
            }

        }

        void StartGeneration(string BottomRFPulse, string TopRFPulse)
        {

            string script = @"script myScript" + Environment.NewLine +
                "   Repeat forever" + Environment.NewLine +
                "      Generate allZeros" + Environment.NewLine +
                "      Clear scriptTrigger0" + Environment.NewLine +
                "      Wait until scriptTrigger0" + Environment.NewLine +
                "      Clear scriptTrigger1" + Environment.NewLine +
                "      Repeat until scriptTrigger1" + Environment.NewLine +
                "         Generate " + BottomRFPulse + Environment.NewLine +
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

        void UpdateGeneration()
        {
            try
            {
                // Abort current generation
                _rfsgSession.Abort();
            }
            catch (Exception ex)
            {
                ShowError("UpdateGeneration()", ex);
            }
        }

        void StopGeneration()
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
        
        void ShowError(string functionName, Exception exception)
        {
            StopGeneration();
            Console.WriteLine("Error in " + functionName + ": " + exception.Message);
        }

        void DriverOperation_Warning(object sender, RfsgWarningEventArgs e)
        {
            // Display the rfsg warning
            Console.WriteLine(e.Message);
        }

        static int CoerceToQuantum(double numberOfSamples, int waveformQuantum)
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
    }
}
