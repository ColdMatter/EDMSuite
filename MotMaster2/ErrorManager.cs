using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using UtilsNS;

namespace ErrorManager
{
    public static class ErrorMgr
    {
        private static Label status;
        private static Button btnReset, btnYes, btnNo;
        private static Color dftForeground;
        private static string prevText;
       
        private static RichTextBox log;

        private static string ErrorPath;
        public static bool AutoSave = true;
        public static bool Verbatim = false;
       
        private static System.IO.StreamWriter ErrorFile;

        public static void Initialize(ref Label _status, ref RichTextBox _log, string _ErrorPath)
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

        private static void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Background,
              new Action(() =>
              {
                 status.Content = "Status:";
                 status.Foreground = new System.Windows.Media.SolidColorBrush(dftForeground);
                 btnReset.Visibility = Visibility.Hidden;
              }));
        }

        public static void StatusLine(string text, Color Foreground)
        {
            if (status == null) return;
            status.Foreground = new System.Windows.Media.SolidColorBrush(Foreground);
            Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Background,
              new Action(() =>
              {
                  btnReset.Visibility = Visibility.Visible; 
                  prevText = status.Content.ToString();
                  status.Content = text;
              }));
        }

        public static void AppendLog(string text, Color? clr = null)
        {
            if (log == null) return;
            string printOut = text;
            if ((Verbatim) || (text.Length < 81)) printOut = text;
            else printOut = text.Substring(0, 80) + "..."; 
            Color ForeColor = clr.GetValueOrDefault(Brushes.Black.Color);
            Application.Current.Dispatcher.BeginInvoke(
              DispatcherPriority.Background,
              new Action(() =>
              {
                  TextRange rangeOfText1 = new TextRange(log.Document.ContentStart, log.Document.ContentEnd);
                  string tx = rangeOfText1.Text;
                  int len = tx.Length; int maxLen = 10000; // the number of chars kept
                  if (len > (2 * maxLen)) // when it exceeds twice the maxLen
                  {
                      tx = tx.Substring(maxLen);
                      var paragraph = new Paragraph();
                      paragraph.Inlines.Add(new Run(tx));
                      log.Document.Blocks.Clear();
                      log.Document.Blocks.Add(paragraph);
                  }
                  rangeOfText1 = new TextRange(log.Document.ContentEnd, log.Document.ContentEnd);
                  rangeOfText1.Text = Utils.RemoveLineEndings(printOut) + "\r";
                  rangeOfText1.ApplyPropertyValue(TextElement.ForegroundProperty, new System.Windows.Media.SolidColorBrush(ForeColor));
                  log.ScrollToEnd();
              }));
        }

        private async static Task WriteFileAsync(string txt)
        {

            if (AutoSave) await ErrorFile.WriteLineAsync(txt);
            else return;
        }

        private static bool IsForcePopup(bool forcePopup)
        {
            return forcePopup || ((status == null) && (log == null));
        }

        public async static void errorMsg(string errorText, int errorID, bool forcePopup = false) 
        {
            if (IsForcePopup(forcePopup))
            {
                MessageBox.Show(errorText, " Error message ("+errorID.ToString()+")");
            }
            else
            {
                StatusLine("Error: "+errorText, Brushes.Red.Color);
            }
            string outText = outText = "(err:" + errorID.ToString() + ") " + errorText;  
            AppendLog(outText, Brushes.Red.Color);
            await WriteFileAsync(outText);
        }

        public async static void warningMsg(string warningText, int warningID = -1, bool forcePopup = false)
        {
            if (IsForcePopup(forcePopup))
            {
                MessageBox.Show(warningText, " Warning message (" + warningID.ToString() + ")");
                return;
            }
            else
            {
                StatusLine("Warning: " + warningText, Brushes.DarkOrange.Color);
            }
            string outText = warningText;
            if (warningID != -1) outText = "(wrn:" + warningID.ToString() + ") " + warningText;
            AppendLog(outText, Brushes.Green.Color);
            await WriteFileAsync(outText);
        }

        public static void simpleMsg(string simpleText, bool forcePopup = false)
        {
            if (IsForcePopup(forcePopup))
            {
                MessageBox.Show(simpleText, " MOTmaster Message");
            }
            else
            {
                StatusLine("Status: " + simpleText + "\n", dftForeground);
                btnReset.Visibility = Visibility.Hidden;
            }
            AppendLog(simpleText, dftForeground);           
        }
    }

    class WarningException : Exception { public WarningException(string p) : base(p) { } }

    class ErrorException : Exception { public ErrorException(string message) : base(message) { }} 
}
