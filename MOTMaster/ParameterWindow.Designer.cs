
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
            this.lvParameters = new System.Windows.Forms.ListView();
            this.colName = new System.Windows.Forms.ColumnHeader();
            this.colValue = new System.Windows.Forms.ColumnHeader();
            this.colType = new System.Windows.Forms.ColumnHeader();
            this.btnAddRow = new System.Windows.Forms.Button();
            this.btnDeleteRow = new System.Windows.Forms.Button();
            this.btnNewGroup = new System.Windows.Forms.Button();
            this.lblTargetGroup = new System.Windows.Forms.Label();
            this.cmbTargetGroup = new System.Windows.Forms.ComboBox();
            this.lblFolder = new System.Windows.Forms.Label();

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
            this.colName.Text = "Parameter Name";
            this.colName.Width = 220;

            // colValue
            this.colValue.Text = "Value";
            this.colValue.Width = 200;

            // colType
            this.colType.Text = "Type";
            this.colType.Width = 150;

            // lvParameters
            this.lvParameters.Columns.AddRange(new System.Windows.Forms.ColumnHeader[]
            {
                this.colName,
                this.colValue,
                this.colType
            });
            this.lvParameters.Location = new System.Drawing.Point(12, 60);
            this.lvParameters.Size = new System.Drawing.Size(760, 450);
            this.lvParameters.View = System.Windows.Forms.View.Details;
            this.lvParameters.FullRowSelect = true;
            this.lvParameters.GridLines = true;
            this.lvParameters.ShowGroups = true;
            this.lvParameters.MultiSelect = true;
            this.lvParameters.HideSelection = false;
            this.lvParameters.AllowDrop = true;
            this.lvParameters.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvParameters_MouseDoubleClick);
            this.lvParameters.MouseDown += new System.Windows.Forms.MouseEventHandler(this.lvParameters_MouseDown);
            this.lvParameters.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.lvParameters_ItemDrag);
            this.lvParameters.DragEnter += new System.Windows.Forms.DragEventHandler(this.lvParameters_DragEnter);
            this.lvParameters.DragDrop += new System.Windows.Forms.DragEventHandler(this.lvParameters_DragDrop);
            this.lvParameters.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lvParameters_KeyDown);

            // btnAddRow
            this.btnAddRow.Location = new System.Drawing.Point(12, 525);
            this.btnAddRow.Size = new System.Drawing.Size(120, 30);
            this.btnAddRow.Text = "+ Add Parameter";
            this.btnAddRow.Click += new System.EventHandler(this.btnAddRow_Click);

            // btnDeleteRow
            this.btnDeleteRow.Location = new System.Drawing.Point(142, 525);
            this.btnDeleteRow.Size = new System.Drawing.Size(140, 30);
            this.btnDeleteRow.Text = "− Remove Parameter";
            this.btnDeleteRow.Click += new System.EventHandler(this.btnDeleteRow_Click);

            // btnNewGroup
            this.btnNewGroup.Location = new System.Drawing.Point(292, 525);
            this.btnNewGroup.Size = new System.Drawing.Size(120, 30);
            this.btnNewGroup.Text = "+ New Group";
            this.btnNewGroup.Click += new System.EventHandler(this.btnNewGroup_Click);

            // lblTargetGroup
            this.lblTargetGroup.AutoSize = true;
            this.lblTargetGroup.Location = new System.Drawing.Point(422, 533);
            this.lblTargetGroup.Text = "Add to group:";

            // cmbTargetGroup
            this.cmbTargetGroup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTargetGroup.Location = new System.Drawing.Point(510, 529);
            this.cmbTargetGroup.Size = new System.Drawing.Size(200, 21);

            // Form1
            this.ClientSize = new System.Drawing.Size(784, 571);
            this.Controls.Add(this.lblSelectFile);
            this.Controls.Add(this.cmbFiles);
            this.Controls.Add(this.lblFolder);
            this.Controls.Add(this.lvParameters);
            this.Controls.Add(this.btnAddRow);
            this.Controls.Add(this.btnDeleteRow);
            this.Controls.Add(this.btnNewGroup);
            this.Controls.Add(this.lblTargetGroup);
            this.Controls.Add(this.cmbTargetGroup);
            this.Text = "Parameter File Editor";
            this.Load += new System.EventHandler(this.ParameterWindow_Load);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.ComboBox cmbFiles;
        private System.Windows.Forms.Label lblSelectFile;
        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.ListView lvParameters;
        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colValue;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.Button btnAddRow;
        private System.Windows.Forms.Button btnDeleteRow;
        private System.Windows.Forms.Button btnNewGroup;
        private System.Windows.Forms.Label lblTargetGroup;
        private System.Windows.Forms.ComboBox cmbTargetGroup;

        #endregion
    }
}
