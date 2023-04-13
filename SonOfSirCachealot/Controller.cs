using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Reflection;
using System.Threading;
using System.Net;
using System.Web;

using Analysis;
using Analysis.EDM;
using Data;
using Data.EDM;

using SonOfSirCachealot;
using System.Collections.Specialized;

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
        HttpListener listener;
        Thread requestHandlerThread;

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
            // start the server
            startServer();
        }

        // this method gets called by the main window menu exit item, and when
        // the form's close button is pressed.
        internal void Exit()
        {
            stopServer();
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

        internal void startServer()
        {
            listener = new HttpListener();
            // Add the prefixes.
            listener.Prefixes.Add("http://localhost:8089/blockstore/");
            log("Starting server ...");
            listener.Start();
            startRequestHandler();
         }

        internal void startRequestHandler()
        {
            requestHandlerThread = new Thread(() =>
            {
                for (; ; )
                {
                    HttpListenerContext context = listener.GetContext();
                    log("Handling request ...");
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    string responseString = "No valid action.";

                    if (request.QueryString["action"].Equals("add"))
                    {
                        StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding);
                        NameValueCollection nvc = HttpUtility.ParseQueryString(
                            HttpUtility.UrlDecode(reader.ReadToEnd()), request.ContentEncoding);
                        string b = nvc["b"];
                        log("Processing add: " + b);
                        BlockStore.AddBlocksJSON(b);
                        responseString = "Blocks added.";
                    }

                    if (request.QueryString["action"].Equals("query"))
                    {
                        string q = request.QueryString["q"];
                        log("Processing query: " + q);
                        responseString = BlockStore.processJSONQuery(q);
                    }

                    if (request.QueryString["action"].Equals("queryAverage"))
                    {
                        string q = request.QueryString["q"];
                        log("Processing query (average): " + q);
                        responseString = BlockStore.processJSONQueryAverage(q);
                    }

                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                    log("Request handled.");
                }
            });
            requestHandlerThread.Start();
            log("Server started.");
        }

        internal void stopServer()
        {
            log("Stopping server ...");
            requestHandlerThread.Abort();
            listener.Stop();
            log("Server stopped.");
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
            return BlockStore.Monitor.GetStats();
        }

        private string GetDatabaseStats()
        {
            return "";
        }


        internal void RunTest1()
        {
            //BlockStore.AddBlock("C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\01Oct0900_0.zip");
            string datRoot = "C:\\Users\\jony\\Files\\Data\\SEDM\\v3\\2009\\October2009\\01Oct0900_";
            List<string> files = new List<string>();
            for (int i = 0; i < 10; i++) files.Add(datRoot + i + ".zip");
            BlockStore.AddBlocks(files.ToArray());
        }

        internal void RunTest2()
        {
            BlockStore.SetIncluded("01Oct0900", 7, true);
        }
    }
}
