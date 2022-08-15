using System;
using System.Windows.Forms;
using wlmData; // a namespace with a "WLM" class covering the header of the wlmData.dll since headers and includes are not supported by C#


namespace SimpleSample
{

    public partial class Form1 : Form
    {
        Boolean bAvail = false;
        Boolean bcOpen = true;
        Boolean bMeas = false;
        Boolean bcStart = true;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (bAvail)
                WLM.ControlWLM(WLM.cCtrlWLMExit, 0, 0);
            else
                WLM.ControlWLM(WLM.cCtrlWLMShow, 0, 0);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (bMeas)
                WLM.Operation(0);
            else
                WLM.Operation(2);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string s;
            double w = 0;
            int i;
            ushort u = 0;

            w = WLM.GetWavelength(w);
            i = Convert.ToInt32(w);
            bAvail = true;

            // check result for error and display message or wavelength
            switch (i)
            {
                case WLM.ErrNoValue:
                    s = "No value measured";
                    break;
                case WLM.ErrNoSignal:
                    s = "No signal";
                    break;
                case WLM.ErrBadSignal:
                    s = "Bad signal";
                    break;
                case WLM.ErrLowSignal:
                    s = "Underexposed";
                    break;
                case WLM.ErrBigSignal:
                    s = "Overexposed";
                    break;
                case WLM.ErrWlmMissing:
                    s = "WLM Server not available";
                    bAvail = false;
                    break;
                case WLM.ErrOutOfRange:
                    s = "Out of range";
                    break;
                case WLM.ErrUnitNotAvailable:
                    s = "Requested unit not available";
                    break;
                default: // no error
                    w = Math.Round(w, (int)WLM.GetWLMVersion(0) - 2);
                    s = Convert.ToString(w) + "  nm";
                    break;
            }

            label1.Text = s;

            // check whether server is available and apply the text of the Open/Close button
            if (bAvail && bcOpen)
            {
                btnOpen.Text = "Close Server";
                btnStart.Enabled = true;
                bcOpen = false;
            }
            else if (!bAvail && !bcOpen)
            {
                btnOpen.Text = "Open Server";
                btnStart.Enabled = false;
                bcOpen = true;
            }

            // check whether measurement is running and apply the text of the Start/Stop button
            bMeas = (WLM.GetOperationState(u) != 0);
            if (bMeas && bcStart)
            {
                btnStart.Text = "Stop Measurement";
                bcStart = false;
            }
            else if (!bMeas && !bcStart)
            {
                btnStart.Text = "Start Measurement";
                bcStart = true;
            }
        }

    }
}
