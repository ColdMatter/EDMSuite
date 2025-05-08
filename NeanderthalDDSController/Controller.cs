using System;
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
            MessageBox.Show("Card found: " + sCardName + " sn " + lSerialNumber.ToString(), "Serial Number", MessageBoxButtons.OK, MessageBoxIcon.Information);
            initializeCard();
           
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

            Task.Run(() =>
            {

                while (isPatternRunning)
                {
                    // This is when the DDS stops all the signal output
                    openCard();
                    startSinglePattern();
                    // Wait till sequence ends
                    Thread.Sleep(patternLength);
                    closeCard();
                }
            });

        }

        public void stopPattern()
        {
            if (isPatternRunning == true)
            {
                isPatternRunning = false;  // Stop the loop
            }

            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_M2CMD, Regs.M2CMD_CARD_FORCETRIGGER);
        }

       

        public void startSinglePattern()
        {

            if (!isPatternRunning)
            {
                //closeCard();
                return;
            }

            //initializeCard();
            addPatternToBuffer(patternList);

            // Check if all commands are excecuted
            while (checkCommandNum(hDevice) != 0 && breakFlag == false)
            {
                // Loop until the condition is met.

            }

            if (breakFlag)
            {
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
                closeCard();
                stopPattern();
                return;
            }

            uint code;
            // reset first
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_RESET);

            // Enable channels
            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_CHENABLE, Regs.CHANNEL0 | Regs.CHANNEL1 | Regs.CHANNEL2 | Regs.CHANNEL3);
            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_ENABLEOUT0, 1);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_ENABLEOUT1, 1);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_ENABLEOUT2, 1);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_ENABLEOUT3, 1);
            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_AMP0, 1000);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_AMP1, 1000);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_AMP2, 1000);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_AMP3, 1000);
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_FILTER0, 0); // full bandwidth, no filter
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_FILTER1, 0); // full bandwidth, no filter
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_FILTER2, 0); // full bandwidth, no filter
            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_FILTER3, 0); // full bandwidth, no filter
            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_CARDMODE, Regs.SPC_REP_STD_DDS);
            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_CLOCKMODE, Regs.SPC_CM_INTPLL); // clock mode internal PLL
            //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_TRG_SRC, Regs.SPCM_DDS_TRG_SRC_TIMER);

            // setup the external trigger input
            //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_TRIG_ORMASK, Regs.SPC_TMASK_EXT0);
            //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_TRIG_EXT0_MODE, Regs.SPC_TM_POS);
            //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_TRIG_EXT0_LEVEL0, 1500);
            //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_TRIG_EXT0_ACDC, Regs.COUPLING_DC);


            // card start
            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_M2CMD, Regs.M2CMD_CARD_START | Regs.M2CMD_CARD_ENABLETRIGGER);
            //Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_M2CMD, Regs.M2CMD_CARD_START);

            // Core distribution
            code = Drv.spcm_dwSetParam_i64(hDevice, Regs.SPC_DDS_CORES_ON_CH0, Regs.SPCM_DDS_CORE0);
            code = Drv.spcm_dwSetParam_i64(hDevice, Regs.SPC_DDS_CORES_ON_CH1, Regs.SPCM_DDS_CORE47);
            code = Drv.spcm_dwSetParam_i64(hDevice, Regs.SPC_DDS_CORES_ON_CH2, Regs.SPCM_DDS_CORE48);
            code = Drv.spcm_dwSetParam_i64(hDevice, Regs.SPC_DDS_CORES_ON_CH3, Regs.SPCM_DDS_CORE49);

            

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
                parameterUpdated.Invoke();
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

            //fixed (ST_LIST_PARAM* pAslist = &aslist[0])
            //{
                // global time in s
                foreach (string key in sortedPatternList.Keys)
                {


                    if (i < sortedPatternList.Count - 1)
                    {
                        double length;
                        length = timeList[i + 1] - timeList[i];

                        Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_TRG_TIMER, length);
                        i++;
                        //aslist[j].lReg = Regs.SPC_DDS_TRG_TIMER;
                        //aslist[j].lType = 1;
                        //aslist[j].dValue = length;
                        //j++;
                    }

                // Ch 0 core 0
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_FREQ, 1e6 * sortedPatternList[key][1][0]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP, sortedPatternList[key][2][0]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_FREQ_SLOPE, 1e9 * sortedPatternList[key][3][0]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP_SLOPE, 1e3 * sortedPatternList[key][4][0]);

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_FREQ;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e6 * sortedPatternList[key][1][0];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_AMP;
                //aslist[j].lType = 1;
                //aslist[j].dValue = sortedPatternList[key][2][0];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_FREQ_SLOPE;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e9 * sortedPatternList[key][3][0];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_AMP_SLOPE;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e3 * sortedPatternList[key][4][0];
                //j++;

                // Ch 1 core 47
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_FREQ, 1e6 * sortedPatternList[key][1][1]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_AMP, sortedPatternList[key][2][1]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_FREQ_SLOPE, 1e9 * sortedPatternList[key][3][1]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_AMP_SLOPE, 1e3 * sortedPatternList[key][4][1]);

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_FREQ;
                //    aslist[j].lType = 1;
                //    aslist[j].dValue = 1e6 * sortedPatternList[key][1][1];
                //    j++;

                //    aslist[j].lReg = Regs.SPC_DDS_CORE0_AMP;
                //    aslist[j].lType = 1;
                //    aslist[j].dValue = sortedPatternList[key][2][1];
                //    j++;

                //    aslist[j].lReg = Regs.SPC_DDS_CORE0_FREQ_SLOPE;
                //    aslist[j].lType = 1;
                //    aslist[j].dValue = 1e9 * sortedPatternList[key][3][1];
                //    j++;

                //    aslist[j].lReg = Regs.SPC_DDS_CORE0_AMP_SLOPE;
                //    aslist[j].lType = 1;
                //    aslist[j].dValue = 1e3 * sortedPatternList[key][4][1];
                //    j++;

                // Ch 2 core 48
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_FREQ, 1e6 * sortedPatternList[key][1][2]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_AMP, sortedPatternList[key][2][2]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_FREQ_SLOPE, 1e9 * sortedPatternList[key][3][2]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_AMP_SLOPE, 1e3 * sortedPatternList[key][4][2]);

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_FREQ;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e6 * sortedPatternList[key][1][2];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_AMP;
                //aslist[j].lType = 1;
                //aslist[j].dValue = sortedPatternList[key][2][2];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_FREQ_SLOPE;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e9 * sortedPatternList[key][3][2];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_AMP_SLOPE;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e3 * sortedPatternList[key][4][2];
                //j++;

                // Ch 3 core 49
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_FREQ, 1e6 * sortedPatternList[key][1][3]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_AMP, sortedPatternList[key][2][3]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_FREQ_SLOPE, 1e9 * sortedPatternList[key][3][3]);
                Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_AMP_SLOPE, 1e3 * sortedPatternList[key][4][3]);

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_FREQ;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e6 * sortedPatternList[key][1][3];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_AMP;
                //aslist[j].lType = 1;
                //aslist[j].dValue = sortedPatternList[key][2][3];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_FREQ_SLOPE;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e9 * sortedPatternList[key][3][3];
                //j++;

                //aslist[j].lReg = Regs.SPC_DDS_CORE0_AMP_SLOPE;
                //aslist[j].lType = 1;
                //aslist[j].dValue = 1e3 * sortedPatternList[key][4][3];
                //j++;

                Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_EXEC_AT_TRG);

                    //aslist[j].lReg = Regs.SPC_DDS_CMD;
                    //aslist[j].lType = 0;
                    //aslist[j].dValue = Regs.SPCM_DDS_CMD_EXEC_AT_TRG;
                    //j++;


                }
                //Write command in one libraray call
                //    int size = Marshal.SizeOf<ST_LIST_PARAM>();
                //int totalSize = parLength * size;
                //Drv.spcm_dwSetParam_ptr(hDevice, Regs.SPC_REGISTER_LIST, (IntPtr)pAslist, (ulong)totalSize);
                //}


            Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_WRITE_TO_CARD);



        }

        // Add single non time dependent frequency and amplitude command to DDS
        public void addPatternToBufferSingle(List<double> pattern)
        {
            uint code;
            //code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_TRG_TIMER, 0.0);
            code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_FREQ, 1e6 * pattern[0]);
            code = Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP, pattern[1]);

            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_FREQ, 1e6 * pattern[2]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE47_AMP, pattern[3]);

            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_FREQ, 1e6 * pattern[4]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE48_AMP, pattern[5]);

            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_FREQ, 1e6 * pattern[6]);
            Drv.spcm_dwSetParam_d64(hDevice, Regs.SPC_DDS_CORE49_AMP, pattern[7]);

            // read back the exact signal frequency
            Drv.spcm_dwGetParam_d64(hDevice, Regs.SPC_DDS_CORE0_FREQ, out double dFreq_Hz);
            Console.WriteLine(dFreq_Hz);

            Drv.spcm_dwGetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP, out double amp);
            Console.WriteLine(amp);

            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_EXEC_AT_TRG);
            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_DDS_CMD, Regs.SPCM_DDS_CMD_WRITE_TO_CARD);
            code = Drv.spcm_dwSetParam_i32(hDevice, Regs.SPC_M2CMD, Regs.M2CMD_CARD_FORCETRIGGER);

            code = Drv.spcm_dwGetParam_d64(hDevice, Regs.SPC_DDS_CORE0_AMP, out amp);
            Console.WriteLine(amp);
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

        public static void indexTable(string key, out int row, out int col)
        {
            row = -1; col = -1;
            switch (key)
            {
                case "time":
                    row = 0; col = 0; break;

                case "Ch0Freq":
                    row = 1; col = 0; break;

                case "Ch1Freq":
                    row = 1; col = 1; break;

                case "Ch2Freq":
                    row = 1; col = 2; break;

                case "Ch3Freq":
                    row = 1; col = 3; break;

                case "Ch0Amp":
                    row = 2; col = 0; break;

                case "Ch1Amp":
                    row = 2; col = 1; break;

                case "Ch2Amp":
                    row = 2; col = 2; break;

                case "Ch3Amp":
                    row = 2; col = 3; break;

                case "Ch0FreqSlope":
                    row = 3; col = 0; break;

                case "Ch1FreqSlope":
                    row = 3; col = 1; break;

                case "Ch2FreqSlope":
                    row = 3; col = 2; break;

                case "Ch3FreqSlope":
                    row = 3; col = 3; break;

                case "Ch0AmpSlope":
                    row = 4; col = 0; break;

                case "Ch1AmpSlope":
                    row = 4; col = 1; break;

                case "Ch2AmpSlope":
                    row = 4; col = 2; break;

                case "Ch3AmpSlope":
                    row = 4; col = 3; break;

                default:
                    throw new KeyNotFoundException($"Key '{key}' not found in the index table.");
            }
        }

        public void UpdatePatternValue(Dictionary<string, List<List<double>>> patternList, string key, int row, int col, double newValue)
        {
            try
            {
                if (!patternList.ContainsKey(key))
                    throw new KeyNotFoundException($"Key '{key}' not found in dictionary.");

                if (row < 0 || row >= patternList[key].Count || col < 0 || col >= patternList[key][row].Count)
                    throw new ArgumentOutOfRangeException("Row or column index is out of range.");

                patternList[key][row][col] = newValue;
                //MessageBox.Show($"Updated [{key}][{row}][{col}] to {newValue}", "Update Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (KeyNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error: Key Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show(ex.Message, "Error: Index Out of Range", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void varySingleParameter(string parName, string key, double val)
        {
            int row; int col;
            indexTable(key, out row, out col); 
            UpdatePatternValue(patternList, key, row, col, val);
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
