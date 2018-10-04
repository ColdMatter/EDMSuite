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

            teraScanRepeatType_ComboBox.Items.Add("single");
            teraScanRepeatType_ComboBox.Items.Add("multi");
            teraScanRepeatType_ComboBox.SelectedItem = (string)SolsTiSPlugin.GetController().Settings["TeraScanRepeatType"];

            teraScanType_ComboBox.Items.Add("medium");
            teraScanType_ComboBox.Items.Add("fine");
            teraScanType_ComboBox.SelectedItem = (string)SolsTiSPlugin.GetController().Settings["TeraScanType"];

            teraScanStart_Numeric.Value = (double)SolsTiSPlugin.GetController().Settings["TeraScanStart"];
            teraScanStop_Numeric.Value = (double)SolsTiSPlugin.GetController().Settings["TeraScanStop"];

            teraScanUnits_ComboBox.SelectedItem = (string)SolsTiSPlugin.GetController().Settings["TeraScanUnits"];
            teraScanRate_ComboBox.SelectedItem = (int)SolsTiSPlugin.GetController().Settings["TeraScanRate"];

            for (int i = 0; i < ((double[])SolsTiSPlugin.GetController().Settings["TeraScanMultiLambda"]).Length; i++)
            {
                multi_wavelength_ListBox.Items.Add(((double[])SolsTiSPlugin.GetController().Settings["TeraScanMultiLambda"])[i]);
            }

            multiScanRange_Numeric.Value = (double)SolsTiSPlugin.GetController().Settings["TeraScanMultiRange"];
        }

        private void teraScanRepeatType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((string)teraScanRepeatType_ComboBox.SelectedItem)
            {
                case "single":
                    teraScanStart_Numeric.IsEnabled = true;
                    teraScanStop_Numeric.IsEnabled = true;
                    multi_wavelength_ListBox.IsEnabled = false;
                    wavelengthToAdd_Numeric.IsEnabled = false;
                    addToList_Button.IsEnabled = false;
                    removeFromList_Button.IsEnabled = false;
                    multiScanRange_Numeric.IsEnabled = false;
                    clearList_Button.IsEnabled = false;
                    listImport_Button.IsEnabled = false;
                    listImport_TextBox.IsEnabled = false;
                    break;
                case "multi":
                    teraScanStart_Numeric.IsEnabled = false;
                    teraScanStop_Numeric.IsEnabled = false;
                    multi_wavelength_ListBox.IsEnabled = true;
                    wavelengthToAdd_Numeric.IsEnabled = true;
                    addToList_Button.IsEnabled = true;
                    removeFromList_Button.IsEnabled = true;
                    multiScanRange_Numeric.IsEnabled = true;
                    clearList_Button.IsEnabled = true;
                    listImport_Button.IsEnabled = true;
                    listImport_TextBox.IsEnabled = true;
                    break;
                default:
                    break;
            }
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
            int numberSeconds = 0;

            switch ((string)teraScanRepeatType_ComboBox.SelectedItem)
            {
                case "single":
                    double start = teraScanStart_Numeric.Value;
                    double stop = teraScanStop_Numeric.Value;
                    double totalWidth;
                    if (start < stop && teraScanRate_ComboBox.SelectedIndex >= 0)
                    {
                        switch ((string)teraScanUnits_ComboBox.SelectedItem)
                        {
                            case "GHz/s":
                                totalWidth = SPEEDOFLIGHT * (1 / start - 1 / stop);
                                numberSeconds = Convert.ToInt32(totalWidth / (int)teraScanRate_ComboBox.SelectedItem);
                                break;
                            case "MHz/s":
                                totalWidth = SPEEDOFLIGHT * (1 / start - 1 / stop) * 1000;
                                numberSeconds = Convert.ToInt32(totalWidth / (int)teraScanRate_ComboBox.SelectedItem);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case "multi":
                    numberSeconds = Convert.ToInt32(multi_wavelength_ListBox.Items.Count * multiScanRange_Numeric.Value);
                    break;
                default:
                    break;
            }

            scanTimeSec_Label.Content = Convert.ToString(numberSeconds % 60);
            scanTimeMins_Label.Content = Convert.ToString(Math.Truncate((double)numberSeconds / 60) % 60);
            scanTimeHours_Label.Content = Convert.ToString(Math.Truncate((double)numberSeconds / 3600));
        }

        private void teraScanStart_Numeric_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 700 || e.NewValue > 1030)
            {
                teraScanStart_Numeric.Value = e.OldValue;
                return;
            }
            CalculateScanTime();
        }

        private void teraScanStop_Numeric_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            if (e.NewValue < 700 || e.NewValue > 1030)
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

        private void addToList_Button_Click(object sender, RoutedEventArgs e)
        {
            if (wavelengthToAdd_Numeric.Value < 700 || wavelengthToAdd_Numeric.Value > 1030)
            {
                MessageBox.Show("Lambda must be between 700nm and 1030nm");
            }
            else
            {
                multi_wavelength_ListBox.Items.Add(wavelengthToAdd_Numeric.Value);
            }

            CalculateScanTime();
        }

        private void removeFromList_Button_Click(object sender, RoutedEventArgs e)
        {
            if (multi_wavelength_ListBox.SelectedIndex >= 0)
            {
                double key = (double)multi_wavelength_ListBox.SelectedValue;
                multi_wavelength_ListBox.Items.Remove(key);
            }

            CalculateScanTime();
        }

        private void clearList_Button_Click(object sender, RoutedEventArgs e)
        {
            multi_wavelength_ListBox.Items.Clear();
            CalculateScanTime();
        }

        private void listImport_Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                double[] doubles = listImport_TextBox.Text.Split(',').Select(Double.Parse).ToArray();
                for (int i = 0; i < doubles.Length; i++)
                {
                    double value = doubles[i];
                    if (value >= 700 || value <= 1030)
                    {
                        multi_wavelength_ListBox.Items.Add(value);
                    }
                }
                CalculateScanTime();
            }
            catch (Exception e1)
            {
                MessageBox.Show("Caught exception: " + e1.Message);
            }
        }

        private void multiScanRange_Numeric_ValueChanged(object sender, NationalInstruments.Controls.ValueChangedEventArgs<double> e)
        {
            CalculateScanTime();
        }

        private void save_Button_Click(object sender, RoutedEventArgs e)
        {
            SolsTiSPlugin.GetController().Settings["TeraScanRepeatType"] = teraScanRepeatType_ComboBox.SelectedItem;
            SolsTiSPlugin.GetController().Settings["TeraScanType"] = teraScanType_ComboBox.SelectedItem;
            SolsTiSPlugin.GetController().Settings["TeraScanStart"] = teraScanStart_Numeric.Value;
            SolsTiSPlugin.GetController().Settings["TeraScanStop"] = teraScanStop_Numeric.Value;
            SolsTiSPlugin.GetController().Settings["TeraScanRate"] = teraScanRate_ComboBox.SelectedItem;
            SolsTiSPlugin.GetController().Settings["TeraScanUnits"] = teraScanUnits_ComboBox.SelectedItem;

            double[] broadcastMultiLambda = new double[multi_wavelength_ListBox.Items.Count];
            for (int i = 0; i < multi_wavelength_ListBox.Items.Count; i++)
			{
			    broadcastMultiLambda[i] = (double)multi_wavelength_ListBox.Items[i];
			}
            Array.Sort(broadcastMultiLambda);
            SolsTiSPlugin.GetController().Settings["TeraScanMultiLambda"] = broadcastMultiLambda;

            SolsTiSPlugin.GetController().Settings["TeraScanMultiRange"] = multiScanRange_Numeric.Value;

            this.Close();
        }

        private void cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
