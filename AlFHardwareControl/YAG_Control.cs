using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAQ.HAL;
using DAQ.Environment;

namespace AlFHardwareControl
{
    public partial class YAG_Control : UserControl
    {

        public BigSkyYAG YAG;

        public YAG_Control()
        {
            InitializeComponent();

            YAG = (BigSkyYAG)Environs.Hardware.Instruments["YAG"];
        }

        private T AttemptComm<T>(Func<T> action)
        {
            T res = default(T);
            lock (YAG)
            {
                try
                {
                    res = action();
                }
                catch (Exception e) when (e is Ivi.Visa.NativeVisaException || e is Ivi.Visa.IOTimeoutException || e is Ivi.Visa.VisaException)
                {
                    YAG.Disconnect();
                }
            }
            return res;

        }

        public void SetTextField(Control box, string text)
        {
            box.Invoke(new SetTextDelegate(SetTextHelper), new object[] { box, text });
        }

        private delegate void SetTextDelegate(Control box, string text);

        private void SetTextHelper(Control box, string text)
        {
            box.Text = text;
        }

        public void UpdateRenderedObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            obj.Invoke(new UpdateObjectDelegate<T>(UpdateObject), new object[] { obj, updateFunc });
        }

        private delegate void UpdateObjectDelegate<T>(T obj, Action<T> updateFunc) where T : Control;

        private void UpdateObject<T>(T obj, Action<T> updateFunc) where T : Control
        {
            updateFunc(obj);
        }

        public void UpdateStatus()
        {
            string temp = AttemptComm(() => { return YAG.SendCommand("CG"); });
            if (temp == default(string))
            {
                this.Invoke((Action)(() =>
                {
                    this.Conn_status.Text = "DISCONNECTED";
                    this.Conn_status.BackColor = Color.Salmon;
                    this.Enabled = false;
                }));
                return;
            }
            this.Invoke((Action)(() =>
            {
                this.Conn_status.Text = "CONNECTED";
                this.Conn_status.BackColor = Color.PaleGreen;
            }));
            temp = temp.Substring(9, 2);
            if (Convert.ToDouble(temp) < 36)
            {
                this.Invoke((Action)(() =>
                {
                    this.Enabled = false;
                }));
                this.Shutdown_Click(this, new EventArgs());
            }
            else
            {
                this.Invoke((Action)(() =>
                {
                    this.Enabled = true;
                }));
            }
            string IFState = AttemptComm(() => { return YAG.SendCommand("IF1"); }) + " " + AttemptComm(() => { return YAG.SendCommand("IF2"); });
            bool IFSuccess = (IFState == "IF1 00 00 00 00 IF2 00 00 00 00");

            this.SetTextField(this.Temp, temp + " C");
            this.SetTextField(this.IF, IFState);
            this.Invoke((Action)(() => { this.IF.BackColor = IFSuccess ? Color.PaleGreen : Color.Salmon; }));
            this.SetTextField(this.Status, AttemptComm(() => { return YAG.SendCommand(""); }));
            this.VMO.CurrentValue = AttemptComm(() => { return YAG.SendCommand("VMO").Substring(9, 6); });
            this.VIS.CurrentValue = AttemptComm(() => { return YAG.SendCommand("VIS").Substring(11, 4); });
            this.VOS.CurrentValue = AttemptComm(() => { return YAG.SendCommand("VOS").Substring(11, 4); });
            this.Delay.CurrentValue = AttemptComm(() => { return YAG.SendCommand("W").Substring(9, 6); });
            this.Ene.CurrentValue = AttemptComm(() => { return YAG.SendCommand("ENE").Substring(10, 5); });
            this.Freq.CurrentValue = AttemptComm(() => { return YAG.SendCommand("F").Substring(7, 8); });

            string status = AttemptComm(() => { return YAG.SendCommand("WOR"); });

            if (status == null) return;
            if ((Convert.ToDouble(status.Substring(6,1)) - 3 > 0) != this.ext_flash.Checked)
            {
                this.Invoke((Action)(() =>
                {
                    this.ext_flash.Checked = (Convert.ToDouble(status.Substring(6, 1)) - 3 > 0);
                }));
            }

            if ((Convert.ToInt32(status.Substring(6, 1)) % 4 != 0) != this.flashlamp.Checked)
            {
                this.Invoke((Action)(() =>
                {
                    this.flashlamp.Checked = (Convert.ToInt32(status.Substring(6, 1)) % 4 != 0);
                    this.ext_flash.Enabled = !this.flashlamp.Checked;
                    this.qswitch.Enabled = this.flashlamp.Checked && this.shutter.Checked;
                    this.VMO.Enabled = !this.flashlamp.Checked;
                    this.VIS.Enabled = !this.flashlamp.Checked;
                    this.VOS.Enabled = !this.flashlamp.Checked;
                    this.Delay.Enabled = !this.flashlamp.Checked;
                    this.Ene.Enabled = !this.flashlamp.Checked;
                    this.Freq.Enabled = !this.flashlamp.Checked;
                }));
            }

            if ((Convert.ToDouble(status.Substring(14, 1)) - 3 > 0) != this.ext_Q.Checked)
            {
                this.Invoke((Action)(() =>
                {
                    this.ext_Q.Checked = (Convert.ToDouble(status.Substring(14, 1)) - 3 > 0);
                }));
            }

            if ((Convert.ToInt32(status.Substring(14, 1)) % 4 != 0) != this.qswitch.Checked)
            {
                this.Invoke((Action)(() =>
                {
                    this.qswitch.Checked = (Convert.ToInt32(status.Substring(14, 1)) % 4 != 0);
                    this.ext_Q.Enabled = !this.qswitch.Checked;
                }));
            }

            bool shutter = AttemptComm(() => { return YAG.SendCommand("R"); }) == "shutter opened ";

            if (shutter != this.shutter.Checked)
            {
                this.Invoke((Action)(() =>
                {
                    this.shutter.Checked = shutter;
                    this.qswitch.Enabled = this.flashlamp.Checked && this.shutter.Checked;
                }));
            }

        }


        private void VMO_OnSetClick(object sender, EventArgs e)
        {
            string val = ((ParamSet.ParamEventArgs)e).Param;
            AttemptComm(() => { return YAG.SendCommand("VMO" + val); });
        }

        private void ext_Q_CheckedChanged(object sender, EventArgs e)
        {
            AttemptComm(() => { return YAG.SendCommand("QSM" + (this.ext_Q.Checked ? "2" : "0")); });
        }

        private void ext_flash_CheckedChanged(object sender, EventArgs e)
        {
            AttemptComm(() => { return YAG.SendCommand("LPM" + (this.ext_flash.Checked ? "1" : "0")); });
        }

        private void shutter_CheckedChanged(object sender, EventArgs e)
        {
            AttemptComm(() => { return YAG.SendCommand("R" + (this.shutter.Checked ? "1" : "0")); });
            this.Invoke((Action)(() =>
            {
                this.qswitch.Enabled = this.flashlamp.Checked && this.shutter.Checked;
            }));
        }

        private void qswitch_CheckedChanged(object sender, EventArgs e)
        {
            if (qswitch.Checked)
                AttemptComm(() => { return YAG.SendCommand("PQ"); });
            else
                AttemptComm(() => { return YAG.SendCommand("SQ"); });

            this.Invoke((Action)(() =>
            {
                this.ext_Q.Enabled = !this.qswitch.Checked;
            }));
        }

        private void VIS_OnSetClick(object sender, EventArgs e)
        {
            string val = ((ParamSet.ParamEventArgs)e).Param;
            AttemptComm(() => { return YAG.SendCommand("VIS" + val); });
        }

        private void VOS_OnSetClick(object sender, EventArgs e)
        {
            string val = ((ParamSet.ParamEventArgs)e).Param;
            AttemptComm(() => { return YAG.SendCommand("VOS" + Math.Floor(Convert.ToDouble(val)*100).ToString()); });
        }

        private void Delay_OnSetClick(object sender, EventArgs e)
        {
            string val = ((ParamSet.ParamEventArgs)e).Param;
            AttemptComm(() => { return YAG.SendCommand("W" + val); });
        }

        private void Ene_OnSetClick(object sender, EventArgs e)
        {
            string val = ((ParamSet.ParamEventArgs)e).Param;
            AttemptComm(() => { return YAG.SendCommand("ENE" + Math.Floor(Convert.ToDouble(val) * 10).ToString()); });
        }

        private void Shutdown_Click(object sender, EventArgs e)
        {
            string res = default(string);
            while (res == default(string))
            {
                res = AttemptComm(() => { return YAG.SendCommand("S"); });
            }
        }

        private void Freq_OnSetClick(object sender, EventArgs e)
        {
            string val = ((ParamSet.ParamEventArgs)e).Param;
            AttemptComm(() => { return YAG.SendCommand("F" + Math.Floor(Convert.ToDouble(val) * 100).ToString()); });
        }

        private void flashlamp_CheckedChanged(object sender, EventArgs e)
        {
            if (flashlamp.Checked)
                AttemptComm(() => { return YAG.SendCommand("A"); });
            else
                AttemptComm(() => { return YAG.SendCommand("S"); });

            this.Invoke((Action)(() =>
            {
                this.ext_flash.Enabled = !this.flashlamp.Checked;
                this.qswitch.Enabled = this.flashlamp.Checked && this.shutter.Checked;
                this.VMO.Enabled = !this.flashlamp.Checked;
                this.VIS.Enabled = !this.flashlamp.Checked;
                this.VOS.Enabled = !this.flashlamp.Checked;
                this.Delay.Enabled = !this.flashlamp.Checked;
                this.Ene.Enabled = !this.flashlamp.Checked;
                this.Freq.Enabled = !this.flashlamp.Checked;
            }));
        }
    }
}
