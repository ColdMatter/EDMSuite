using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using NationalInstruments.Controls;

namespace NavigatorHardwareControl
{
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    ///
    
    public partial class ControlWindow : Window
    {
       
        public Controller controller;
        public TextBoxStreamWriter console;
        private TextReader reader;
        private bool isReading;

        public ControlWindow()
        {
            InitializeComponent();
            //I don't like initialising the controller here, but this seems to be the easiest way to deal with object references
            controller = new Controller();
            console = new TextBoxStreamWriter(consoleRichTextBox);
            //Sets the Console to stream to the consoleTextBox
            Console.SetOut(console);
            controller.Start();
           
           
        }

        private void populateDataStore()
        {
            double val;
            //iterates through each element in the GUI and sets the corresponding item in the dataStore object
            foreach (TextBox tb in FindVisualChildren<TextBox>(ddsGroup))
            {
                if (tb.Text == "")
                {
                    val = 0.0;
                }
                else
                {
                    val = double.Parse(tb.Text);
                }
                controller.dataStore.ddsFreqs[tb.Name] = val;
            }
            foreach (TextBox tb in FindVisualChildren<TextBox>(lockGroup))
            {
                if (tb.Text == "")
                {
                    val = 0.0;
                }
                else
                {
                    val = double.Parse(tb.Text);
                }
                controller.dataStore.laserVals[tb.Name] = val;
            }
            foreach (BooleanButton bb in FindVisualChildren<BooleanButton>(ddsGroup))
            {
                controller.dataStore.laserStates[bb.Name] = bb.Value;
            }
            
        }

        private void populateGUI()
        {
            //iterates through each element in the GUI and assigns it a value based on the item in the dataStore object
            foreach (TextBox tb in FindVisualChildren<TextBox>(ddsGroup))
            {
                tb.Text = controller.dataStore.ddsFreqs[tb.Name].ToString();
            }
            foreach (TextBox tb in FindVisualChildren<TextBox>(lockGroup))
            {
                tb.Text=controller.dataStore.laserVals[tb.Name].ToString();
            }
            foreach (BooleanButton bb in FindVisualChildren<BooleanButton>(ddsGroup))
            {
                bb.Value = controller.dataStore.laserStates[bb.Name];
            }
        }
        #region EventHandlers
        private void edfa0LED_Click(object sender, RoutedEventArgs e)
        {
            if (edfa0LED.Value == true)
            {
                edfa0LED.Value = false;
                controller.muquans.StopEDFA("edfa0");
            }
            else

            { edfa0LED.Value = true;
                controller.muquans.StartEDFA("edfa0");
            }
           
            WriteToConsole("Edfa0LED clicked");
           
        }
        //TODO make this more general so that it applies to each slave
        private async void lockLaserButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as BooleanButton;
            button.Value = !button.Value;
            string laserID;
            string line;
            switch (button.Name)
            {
                case "lockMaster":
                    laserID = "master";
                    break;
                case "lockSlave0":
                    laserID = "slave0";
                    break;
                case "lockSlave1":
                    laserID = "slave1";
                    break;
                case "lockSlave2":
                    laserID = "slave2";
                    break;
                default:
                    laserID = "NoLaser";
                    break;
            }
            if (button.Value)
            {
               reader =  controller.muquans.LockLaser(laserID);
                isReading = true;
                while (isReading)
                {
                    line = reader.ReadLine();
                    await console.WriteLineAsync(line);
                }
               WriteToConsole("Locked "+ laserID);
               
            }
            else
            {
                controller.muquans.UnlockLaser(laserID);
                WriteToConsole("Unlocked "+laserID);
            }
           
          
        }

        public void WriteToConsole(string text)
        {
            console.WriteLine(text);
        }

        private void edfa2LED_Click(object sender, RoutedEventArgs e)
        {

        }
 


        private void monitorButton_Click(object sender, RoutedEventArgs e)
        {
            console.WriteLine("This button can be used to monitor the error signal for the PID loop to lock this laser. This is not yet implemented");
        }

        private void edfa0LED_ValueChanged(object sender, RoutedPropertyChangedEventArgs<bool> e)
        {
            if (edfa0LED.Value == true)
            {  
           //  controller.muquans.StartEDFA("edfa0");
            }
            else
            {
              controller.muquans.StopEDFA("edfa0");
            }
        }

        private void edfaLockButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as BooleanButton;
            TextBox edfaText;
            LED edfaLED;
            Switch edfaLockType;
            string edfaID;

            switch (button.Name)
            {
                default:
                    edfaText = new TextBox();
                    edfaLED = new LED();
                    edfaLockType = new Switch();
                    edfaID = "None";
                    break;
                case "edfa0LockButton":
                    edfaText = edfa0Text;
                    edfaLED = edfa0LED;
                    edfaID = "edfa0";
                    edfaLockType = edfa0LockType;
                    break;
                case "edfa1LockButton":
                    edfaText = edfa1Text;
                    edfaLED = edfa1LED;
                    edfaID = "edfa1";
                    edfaLockType = edfa1LockType;
                    break;
                case "edfa2LockButton":
                    edfaText = edfa2Text;
                    edfaLED = edfa2LED;
                    edfaLockType = edfa2LockType;
                    edfaID = "edfa2";
                    break;
            }

            if (edfaText.Text == "")
            {
                MessageBox.Show("No value entered for PID. EDFA will not turn on without one.");
            }
            else
            {
                edfaLED.Value = !edfaLED.Value;
                string type =  "";
                bool lockParam = edfaLockType.Value;
                double lockValue = Double.Parse(edfaText.Text);
                if (edfaLED.Value)
                controller.muquans.LockEDFA(edfaID, lockParam,lockValue);
                if (lockParam)
                {
                    type =  "Power";
                }
                else
                {
                    type = "Current";
                }
                console.WriteLine("Locking " + edfaID + ": " + type + " set to " + lockParam + "V");
            }
        }

        private void SaveParameters_Click(object sender, RoutedEventArgs e)
        {
            populateDataStore();
            controller.SaveParametersWithDialog();
        }
        private void LoadParameters_Click(object sender, RoutedEventArgs e)
        {
            controller.LoadParametersWithDialog();
            populateGUI();
        }
        private void SaveDefault_Click(object sender, RoutedEventArgs e)
        {

        }

        private void consoleRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            //This is used to interrupt any text streams much like a console
            var textBox = sender as RichTextBox;
            if(Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C))
            {
                try
                {
                    isReading = false;
                    reader.Dispose();
                }
                catch (Exception s)
                {
                    console.WriteLine("Problem closing a text stream: " + s.Message);
                }
            }
        }
        #endregion
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
    public class TextBoxStreamWriter : TextWriter
    {
        RichTextBox output = null;
        /// <summary>
        /// This implements a TextWriter that can be used to stream text to a TextBox object. In particular, this is useful for streaming Console text to the textbox in the GUI. It also asynchronously writes so it will not block other code from executing.
        /// </summary>
        /// <param name="textBox"></param>
        public TextBoxStreamWriter(RichTextBox textBox)
        {
            output = textBox;
        }
        //public override void Write(char value)
        //{
        //    base.Write(value);
        //    output.AppendText(value.ToString());
        //}
       
        public override void WriteLine(string value)
        {
            base.WriteLine(value);
            output.AppendText(">>" + value.ToString()+"\n");
        }
        public override Encoding Encoding
        {
            get
            {
                return Encoding.UTF8;
            }
        }

    }

    
}
