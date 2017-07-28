using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace RemoteMessagingNS
{
    class RemoteMessaging
    {
        string partner;
        IntPtr windowHandle;
        private string _lastRcvMsg = "";
        public string lastRcvMsg { get { return _lastRcvMsg;  } }
        private string _lastSndMsg = "";
        public string lastSndMsg { get { return _lastSndMsg; } }
        public List<string> msgLog;

        public bool Enabled = true;

        public RemoteMessaging(string Partner)
        {
            partner = Partner;
            windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            HwndSource hwndSource = HwndSource.FromHwnd(windowHandle);
            hwndSource.AddHook(new HwndSourceHook(WndProc));

            msgLog = new List<string>();
        }

        public delegate bool RemoteHandler(string msg);
        public event RemoteHandler Remote;
        protected bool OnRemote(string msg)
        {
            if (Remote != null) return Remote(msg);
            else return false;
        }

        public delegate void ActiveCommHandler(bool msg);
        public event ActiveCommHandler ActiveComm;
        protected void OnActiveComm(bool msg)
        {
            if (ActiveComm != null) ActiveComm(msg);
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
                        _lastRcvMsg = myStruct.Message;
                        msgLog.Add("R: " + _lastRcvMsg);
                        switch (lastRcvMsg) 
                        {
                        case("ping"):
                                handled = sendCommand("pong");
                                OnActiveComm(handled);
                                break;
                        case("pong"):
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
                MessageBox.Show("Unable to find the "+partner +" window");
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
                    MessageBox.Show(String.Format("SendMessage(WM_COPYDATA) failed w/err 0x{0:X}", result));
                    return false;
                }
                else
                {
                    _lastSndMsg = msg; msgLog.Add("S: " + _lastSndMsg);
                }
                return true;
            }                
            finally
            {
                Marshal.FreeHGlobal(pMyStruct);
            }
        }

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
        }

        public bool CheckConnection()
        {
            bool back = sendCommand("ping");
            if (back)
            {
                for (int i = 0; i<500; i++)
                {
                    Thread.Sleep(10);
                    DoEvents();
                    if (lastRcvMsg.Equals("pong")) break;
                }
            }
            return back && (lastRcvMsg.Equals("pong"));
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]       
        internal struct MyStruct
        {
            public int Number;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
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
}
