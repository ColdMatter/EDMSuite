using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ;

namespace AlFHardwareControl
{
    public partial class MSquaredLaserView : UserControl
    {

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


        private List<Line> lines = new List<Line>();
        private static Dictionary<string, double> lineData = new Dictionary<string, double> { };
        private M2LaserInterface laser;

        public MSquaredLaserView()
        {
            InitializeComponent();
            if (lineData.Count == 0)
            {

            }

            laser = M2LaserInterface.getInterface(new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 192, 168, 1, 1 }), 29922),
                                                  new System.Net.IPEndPoint(new System.Net.IPAddress(new byte[] { 192, 168, 1, 222 }), 29922));
            
        }

        private static double wavelength_to_frequency(double wavelength)
        {
            return 299_792_458 / wavelength / 1e3;
        }

        private static double frequency_to_wavenumber(double freq)
        {
            return freq * 1e12 / 299_792_458;
        }

        private double current_reading = 327;
        public void UpdateStatus()
        {
            laser.IssueCommand("poll_wave_m", new Dictionary<string, object> { }, poll_wave_reply, true);
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
            Dictionary<string, object> message = (Dictionary<string, object>)data["message"];
            if ((string)message["op"] != "poll_wave_m_reply") return;

            Dictionary<string, object> parameters = (Dictionary<string, object>)message["parameters"];
            current_reading = wavelength_to_frequency(Convert.ToDouble(((List<object>)parameters["current_wavelength"])[0]));
            remote_lock_state = wavemeter_states[(int)((List<object>)parameters["status"])[0]];
            bool lock_status = (int)((List<object>)parameters["status"])[0] > 2;

            this.Invoke((Action)(()=>
            {
                Conn_status.Text = remote_lock_state.Item2;
                Conn_status.BackColor = remote_lock_state.Item1;
                this.M2_Control_Group.Enabled = !(Conn_status.BackColor == Color.Salmon);
                if (lock_status != locked)
                    lockCheckBox.Checked = lock_status;
            }));

            UpdateError();
        }

        private double MHzerror = 0;
        private void UpdateError()
        {
            MHzerror = (SP - current_reading) * 1e6;
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
            locked = lockCheckBox.Checked;
            laser.IssueCommand("lock_wave_m", new Dictionary<string, object> { { "operation", locked ? "on" : "off" } });
            LinesSelector.Enabled = !locked;
            AddLine.Enabled = !locked;
            RemoveLine.Enabled = !locked;
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

        private void lockTolerance_OnSetClick(object sender, EventArgs e)
        {
            RecalculateSP();
        }

        private void lockPrecision_OnSetClick(object sender, EventArgs e)
        {
            RecalculateSP();
        }

        private void LinesSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedLine = lines[LinesSelector.SelectedIndex];
            lineName.Text = selectedLine.name;
            lineFrequency.Text = selectedLine.frequency.ToString();
            RecalculateSP();
        }

        private void AddLine_Click(object sender, EventArgs e)
        {
            selectedLine = new Line(selectedLine.name, selectedLine.frequency);
            lines.Add(selectedLine);
            LinesSelector.Items.Add(selectedLine.name);
            LinesSelector.SelectedIndex = LinesSelector.Items.Count - 1;
            RecalculateSP();
        }

        private void RemoveLine_Click(object sender, EventArgs e)
        {
            if (LinesSelector.Items.Count == 0) return;
            lines.Remove(selectedLine);
            LinesSelector.Items.RemoveAt(LinesSelector.SelectedIndex);
            if (LinesSelector.Items.Count != 0)
                LinesSelector.SelectedIndex = 0;
            RecalculateSP();
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
            if (LinesSelector.Items.Count != 0)
                LinesSelector.Items[LinesSelector.SelectedIndex] = lineName.Text;
        }

        private double vel = 0;
        private double prec = 5e-5;
        private double tol = 1e-5;
        private double off = 0;

        private void MSquaredLaserView_Load(object sender, EventArgs e)
        {
            laser.registerForConnectionChange(connectionChange);
            VelSet.CurrentValue = vel.ToString();
            lockPrecision.CurrentValue = prec.ToString();
            lockTolerance.CurrentValue = tol.ToString();
            offset.CurrentValue = off.ToString();
            RecalculateSP();
        }
    }
}
