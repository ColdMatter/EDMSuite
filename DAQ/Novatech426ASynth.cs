using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using NationalInstruments.Visa;
using Ivi.Visa;

using DAQ.Environment;

namespace DAQ.HAL
{
    /// <summary>
    /// This class represents an RS232 controlled Novatech 426A dual channel 400MHz signal generator.
    /// Each of the two outputs (channel 0 and channel 1) is independently programmable in frequency,
    /// phase and amplitude. Commands are taken from the Novatech 426A datasheet (Rev 1.04):
    ///   FN xxx.xxxxxxxxxxx   frequency of output N in MHz (10 uHz resolution, max 403 MHz)
    ///   PN xxxxx             phase of output N, integer 0-16383 (x * 360/16384 degrees)
    ///   VN xxxx              amplitude of output N, integer 0-1023 (1023 = full scale, ~0dBm at 100MHz)
    ///   FR xx.xxx            external reference frequency in MHz (1-25 MHz)
    ///   FD xxx.xxx           direct external clock frequency in MHz (250-1000 MHz)
    ///   C x                  clock source: E = external reference, D = internal TCXO, P = direct external clock
    ///   E x                  serial echo: D = disable, E = enable
    ///   I x                  update mode: A = immediate, M = manual (deferred), P = apply pending updates
    ///   PS                   synchronise the phase of both channels
    ///   S                    save settings to non-volatile memory
    ///   R                    reset (same as power cycle)
    ///   Q                    query status of all settings
    /// The serial port runs at 19.2 kBaud, 8 data bits, no parity, 1 stop bit.
    /// </summary>
    public class Novatech426ASynth : RS232Instrument
    {
        public static class CommandTypes
        {
            public static String Frequency { get { return "F"; } }
            public static String Phase { get { return "P"; } }
            public static String Amplitude { get { return "V"; } }
            public static String ReferenceFrequency { get { return "FR"; } }
            public static String DirectFrequency { get { return "FD"; } }
            public static String ClockSource { get { return "C"; } }
            public static String Echo { get { return "E"; } }
            public static String UpdateMode { get { return "I"; } }
            public static String PhaseSync { get { return "PS"; } }
            public static String Save { get { return "S"; } }
            public static String Reset { get { return "R"; } }
            public static String Query { get { return "Q"; } }
        }

        public enum ClockSources
        {
            ExternalReference, // C E
            Internal,          // C D (factory default)
            ExternalDirect     // C P
        }

        public enum UpdateModes
        {
            Automatic, // I A (factory default)
            Manual     // I M
        }

        // Command terminator expected by the 426A
        protected const string CommandTerminator = "\r";

        public class Novatech426AChannel
        {
            protected double maxFreq = 400.0;  // MHz
            protected double minFreq = 0.3;    // MHz
            protected int maxAmp = 1023;
            protected int minAmp = 0;
            protected int maxPhase = 16383;
            protected int minPhase = 0;

            protected double frequency;
            protected int amplitude = 1023;
            protected int phase;
            protected bool enabled = true;
            protected int channelNumber;
            protected Novatech426ASynth novatech;

            public Novatech426AChannel(Novatech426ASynth novatech, int channelNumber)
            {
                this.channelNumber = channelNumber;
                this.novatech = novatech;
            }

            public int ChannelNumber { get { return channelNumber; } }

            protected void ValidateEntry(double value, double maxValue, double minValue, string name, string unit)
            {
                if (value > maxValue || value < minValue)
                {
                    throw new System.ArgumentException(
                        value.ToString() + " is not a valid value for " + name + ". Value must lie between "
                        + minValue.ToString() + "-" + maxValue.ToString() + " " + unit
                    );
                }
            }

            protected void Write(string command, string argument)
            {
                novatech.SendCommand(command + channelNumber.ToString() + " " + argument);
            }

            /// <summary>
            /// Output frequency in MHz.
            /// </summary>
            public double Frequency
            {
                get { return frequency; }
                set
                {
                    ValidateEntry(value, maxFreq, minFreq, "Frequency", "MHz");
                    // 11 decimal places in MHz = 10 uHz resolution
                    Write(CommandTypes.Frequency, value.ToString("F11", CultureInfo.InvariantCulture));
                    frequency = value;
                }
            }

            /// <summary>
            /// Output amplitude as the raw 10-bit DAC value, 0-1023 (1023 is full scale).
            /// Setting this while the channel is disabled only stores the value; it is written
            /// to the device when the channel is re-enabled.
            /// </summary>
            public int Amplitude
            {
                get { return amplitude; }
                set
                {
                    ValidateEntry(value, maxAmp, minAmp, "Amplitude", "(0-1023)");
                    if (enabled) Write(CommandTypes.Amplitude, value.ToString());
                    amplitude = value;
                }
            }

            /// <summary>
            /// Output phase as the raw 14-bit value, 0-16383 (phase = value * 360/16384 degrees).
            /// </summary>
            public int Phase
            {
                get { return phase; }
                set
                {
                    ValidateEntry(value, maxPhase, minPhase, "Phase", "(0-16383)");
                    Write(CommandTypes.Phase, value.ToString());
                    phase = value;
                }
            }

            /// <summary>
            /// Output phase in degrees, rounded to the nearest 360/16384 degree step.
            /// </summary>
            public double PhaseDegrees
            {
                get { return phase * 360.0 / 16384.0; }
                set
                {
                    double wrapped = ((value % 360.0) + 360.0) % 360.0;
                    Phase = (int)Math.Round(wrapped * 16384.0 / 360.0) % 16384;
                }
            }

            /// <summary>
            /// The 426A has no RF on/off command, so disabling sets the amplitude to 0 and
            /// enabling restores the last amplitude set.
            /// </summary>
            public bool Enabled
            {
                get { return enabled; }
                set
                {
                    Write(CommandTypes.Amplitude, value ? amplitude.ToString() : "0");
                    enabled = value;
                }
            }

            public void SyncSettings(double frequency, int phase, int amplitude)
            {
                this.frequency = frequency;
                this.phase = phase;
                if (amplitude > 0)
                {
                    this.amplitude = amplitude;
                    enabled = true;
                }
                else enabled = false;
            }
        }

        public Novatech426AChannel Channel0;
        public Novatech426AChannel Channel1;

        public Novatech426ASynth(String visaAddress) : base(visaAddress)
        {
            // Serial connection parameters for the Novatech 426A
            base.BaudRate = 19200;
            base.DataBits = 8;
            base.StopBit = SerialStopBitsMode.One;
            base.ParitySetting = SerialParity.None;
            base.FlowControl = SerialFlowControlModes.None;
            base.TerminationCharacter = 0xa;

            Channel0 = new Novatech426AChannel(this, 0);
            Channel1 = new Novatech426AChannel(this, 1);
        }

        public Novatech426AChannel Channel(int channel)
        {
            if (channel == 0) return Channel0;
            if (channel == 1) return Channel1;
            throw new System.ArgumentException(channel.ToString() + " is not a valid channel. Channel must be 0 or 1");
        }

        /// <summary>
        /// Sends a single command, appending the terminator. Opens and closes the serial session.
        /// </summary>
        protected void SendCommand(string command)
        {
            Write(command + CommandTerminator);
        }

        protected ClockSources clockSource = ClockSources.Internal;
        public ClockSources ClockSource
        {
            get { return clockSource; }
            set
            {
                string x;
                switch (value)
                {
                    case ClockSources.ExternalReference: x = "E"; break;
                    case ClockSources.ExternalDirect: x = "P"; break;
                    default: x = "D"; break;
                }
                SendCommand(CommandTypes.ClockSource + " " + x);
                clockSource = value;
            }
        }

        protected UpdateModes updateMode = UpdateModes.Automatic;
        /// <summary>
        /// Automatic: settings take effect as soon as each command is executed.
        /// Manual: settings are deferred until ApplyPendingUpdates() is called.
        /// </summary>
        public UpdateModes UpdateMode
        {
            get { return updateMode; }
            set
            {
                SendCommand(CommandTypes.UpdateMode + " " + (value == UpdateModes.Manual ? "M" : "A"));
                updateMode = value;
            }
        }

        /// <summary>
        /// Updates all channels with pending updates (only relevant in Manual update mode).
        /// </summary>
        public void ApplyPendingUpdates()
        {
            SendCommand(CommandTypes.UpdateMode + " P");
        }

        /// <summary>
        /// External reference frequency in MHz (1-25 MHz, 1kHz steps). Used when ClockSource is ExternalReference.
        /// </summary>
        public void SetReferenceFrequency(double frequency)
        {
            if (frequency < 1.0 || frequency > 25.0)
                throw new System.ArgumentException(frequency.ToString() + " is not a valid reference frequency. Value must lie between 1-25 MHz");
            SendCommand(CommandTypes.ReferenceFrequency + " " + frequency.ToString("F3", CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Direct external clock frequency in MHz (250-1000 MHz). Used when ClockSource is ExternalDirect.
        /// </summary>
        public void SetDirectFrequency(double frequency)
        {
            if (frequency < 250.0 || frequency > 1000.0)
                throw new System.ArgumentException(frequency.ToString() + " is not a valid direct clock frequency. Value must lie between 250-1000 MHz");
            SendCommand(CommandTypes.DirectFrequency + " " + frequency.ToString("F3", CultureInfo.InvariantCulture));
        }

        public void SetEcho(bool enabled)
        {
            SendCommand(CommandTypes.Echo + " " + (enabled ? "E" : "D"));
        }

        /// <summary>
        /// Synchronises the phase alignment of both channels.
        /// </summary>
        public void SynchronisePhases()
        {
            SendCommand(CommandTypes.PhaseSync);
        }

        /// <summary>
        /// Saves the current settings to non-volatile memory, so they are restored at power up.
        /// </summary>
        public void SaveSettings()
        {
            SendCommand(CommandTypes.Save);
        }

        /// <summary>
        /// Resets the device (equivalent to a power cycle). Saved settings are preserved.
        /// </summary>
        public void ResetDevice()
        {
            SendCommand(CommandTypes.Reset);
        }

        /// <summary>
        /// Sends the Q command and returns the raw status string, e.g.
        /// "Q, F0 = 10.00000000000, P0 = 0.00, V0 = 1023, ..., Firmware version: 0.2, OK"
        /// </summary>
        public string QueryStatus()
        {
            if (Environs.Debug) return "";
            StringBuilder response = new StringBuilder();
            Connect(SerialTerminationMethod.TerminationCharacter);
            try
            {
                Write(CommandTypes.Query + CommandTerminator, true);
                // The response may come back over several lines; read until the trailing OK (or an error)
                for (int i = 0; i < 20; i++)
                {
                    string line = Read();
                    response.Append(line);
                    string trimmed = line.Trim();
                    if (trimmed.EndsWith("OK") || trimmed.EndsWith("?")) break;
                }
            }
            finally
            {
                Disconnect();
            }
            return response.ToString();
        }

        /// <summary>
        /// Reads the current settings from the device and updates the cached channel values.
        /// </summary>
        public void ReadSettingsFromDevice()
        {
            string status = QueryStatus();
            if (status == "") return;

            Channel0.SyncSettings(ParseDouble(status, "F0"), ParsePhase(status, "P0"), (int)ParseDouble(status, "V0"));
            Channel1.SyncSettings(ParseDouble(status, "F1"), ParsePhase(status, "P1"), (int)ParseDouble(status, "V1"));

            Match clock = Regex.Match(status, @"Clock mode:\s*([EDP])");
            if (clock.Success)
            {
                switch (clock.Groups[1].Value)
                {
                    case "E": clockSource = ClockSources.ExternalReference; break;
                    case "P": clockSource = ClockSources.ExternalDirect; break;
                    default: clockSource = ClockSources.Internal; break;
                }
            }
            Match update = Regex.Match(status, @"Update mode:\s*([AM])");
            if (update.Success) updateMode = update.Groups[1].Value == "M" ? UpdateModes.Manual : UpdateModes.Automatic;
        }

        protected static double ParseDouble(string status, string key)
        {
            Match m = Regex.Match(status, @"\b" + key + @"\s*[=:]\s*([-+]?[0-9]*\.?[0-9]+)");
            if (!m.Success) return 0.0;
            return Double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
        }

        // The Q response reports phase with two decimals (e.g. "P0 = 0.00"); the datasheet doesn't say whether
        // this is in degrees or the raw 0-16383 value. Assumed raw here - check against the real device.
        protected static int ParsePhase(string status, string key)
        {
            return (int)Math.Round(ParseDouble(status, key));
        }
    }
}
