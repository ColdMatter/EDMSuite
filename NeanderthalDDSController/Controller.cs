﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Spcm;
using System.Threading;

namespace NeanderthalDDSController
{
    public delegate void updateHandler();
    public class Controller : MarshalByRefObject
    {
        IntPtr hDevice, pBuffer;
        GCHandle hBufferHandle;
        int lErrorVal, lCardType, lSerialNumber, lMaxChannels, lBytesPerSample, lValue;
        uint dwErrorReg, dwErrorCode;
        long i, llMemSet, llAverage, llInstMem, llMaxSamplerate;
        short nMin, nMax;
        short[] nData;
        sbyte[] byData;
        public bool isPatternRunning = false;
        public int patternLength = 300;
        StringBuilder sErrorText = new StringBuilder(1024);
        public bool breakFlag = false;

        private NeanderthalForm ui;
        public Dictionary<string, List<List<double>>> patternList = new Dictionary<string, List<List<double>>> ();
        public Dictionary<string, List<List<double>>> sortedPatternList = new Dictionary<string, List<List<double>>>();
        public event updateHandler parameterUpdated;

        [StructLayout(LayoutKind.Explicit)]
        public struct ST_LIST_PARAM
        {
            [FieldOffset(0)]
            public int lReg;

            [FieldOffset(4)]
            public int lType;

            // The union starts immediately after lType (at offset 8)
            [FieldOffset(8)]
            public double dValue;

            [FieldOffset(8)]
            public long llValue;
        }

        public override object InitializeLifetimeService()
        {
            return null; // Returning null means infinite lifetime.
        }

        public void readCard()
        {
            hDevice = Drv.spcm_hOpen("/dev/spcm0");
            if (hDevice == IntPtr.Zero)
            {
                MessageBox.Show("Error: Could not open card\n");
                Console.WriteLine("Error: Could not open card\n");
                Environment.Exit(1);
            }

            byte[] byValueBuffer = new byte[20];
            IntPtr pValueBuffer = GCHandle.Alloc(byValueBuffer, GCHandleType.Pinned).AddrOfPinnedObject();

            dwErrorCode = Drv.spcm_dwGetParam_ptr(hDevice, Regs.SPC_PCITYP, pValueBuffer, 20);

            string sCardName = System.Text.Encoding.UTF8.GetString(byValueBuffer).Trim('\0');

            // ----- get card type -----
            dwErrorCode = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_PCITYP, out lCardType);
            dwErrorCode = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_PCISERIALNR, out lSerialNumber);

            Console.WriteLine(sCardName + " sn {0}\n", lSerialNumber);
            //MessageBox.Show("Card found: " + sCardName + " sn " + lSerialNumber.ToString(), "Serial Number", MessageBoxButtons.OK, MessageBoxIcon.Information);
            initializeCard();
            //closeCard();
        }

        public void start()
        {
            //ApplicationConfiguration.Initialize();
            ui = new NeanderthalForm(this);
            Application.Run(ui);
        }



        public Controller() {
        }


        public void startRepetitivePattern()
        {
            isPatternRunning = true;
            int iValue;
            long lValue;
            uint code;

            Task.Run(() =>
            {

                while (isPatternRunning)
                //for (int i = 0; i < 10; i++)
                {
                    
                    // This is when the DDS stops all the signal output
                    //openCard();
                    //code = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_M2STATUS, out iValue);
                    //Console.WriteLine(iValue);
                    //code = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_DDS_STATUS, out iValue);
                    //Console.WriteLine(iValue);
                    startSinglePattern();

                    // Wait till sequence ends
                    Thread.Sleep(patternLength);
                    //closeCard();
                    //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_RESET);
                    //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_TRG_SRC, Regs.SPCM_DDS_TRG_SRC_NONE);
                    //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_TRG_SRC, Regs.SPCM_DDS_TRG_SRC_TIMER);
                    //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_EXEC_AT_TRG);
                    //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_EXEC_NOW);
                }
                stopPattern();
                
            });

        }

        public void stopPattern()
        {
            if (isPatternRunning == true)
            {
                isPatternRunning = false;  // Stop the loop
                
                //closeCard();
            }
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_M2CMD, Regs.M2CMD_CARD_FORCETRIGGER);

        }

       

        public void startSinglePattern()
        {

            int iValue;
            long lValue;
            uint code;

            //initializeCard();
            addPatternToBuffer(patternList);

            // Check if all commands are excecuted
            while (checkCommandNum(hDevice) != 0 && !breakFlag)
            {
                // Loop until the condition is met.

            }

            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_TRG_SRC, Regs.SPCM_DDS_TRG_SRC_TIMER);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_TRG_TIMER, 1e-6);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_EXEC_NOW);

            if (breakFlag)
            {
                isPatternRunning = false;
                stopPattern();
            }

            
        }

        public void startPatternExternal()
        {
            setBreakFlag(false);
            startRepetitivePattern();

        }

        public void setBreakFlag(bool val)
        {
            breakFlag = val;
        }

        public void initializeCard()
        {

            
            if (hDevice == IntPtr.Zero)
            {
                MessageBox.Show("Error: Could not open card\n", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                stopPattern();
                return;
            }

            uint code;
            int iValue;

            //code = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_M2STATUS, out iValue);
            //Console.WriteLine(iValue);
            //code = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_DDS_STATUS, out iValue);
            //Console.WriteLine(iValue);

            // reset first
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_RESET);

            // Enable channels
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_CHENABLE, Regs.CHANNEL0 | Regs.CHANNEL1 | Regs.CHANNEL2 | Regs.CHANNEL3);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_ENABLEOUT0, 1);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_ENABLEOUT1, 1);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_ENABLEOUT2, 1);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_ENABLEOUT3, 1);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_AMP0, 1000);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_AMP1, 1000);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_AMP2, 1000);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_AMP3, 1000);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_FILTER0, 0); // full bandwidth, no filter
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_FILTER1, 0); // full bandwidth, no filter
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_FILTER2, 0); // full bandwidth, no filter
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_FILTER3, 0); // full bandwidth, no filter
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_CLOCKMODE, Regs.SPC_CM_INTPLL); // clock mode internal PLL
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_TRG_SRC, Regs.SPCM_DDS_TRG_SRC_TIMER);

            // setup the external trigger input
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_TRIG_ORMASK, Regs.SPC_TMASK_EXT0);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_TRIG_EXT0_MODE, Regs.SPC_TM_POS);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_TRIG_EXT0_LEVEL0, 1500);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_TRIG_EXT0_ACDC, Regs.COUPLING_DC);


            // card start
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_M2CMD, Regs.M2CMD_CARD_START | Regs.M2CMD_CARD_ENABLETRIGGER);


            // Core distribution
            Drv.spcm_dwSetParam_i64(hDevice, Regs.SPC_DDS_CORES_ON_CH0, Regs.SPCM_DDS_CORE0);
            Drv.spcm_dwSetParam_i64(hDevice, Regs.SPC_DDS_CORES_ON_CH1, Regs.SPCM_DDS_CORE47);
            Drv.spcm_dwSetParam_i64(hDevice, Regs.SPC_DDS_CORES_ON_CH2, Regs.SPCM_DDS_CORE48);
            Drv.spcm_dwSetParam_i64(hDevice, Regs.SPC_DDS_CORES_ON_CH3, Regs.SPCM_DDS_CORE49);

            //code = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_M2STATUS, out iValue);
            //Console.WriteLine(iValue);
            //code = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_DDS_STATUS, out iValue);
            //Console.WriteLine(iValue);

        }


        // Add parameters to pattern list


        public void addParToPatternList(string name, List<double> timeDelay, List<double> freq, List<double> amp, List<double> freq_slpoe, List<double> amp_slpoe)
        {
            try
            {
                if (patternList.ContainsKey(name))
                {
                    throw new ArgumentException($"The key '{name}' already exists in the pattern list.");
                }

                patternList.Add(name, new List<List<double>> { timeDelay, freq, amp, freq_slpoe, amp_slpoe });
                //parameterUpdated.Invoke();
                // Ensure parameterUpdated.Invoke() runs on the UI thread
                if (parameterUpdated != null)
                {
                    if (parameterUpdated.Target is Control control && control.InvokeRequired)
                    {
                        control.Invoke(new MethodInvoker(() => parameterUpdated.Invoke()));
                    }
                    else
                    {
                        parameterUpdated.Invoke();
                    }
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Duplicate Key Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Remove parameters from pattern list
        public void removeParFromPatternList(string name)
        {
            try
            {
                // Check if the key exists before attempting to remove it
                if (!patternList.ContainsKey(name))
                {
                    throw new KeyNotFoundException($"The key '{name}' does not exist in the pattern list.");
                }

                // Remove the item from the dictionary
                patternList.Remove(name);
                parameterUpdated.Invoke();
                //MessageBox.Show($"'{name}' has been removed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (KeyNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Key Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void clearPatternList()
        {
            patternList.Clear();
            //parameterUpdated.Invoke();
            InvokeParameterUpdatedSafely();
        }




        public unsafe void addPatternToBuffer(Dictionary<string, List<List<double>>> pattern)
        {
            // in second
            List<double> timeList = new List<double>();

            // sort pattern list by first value
            sortedPatternList = patternList
                .OrderBy(kvp => kvp.Value[0][0]) // Sort by first element in the first list
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Convert back to Dictionary

            int parLength = sortedPatternList.Count * 18;
            ST_LIST_PARAM[] aslist = new ST_LIST_PARAM[parLength];
            int j = 0;

            // Get a time list of the event
            foreach (string key in sortedPatternList.Keys){
                timeList.Add(sortedPatternList[key][0][0] / 1000.0);
                    }

            int i = 0;

            uint code;
            
            // global time in s
            foreach (string key in sortedPatternList.Keys)
            {


                if (i < sortedPatternList.Count - 1)
                {
                    double length;
                    length = timeList[i + 1] - timeList[i];

                    code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_TRG_TIMER, length);
                    i++;
                    
                }

                // Ch 0 core 0
                code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_FREQ, 1e6 * sortedPatternList[key][1][0]);
                code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP, sortedPatternList[key][2][0]);
                code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_FREQ_SLOPE, 1e9 * sortedPatternList[key][3][0]);
                code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP_SLOPE, 1e3 * sortedPatternList[key][4][0]);


                // Ch 1 core 47
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_FREQ, 1e6 * sortedPatternList[key][1][1]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_AMP, sortedPatternList[key][2][1]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_FREQ_SLOPE, 1e9 * sortedPatternList[key][3][1]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_AMP_SLOPE, 1e3 * sortedPatternList[key][4][1]);

           

                // Ch 2 core 48
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_FREQ, 1e6 * sortedPatternList[key][1][2]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_AMP, sortedPatternList[key][2][2]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_FREQ_SLOPE, 1e9 * sortedPatternList[key][3][2]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_AMP_SLOPE, 1e3 * sortedPatternList[key][4][2]);


                // Ch 3 core 49
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_FREQ, 1e6 * sortedPatternList[key][1][3]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_AMP, sortedPatternList[key][2][3]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_FREQ_SLOPE, 1e9 * sortedPatternList[key][3][3]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_AMP_SLOPE, 1e3 * sortedPatternList[key][4][3]);

                code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_EXEC_AT_TRG);

            }

            // Added per instruction of Spectrum
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_TRG_SRC, Regs.SPCM_DDS_TRG_SRC_NONE);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_EXEC_AT_TRG);


            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_WRITE_TO_CARD);



        }

        // Add single non time dependent frequency and amplitude command to DDS
        public void addPatternToBufferSingle(List<double> pattern)
        {
            uint code;
            //code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_TRG_TIMER, 0.0);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_FREQ, 1e6 * pattern[0]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP, pattern[1]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_FREQ_SLOPE, 1e9 * pattern[2]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP_SLOPE, 1e3 * pattern[3]);

            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_FREQ, 1e6 * pattern[4]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_AMP, pattern[5]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_FREQ_SLOPE, 1e9 * pattern[6]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_AMP_SLOPE, 1e3 * pattern[7]);

            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_FREQ, 1e6 * pattern[8]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_AMP, pattern[9]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_FREQ_SLOPE, 1e9 * pattern[10]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_AMP_SLOPE, 1e3 * pattern[11]);

            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_FREQ, 1e6 * pattern[12]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_AMP, pattern[13]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_FREQ_SLOPE, 1e9 * pattern[14]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_AMP_SLOPE, 1e3 * pattern[15]);


            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_EXEC_AT_TRG);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_WRITE_TO_CARD);
        }

        public int checkCommandNum(nint dev)
        {
            int comNum;
            dwErrorCode = Drv.spcm_dwGetParam_i32(hDevice, Regs.SPC_DDS_QUEUE_CMD_COUNT, out comNum);
            return comNum;
        }

        public void closeCard()
        {
            Drv.spcm_vClose(hDevice);
        }

        // This is when the DDS stops all the signal output
        public void openCard()
        {
            hDevice = Drv.spcm_hOpen("/dev/spcm0");
        }

        public void writePatternToCard()
        {
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_WRITE_TO_CARD);
        }

       

        private void InvokeParameterUpdatedSafely()
        {
            if (parameterUpdated == null)
                return;

            foreach (Delegate handler in parameterUpdated.GetInvocationList())
            {
                Control targetControl = handler.Target as Control;
                if (targetControl != null && targetControl.InvokeRequired)
                {
                    targetControl.Invoke(handler);
                }
                else
                {
                    handler.DynamicInvoke();
                }
            }
        }


    }

}
