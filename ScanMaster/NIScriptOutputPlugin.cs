using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using ScanMaster.Acquire.Plugins;

using DAQ.Environment;
using DAQ.HAL;

using RfArbitraryWaveformGenerator;

namespace ScanMaster.Acquire.Plugin
{
    [Serializable]
    public class NIScriptOutputPlugin : ScanOutputPlugin
    {
        [NonSerialized]
        private double currentScanParameter;

        [NonSerialized]
        private RfArbitraryWaveformGenerator.Controller rfAWGController;
        private double[] scanPointArray;
        private string scannedParameter;

        [NonSerialized]
        private Task dot;
        [NonSerialized]
        private DigitalSingleChannelWriter writer;

        protected override void InitialiseSettings()
        {
            settings["dummy"] = "chris";
            settings["scannedParameter"] = "amplitude";
            settings["rf1Length"] = 10.0;
            settings["rf2Length"] = 10.0;
            settings["rf1StartTime"] = 800.0;
            settings["rf2StartTime"] = 1725.0;
            settings["rfCentreFrequency"] = 170.0;
            settings["rfPeakPower"] = 15.0;
            settings["sampleRate"] = 100000000.0;
        }

        public override void AcquisitionStarting()
        {
            // Create digital task for the trigger
            dot = new Task("ttlSwitchTask");
            writer = new DigitalSingleChannelWriter(dot.Stream);
            ((DigitalOutputChannel)Environs.Hardware.DigitalOutputChannels["rfAWGTrigger"]).AddToTask(dot);
            dot.Control(TaskAction.Verify);

            if (settings["scannedParameter"] == null)
            {
                scannedParameter = "amplitude";
            }
            else
            {
                scannedParameter = (String)settings["scannedParameter"];
            }

            // This can certainly be done better.. connect to the rf AWG via remoting
            rfAWGController = (RfArbitraryWaveformGenerator.Controller)(Activator.GetObject(typeof(RfArbitraryWaveformGenerator.Controller), "tcp://localhost:1191/controller.rem"));

            // Generate scan points based on scan range and number of points
            scanPointArray = generateScanPoints((double)settings["start"], (double)settings["end"], (int)settings["pointsPerScan"]);

            // Generate all pulses for each scan point and load into rf AWG
            // Here we also convert the user-input amplitude (in dBm) to a normalised amplitude where 1.0 corresponds to
            // the peak power output from the AWG as specified in settings["rfPeakPower"]
            int i = 0;
            RfArbitraryWaveformGenerator.RfPulse rf1Pulse = new RfArbitraryWaveformGenerator.RfPulse();
            RfArbitraryWaveformGenerator.RfPulse rf2Pulse = new RfArbitraryWaveformGenerator.RfPulse();
            rfAWGController.ClearAllPulses();

            switch (scannedParameter)
            {
                case "amplitude":
                    double normalisedAmplitude, normalisation;
                    foreach (double point in scanPointArray)
                    {
                        normalisation = Math.Pow(10, -0.75);
                        normalisedAmplitude = normalisation * Math.Pow(10, point);
                        rf1Pulse.Name = "pulse1" + i.ToString();
                        RfArbitraryWaveformGenerator.PulseMaker.MakeTopHat(rf1Pulse.Name, (double)settings["rf1Length"], (double)settings["sampleRate"], normalisedAmplitude, 0.0, 0.0);
                        rfAWGController.AddPulse(rf1Pulse);
                        rf2Pulse.Name = "pulse2" + i.ToString();
                        RfArbitraryWaveformGenerator.PulseMaker.MakeTopHat(rf2Pulse.Name, (double)settings["rf2Length"], (double)settings["sampleRate"], normalisedAmplitude, 0.0, 0.0);
                        rfAWGController.AddPulse(rf2Pulse);
                        i++;
                    }
                    break;
                case "frequency":
                    double frequencyStep;
                    foreach (double point in scanPointArray)
                    {
                        frequencyStep = point - (double)settings["rfCenterFrequency"];
                        rf1Pulse.Name = "pulse1" + i.ToString();
                        RfArbitraryWaveformGenerator.PulseMaker.MakeTopHat(rf1Pulse.Name, (double)settings["rf1Length"], (double)settings["sampleRate"], 1.0, frequencyStep, 0.0);
                        rfAWGController.AddPulse(rf1Pulse);
                        rf2Pulse.Name = "pulse2" + i.ToString();
                        RfArbitraryWaveformGenerator.PulseMaker.MakeTopHat(rf2Pulse.Name, (double)settings["rf2Length"], (double)settings["sampleRate"], 1.0, frequencyStep, 0.0);
                        rfAWGController.AddPulse(rf2Pulse);
                        i++;
                    }
                    break;
            }

            // Generate the zero pulses
            RfArbitraryWaveformGenerator.RfPulse waitZeros = new RfArbitraryWaveformGenerator.RfPulse("waitZeros");
            waitZeros = RfArbitraryWaveformGenerator.PulseMaker.MakeZeros(waitZeros.Name, 0.12, (double)settings["sampleRate"]);
            rfAWGController.AddPulse(waitZeros);

            double interferometerLength = Convert.ToDouble(settings["rf2StartTime"]) - Convert.ToDouble(settings["rf1StartTime"]) - (double)settings["rf1Length"];
            RfArbitraryWaveformGenerator.RfPulse timeBetweenPulsesZeros = new RfArbitraryWaveformGenerator.RfPulse("timeBetweenPulsesZeros");
            timeBetweenPulsesZeros = RfArbitraryWaveformGenerator.PulseMaker.MakeZeros(timeBetweenPulsesZeros.Name, interferometerLength, (double)settings["sampleRate"]);
            rfAWGController.AddPulse(timeBetweenPulsesZeros);

            // Generate sequence for pulses
            int[] rf1PulseSequence, rf2PulseSequence, pulseSequence;
            rf1PulseSequence = new int[scanPointArray.Length];
            rf2PulseSequence = new int[scanPointArray.Length];
            switch ((string)settings["scanMode"])
            {
                case "up":
                    pulseSequence = Enumerable.Range(0, scanPointArray.Length).ToArray();
                    break;
                case "down":
                    pulseSequence = Enumerable.Range(0, scanPointArray.Length).Reverse().ToArray();
                    break;
                default:
                    pulseSequence = Enumerable.Range(0, scanPointArray.Length).ToArray();
                    break;
            }
            for (i = 0; i < scanPointArray.Length; i++)
            {
                rf1PulseSequence[i] = Convert.ToInt32("1" + pulseSequence[i].ToString());
                rf2PulseSequence[i] = Convert.ToInt32("2" + pulseSequence[i].ToString());
            }

            // Generate + upload script
            rfAWGController.Script = RfArbitraryWaveformGenerator.ScriptWriter.WriteScript(rfAWGController.ChosenRfPulseList, rf1PulseSequence, rf2PulseSequence);

            // StartGeneration()
            rfAWGController.StartGeneration();
        }

        public override void ScanStarting()
        {

        }

        public override void ScanFinished()
        {

        }

        public override void AcquisitionFinished()
        {
            rfAWGController.StopGeneration();
            dot.Dispose();
        }

        [XmlIgnore]
        public override double ScanParameter
        {
            get
            {
                return currentScanParameter;
            }
            set
            {
                this.currentScanParameter = value;

                // Send a trigger to the rf AWG when we move to the next point in a scan
                writer.WriteSingleSampleSingleLine(true, true);
                writer.WriteSingleSampleSingleLine(true, false);
            }
        }

        private static double[] generateScanPoints(double scanStart, double scanEnd, int scanPoints)
        {
            double[] scanPointArray = new double[scanPoints];
            double increment = (scanEnd - scanStart) / scanPoints;

            for (int i = 0; i < scanPoints; i++)
            {
                scanPointArray[i] = scanStart + increment * (i + 0.5);
            }

            return scanPointArray;
        }

        private static void generateScanSequence(ref List<KeyValuePair<int, string>> pulseList, string scanMode)
        {
            switch (scanMode)
            {
                case "up":
                    pulseList.Sort((a, b) => a.Key.CompareTo(b.Key));
                    break;
                case "down":
                    pulseList.Sort((a, b) => b.Key.CompareTo(a.Key));
                    break;
                default:
                    pulseList.Sort((a, b) => a.Key.CompareTo(b.Key));
                    break;
            }

            return;
        }

    }
}
