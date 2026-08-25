
namespace MOTMaster
{
    partial class ParameterWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmbFiles = new System.Windows.Forms.ComboBox();
            this.lblSelectFile = new System.Windows.Forms.Label();
            this.dgvParameters = new System.Windows.Forms.DataGridView();
            this.btnAddRow = new System.Windows.Forms.Button();
            this.lblFolder = new System.Windows.Forms.Label();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewComboBoxColumn();

            ((System.ComponentModel.ISupportInitialize)(this.dgvParameters)).BeginInit();
            this.SuspendLayout();

            // lblSelectFile
            this.lblSelectFile.AutoSize = true;
            this.lblSelectFile.Location = new System.Drawing.Point(12, 15);
            this.lblSelectFile.Text = "Select File:";

            // cmbFiles
            this.cmbFiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFiles.Location = new System.Drawing.Point(90, 12);
            this.cmbFiles.Size = new System.Drawing.Size(300, 21);
            this.cmbFiles.SelectedIndexChanged += new System.EventHandler(this.cmbFiles_SelectedIndexChanged);

            // lblFolder
            this.lblFolder.AutoSize = true;
            this.lblFolder.Location = new System.Drawing.Point(12, 42);
            this.lblFolder.ForeColor = System.Drawing.Color.Gray;
            this.lblFolder.Text = "";

            // colName
            this.colName.HeaderText = "Parameter Name";
            this.colName.Name = "colName";
            this.colName.Width = 200;

            // colValue
            this.colValue.HeaderText = "Value";
            this.colValue.Name = "colValue";
            this.colValue.Width = 200;

            // colType
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.Width = 150;
            this.colType.DisplayStyle = System.Windows.Forms.DataGridViewComboBoxDisplayStyle.ComboBox;

            // dgvParameters
            this.dgvParameters.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[]
            {
                this.colName,
                this.colValue,
                this.colType
            });
            this.dgvParameters.Location = new System.Drawing.Point(12, 60);
            this.dgvParameters.Size = new System.Drawing.Size(760, 450);
            this.dgvParameters.AllowUserToAddRows = false;
            this.dgvParameters.AllowUserToDeleteRows = true;
            this.dgvParameters.RowHeadersWidth = 40;
            this.dgvParameters.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvParameters.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvParameters_CellEndEdit);
            this.dgvParameters.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.dgvParameters_UserDeletedRow);

            // btnAddRow
            this.btnAddRow.Location = new System.Drawing.Point(12, 525);
            this.btnAddRow.Size = new System.Drawing.Size(120, 30);
            this.btnAddRow.Text = "+ Add Parameter";
            this.btnAddRow.Click += new System.EventHandler(this.btnAddRow_Click);

            this.btnDeleteRow = new System.Windows.Forms.Button();

            // btnDeleteRow
            this.btnDeleteRow.Location = new System.Drawing.Point(142, 525);
            this.btnDeleteRow.Size = new System.Drawing.Size(120, 30);
            this.btnDeleteRow.Text = "− Remove Parameter";
            this.btnDeleteRow.Click += new System.EventHandler(this.btnDeleteRow_Click);

            // Add to form controls
            this.Controls.Add(this.btnDeleteRow);


            // Form1
            this.ClientSize = new System.Drawing.Size(784, 571);
            this.Controls.Add(this.lblSelectFile);
            this.Controls.Add(this.cmbFiles);
            this.Controls.Add(this.lblFolder);
            this.Controls.Add(this.dgvParameters);
            this.Controls.Add(this.btnAddRow);
            this.Text = "Parameter File Editor";
            this.Load += new System.EventHandler(this.ParameterWindow_Load);

            ((System.ComponentModel.ISupportInitialize)(this.dgvParameters)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ComboBox cmbFiles;
        private System.Windows.Forms.Label lblSelectFile;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.DataGridView dgvParameters;
        private System.Windows.Forms.Button btnAddRow;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
        private System.Windows.Forms.DataGridViewComboBoxColumn colType;
        private System.Windows.Forms.Button btnDeleteRow;
    }

    #endregion
}