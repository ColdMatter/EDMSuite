using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using DAQ;

namespace AlFHardwareControl
{
    public partial class MSquaredLaserView : UserControl
    {

        [Serializable]
        public class Line
        {
            public string name;
            public double frequency;

            public Line() { }
            public Line(string _name, double _frequency)
            {
                name = _name;
                frequency = _frequency;
            }
        }


        private static List<Line> lineData;
        private M2LaserInterface laser;

        public bool FallbackActive = false;

        private delegate void LineDataUpdateDelegate();
        private static event LineDataUpdateDelegate lineDataChanged;

        public MSquaredLaserView()
        {
            InitializeComponent();
            if (lineData == null)
            {
                if (File.Exists("C:\\Users\\alfultra\\OneDrive - Imperial College London\\Desktop\\LineData.xml"))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(List<Line>));
                    using (FileStream fs = System.IO.File.Open("C:\\Users\\alfultra\\OneDrive - Imperial College London\\Desktop\\LineData.xml", FileMode.Open))
                    {
                        lineData = (List<Line>) ser.Deserialize(fs);
                    }
                }
                else
                    lineData = new List<Line> { };
            }

            laser = M2LaserInterface.getInterface(new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 192, 168, 1, 1 }), 29922),
                                                  new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 192, 168, 1, 222 }), 29922));
            lineDataChanged += updateLineSelector;

        }

        ~MSquaredLaserView()
        {
            lineDataChanged -= updateLineSelector;
        }

        public static void saveLineData()
        {
            lock (lineData)
            {
                XmlSerializer ser = new XmlSerializer(typeof(List<Line>));
                using (FileStream fs = System.IO.File.Open("C:\\Users\\alfultra\\OneDrive - Imperial College London\\Desktop\\LineData.xml", FileMode.Create, FileAccess.Write))
                {
                    ser.Serialize(fs, lineData);
                }
            }
        }

        private static double wavelength_to_frequency(double wavelength)
        {
            return 299_792_458 / wavelength / 1e3;
        }

        private static double frequency_to_wavelength(double freq)
        {
            return 299_792_458 / freq / 1e3;
        }


        private static double frequency_to_wavenumber(double freq)
        {
            return freq * 1e12 / 299_792_458;
        }

        private double current_reading = 327;
        public void UpdateStatus()
        {
            laser.IssueCommand("poll_wave_m", new Dictionary<string, object> { });
        }

        private List<Tuple<Color, string>> wavemeter_states = new List<Tuple<Color, string>>
        {
            new Tuple<Color, string>(Color.NavajoWhite, "NO TUNING"),
            new Tuple<Color, string>(Color.Salmon, "NO WAVEMETER"),
            new Tuple<Color, string>(Color.NavajoWhite, "TUNING"),
            new Tuple<Color, string>(Color.PaleGreen, "LOCKED")
        };

        private Tuple<Color, string> remote_lock_state;
        private void poll_wave_reply(Dictionary<string,object> data)
        {

            Dictionary<string, object> parameters = (Dictionary<string, object>)((Dictionary<string, object>)data["message"])["parameters"];
            if (!FallbackActive)
                current_reading = wavelength_to_frequency(Convert.ToDouble(((List<object>)parameters["current_wavelength"])[0]));
            else
            {
                try
                {
                    current_reading = wavelength_to_frequency(laser.get_wavelength());
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    current_reading = -1;
                }

            }
            remote_lock_state = wavemeter_states[(int)((List<object>)parameters["status"])[0]];
            bool lock_status = (int)((List<object>)parameters["status"])[0] > 2;

            this.Invoke((Action)(()=>
            {
                Conn_status.Text = remote_lock_state.Item2;
                Conn_status.BackColor = remote_lock_state.Item1;
                this.M2_Control_Group.Enabled = !(Conn_status.BackColor == Color.Salmon);
                locked = lock_status;
                if (lock_status != lockCheckBox.Checked)
                    lockCheckBox.Checked = lock_status;
                if (current_reading > 0)
                    CurrFreq.Text = current_reading.ToString();
            }));

            UpdateError();
        }

        private double MHzerror = 0;
        private void UpdateError()
        {
            MHzerror = (current_reading - SP) * 1e6;
            this.Invoke((Action)(()=>
            {
                error.Text = MHzerror.ToString();
            }));
        }

        private void connectionChange(bool status)
        {
            this.Invoke((Action)(()=>
            {
                Conn_status.Text = status ? "LINK UP" : "DISCONNECTED";
                Conn_status.BackColor = status ? Color.NavajoWhite : Color.Salmon;
                this.M2_Control_Group.Enabled = false;
            }));
        }

        private bool locked = false;
        private void lockCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (locked != lockCheckBox.Checked)
                laser.IssueCommand("lock_wave_m", new Dictionary<string, object> 
                {
                    { "operation", lockCheckBox.Checked ? "on" : "off" }
                });
        }

        private double SP = 327;
        private Line selectedLine = new Line("Default", 327);
        private void RecalculateSP()
        {
            SP = selectedLine.frequency + off * 1e-6;
            SP += frequency_to_wavenumber(SP) * vel * 1e-12;
            this.Invoke((Action)(() =>
            {
                setpoint.Text = SP.ToString();
            }));
            if (!FallbackActive)
                laser.IssueCommand("set_wave_m", new Dictionary<string, object>
                {
                    { "wavelength", new List<object> { frequency_to_wavelength(SP) } }
                });
            else
                laser.set_wavelength(frequency_to_wavelength(SP));
        }

        private void offset_OnSetClick(object sender, EventArgs e)
        {
            try
            {
                off = Convert.ToDouble(((ParamSet.ParamEventArgs)e).Param);
                offset.CurrentValue = off.ToString();
                RecalculateSP();
            }
            catch (FormatException)
            {

            }
        }

        private void VelSet_OnSetClick(object sender, EventArgs e)
        {
            try
            {
                vel = Convert.ToDouble(((ParamSet.ParamEventArgs)e).Param);
                VelSet.CurrentValue = vel.ToString();
                RecalculateSP();
            }
            catch (FormatException)
            {

            }
            RecalculateSP();
        }

        private void LinesSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedLine = lineData[LinesSelector.SelectedIndex];
            lineName.Text = selectedLine.name;
            lineFrequency.Text = selectedLine.frequency.ToString();
            RecalculateSP();
        }

        private void AddLine_Click(object sender, EventArgs e)
        {
            selectedLine = new Line(lineName.Text, selectedLine.frequency);
            try
            {
                selectedLine.frequency = Convert.ToDouble(lineFrequency.Text);
                RecalculateSP();
            }
            catch (FormatException)
            {
                lineFrequency.Text = selectedLine.frequency.ToString();
            }
            lock (lineData)
            {
                lineData.Add(selectedLine);
            }
            LinesSelector.Text = selectedLine.name;
            lineDataChanged?.Invoke();
        }

        private void RemoveLine_Click(object sender, EventArgs e)
        {
            if (!lineData.Contains(selectedLine)) return;
            lineData.Remove(selectedLine);
            lineDataChanged?.Invoke();
        }

        private void updateLineData_Click(object sender, EventArgs e)
        {
            try
            {
                selectedLine.frequency = Convert.ToDouble(lineFrequency.Text);
                RecalculateSP();
            }
            catch(FormatException)
            {
                lineFrequency.Text = selectedLine.frequency.ToString();
            }
            selectedLine.name = lineName.Text;
            LinesSelector.Text = lineName.Text;
            lineDataChanged?.Invoke();
        }

        private void updateLineSelector()
        {
            LinesSelector.Items.Clear();
            LinesSelector.Items.AddRange(lineData.AsEnumerable().Select(i => i.name).ToArray());
        }

        private double vel = 0;
        private double off = 0;

        private void MSquaredLaserView_Load(object sender, EventArgs e)
        {
            laser.registerForConnectionChange(connectionChange);
            laser.RegisterCallback("poll_wave_m_reply", poll_wave_reply);
            VelSet.CurrentValue = vel.ToString();
            offset.CurrentValue = off.ToString();
            updateLineSelector();
            if (lineData.Count != 0)
                LinesSelector.SelectedIndex = 0;
            RecalculateSP();
        }
    }
}
