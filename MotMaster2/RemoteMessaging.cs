using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace MOTMaster2
{
    public class memLog : List<string>
    {
        public bool Enabled = true;
        private int bufferLimit;
        public memLog(int depth = 32)
            : base()
        {
            bufferLimit = depth;
        }
        public void log(string txt)
        {
            if (!Enabled) return;
            Add(txt);
            while (Count > bufferLimit) RemoveAt(0);
        }
    }
    public class RemoteMessaging
    {
        public string partner { get; private set; }
        private IntPtr windowHandle;
        public string lastRcvMsg { get; private set; }
        public string lastSndMsg { get; private set; }
        public memLog Log;

        public DispatcherTimer dTimer, sTimer;
        private int _autoCheckPeriod = 10; // sec
        public int autoCheckPeriod
        {
            get { return _autoCheckPeriod; }
            set { _autoCheckPeriod = value; dTimer.Interval = new TimeSpan(0, 0, _autoCheckPeriod); }
        }

        public bool Enabled = true;
        public bool Connected { get; private set; }
        public bool partnerPresent { 
            get 
            { IntPtr hTargetWnd = NativeMethod.FindWindow(null, partner);
            return (hTargetWnd != IntPtr.Zero);
            } }

        public RemoteMessaging(string Partner)
        {
            partner = Partner;
            windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle; 
            HwndSource hwndSource = HwndSource.FromHwnd(windowHandle);
            hwndSource.AddHook(new HwndSourceHook(WndProc));

            Log = new memLog(); Log.Enabled = false; // for debug use 
            lastRcvMsg = ""; lastSndMsg = "";

            dTimer = new System.Windows.Threading.DispatcherTimer();
            dTimer.Tick += new EventHandler(dTimer_Tick);
            dTimer.Interval = new TimeSpan(0, 0, autoCheckPeriod);
            dTimer.Start();

            sTimer = new DispatcherTimer(DispatcherPriority.Send);
            sTimer.Tick += new EventHandler(sTimer_Tick);
            sTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }
        private void ResetTimer()
        {
            dTimer.Stop();
            if (Enabled) dTimer.Start();
        }

        private void dTimer_Tick(object sender, EventArgs e)
        {
            if (!Enabled) return;
            if (!CheckConnection()) Console.WriteLine("Warning: the partner <" + partner + "> is not responsive!");
            //else Console.WriteLine("Info: the partner <" + partner + "> is responsive.");

            // Forcing the CommandManager to raise the RequerySuggested event
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        public delegate bool RemoteHandler(string msg);
        public event RemoteHandler OnReceive;
        protected bool OnRemote(string msg)
        {
            if (OnReceive != null) return OnReceive(msg);
            else return false;
        }

        public delegate void ActiveCommHandler(bool active, bool forced);
        public event ActiveCommHandler ActiveComm;
        protected void OnActiveComm(bool active, bool forced)
        {
            Connected = active;
            if (ActiveComm != null) ActiveComm(active, forced);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) // receive
        {
            if ((msg == WM_COPYDATA) && Enabled)
            {
                COPYDATASTRUCT cds = (COPYDATASTRUCT)Marshal.PtrToStructure(lParam, (typeof(COPYDATASTRUCT)));
                if (cds.cbData == Marshal.SizeOf(typeof(MyStruct)))
                {
                    MyStruct myStruct = (MyStruct)Marshal.PtrToStructure(cds.lpData, typeof(MyStruct));
                    int msgID = myStruct.Number;
                    if (msgID == 666)
                    {
                        lastRcvMsg = myStruct.Message;
                        Log.log("R: " + lastRcvMsg);
                        ResetTimer();
                        switch (lastRcvMsg)
                        {
                            case ("ping"):
                                handled = sendCommand("pong");
                                if (lastConnection != handled) OnActiveComm(handled, false); // fire only if the state has been changed
                                lastConnection = handled; Connected = handled;
                                break;
                            case ("pong"):
                                handled = true;
                                break;
                            default: handled = OnRemote(lastRcvMsg); // the command systax is OK
                                break;
                        }
                    }
                    else handled = false;
                }
            }
            return hwnd;
        }

        private string json2send = "";
        private bool lastSentOK = true;
        private void sTimer_Tick(object sender, EventArgs e)
        {
            sTimer.Stop();
            if (json2send == "") throw new Exception("no message to be sent");
            lastSentOK = sendCommand(json2send);
            AsyncSent(lastSentOK, json2send);
        }

        public delegate void AsyncSentHandler(bool OK, string json2send);
        public event AsyncSentHandler OnAsyncSent;
        protected void AsyncSent(bool OK, string json2send)
        {
            if (OnAsyncSent != null) OnAsyncSent(OK, json2send);
        }

        public bool sendCommand(string msg, int delay = 0)
        {
            if (!Enabled) return false;
            if (delay > 0)
            {
                sTimer.Interval = new TimeSpan(0, 0, 0, 0, delay);
                json2send = msg;
                sTimer.Start();
                return true;
            }

            // Find the target window handle.
            IntPtr hTargetWnd = NativeMethod.FindWindow(null, partner);
            if (hTargetWnd == IntPtr.Zero)
            {
                Console.WriteLine("Unable to find the " + partner + " window");
                return false;
            }

            // Prepare the COPYDATASTRUCT struct with the data to be sent.
            MyStruct myStruct;

            myStruct.Number = 666;
            myStruct.Message = msg;

            // Marshal the managed struct to a native block of memory.
            int myStructSize = Marshal.SizeOf(myStruct);
            IntPtr pMyStruct = Marshal.AllocHGlobal(myStructSize);
            try
            {
                Marshal.StructureToPtr(myStruct, pMyStruct, true);

                COPYDATASTRUCT cds = new COPYDATASTRUCT();
                cds.cbData = myStructSize;
                cds.lpData = pMyStruct;

                // Send the COPYDATASTRUCT struct through the WM_COPYDATA message to 
                // the receiving window. (The application must use SendMessage, 
                // instead of PostMessage to send WM_COPYDATA because the receiving 
                // application must accept while it is guaranteed to be valid.)
                NativeMethod.SendMessage(hTargetWnd, WM_COPYDATA, windowHandle, ref cds);

                int result = Marshal.GetLastWin32Error();
                if (result != 0)
                {
                    Console.WriteLine(String.Format("SendMessage(WM_COPYDATA) failed w/err 0x{0:X}", result));
                    return false;
                }
                else
                {
                    lastSndMsg = msg; Log.log("S: " + lastSndMsg);
                    ResetTimer();
                }
                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(pMyStruct);
            }
        }

        private bool lastConnection = false;
        public bool CheckConnection(bool forced = false)
        {
            bool back = sendCommand("ping");
            if (back)
            {
                for (int i = 0; i < 200; i++)
                {
                    Thread.Sleep(10);
                    if (lastRcvMsg.Equals("pong")) break;
                }
            }
            back = back && (lastRcvMsg.Equals("pong"));
            if ((lastConnection != back) || forced) OnActiveComm(back, forced); // fire only if the state has been changed
            lastConnection = back; Connected = back;
            return back;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct MyStruct
        {
            public int Number;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1048576)]
            public string Message;
        }

        #region Native API Signatures and Types

        /// <summary>
        /// An application sends the WM_COPYDATA message to pass data to another 
        /// application.
        /// </summary>
        internal const int WM_COPYDATA = 0x004A;

        /// <summary>
        /// The COPYDATASTRUCT structure contains data to be passed to another 
        /// application by the WM_COPYDATA message. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct COPYDATASTRUCT
        {
            public IntPtr dwData;       // Specifies data to be passed
            public int cbData;          // Specifies the data size in bytes
            public IntPtr lpData;       // Pointer to data to be passed
        }

        internal class NativeMethod
        {
            /// <summary>
            /// Sends the specified message to a window or windows. The SendMessage 
            /// function calls the window procedure for the specified window and does 
            /// not return until the window procedure has processed the message. 
            /// </summary>
            /// <param name="hWnd">
            /// Handle to the window whose window procedure will receive the message.
            /// </param>
            /// <param name="Msg">Specifies the message to be sent.</param>
            /// <param name="wParam">
            /// Specifies additional message-specific information.
            /// </param>
            /// <param name="lParam">
            /// Specifies additional message-specific information.
            /// </param>
            /// <returns></returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr SendMessage(IntPtr hWnd, int Msg,
                IntPtr wParam, ref COPYDATASTRUCT lParam);


            /// <summary>
            /// The FindWindow function retrieves a handle to the top-level window 
            /// whose class name and window name match the specified strings. This 
            /// function does not search child windows. This function does not 
            /// perform a case-sensitive search.
            /// </summary>
            /// <param name="lpClassName">Class name</param>
            /// <param name="lpWindowName">Window caption</param>
            /// <returns></returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }
        #endregion
    }

    public class MMexec
    {
        Random rnd = new Random();
        public string mmexec { get; set; }
        public string sender { get; set; }
        public string cmd { get; set; }
        public int id { get; set; }
        public Dictionary<string, object> prms;

        public MMexec(string Caption = "", string Sender = "", string Command = "", int ID = -1)
        {
            mmexec = Caption;
            sender = Sender;
            cmd = Command;
            if (ID == -1) id = rnd.Next(int.MaxValue);
            else id = ID;
            prms = new Dictionary<string, object>();
        }
        public void Clear()
        {
            mmexec = "";
            sender = "";
            cmd = "";
            id = -1;
            prms.Clear();
        }
        public MMexec Clone()
        {
            MMexec mm = new MMexec();
            mm.mmexec = mmexec;
            mm.sender = sender;
            mm.cmd = cmd;
            mm.id = id;
            mm.prms = new Dictionary<string, object>(prms);
            return mm;
        }

        public string Abort(string Sender = "")
        {
            cmd = "abort";
            mmexec = "abort!Abort!ABORT!";
            if (!Sender.Equals("")) sender = Sender;
            prms.Clear();
            return JsonConvert.SerializeObject(this);
        }
    }

    public class MMscan
    {
        public string groupID;
        public string sParam;
        public double sFrom;
        public double sTo;
        public double sBy;
        public double Value;

        public bool Check()
        {
            if((sFrom == sTo) || (sBy == 0) || (Math.Abs(sBy) > Math.Abs(sTo - sFrom))) return false;
            if ((sBy > 0) && (sFrom > sTo)) return false;
            if ((sBy < 0) && (sFrom < sTo)) return false;
            return true;
        }

        public MMscan NextInChain = null;
        public bool Next()
        {
            bool NextValue = false;
            if(NextInChain != null)
            {
                NextValue = NextInChain.Next();
            }
            if (NextValue) return true;
            else
            {
                Value += sBy;
                if (Value > sTo)
                {
                    Value = sFrom;
                    return false;
                }
                else return true;
            }
        }

        public string AsString
        {
            get { return sParam + "\t" + sFrom.ToString("G6") + " .. " + sTo.ToString("G6") + "; " + sBy.ToString("G6"); }
            set 
            {
                if (value == null) return;
                if (value == "")
                {
                    TestInit(); return;
                }
                string[] parts = value.Split('\t'); sParam = parts[0];
                string ss = parts[1]; int j = ss.IndexOf(".."); if(j == -1) return;
                parts[0] = ss.Substring(0, j); parts[1] = ss.Substring(j+2);
                sFrom = Convert.ToDouble(parts[0]);
                parts = parts[1].Split(';'); sTo = Convert.ToDouble(parts[0]);
                sBy = Convert.ToDouble(parts[1]);
            }
        }

        public void TestInit()
        {
            groupID = DateTime.Now.ToString("yy-MM-dd_H-mm-ss");
            sParam = "prm";
            sFrom = 0;
            sTo = 4 * 3.14;
            sBy = 0.1;
        }
        public void ToDictionary(ref Dictionary<string, object> dict)
        {
            if (dict == null) dict = new Dictionary<string, object>();
            dict["groupID"] = groupID;
            dict["param"] = sParam;
            dict["from"] = sFrom;
            dict["to"] = sTo;
            dict["by"] = sBy;
        }
        public bool FromDictionary(Dictionary<string, object> dict)
        {
            if (!string.IsNullOrEmpty((string)dict["groupID"])) groupID = (string)dict["groupID"];
            else return false;
            if (!string.IsNullOrEmpty((string)dict["param"])) sParam = (string)dict["param"];
            else return false;
            try
            {
                sFrom = (double)dict["from"];
                sTo = (double)dict["to"];
                sBy = (double)dict["by"];
            }
            catch (InvalidCastException e)
            {
                return false;
            }
            return true;
        }
    }
}
