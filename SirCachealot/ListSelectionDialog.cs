using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SirCachealot
{
    public partial class ListSelectionDialog : Form
    {
        public ListSelectionDialog()
        {
            InitializeComponent();
        }

        public void Populate(List<string> items)
        {
            foreach (string item in items)
                listBox1.Items.Add(item);
        }

        public string SelectedItem()
        {
            if (listBox1.SelectedIndex != -1)
                return listBox1.SelectedItem as string;
            else return "";
        }
    }
}