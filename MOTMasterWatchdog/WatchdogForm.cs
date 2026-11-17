using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

using DAQ.Analog;
using DAQ.Environment;
using DAQ.HAL;

namespace MOTMasterWatchdog
{
    /// <summary>
    /// Watches the MOTMaster process from outside it. MOTMaster sets the experiment's
    /// safe outputs itself at the end of every run; this covers the cases it can't:
    /// the process being killed, or dying before its own handlers run. When MOTMaster
    /// goes away the safe outputs are written. When it stops responding the user is
    /// alarmed and can choose to kill it.
    ///
    /// An experiment opts in by declaring its safe outputs (DAQMxAnalogSafeState.InfoKey)
    /// in its Hardware class; without that this app watches nothing and writes nothing.
    /// </summary>
    public class WatchdogForm : Form
    {
        const string MOTMasterProcessName = "MOTMaster";
        const int PollIntervalMs = 500;
        // DAQmx can take a moment to free a dead process's channel reservations.
        const int SafeAttemptsAfterExit = 20;
        static readonly TimeSpan HangAlarmAfter = TimeSpan.FromSeconds(10);

        Dictionary<string, double> safeOutputs;
        bool sawMOTMaster = false;
        int safeAttemptsLeft = 0;
        DateTime? notRespondingSince = null;
        bool safeFailedAlarm = false;
        DateTime lastBeep = DateTime.MinValue;

        Label statusLabel;
        Label alarmLabel;
        Button setSafeNowButton;
        Button killButton;
        Button acknowledgeButton;
        ListView outputsList;
        TextBox logBox;
        Timer pollTimer;

        public WatchdogForm()
        {
            BuildUI();

            List<string> problems = new List<string>();
            try
            {
                safeOutputs = DAQMxAnalogSafeState.FromHardware();
                problems = DAQMxAnalogSafeState.Problems(safeOutputs);
            }
            catch (Exception ex)
            {
                problems.Add("Could not load this machine's Hardware class: " + ex.Message);
            }

            if (problems.Count > 0)
            {
                // Refuse to run half-configured: better seen now than a watchdog that
                // looks fine and then can't write.
                statusLabel.Text = "NOT PROTECTING: fix the safe outputs in the Hardware class, rebuild and restart.";
                statusLabel.ForeColor = Color.Firebrick;
                foreach (string p in problems) Log(p);
                setSafeNowButton.Enabled = false;
                safeOutputs = null;
                return;
            }

            if (safeOutputs == null || safeOutputs.Count == 0)
            {
                statusLabel.Text = "Idle: this experiment declares no safe outputs.";
                Log("Nothing to watch. Declare " + DAQMxAnalogSafeState.InfoKey + " in the Hardware class to opt in.");
                setSafeNowButton.Enabled = false;
                return;
            }

            FillOutputsList();
            Log("Protecting " + DAQMxAnalogSafeState.Describe(safeOutputs) + ". These are set if MOTMaster exits.");

            pollTimer = new Timer { Interval = PollIntervalMs };
            pollTimer.Tick += (s, e) => Poll();
            pollTimer.Start();
            Poll();
        }

        void Poll()
        {
            Process[] procs = Process.GetProcessesByName(MOTMasterProcessName);
            try
            {
                bool running = procs.Length > 0;
                setSafeNowButton.Enabled = !running;

                if (running)
                {
                    if (safeAttemptsLeft > 0)
                    {
                        // It came back before the old one's outputs could be made safe.
                        safeAttemptsLeft = 0;
                        RaiseSafeFailedAlarm("MOTMaster restarted before the safe outputs could be set.");
                    }
                    if (!sawMOTMaster) Log("MOTMaster is running (pid " + procs[0].Id + ").");
                    sawMOTMaster = true;
                    CheckResponding(procs);
                    if (!notRespondingSince.HasValue)
                        statusLabel.Text = "Watching MOTMaster (pid " + procs[0].Id + ").";
                }
                else
                {
                    if (sawMOTMaster)
                    {
                        sawMOTMaster = false;
                        ClearHang();
                        safeAttemptsLeft = SafeAttemptsAfterExit;
                        Log("MOTMaster has exited. Setting safe outputs.");
                    }
                    if (safeAttemptsLeft > 0) TrySetSafeAfterExit();
                    if (!safeFailedAlarm && safeAttemptsLeft == 0)
                        statusLabel.Text = "MOTMaster is not running.";
                }

                UpdateAlarm();
            }
            finally
            {
                foreach (Process p in procs) p.Dispose();
            }
        }

        void TrySetSafeAfterExit()
        {
            try
            {
                DAQMxAnalogSafeState.SetSafeValues(safeOutputs);
                safeAttemptsLeft = 0;
                Log("Safe outputs set.");
            }
            catch (Exception ex)
            {
                safeAttemptsLeft--;
                if (safeAttemptsLeft == 0)
                {
                    SharedCode.AppLog.Error("MOTMasterWatchdog.setSafe", ex);
                    RaiseSafeFailedAlarm("Could not set the safe outputs after MOTMaster exited: " + ex.Message);
                }
            }
        }

        void CheckResponding(Process[] procs)
        {
            bool responding = true;
            foreach (Process p in procs)
            {
                // Responding is only meaningful for a process with a window; a failure
                // to ask is not evidence of a hang.
                try { if (!p.Responding) responding = false; } catch { }
            }

            if (responding)
            {
                if (notRespondingSince.HasValue) Log("MOTMaster is responding again.");
                ClearHang();
                return;
            }

            if (!notRespondingSince.HasValue) notRespondingSince = DateTime.Now;
            if (DateTime.Now - notRespondingSince.Value >= HangAlarmAfter && !killButton.Visible)
            {
                Log("MOTMaster has not responded for " + HangAlarmAfter.TotalSeconds + " s.");
                SharedCode.AppLog.Write("MOTMasterWatchdog", "MOTMaster not responding.");
                killButton.Visible = true;
            }
        }

        void ClearHang()
        {
            notRespondingSince = null;
            killButton.Visible = false;
        }

        void RaiseSafeFailedAlarm(string message)
        {
            safeFailedAlarm = true;
            Log("ALARM: " + message);
            statusLabel.Text = message;
            acknowledgeButton.Visible = true;
        }

        void UpdateAlarm()
        {
            string alarm = null;
            if (safeFailedAlarm)
                alarm = "OUTPUTS NOT MADE SAFE: " + string.Join(", ", safeOutputs.Keys) + " may still be energised. Check them now.";
            else if (killButton.Visible)
                alarm = "MOTMaster is not responding. The outputs may be held at their last value.";

            alarmLabel.Visible = alarm != null;
            alarmLabel.Text = alarm ?? "";
            if (alarm == null)
            {
                TopMost = false;
                return;
            }

            TopMost = true;
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            if (DateTime.Now - lastBeep > TimeSpan.FromSeconds(2))
            {
                SystemSounds.Exclamation.Play();
                lastBeep = DateTime.Now;
            }
        }

        void setSafeNowButton_Click(object sender, EventArgs e)
        {
            try
            {
                DAQMxAnalogSafeState.SetSafeValues(safeOutputs);
                Log("Safe outputs set (manual).");
            }
            catch (Exception ex)
            {
                Log("Manual set failed: " + ex.Message);
                MessageBox.Show("Could not set the safe outputs:\n\n" + ex.Message, "MOTMaster watchdog",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void killButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Kill MOTMaster now? Any run in progress stops immediately and nothing from it "
                    + "is saved. The safe outputs are then set.", "MOTMaster watchdog",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
                return;

            foreach (Process p in Process.GetProcessesByName(MOTMasterProcessName))
            {
                try
                {
                    p.Kill();
                    Log("Killed MOTMaster (pid " + p.Id + ").");
                }
                catch (Exception ex)
                {
                    Log("Could not kill MOTMaster (pid " + p.Id + "): " + ex.Message);
                }
                finally
                {
                    p.Dispose();
                }
            }
        }

        void acknowledgeButton_Click(object sender, EventArgs e)
        {
            safeFailedAlarm = false;
            acknowledgeButton.Visible = false;
            UpdateAlarm();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (safeOutputs != null && safeOutputs.Count > 0 && e.CloseReason == CloseReason.UserClosing &&
                MessageBox.Show("Close the watchdog? " + string.Join(", ", safeOutputs.Keys)
                    + " will no longer be made safe if MOTMaster crashes.", "MOTMaster watchdog",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK)
            {
                e.Cancel = true;
                return;
            }
            base.OnFormClosing(e);
        }

        void FillOutputsList()
        {
            outputsList.Items.Clear();
            foreach (KeyValuePair<string, double> kv in safeOutputs)
            {
                AnalogOutputChannel c = (AnalogOutputChannel)Environs.Hardware.AnalogOutputChannels[kv.Key];
                outputsList.Items.Add(new ListViewItem(new string[] {
                    kv.Key,
                    kv.Value.ToString("0.###") + " V",
                    c.RangeLow + " to " + c.RangeHigh + " V",
                    c.PhysicalChannel }));
            }
        }

        void Log(string message)
        {
            logBox.AppendText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + message + Environment.NewLine);
        }

        void BuildUI()
        {
            Text = "MOTMaster watchdog";
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            ClientSize = new Size(600, 420);
            MinimumSize = new Size(460, 340);
            Font = new Font("Segoe UI", 9f);

            statusLabel = new Label { Dock = DockStyle.Top, Height = 32, Padding = new Padding(8, 8, 8, 0) };
            alarmLabel = new Label
            {
                Dock = DockStyle.Top, Height = 48, Visible = false, Padding = new Padding(8),
                BackColor = Color.Firebrick, ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold)
            };

            // Read-only on purpose: the safe values are declared in the Hardware class,
            // the one place both MOTMaster and this app read them from.
            Label outputsCaption = new Label
            {
                Dock = DockStyle.Top, Height = 22, Padding = new Padding(8, 4, 8, 0), ForeColor = SystemColors.GrayText,
                Text = "Safe outputs (edit in the Hardware class, e.g. DAQ/MoleculeMOTHardware.cs):"
            };
            outputsList = new ListView
            {
                Dock = DockStyle.Top, Height = 110, View = View.Details, FullRowSelect = true,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            outputsList.Columns.Add("Channel", 170);
            outputsList.Columns.Add("Safe value", 90);
            outputsList.Columns.Add("Range", 110);
            outputsList.Columns.Add("Physical channel", 180);

            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 38, Padding = new Padding(4) };
            setSafeNowButton = new Button { Text = "Set safe outputs now", AutoSize = true };
            setSafeNowButton.Click += setSafeNowButton_Click;
            killButton = new Button { Text = "Kill MOTMaster and make safe", AutoSize = true, Visible = false, ForeColor = Color.Firebrick };
            killButton.Click += killButton_Click;
            acknowledgeButton = new Button { Text = "Acknowledge alarm", AutoSize = true, Visible = false };
            acknowledgeButton.Click += acknowledgeButton_Click;
            buttons.Controls.AddRange(new Control[] { setSafeNowButton, killButton, acknowledgeButton });

            logBox = new TextBox
            {
                Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9f)
            };

            // Docked Top controls stack in reverse order of adding.
            Controls.Add(logBox);
            Controls.Add(buttons);
            Controls.Add(outputsList);
            Controls.Add(outputsCaption);
            Controls.Add(alarmLabel);
            Controls.Add(statusLabel);
        }
    }
}
