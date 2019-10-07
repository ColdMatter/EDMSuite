using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SirCachealot
{
    public partial class GateListDialog : Form
    {
        public GateListDialog()
        {
            InitializeComponent();
        }

        public void Populate(List<string[]> items)
        {
            DataTable dataTable = new DataTable();
            foreach (string[] row in items) dataTable.Rows.Add(row);
            this.gateDataView.DataSource = dataTable;
        }
    }
}
