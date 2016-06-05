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

namespace NavigatorHardwareControl
{
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : Window
    {
       
        public Controller controller;
        public ControlWindow()
        {
            InitializeComponent();
        }


        private void edfa0LED_Click(object sender, RoutedEventArgs e)
        {
            if (edfa0LED.Value == true)
            { edfa0LED.Value = false; }
            else
            { edfa0LED.Value = true; }

            WriteToConsole("Edfa0LED clicked");
           
        }
        //TODO make this more general so that it applies to each slave
        private void lockSlave0Button_Click(object sender, RoutedEventArgs e)
        {
            lockSlave0Button.Value = !lockSlave0Button.Value;
            edfa0LED.Value = lockSlave0Button.Value;
            WriteToConsole("Locked Slave0");
          
        }

        public void WriteToConsole(string text)
        {
            consoleRichTextBox.AppendText(">> " + text + "\n");
        }

        private void lockSlave1Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void edfa1LED_Click(object sender, RoutedEventArgs e)
        {

        }

        private void lockSlave2Button_Click(object sender, RoutedEventArgs e)
        {

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
            MessageBox.Show("This button can be used to monitor the error signal for the PID loop to lock this laser. This is not yet implemented");
        }
    }
}
