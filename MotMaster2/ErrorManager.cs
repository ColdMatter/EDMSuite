using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
//using System.IO.StreamWriter;

namespace MOTMaster2
{
    class ErrorManager
    {
        private Label status;
        Button btnReset, btnYes, btnNo; 
        Color dftForeground;
        private string prevText;
       
        private RichTextBox log;

        string ErrorPath;
        public bool AutoSave = true;
       
        private System.IO.StreamWriter ErrorFile;

        public ErrorManager(ref Label _status, ref RichTextBox _log, string _ErrorPath)
        {
            status = _status;
            if (status != null)
            {
                dftForeground = ((System.Windows.Media.SolidColorBrush)(status.Foreground)).Color;  
         
                btnReset = new Button();
                btnReset.Content = "X";
                btnReset.Width = 25;
                btnReset.Height = 25;
                btnReset.Click += btnReset_Click;
                (status.Parent as StackPanel).Children.Add(btnReset);
                btnReset.Visibility = Visibility.Hidden;

                btnYes = new Button();
                btnYes.Content = "Yes";
                btnYes.Width = 25;
                btnYes.Height = 25;
                btnYes.Click += btnReset_Click;
                (status.Parent as StackPanel).Children.Add(btnYes);
                btnYes.Visibility = Visibility.Hidden;

                btnNo = new Button();
                btnNo.Content = "No";
                btnNo.Width = 25;
                btnNo.Height = 25;
                btnNo.Click += btnReset_Click;
                (status.Parent as StackPanel).Children.Add(btnNo);
                btnNo.Visibility = Visibility.Hidden;
            }
            log = _log;           
            ErrorPath = _ErrorPath;
            if (!ErrorPath.EndsWith("\\")) ErrorPath += "\\";
            AutoSave = AutoSave && Directory.Exists(ErrorPath);
            if (Directory.Exists(ErrorPath))
            {
                ErrorFile = new System.IO.StreamWriter(ErrorPath + "error.log");
                ErrorFile.AutoFlush = true;
            }                   
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            status.Content = prevText;
            status.Foreground = new System.Windows.Media.SolidColorBrush(dftForeground);
            btnReset.Visibility = Visibility.Hidden; 
        }

        public void StatusLine(string text, Color Foreground)
        {
            if (status == null) return;
            status.Foreground = new System.Windows.Media.SolidColorBrush(Foreground);

            btnReset.Visibility = Visibility.Visible; 
            prevText = status.Content.ToString();
            status.Content = text;
        }

        public void AppendLog(string text, Color Foreground)
        {
            if (log == null) return;

            TextRange rangeOfText1 = new TextRange(log.Document.ContentEnd, log.Document.ContentEnd);
            rangeOfText1.Text = text;
            rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, new System.Windows.Media.SolidColorBrush(Foreground));      
        }

        private async Task WriteFileAsync(string txt)
        {
            if (AutoSave) await ErrorFile.WriteLineAsync(txt);
        }

        public async void errorMsg(string errorText, int errorID, bool forcePopup = false) 
        {
            if (forcePopup)
            {
                MessageBox.Show(errorText, " Error message ("+errorID.ToString()+")");
            }
            else
            {
                StatusLine("Error: "+errorText, Brushes.Red.Color);
            }
            string outText = outText = "(wrn:" + errorID.ToString() + ") " + errorText;  
            AppendLog(outText + "\n", Brushes.Red.Color);
            await WriteFileAsync(outText);
        }

        public async void warningMsg(string warningText, int warningID = -1, bool forcePopup = false)
        {
            if (forcePopup)
            {
                MessageBox.Show(warningText, " Warning message (" + warningID.ToString() + ")");
            }
            else
            {
                StatusLine("Warning: " + warningText, Brushes.Green.Color);
            }
            string outText = warningText;
            if (warningID != -1) outText = "(wrn:" + warningID.ToString() + ") " + warningText;  
            AppendLog(outText, Brushes.Green.Color);
            await WriteFileAsync(outText);
        }

        public void simpleMsg(string simpleText, bool forcePopup = false)
        {
            if (forcePopup)
            {
                MessageBox.Show(simpleText, " MOTmaster Message");
            }
            else
            {
                StatusLine("Status: " + simpleText + "\n", dftForeground);
            }
            AppendLog(simpleText, dftForeground);           
        }
    }

    class WarningException : Exception { public WarningException(string p) : base(p) { } }

    class ErrorException : Exception { public ErrorException(string message) : base(message) { }} 
}
