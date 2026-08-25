using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using SpectrumDDS;

namespace SpectrumDDSController
{
    internal static class Runner
    {
        /// <summary>TCP port this controller publishes on.</summary>
        /// <remarks>
        /// The suite's port map: 1172 hardware control and the reporter, 1187
        /// MOTMaster, 1818 the DDS. MOTMaster looks for
        /// <c>tcp://127.0.0.1:1818/controller.rem</c>.
        /// </remarks>
        public const int RemotingPort = 1818;

        /// <param name="args">
        /// An optional path to a pattern JSON file to open on startup, so a pattern
        /// can be brought up from a shortcut or the command line.
        /// </param>
        [STAThread]
        private static void Main(string[] args)
        {
            // Record what killed the process, or what the runtime put a dialog up
            // about, without changing either. AppDomain.UnhandledException is
            // notification-only. Application.ThreadException is not -- attaching to
            // it at all suppresses the ThreadExceptionDialog WinForms would show and
            // silently carries on -- so the default is reproduced explicitly.
            AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
            {
                DdsLog.Error("Runner", e.ExceptionObject as Exception);
            };
            Application.ThreadException += delegate(object s, System.Threading.ThreadExceptionEventArgs e)
            {
                DdsLog.Error("Runner", e.Exception);
                if (new ThreadExceptionDialog(e.Exception).ShowDialog() == DialogResult.Abort)
                    Application.Exit();
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Controller controller = new Controller();

            try
            {
                TcpChannel channel = new TcpChannel(RemotingPort);
                ChannelServices.RegisterChannel(channel, false);
                RemotingServices.Marshal(controller, "controller.rem");
            }
            catch (Exception ex)
            {
                // Worth running anyway: manual control is useful even when the port
                // is taken by a leftover instance, and this is the usual sign of one.
                DdsLog.Error("Runner (remoting port " + RemotingPort + ")", ex);
                MessageBox.Show(
                    "Could not publish the DDS controller on port " + RemotingPort + ":" +
                    Environment.NewLine + Environment.NewLine + ex.Message +
                    Environment.NewLine + Environment.NewLine +
                    "MOTMaster will not be able to drive the DDS. Is another copy of " +
                    "SpectrumDDSController already running?",
                    "Spectrum DDS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            try
            {
                string patternFile = args != null && args.Length > 0 ? args[0] : null;
                Application.Run(new MainWindow(controller, patternFile));
            }
            catch (Exception ex)
            {
                // Without this a throw while the window is being built kills the
                // process silently, with nothing on screen and nothing on disk to
                // say why. Log first, then tell whoever is sitting in front of it.
                string log = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "startup-error.txt");
                try { System.IO.File.WriteAllText(log, DateTime.Now + Environment.NewLine + ex); }
                catch (Exception) { }

                // Also into the dated event log, so a startup failure sits in the
                // same file as everything else that went wrong that day. The write
                // above stays exactly as it is: it is the last line of defence and
                // must not come to depend on this.
                DdsLog.Error("Runner (startup)", ex);

                MessageBox.Show(
                    "The DDS controller could not start:" + Environment.NewLine +
                    Environment.NewLine + ex.Message + Environment.NewLine +
                    Environment.NewLine + "Details written to " + log,
                    "Spectrum DDS", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
