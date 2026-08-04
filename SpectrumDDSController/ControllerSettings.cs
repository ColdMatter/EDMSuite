using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using SpectrumDDS;

namespace SpectrumDDSController
{
    /// <summary>
    /// The Manual tab settings that survive a restart: the per-channel amplitude
    /// clamps, the last frequency and amplitude typed for each channel, and the
    /// output level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Restoring a frequency and an amplitude puts them in the spinners and
    /// nothing more.</b> No tone is applied and no register is written until
    /// somebody presses Apply now, so starting the program never puts RF on an AOM
    /// by itself.
    /// </para>
    /// <para>
    /// The file lives under the user's AppData rather than beside the executable:
    /// a clean rebuild wipes the output directory, and on this machine the program
    /// runs from a directory the user may not be able to write to.
    /// DataContractJsonSerializer for consistency with
    /// <see cref="SpectrumDDS.DDSPatternFile"/>, which is why the solution still
    /// needs no JSON package.
    /// </para>
    /// </remarks>
    [DataContract(Name = "SpectrumDDSControllerSettings")]
    public class ControllerSettings
    {
        [DataMember(Name = "maximumAmplitudes", Order = 0)]
        public double[] MaximumAmplitudes;

        [DataMember(Name = "manualFrequenciesMHz", Order = 1)]
        public double[] ManualFrequenciesMHz;

        [DataMember(Name = "manualAmplitudes", Order = 2)]
        public double[] ManualAmplitudes;

        [DataMember(Name = "outputLevelMillivolts", Order = 3)]
        public int OutputLevelMillivolts;

        public static string Path
        {
            get
            {
                return System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "SpectrumDDSController", "settings.json");
            }
        }

        public static ControllerSettings Defaults()
        {
            ControllerSettings s = new ControllerSettings
            {
                MaximumAmplitudes = new double[DDSPattern.ChannelCount],
                ManualFrequenciesMHz = new double[DDSPattern.ChannelCount],
                ManualAmplitudes = new double[DDSPattern.ChannelCount],
                OutputLevelMillivolts = SpectrumDDSDriver.DefaultOutputLevelMillivolts,
            };
            for (int ch = 0; ch < DDSPattern.ChannelCount; ch++)
            {
                s.MaximumAmplitudes[ch] = SpectrumDDSDriver.DefaultMaximumAmplitude;
                s.ManualFrequenciesMHz[ch] = 80.0 + 10.0 * ch;
                s.ManualAmplitudes[ch] = 0.0;
            }
            return s;
        }

        /// <summary>
        /// Read the settings, falling back to the defaults for anything missing.
        /// </summary>
        /// <remarks>
        /// Never throws. A settings file that is absent, truncated or hand-edited
        /// into nonsense is not a reason to refuse to start, and the defaults are
        /// the conservative ones.
        /// </remarks>
        public static ControllerSettings Load()
        {
            ControllerSettings settings = null;
            try
            {
                if (File.Exists(Path))
                {
                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(typeof(ControllerSettings));
                    using (FileStream stream = File.OpenRead(Path))
                        settings = (ControllerSettings)serializer.ReadObject(stream);
                }
            }
            catch (Exception)
            {
                settings = null;
            }

            if (settings == null) return Defaults();
            settings.Sanitise();
            return settings;
        }

        /// <summary>Write the settings. Never throws; failing to save is not fatal.</summary>
        /// <returns>Null on success, or why it could not be saved.</returns>
        public string Save()
        {
            try
            {
                Sanitise();
                string directory = System.IO.Path.GetDirectoryName(Path);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                DataContractJsonSerializer serializer =
                    new DataContractJsonSerializer(typeof(ControllerSettings));
                using (FileStream stream = File.Create(Path))
                    serializer.WriteObject(stream, this);
                return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Force everything into range, so a corrupt or hand-edited file cannot
        /// raise a clamp or ask for an output level the card will reject.
        /// </summary>
        public void Sanitise()
        {
            ControllerSettings defaults = null;

            MaximumAmplitudes = Fix(MaximumAmplitudes, ref defaults, d => d.MaximumAmplitudes);
            ManualFrequenciesMHz = Fix(ManualFrequenciesMHz, ref defaults, d => d.ManualFrequenciesMHz);
            ManualAmplitudes = Fix(ManualAmplitudes, ref defaults, d => d.ManualAmplitudes);

            for (int ch = 0; ch < DDSPattern.ChannelCount; ch++)
            {
                MaximumAmplitudes[ch] = Clamp(MaximumAmplitudes[ch], 0.0, 1.0,
                    SpectrumDDSDriver.DefaultMaximumAmplitude);
                ManualFrequenciesMHz[ch] = Clamp(ManualFrequenciesMHz[ch], 0.0, 625.0, 0.0);
                // An amplitude past its own clamp would be refused the moment Apply
                // was pressed, so bring it down to something that can be applied.
                ManualAmplitudes[ch] = Clamp(ManualAmplitudes[ch], 0.0, MaximumAmplitudes[ch], 0.0);
            }

            if (OutputLevelMillivolts < 80 || OutputLevelMillivolts > 2500)
                OutputLevelMillivolts = SpectrumDDSDriver.DefaultOutputLevelMillivolts;
        }

        private static double[] Fix(double[] values, ref ControllerSettings defaults,
                                    Func<ControllerSettings, double[]> pick)
        {
            if (values != null && values.Length == DDSPattern.ChannelCount) return values;
            if (defaults == null) defaults = Defaults();
            return pick(defaults);
        }

        private static double Clamp(double value, double low, double high, double fallback)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) return fallback;
            if (value < low) return low;
            if (value > high) return high;
            return value;
        }
    }
}
