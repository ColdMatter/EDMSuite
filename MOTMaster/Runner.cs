using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace MOTMaster
{
    static class Runner
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // A throw that reaches here used to kill MOTMaster -- or put up the
            // runtime's own dialog -- with nothing left behind to say why. Both
            // handlers only record; what the user sees is unchanged.
            //
            // AppDomain.UnhandledException is notification-only: the process still
            // terminates exactly as before. Application.ThreadException is not --
            // merely attaching to it suppresses the ThreadExceptionDialog WinForms
            // would otherwise show and silently continues, which would be a real
            // change in behaviour for every experiment on this suite. So the
            // default is reproduced explicitly here: same dialog, and Abort still
            // ends the application.
            AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
            {
                SharedCode.AppLog.Error("Runner", e.ExceptionObject as Exception);
            };
            Application.ThreadException += delegate(object s, System.Threading.ThreadExceptionEventArgs e)
            {
                SharedCode.AppLog.Error("Runner", e.Exception);
                if (new ThreadExceptionDialog(e.Exception).ShowDialog() == DialogResult.Abort)
                    Application.Exit();
            };

            // instantiate the controller
            Controller controller = new Controller();

            // publish the controller to the remoting system
            TcpChannel channel = new TcpChannel(1187);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(controller, "controller.rem");


            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            //Application.run(new MOTMasterWindow());
            controller.StartApplication();

            // the application is finishing - close down the remoting channel
            RemotingServices.Disconnect(controller);
            ChannelServices.UnregisterChannel(channel);
        }
    }
}
