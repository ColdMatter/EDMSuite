using System;
using System.Collections;
using System.Xml.Serialization;

using NationalInstruments.DAQmx;

using DAQ.Environment;
using DAQ.HAL;

using ScanMaster.Acquire.Plugin;

namespace ScanMaster.Acquire.Plugins
{
    /// <summary>
    /// A plugin to capture GPIB data using an E-series board.
    /// </summary>
    [Serializable]
    public class SingleCounterInputPlugin : GPIBInputPlugin
    {
        [NonSerialized]
        private Agilent53131A counter;

        [NonSerialized]
        private double latestData;
        
        protected override void InitialiseSettings()
        {
            
        }

        public override void AcquisitionStarting()
        {
            counter = (Agilent53131A)Environs.Hardware.Instruments["rfCounter"];
            counter.Connect();
            counter.Channel = 3;
            counter.WriteToCounter(":FUNC 'FREQ " + counter.Channel + "'");
            counter.WriteToCounter(":FREQ:ARM:STAR:SOUR IMM");
            counter.WriteToCounter(":FREQ:ARM:STOP:SOUR TIM");
            counter.WriteToCounter(":FREQ:ARM:STOP:TIM 0.001");
            counter.WriteToCounter(":ROSC:SOUR INT");
            counter.WriteToCounter(":DIAG:CAL:INT:AUTO OFF");
            counter.WriteToCounter(":DISP:ENAB OFF");
            counter.WriteToCounter(":CAL:MATH:STATE OFF");
            counter.WriteToCounter(":CAL2:LIM:STATE OFF");
            counter.WriteToCounter(":CAL3:AVER:STATE OFF");
            counter.WriteToCounter(":HCOPY:CONT OFF");
            counter.WriteToCounter("*DDT #15FETC?");
        }

        public override void ScanStarting()
        {
        }

        public override void ScanFinished()
        {
        }

        public override void AcquisitionFinished()
        {
            counter.Disconnect();
        }

        public override void ArmAndWait()
        {
            lock (this)
            {
                if (!Environs.Debug)
                {
                    
                    latestData=Frequency;
                    
                }
            }
        }

        public double Frequency
        {
            get
            {
                if (!Environs.Debug)
                {
                    counter.WriteToCounter("READ:FREQ?");
                    string fr = counter.ReadFromCounter();
                    return Double.Parse(fr);
                }
                else
                {
                    return 170751000.0 + 2000 * (new Random()).NextDouble();
                }
            }
        }


        [XmlIgnore]
        public override double GPIBval
        {
            get
            {
                return latestData;
            }
        }
    }
}
