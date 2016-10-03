using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mcl_FreqCounter64;
using System.Timers;
namespace DAQ.HAL
{
    public class mcFreqCtr
    {
        private mcl_FreqCounter64.USB_FreqCounter Counter;

        private float gateTime;

        private Timer CounterTimer;

        private string SN = "";

        public bool run = false;

        private double freq = 0.0;

        private int status;

        public mcFreqCtr()
        {

        }

        public int connectFrqCtr()
        {

            Counter = new mcl_FreqCounter64.USB_FreqCounter();

            status = Counter.Connect(ref(SN));

            return status;

        }

        public bool isConnected()
        {
            return status == 1;
        }

        public void updateFrequency()
        {
            status = Counter.ReadFreq(ref(freq));
        }

        public double f
        {
            get
            {
                updateFrequency();
                return freq;
            }
            set
            {
                freq = value;
            }
        }

        public float GT
        {
            get
            {
                return gateTime;
            }
            set
            {
                gateTime = value;
                Counter.SetSampleTime(ref(gateTime));
            }
        }

        public int getStatus
        {
            get
            {
                return status;
            }
            set
            {

            }
        }

    }
}