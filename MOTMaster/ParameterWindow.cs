using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using DAQ.Environment;

namespace MOTMaster
{
    public partial class ParameterWindow : Form
    {
        private readonly string _parameterFolder = (string)Environs.FileSystem.Paths["scriptListPath"];

        // Supported parameter types shown in the Type column dropdown
        private readonly Type[] _parameterTypes =
        {
            typeof(int), typeof(double)
        };

        private string _currentFilePath = null;
        private bool _isSuppressingEvents = false;

        // ---------------------------------------------------------------
        public ParameterWindow()
        {
            InitializeComponent();
        }

        // ================================================================
        // FORM LOAD
        // ================================================================
        private void ParameterWindow_Load(object sender, EventArgs e)
        {
            // Populate the Type combobox column items
            foreach (var t in _parameterTypes)
                colType.Items.Add(t.FullName);

            // Ensure the folder exists
            if (!Directory.Exists(_parameterFolder))
            {
                Directory.CreateDirectory(_parameterFolder);
                MessageBox.Show(
                    $"Parameter folder created at:\n{_parameterFolder}\n\nAdd.txt files to this folder.",
                    "Folder Created",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            lblFolder.Text = $"Folder: {_parameterFolder}";
            LoadFileList();
        }

        // ================================================================
        // FILE LIST
        // ================================================================
        private void LoadFileList()
        {
            cmbFiles.Items.Clear();

            var files = Directory.GetFiles(_parameterFolder, "*.txt");
            foreach (var file in files)
                cmbFiles.Items.Add(Path.GetFileName(file));

            if (cmbFiles.Items.Count > 0)
                cmbFiles.SelectedIndex = 0;
            else
                dgvParameters.Rows.Clear();
        }

        private void cmbFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFiles.SelectedItem == null) return;

            _currentFilePath = Path.Combine(_parameterFolder, cmbFiles.SelectedItem.ToString());
            LoadParametersFromFile();
        }

        // ================================================================
        // LOAD PARAMETERS INTO GRID
        // ================================================================
        private void LoadParametersFromFile()
        {
            _isSuppressingEvents = true;
            dgvParameters.Rows.Clear();

            var entries = ParameterFileManager.ReadFile(_currentFilePath, _parameterTypes[0]);

            foreach (var entry in entries)
            {
                int rowIndex = dgvParameters.Rows.Add();
                var row = dgvParameters.Rows[rowIndex];

                row.Cells["colName"].Value = entry.Name;
                row.Cells["colValue"].Value = entry.Value;
                row.Cells["colType"].Value = entry.Type.FullName;
            }

            _isSuppressingEvents = false;
        }

        // ================================================================
        // SAVE PARAMETERS FROM GRID
        // ================================================================
        private void SaveParametersToFile()
        {
            if (_currentFilePath == null) return;

            var entries = new List<ParameterEntry>();

            foreach (DataGridViewRow row in dgvParameters.Rows)
            {
                var name = row.Cells["colName"].Value?.ToString() ?? string.Empty;
                var value = row.Cells["colValue"].Value?.ToString() ?? string.Empty;
                var typeStr = row.Cells["colType"].Value?.ToString() ?? _parameterTypes[0].FullName;

                // Skip completely empty rows
                if (string.IsNullOrWhiteSpace(name) &&
                    string.IsNullOrWhiteSpace(value))
                    continue;

                // Resolve directly — fall back to first type if unrecognised
                Type resolvedType = Type.GetType(typeStr) ?? _parameterTypes[0];

                entries.Add(new ParameterEntry(name, value, resolvedType));
            }

            ParameterFileManager.WriteFile(_currentFilePath, entries);
        }

        // ================================================================
        // CELL EDIT COMPLETE — auto-save on every edit
        // ================================================================
        private void dgvParameters_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_isSuppressingEvents) return;
            SaveParametersToFile();
        }

        // ================================================================
        // ADD NEW ROW
        // ================================================================
        private void btnAddRow_Click(object sender, EventArgs e)
        {
            if (_currentFilePath == null)
            {
                MessageBox.Show(
                    "Please select a file first.",
                    "No File Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            int rowIndex = dgvParameters.Rows.Add();
            var row = dgvParameters.Rows[rowIndex];

            row.Cells["colName"].Value = "NewParameter";
            row.Cells["colValue"].Value = "0";
            row.Cells["colType"].Value = _parameterTypes[0].FullName; // default: System.Int32

            // Scroll to and select the new row
            dgvParameters.ClearSelection();
            dgvParameters.CurrentCell = row.Cells["colName"];
            dgvParameters.BeginEdit(true);

            SaveParametersToFile();
        }

        // ================================================================
        // REMOVE ROW — Button
        // ================================================================
        private void btnDeleteRow_Click(object sender, EventArgs e)
        {
            DeleteSelectedRows();
        }

        // ================================================================
        // REMOVE ROW — Keyboard Delete key (hooks into built-in DGV behaviour)
        // ================================================================
        private void dgvParameters_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            SaveParametersToFile();
        }

        // ================================================================
        // SHARED DELETE LOGIC
        // ================================================================
        private void DeleteSelectedRows()
        {
            if (dgvParameters.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select one or more rows to remove.",
                    "No Row Selected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to remove {dgvParameters.SelectedRows.Count} parameter(s)?",
                "Confirm Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            // Iterate in reverse to avoid index shifting during removal
            foreach (DataGridViewRow row in dgvParameters.SelectedRows)
            {
                if (!row.IsNewRow)
                    dgvParameters.Rows.Remove(row);
            }

            SaveParametersToFile();
        }
    }
}
