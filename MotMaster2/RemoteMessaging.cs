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

namespace MOTMaster2
{
    class RemoteMessaging
    {
        public string partner { get; private set; }
        private IntPtr windowHandle;
        public string lastRcvMsg { get; private set; }
        public string lastSndMsg { get; private set; }
        public List<string> msgLog;
        public System.Windows.Threading.DispatcherTimer dTimer = null;
        private int _autoCheckPeriod = 100; // sec
        public int autoCheckPeriod
        {
            get { return _autoCheckPeriod; }
            set { _autoCheckPeriod = value; dTimer.Interval = new TimeSpan(0, 0, _autoCheckPeriod); }
        }

        public bool Enabled = true;

        public RemoteMessaging(string Partner)
        {
            partner = Partner;
            windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            HwndSource hwndSource = HwndSource.FromHwnd(windowHandle);
            hwndSource.AddHook(new HwndSourceHook(WndProc));

            msgLog = new List<string>();
            lastRcvMsg = ""; lastSndMsg = "";

            dTimer = new System.Windows.Threading.DispatcherTimer();
            dTimer.Tick += new EventHandler(dTimer_Tick);
            dTimer.Interval = new TimeSpan(0, 0, autoCheckPeriod);
            dTimer.Start();
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

        public delegate void ActiveCommHandler(bool active);
        public event ActiveCommHandler ActiveComm;
        protected void OnActiveComm(bool active)
        {
            if (ActiveComm != null) ActiveComm(active);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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
                        msgLog.Add("R: " + lastRcvMsg);
                        ResetTimer();
                        switch (lastRcvMsg)
                        {
                            case ("ping"):
                                handled = sendCommand("pong");
                                OnActiveComm(handled);
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

        public bool sendCommand(string msg)
        {
            if (!Enabled) return false;
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
                    lastSndMsg = msg; msgLog.Add("S: " + lastSndMsg);
                    ResetTimer();
                }
                return true;
            }
            finally
            {
                Marshal.FreeHGlobal(pMyStruct);
            }
        }
/*
        public void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrame), frame);
            Dispatcher.PushFrame(frame);
        }

        public object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }*/

        public bool CheckConnection()
        {
            bool back = sendCommand("ping");
            if (back)
            {
                for (int i = 0; i < 200; i++)
                {
                    Thread.Sleep(10);
                    //DoEvents();
                    if (lastRcvMsg.Equals("pong")) break;
                }
            }
            back = back && (lastRcvMsg.Equals("pong"));
            OnActiveComm(back);
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
        public MMexec()
        {
            id = rnd.Next(int.MaxValue);
            prms = new Dictionary<string, object>();
        }
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

    public struct MMscan
    {
        public string groupID;
        public string sParam;
        public double sFrom;
        public double sTo;
        public double sBy;
        public void TestInit()
        {
            groupID = DateTime.Now.ToString("yy-MM-dd_H-mm-ss");
            sParam = "frng";
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
        public void FromDictionary(Dictionary<string, object> dict)
        {
            groupID = (string)dict["groupID"];
            sParam = (string)dict["param"];
            sFrom = (double)dict["from"];
            sTo = (double)dict["to"];
            sBy = (double)dict["by"];
        }
    }
}
