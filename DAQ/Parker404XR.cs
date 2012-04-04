using System;
using NationalInstruments.VisaNS;
using DAQ.Environment;

namespace DAQ.HAL
{
    public class Parker404XR : RS232Instrument
    {
        string initFile;
        bool autoTrigger;
        public Parker404XR(String visaAddress, String initFile): base(visaAddress)
        {
            this.initFile = initFile; 
        }
        public void Connect()
        {
            base.Connect(SerialTerminationMethod.TerminationCharacter);
            autoTrigger = true;
            base.Write("1E0\r\n"); 
        }
        public void Initialize(double acceleration, double deceleration, double distance, double velocity)
        {
            
            //I just hacked the example program built using EASY-V, and examples . I don't know what most of these do. A command reference can be found in the manual for the VIX500.
            base.Write("1E1\r\n");
            base.Write("1K\r\n");
            base.Write("1CLEAR(ALL)\r\n");

            base.Write("1START:\r\n");
            base.Write("1DECLARE(MOVE)\r\n");
            base.Write("1DECLARE(MOVE2)\r\n");
            base.Write("1DECLARE(INIT)\r\n");
            base.Write("1DECLARE(ALPHA)\r\n");
            base.Write("1DECLARE(BETA)\r\n");
            base.Write("1DECLARE(GAMMA)\r\n");
            base.Write("1END\r\n");

            //The first move
            //20,20,52580,40
            //Note conversion factors for accleration/ distance etc. using 1 step = 5*10^(-3)mm and 1 revolution = 4000 steps
            base.Write("1ALPHA:\r\n");
            base.Write("1O(000)\r\n");
            base.Write("1PROFILE1(" + (acceleration / 20).ToString() + "," + (deceleration / 20).ToString() + "," + (distance / (5 * Math.Pow(10, -3))).ToString() + "," + (velocity / 20).ToString() + ")\r\n");
            base.Write("1USE(1)\r\n");
            base.Write("1GOTO(MOVE)\r\n");
            base.Write("1END\r\n");

            //The return
            base.Write("1BETA:\r\n");
            base.Write("1O(000)\r\n");
            base.Write("1PROFILE1(" + (acceleration / 20).ToString() + "," + (deceleration / 20).ToString() + ",-" + (distance / (5 * Math.Pow(10, -3))).ToString() + "," + (velocity / 20).ToString() + ")\r\n");
            base.Write("1USE(1)\r\n");
            base.Write("1GOTO(MOVE)\r\n");
            base.Write("1END\r\n");

            //Homing routine
            base.Write("1GAMMA:\r\n");
            base.Write("1O(000)\r\n");
            base.Write("1HOME1(-,0,15,100,1)\r\n");
            base.Write("1GOTO(MOVE2)\r\n");
            base.Write("1END\r\n");

            //What to do in case of an error
            base.Write("1FAULT:\r\n");
            base.Write("1\"FAULT\"\r\n");
            base.Write("1GH\r\n");
            base.Write("1END\r\n");

            //Other stuff I don't really understand
            base.Write("1INIT:\r\n");
            base.Write("1OFF\r\n");
            base.Write("1W(AO,0)\r\n");
            base.Write("1W(AB,0)\r\n");
            base.Write("1W(AM,0)\r\n");
            base.Write("1W(EX,3)\r\n");
            base.Write("1W(EQ,0)\r\n");
            base.Write("1W(BR,9600)\r\n");
            base.Write("1W(CL,100)\r\n");
            base.Write("1W(CQ,1)\r\n");
            base.Write("1W(IC,7904)\r\n");// Sets user inputs (including limits and home). 7904 means triggers on high for all channels & triggers on 24V (except input 1 which triggers on 5V. Use this for sending trigger TTLs!)
            base.Write("1W(EI,2)\r\n");
            base.Write("1W(EO,2)\r\n");
            base.Write("1LIMITS(0,1,0,200.0)\r\n");
            base.Write("1W(EW,50)\r\n");
            base.Write("1W(IT,10)\r\n");
            base.Write("1GAINS(5.00,0.00,10.00,5.00,0)\r\n");
            base.Write("1W(IM,1)\r\n");
            base.Write("1W(IW,25)\r\n");
            base.Write("1W(ES,1)\r\n");
            base.Write("1W(TT,65)\r\n");
            base.Write("1MOTOR(39169,6.7,4096,4100,1680,1.30,3.60,0.191)\r\n");
            base.Write("1W(PC,300)\r\n");
            base.Write("1W(TL,4096)\r\n");
            base.Write("1END\r\n");

            base.Write("1MOVE:\r\n");
            base.Write("1O(1)\r\n");
            base.Write("1G\r\n");
            base.Write("1O(0)\r\n");
            base.Write("1T1\r\n");
            base.Write("1END\r\n");

            base.Write("1MOVE2:\r\n");
            base.Write("1O(0)\r\n");
            base.Write("1GH\r\n");
            base.Write("1END\r\n");

            base.Write("1ARM01\r\n");
            base.Write("1GOTO(INIT)\r\n");
            //base.Write("1SV\r\n"); This line isn't really needed for immediate control, it saves your TS script into memory.
        }

        public void Restart()
        {
            base.Write("1Z\r\n");
        }
        public void On()
        {
            base.Write("1ON\r\n");
        }

        public void Move()
        {
            if (autoTrigger)// I had call this before each move operation, as TR (wait for trigger) config gets wiped after each move.
            {
                base.Write("1TR(IN,=,XXXXX)\r\n");
            }
            else
            {
                base.Write("1TR(IN,=,1XXXX)\r\n");
            }
            base.Write("1GOTO(ALPHA)\r\n");
            base.Write("1R(ST)\r\n");
        }
        public void Return()
        {
            if (autoTrigger)
            {
                base.Write("1TR(IN,=,XXXXX)\r\n");
            }
            else
            {
                base.Write("1TR(IN,=,1XXXX)\r\n");
            }
            base.Write("1GOTO(BETA)\r\n");
            base.Write("1R(ST)\r\n");
        }
        public void DisarmMove()
        {
            base.Write("1OFF\r\n");
        }
        public string Read()
        {
            return base.Read();
        }
        public void Clear()
        {
            base.Clear();
            base.Write("1CLEAR(ALL)");
        }
        public void AutoTriggerEnable()
        {
            autoTrigger = true;     
        }
        public void AutoTriggerDisable()
        {
            autoTrigger = false;
        }
        public void Home()
        {
            if (autoTrigger)
            {
                base.Write("1TR(IN,=,XXXXX)\r\n");
            }
            else
            {
                base.Write("1TR(IN,=,1XXXX)\r\n");
            }
            base.Write("1GOTO(GAMMA)\r\n");
            base.Write("1R(ST)\r\n");
        }
        public void CheckStatus()
        {
            base.Write("1R(ST)\r\n");
        }

        public void ListAll()
        {
            base.Write("1LIST(ALL)\r\n");
        }

        public void CommsDisable()
        {
            base.Write("1E0\r\n");
        }
    }
}
