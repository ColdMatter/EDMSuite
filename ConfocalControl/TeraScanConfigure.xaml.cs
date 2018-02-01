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
using System.Windows.Shapes;

namespace ConfocalControl
{
    /// <summary>
    /// Interaction logic for TeraScanConfigure.xaml
    /// </summary>
    public partial class TeraScanConfigure : Window
    {
        double SPEEDOFLIGHT = 299792458;

        public TeraScanConfigure()
        {
            InitializeComponent();

            teraScanType_ComboBox.Items.Add("medium");
            teraScanType_ComboBox.Items.Add("fine");
            teraScanType_ComboBox.SelectedItem = (string)SolsTiSPlugin.GetController().Settings["TeraScanType"];

            teraScanStart_Numeric.Value = (double)SolsTiSPlugin.GetController().Settings["TeraScanStart"];
            teraScanStop_Numeric.Value = (double)SolsTiSPlugin.GetController().Settings["TeraScanStop"];

            teraScanUnits_ComboBox.SelectedItem = (string)SolsTiSPlugin.GetController().Settings["TeraScanUnits"];
            teraScanRate_ComboBox.SelectedItem = (int)SolsTiSPlugin.GetController().Settings["TeraScanRate"];
        }

        private void teraScanType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            teraScanUnits_ComboBox.Items.Clear();
            switch ((string)teraScanType_ComboBox.SelectedItem)
            {
                case "medium":
                    teraScanUnits_ComboBox.Items.Add("GHz/s");
                    teraScanUnits_ComboBox.SelectedIndex = 0;
                    break;
                case "fine":
                    teraScanUnits_ComboBox.Items.Add("MHz/s");
                    teraScanUnits_ComboBox.Items.Add("GHz/s");
                    teraScanUnits_ComboBox.SelectedIndex = 0;
                    break;
                default:
                    break;
            }
        }

        private void teraScanUnits_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            teraScanRate_ComboBox.Items.Clear();
            switch ((string)teraScanType_ComboBox.SelectedItem)
            {
                case "medium":
                    switch ((string)teraScanUnits_ComboBox.SelectedItem)
                    {
                        case "GHz/s":
                            teraScanRate_ComboBox.Items.Add(100);
                            teraScanRate_ComboBox.Items.Add(50);
                            teraScanRate_ComboBox.Items.Add(20);
                            teraScanRate_ComboBox.Items.Add(15);
                            teraScanRate_ComboBox.Items.Add(10);
                            teraScanRate_ComboBox.Items.Add(5);
                            teraScanRate_ComboBox.Items.Add(2);
                            teraScanRate_ComboBox.Items.Add(1);
                            break;
                        default:
                            break;
                    }
                    break;

                case "fine":
                    switch ((string)teraScanUnits_ComboBox.SelectedItem)
                    {
                        case "GHz/s":
                            teraScanRate_ComboBox.Items.Add(20);
                            teraScanRate_ComboBox.Items.Add(15);
                            teraScanRate_ComboBox.Items.Add(10);
                            teraScanRate_ComboBox.Items.Add(5);
                            teraScanRate_ComboBox.Items.Add(2);
                            teraScanRate_ComboBox.Items.Add(1);
                            break;
                        case "MHz/s":
                            teraScanRate_ComboBox.Items.Add(500);
                            teraScanRate_ComboBox.Items.Add(200);
                            teraScanRate_ComboBox.Items.Add(100);
                            teraScanRate_ComboBox.Items.Add(50);
                            teraScanRate_ComboBox.Items.Add(20);
                            teraScanRate_ComboBox.Items.Add(15);
                            teraScanRate_ComboBox.Items.Add(10);
                            teraScanRate_ComboBox.Items.Add(5);
                            teraScanRate_ComboBox.Items.Add(2);
                            teraScanRate_ComboBox.Items.Add(1);
                            break;
                        default:
                            break;
                    }
                    break;

                default:
                    break;
            }
            teraScanRate_ComboBox.SelectedIndex = 0;
        }

        public void CalculateScanTime()
        {
            double start = teraScanStart_Numeric.Value;
            double stop = teraScanStop_Numeric.Value;
            double totalWidth;
            int numberSeconds;
            if (start < stop && teraScanRate_ComboBox.SelectedIndex >= 0)
            {
                switch ((string)teraScanUnits_ComboBox.SelectedItem)
                {
                    case "GHz/s":
                        totalWidth = SPEEDOFLIGHT * (1 / start - 1 / stop);
                        numberSeconds = Convert.ToInt32(totalWidth / (int)teraScanRate_ComboBox.SelectedItem);
                        scanTimeSec_Label.Content = Convert.ToString(numberSeconds % 60);
                        scanTimeMins_Label.Content = Convert.ToString(Math.Truncate((double)numberSeconds / 60) % 60);
                        scanTimeHours_Label.Content = Convert.ToString(Math.Truncate((double)numberSeconds / 3600));
                        break;
                    case "MHz/s":
                        totalWidth = SPEEDOFLIGHT * (1 / start - 1 / stop) * 1000;
                        numberSeconds = Convert.ToInt32(totalWidth / (int)teraScanRate_ComboBox.SelectedItem);
                        scanTimeSec_Label.Content = Convert.ToString(numberSeconds % 60);
                        scanTimeMins_Label.Content = Convert.ToString(Math.Truncate((double)numberSeconds / 60) % 60);
                        scanTimeHours_Label.Content = Convert.ToString(Math.Truncate((double)numberSeconds / 3600));
                        break;
                    default:
                        break;
                }
            }
        }

        private void teraScanStart_Numeric_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 700 || e.NewValue > 1000)
            {
                teraScanStart_Numeric.Value = e.OldValue;
                return;
            }
            CalculateScanTime();
        }

        private void teraScanStop_Numeric_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 700 || e.NewValue > 1000)
            {
                teraScanStop_Numeric.Value = e.OldValue;
                return;
            }
            CalculateScanTime();
        }

        private void teraScanRate_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CalculateScanTime();
        }

        private void save_Button_Click(object sender, RoutedEventArgs e)
        {
            SolsTiSPlugin.GetController().Settings["TeraScanStart"] = teraScanStart_Numeric.Value;
            SolsTiSPlugin.GetController().Settings["TeraScanStop"] = teraScanStop_Numeric.Value;
            SolsTiSPlugin.GetController().Settings["TeraScanType"] = teraScanType_ComboBox.SelectedItem;
            SolsTiSPlugin.GetController().Settings["TeraScanRate"] = teraScanRate_ComboBox.SelectedItem;
            SolsTiSPlugin.GetController().Settings["TeraScanUnits"] = teraScanUnits_ComboBox.SelectedItem;

            this.Close();
        }

        private void cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
