using System;
using System.Windows;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;


namespace UtilsNS
{
    public static class Utils
    {
        public static bool isNull(System.Object o)
        {
            return object.ReferenceEquals(null, o);
        }

        public static double EnsureRange(double Value, double MinValue, double MaxValue)
        {
            return Math.Max(Math.Min(Value, MaxValue), MinValue);
        }
        public static int EnsureRange(int Value, int MinValue, int MaxValue)
        {
            return Math.Max(Math.Min(Value, MaxValue), MinValue);
        }
        public static bool InRange(double Value, double MinValue, double MaxValue)
        {
            return ((MinValue <= Value) && (Value <= MaxValue));
        }
        public static bool InRange(int Value, int MinValue, int MaxValue)
        {
            return ((MinValue <= Value) && (Value <= MaxValue));
        }
        public static void errorMessage(string errorMsg)
        {
            Console.WriteLine("Error: " + errorMsg);
        }

        [DllImport("user32.dll", SetLastError=true)]
        static extern int MessageBoxTimeout(IntPtr hwnd, String text, String title, uint type, Int16 wLanguageId, Int32 milliseconds);
        public static void TimedMessageBox(string text, string title = "Information", int milliseconds = 1500)
        {
            int returnValue = MessageBoxTimeout(IntPtr.Zero, text, title, Convert.ToUInt32(0), 1, milliseconds);
            //return (MessageBoxReturnStatus)returnValue;
        }

        public static string basePath = Directory.GetParent(System.Windows.Forms.Application.ExecutablePath).Parent.FullName;
        public static string configPath { get { return basePath + "\\Config\\"; } }
        public static string dataPath { get { return basePath + "\\Data\\"; } }        
    }

    #region async file logger
    /// <summary>
    /// Async data storage device 
    /// first you set the full path of the file, otherwise it will save in data dir under date-time file name
    /// when you want the logging to start you set Enabled to true
    /// at the end you set Enabled to false (that will flush the buffer to HD)
    /// </summary>
    public class AutoFileLogger
    {
        public string header = ""; // that will be put as a file first line with # in front of it
        List<string> buffer;
        public int bufferLimit = 2560;
        public int bufferSize { get { return buffer.Count; } }
        public bool writing { get; private set; }
        public bool missingData { get; private set; }
        Stopwatch stw;

        public AutoFileLogger(string Filename = "")
        {
            _AutoSaveFileName = Filename;
            buffer = new List<string>();
            stw = new Stopwatch();
        }

        public int log(List<string> newItems)
        {
            if (!Enabled) return buffer.Count;
            buffer.AddRange(newItems);
            if (buffer.Count > bufferLimit) Flush();
            return buffer.Count;
        }

        public int log(string newItem)
        {
            if (!Enabled) return buffer.Count;
            buffer.Add(newItem);
            if (buffer.Count > bufferLimit) Flush();
            return buffer.Count;
        }

        private void ConsoleLine(string txt)
        {
#if DEBUG
            Console.WriteLine(txt);
#endif
        }

        public void DropLastChar()
        {
            string lastItem = buffer[buffer.Count - 1];
            buffer[buffer.Count - 1] = lastItem.Substring(0,lastItem.Length - 1);
        }
        public Task Flush() // do not forget to flush when exit (OR switch Enabled Off)
        {
            if (buffer.Count == 0) return null;
            string strBuffer = "";
            for (int i = 0; i < buffer.Count; i++)
            {
                strBuffer += buffer[i] + "\n";
            }
            buffer.Clear();
            ConsoleLine("0h: " + stw.ElapsedMilliseconds.ToString());
            var task = Task.Run(() => FileWriteAsync(AutoSaveFileName, strBuffer, true));
            return task;
        }

        private async Task FileWriteAsync(string filePath, string messaage, bool append = true)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write,
                                                                                               FileShare.None, 65536, true))
                using (StreamWriter sw = new StreamWriter(stream))
                {
                    writing = true;
                    ConsoleLine("1k: " + stw.ElapsedMilliseconds.ToString());
                    await sw.WriteAsync(messaage);
                    ConsoleLine("2p: " + stw.ElapsedMilliseconds.ToString());
                    writing = false;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(">> IOException - " + e.Message);
                missingData = true;
            }
        }

        private bool _Enabled = false;
        public bool Enabled
        {
            get { return _Enabled; }
            set
            {
                if (value == _Enabled) return;
                if (value && !_Enabled) // when it goes from false to true
                {
                    string dir = "";
                    if (!_AutoSaveFileName.Equals("")) dir = Directory.GetParent(_AutoSaveFileName).FullName;
                    if (!Directory.Exists(dir))
                        _AutoSaveFileName = Utils.dataPath + DateTime.Now.ToString("yy-MM-dd_H-mm-ss") + ".ahf"; //axel hub file

                    string hdr = "";
                    if (header != "") hdr = "# " + header + "\n";
                    var task = Task.Run(() => FileWriteAsync(AutoSaveFileName, hdr, false));

                    task.Wait();
                    writing = false;
                    missingData = false;
                    stw.Start();
                    _Enabled = true;
                }
                if (!value && _Enabled) // when it goes from true to false
                {
                    while (writing)
                    {
                        Thread.Sleep(100);
                    }
                    Task task = Flush();
                    if (task != null) task.Wait();
                    if (missingData) Console.WriteLine("Some data maybe missing from the log");
                    stw.Reset();
                    header = "";
                    _Enabled = false;
                }
            }
        }

        private string _AutoSaveFileName = "";
        public string AutoSaveFileName
        {
            get
            {
                return _AutoSaveFileName;
            }
            set
            {
                if (_Enabled) throw new Exception("Logger.Enabled must be Off when you set AutoSaveFileName.");
                _AutoSaveFileName = value;
            }
        }
    }
    #endregion



}