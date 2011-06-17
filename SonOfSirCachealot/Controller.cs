using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Reflection;
using System.Threading;

using Analysis;
using Analysis.EDM;
using Data;
using Data.EDM;

using SonOfSirCachealot.Database;

namespace SonOfSirCachealot
{
    public class Controller : MarshalByRefObject
    {
        // for static logging methods
        private static Controller controller;
 
        // UI
        internal MainWindow mainWindow;
        System.Threading.Timer statusMonitorTimer;
 
        // Database
        public BlockStore BlockStore;
 
        // This method is called before the main form is created.
        // Don't do any UI stuff here!
        internal void Initialise()
        {
            controller = this;
            BlockStore = new BlockStore();
        }

        // This method is called by the GUI thread once the form has
        // loaded and the UI is ready.
        internal void UIInitialise()
        {
            // put the version number in the title bar to avoid confusion!
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            mainWindow.Text += "  " + version.ToString();
            // This will load the shared code assembly so that we can get its
            // version number and display that as well.
            TOF t = new TOF();
            Version sharedCodeVersion = Assembly.GetAssembly(t.GetType()).GetName().Version;
            mainWindow.Text += " (" + sharedCodeVersion.ToString() + ")";
            // start the status monitor
            statusMonitorTimer = new System.Threading.Timer(new TimerCallback(UpdateStatusMonitor), null, 500, 500);
        }

        // this method gets called by the main window menu exit item, and when
        // the form's close button is pressed.
        internal void Exit()
        {
            // not sure whether this is needed, or even helpful.
            statusMonitorTimer.Dispose();
        }

        internal void UpdateStatusMonitor(object unused)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine(GetThreadStats());
            b.AppendLine("");
            b.AppendLine(GetDatabaseStats());
            mainWindow.SetStatsText(b.ToString());
        }

        internal static void log(string txt)
        {
            controller.mainWindow.AppendToLog(txt);
        }

        internal static void errorLog(string txt)
        {
            controller.mainWindow.AppendToErrorLog(txt);
        }

        // without this method, any remote connections to this object will time out after
        // five minutes of inactivity.
        // It just overrides the lifetime lease system completely.
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        private string GetThreadStats()
        {
            return "";
        }
  
        private string GetDatabaseStats()
        {
            StringBuilder b = new StringBuilder();
            return b.ToString();
        }


        internal void RunTest1()
        {
            //BlockStore.AddBlock("C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\01Oct0900_0.zip");
            string datRoot = "C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\01Oct0900_";
            List<string> files = new List<string>();
            for (int i = 0; i < 20; i++) files.Add(datRoot + i + ".zip");
            BlockStore.AddBlocks(files.ToArray());
        }
    }
}
