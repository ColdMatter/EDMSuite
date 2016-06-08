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
        //TODO implement a better way of streaming the output from the muquans laser to the consoleTextBox
        private void lockSlave0Button_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as BooleanButton;
            button.Value = !button.Value;

            if (button.Value)
            {
               reader =  controller.muquans.LockLaser("slave0");

               WriteToConsole("Locked Slave0");
               
            }
            else
            {
                controller.muquans.UnlockLaser("slave0");
                WriteToConsole("Unlocked Slave0");
            }
           
          
        }

        public void WriteToConsole(string text)
        {
            console.WriteLine(text);
        }

        private void lockSlave1Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void edfa1LED_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lockSlave2Button_Click(object sender, RoutedEventArgs e)
        {
            controller.muquans.LockLaser("slave2");
            WriteToConsole("Locking Slave2 laser...");
        }

        private void edfa2LED_Click(object sender, RoutedEventArgs e)
        {

        }
        //TODO Make the textbox only accept numeric values and reference a displayed and true value
        private void voltageBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.Text += " V";
        }

        private string formatUnits(string value, string unit)
        {
            return value + " " + unit;
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

        private void edfa0LockButton_Click(object sender, RoutedEventArgs e)
        {
            if (edfa0Text.Text == "")
            {
                MessageBox.Show("No value entered for PID. EDFA will not turn on without one.");
            }
            else
            {
                edfa0LED.Value = !edfa0LED.Value;
                bool lockParam = edfa0LockType.Value;
                double lockValue = Double.Parse(edfa0Text.Text);
                if (edfa0LED.Value)
                controller.muquans.LockEDFA("edfa0", lockParam,lockValue);
            }
        }

        private void SaveParameters_Click(object sender, RoutedEventArgs e)
        {
            controller.SaveParametersWithDialog();
        }
        private void LoadParameters_Click(object sender, RoutedEventArgs e)
        {
            controller.LoadParametersWithDialog();
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
                    reader.Dispose();
                }
                catch (Exception s)
                {
                    console.WriteLine("Problem closing a text stream: " + s.Message);
                }
            }
        }
    }
    public class TextBoxStreamWriter : TextWriter
    {
        RichTextBox output = null;
        /// <summary>
        /// This implements a TextWriter that can be used to stream text to a TextBox object. In particular, this is useful for streaming Console text to the textbox in the GUI
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
