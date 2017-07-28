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
using RemoteMessagingNS;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RemoteMessaging messenger = null;

        public MainWindow()
        {
            InitializeComponent();
            messenger = new RemoteMessaging("Test app. for MM<->AxelHub");
            messenger.Remote += Interpreter;

        }

       /*  private async void btnRemote_Click(object sender, RoutedEventArgs e)
        {
           if (btnRemote.Content.Equals("Connect"))
            {
                
                rtbListen.AppendText("Awaiting remote requests\n");
                btnRemote.Content = "Disconnect";
                btnRemote.Background = Brushes.LightGreen;
                try
                {
                    await messenger.Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error with remote command: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Closing remote connection");
                messenger.Close();
                btnRemote.Content = "Connect";
                btnRemote.Background = Brushes.LightBlue;
            }
        }*/

        public bool Interpreter(string json)
        {
            rtbListen.AppendText("Msg received: "+ json + "\n");
            return true;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            messenger.sendCommand(tbCommand.Text);
        }
    }
}
