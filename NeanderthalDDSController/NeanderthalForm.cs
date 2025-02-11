using System.Windows.Forms;
using System;
using System.IO;
using System.Xml.Serialization;

namespace NeanderthalDDSController
{
    public partial class NeanderthalForm : Form
    {
        public List<List<double>> par = new List<List<double>>();

        public string eventName;

        public Controller controller;

        public NeanderthalForm()
        {
            InitializeComponent();
        }



        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        private Button button_add;
        private Button button_delete;


        private void Form_Load(object sender, EventArgs e)
        {

        }

        private void Form_closing(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }


        public void timer_tick(object sender, EventArgs e)
        {
            updatePatternList();
        }

        public void updatePatternList()
        {
            patternGridView.Rows.Clear();
            foreach (string key in controller.patternList.Keys)
            {
                object[] item = {key, controller.patternList[key][0][0], controller.patternList[key][1][0], controller.patternList[key][1][1], controller.patternList[key][1][2], controller.patternList[key][1][3],
                controller.patternList[key][2][0], controller.patternList[key][2][1], controller.patternList[key][2][2], controller.patternList[key][2][3],
                controller.patternList[key][3][0], controller.patternList[key][3][1], controller.patternList[key][3][2], controller.patternList[key][3][3],
                controller.patternList[key][4][0], controller.patternList[key][4][1], controller.patternList[key][4][2], controller.patternList[key][4][3]};

                patternGridView.Rows.Add(item);
            }



        }

        private void button_add_click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox_eventName.Text))
                {
                    throw new ArgumentException("Event name cannot be empty.");
                }

                eventName = textBox_eventName.Text;

                //MessageBox.Show("Event added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            List<double> timeDelay = new List<double>();
            List<double> freqs = new List<double>();
            List<double> amps = new List<double>();
            List<double> freq_slopes = new List<double>();
            List<double> amp_slpoes = new List<double>();
            List<List<double>> parList = new List<List<double>>();

            try
            {
                timeDelay.Add(Convert.ToDouble(textBox_eventTime.Text));

                freqs.Add(Convert.ToDouble(textBox_ch0_freq.Text));
                freqs.Add(Convert.ToDouble(textBox_ch1_freq.Text));
                freqs.Add(Convert.ToDouble(textBox_ch2_freq.Text));
                freqs.Add(Convert.ToDouble(textBox_ch3_freq.Text));

                amps.Add(Convert.ToDouble(textBox_ch0_amp.Text));
                amps.Add(Convert.ToDouble(textBox_ch1_amp.Text));
                amps.Add(Convert.ToDouble(textBox_ch2_amp.Text));
                amps.Add(Convert.ToDouble(textBox_ch3_amp.Text));

                freq_slopes.Add(Convert.ToDouble(textBox_ch0_freq_slope.Text));
                freq_slopes.Add(Convert.ToDouble(textBox_ch1_freq_slope.Text));
                freq_slopes.Add(Convert.ToDouble(textBox_ch2_freq_slope.Text));
                freq_slopes.Add(Convert.ToDouble(textBox_ch3_freq_slope.Text));

                amp_slpoes.Add(Convert.ToDouble(textBox_ch0_amp_slope.Text));
                amp_slpoes.Add(Convert.ToDouble(textBox_ch1_amp_slope.Text));
                amp_slpoes.Add(Convert.ToDouble(textBox_ch2_amp_slope.Text));
                amp_slpoes.Add(Convert.ToDouble(textBox_ch3_amp_slope.Text));
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid input. Please enter valid numeric values.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            controller.addParToPatternList(eventName, timeDelay, freqs, amps, freq_slopes, amp_slpoes);

            updatePatternList();
            updatePatternIndicator();
        }

        public void updatePatternIndicator()
        {
            bool isRunning = controller.isPatternRunning;
            if (isRunning)
            {
                labelRunIndicator.Text = "Pattern Running";
                labelRunIndicator.ForeColor = Color.Green;
            }

            else
            {
                labelRunIndicator.Text = "Pattern Stopped";
                labelRunIndicator.ForeColor = Color.Red;
            }
        }

        private void button_Delete_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if any row is selected
                if (patternGridView.SelectedRows.Count > 0)
                {

                    foreach (DataGridViewRow row in patternGridView.SelectedRows)
                    {
                        try
                        {
                            // Ensure the cell is not null before converting to string
                            if (row.Cells[0].Value != null)
                            {
                                string str = row.Cells[0].Value.ToString();

                                // Call the method to remove the item from the list using the key
                                controller.removeParFromPatternList(str);
                            }
                            else
                            {
                                throw new NullReferenceException("The cell value is null.");
                            }
                        }
                        catch (NullReferenceException ex)
                        {
                            MessageBox.Show(ex.Message, "Null Value Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        catch (ArgumentException ex)
                        {
                            MessageBox.Show(ex.Message, "Invalid Value Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        catch (Exception ex)
                        {
                            // Catch any unexpected errors
                            MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }


                }
                else
                {
                    MessageBox.Show("Please select a row to delete.", "No Row Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            updatePatternList();
        }

        private void button_start_pattern_clicked(object sender, EventArgs e)
        {
            controller.startRepetitivePattern();
            updatePatternIndicator();
        }

        private void button_stop_pattern_clicked(object sender, EventArgs e)
        {
            controller.stopPattern();
            updatePatternIndicator();
        }

        private void textBox_pattern_length_changed(object sender, EventArgs e)
        {
            if (int.TryParse(textBoxPatternLength.Text, out int number))
            {
                controller.patternLength = number;
            }
            else
            {
                controller.patternLength = 300;
            }
        }

        private void save_pattern_clicked(object sender, EventArgs e)
        {

            SaveDataToFile(controller);

        }

        private void load_pattern_clicked(object sender, EventArgs e)
        {
            string filename = LoadDataFromFile(controller);
            updatePatternList();
            lablePatternName.Text = filename;
        }



        public static void SaveDataToFile(Controller controller)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                saveFileDialog.Title = "Save Data";
                saveFileDialog.DefaultExt = "csv";
                saveFileDialog.AddExtension = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    // Try to delete file if it exists (to free up any locks)
                    if (File.Exists(filePath))
                    {
                        try
                        {
                            File.Delete(filePath); // Delete if not in use
                        }
                        catch (IOException)
                        {
                            MessageBox.Show("File is currently in use. Close it and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Open with FileStream to allow other processes to access it
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read | FileShare.Delete))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        foreach (string key in controller.patternList.Keys)
                        {
                            object[] item =
                            {
                            key, controller.patternList[key][0][0],
                            controller.patternList[key][1][0], controller.patternList[key][1][1], controller.patternList[key][1][2], controller.patternList[key][1][3],
                            controller.patternList[key][2][0], controller.patternList[key][2][1], controller.patternList[key][2][2], controller.patternList[key][2][3],
                            controller.patternList[key][3][0], controller.patternList[key][3][1], controller.patternList[key][3][2], controller.patternList[key][3][3],
                            controller.patternList[key][4][0], controller.patternList[key][4][1], controller.patternList[key][4][2], controller.patternList[key][4][3]
                        };

                            writer.WriteLine(string.Join(",", item)); // Write data as CSV
                        }
                    }

                    MessageBox.Show("File saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public static string LoadDataFromFile(Controller controller)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"; // Set file types
                openFileDialog.Title = "Open Data File"; // Window title

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = openFileDialog.FileName;
                    using (FileStream fileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        controller.patternList.Clear(); // Clear existing data before loading

                        while (!reader.EndOfStream)
                        {
                            string line = reader.ReadLine();
                            string[] values = line.Split(',');

                            if (values.Length != 18) continue; // Ensure correct format

                            string key = values[0]; // First column is the key
                            List<List<double>> pattern = new List<List<double>>
                        {
                            new List<double> { double.Parse(values[1]) },
                            new List<double> { double.Parse(values[2]), double.Parse(values[3]), double.Parse(values[4]), double.Parse(values[5]) },
                            new List<double> { double.Parse(values[6]), double.Parse(values[7]), double.Parse(values[8]), double.Parse(values[9]) },
                            new List<double> { double.Parse(values[10]), double.Parse(values[11]), double.Parse(values[12]), double.Parse(values[13]) },
                            new List<double> { double.Parse(values[14]), double.Parse(values[15]), double.Parse(values[16]), double.Parse(values[17]) }
                        };

                            controller.patternList[key] = pattern;
                        }
                    }

                    //MessageBox.Show("File loaded successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return fileName;
                }
                return null;
            }
        }

        #region FormatTsar
        private void TextBox_Amp_TextChanged(object sender, EventArgs e)
        {
            TextBox currentTextBox = sender as TextBox;

            // Ensure the TextBox is not null
            if (currentTextBox != null)
            {
                // Try to parse the input as a double
                if (double.TryParse(currentTextBox.Text, out double value))
                {
                    // Check if the value is between 0.0 and 1.0
                    if (value < 0.0 || value > 1.0)
                    {
                        MessageBox.Show("Please enter a value between 0.0 and 1.0.", "Invalid Value", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        currentTextBox.Text = ""; // Clear the invalid input
                    }
                }
                else if (!string.IsNullOrEmpty(currentTextBox.Text))
                {
                    MessageBox.Show("Please enter a valid number.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    currentTextBox.Text = ""; // Clear invalid input
                }
            }
        }

        #endregion

        private void CavemanForm_Load(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void Menu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
