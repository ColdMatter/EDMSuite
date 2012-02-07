using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BuffergasHardwareControl
{
    public partial class ControlWindow : Form
    {
        
        private double flow_voltage;
        private ControlWindow window;
        public Controller controller;
        private Dictionary<string, TextBox> AOTextBoxes = new Dictionary<string, TextBox>();
        private Dictionary<string, CheckBox> DOCheckBoxes = new Dictionary<string, CheckBox>();
        public int nos;
        private string textstring;

        public ControlWindow()
        {
            InitializeComponent();
        }

        #region ThreadSafe wrappers

        private void setCheckBox(CheckBox box, bool state)
        {
            box.Invoke(new setCheckDelegate(setCheckHelper), new object[] { box, state });
        }
        private delegate void setCheckDelegate(CheckBox box, bool state);
        private void setCheckHelper(CheckBox box, bool state)
        {
            box.Checked = state;
        }

        private void setTabEnable(TabControl box, bool state)
        {
            box.Invoke(new setTabEnableDelegate(setTabEnableHelper), new object[] { box, state });
        }
        private delegate void setTabEnableDelegate(TabControl box, bool state);
        private void setTabEnableHelper(TabControl box, bool state)
        {
            box.Enabled = state;
        }

        private void setTextBox(TextBox box, string text)
        {
            box.Invoke(new setTextDelegate(setTextHelper), new object[] { box, text });
        }
        private delegate void setTextDelegate(TextBox box, string text);
        private void setTextHelper(TextBox box, string text)
        {
            box.Text = text;
        }

        private void setRichTextBox(RichTextBox box, string text)
        {
            box.Invoke(new setRichTextDelegate(setRichTextHelper), new object[] { box, text });
        }
        private delegate void setRichTextDelegate(RichTextBox box, string text);
        
        private void setRichTextHelper(RichTextBox box, string text)
        {
            box.AppendText(text);
            consoleRichTextBox.ScrollToCaret();
        }

        //private void setLED(NationalInstruments.UI.WindowsForms.Led led, bool val)
        //{
        //    led.Invoke(new SetLedDelegate(SetLedHelper), new object[] { led, val });
        //}
        //private delegate void SetLedDelegate(NationalInstruments.UI.WindowsForms.Led led, bool val);
        //private void SetLedHelper(NationalInstruments.UI.WindowsForms.Led led, bool val)
        //{
        //    led.Value = val;
        //}

        #endregion

        #region Public properties for controlling UI.
        //This gets/sets the values on the GUI panel
       public void WriteToConsole(string text)
       {
            setRichTextBox(consoleRichTextBox, ">> " + text + "\n");

        }

       
       private void ReadIntFromCommandWindow()
       {

           textstring=commandTextBox.Text;
           setTextBox(commandTextBox, textstring);         
           WriteToConsole(textstring);
           nos = Convert.ToInt32(textstring);
          

       }













        //public double ReadAnalog(string channelName)
        //{
        //    return double.Parse(AOTextBoxes[channelName].Text);
        //}
        //public void SetAnalog(string channelName, double value)
        //{
        //    setTextBox(AOTextBoxes[channelName], Convert.ToString(value));
        //}
        //public bool ReadDigital(string channelName)
        //{
        //    return DOCheckBoxes[channelName].Checked;
        //}
        //public void SetDigital(string channelName, bool value)
        //{
        //    setCheckBox(DOCheckBoxes[channelName], value);
        //}
        #endregion

        #region Flow Control
        private void FlowVoltageBox_ValueChanged(object sender, EventArgs e)
        {
            //the conversion factor from voltage to flow, currently set to 1
            flow_voltage = ((double)FlowVoltageBox.Value) * 1;
            controller.FlowControlVoltage = flow_voltage;
        }

       

        private void FlowmeterButton_Click(object sender, EventArgs e)
            {
                flowmeterTextBox1.Text = controller.FlowInputVoltage.ToString();
            }
        #endregion

        #region Camera Control








        //event based methods


        private void snapshotButton_Click(object sender, EventArgs e)
        {
            controller.CameraSnapshot();
            WriteToConsole("Taking a Snapshot");
        }

        private void streamButton_Click(object sender, EventArgs e)
        {
            controller.CameraStream();
            WriteToConsole("Camera Streaming");
        }

        private void stopStreamButton_Click(object sender, EventArgs e)
        {
            controller.StopCameraStream();
            WriteToConsole("Camera stopped");
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.SaveImageWithDialog();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        private void openImageViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StartCameraControl();
        }
      

        private void saveImagesToolStripMenu_Click(object sender, EventArgs e)
        {
            WriteToConsole("Control not working yet, use save ImageData");
            //controller.StoreImageWithDialog(nos, imaged);
        }

       

      
        private void saveImageDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            controller.StoreImageDataWithDialog();
            WriteToConsole("ImageData saved");


        }

        #endregion



       
     //  private byte[][,] imaged;
       
        private void commandTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
               ReadIntFromCommandWindow();
               commandTextBox.Clear(); 
              // imaged = controller.GrabMultipleImages(nos);
              //controller.GrabNextImage();
             //  WriteToConsole("Images acquired");
            
            }

        }

        private void disposeButton_Click(object sender, EventArgs e)
        {
            controller.DisposeImages();
            WriteToConsole("Images deleted");

        }





      

                 




      

        //#region UI state

        //public void UpdateUIState(Controller.SHCUIControlState state)
        //{
        //    switch (state)
        //    {
        //        case Controller.SHCUIControlState.OFF:

        //            setLED(remoteControlLED, false);
                    

        //            break;

        //        case Controller.SHCUIControlState.LOCAL:

        //            setLED(remoteControlLED, false);
                    
        //            break;

        //        case Controller.SHCUIControlState.REMOTE:

        //            setLED(remoteControlLED, true);
                    

        //            break;
        //    }
        //}


        //#endregion

    }


}
