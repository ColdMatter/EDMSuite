namespace SirCachealot
{
    partial class GateListDialog
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
            this.newGateButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.gateDataView = new System.Windows.Forms.DataGridView();
            this.GateName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StartTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.EndTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Integrate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gateDataView)).BeginInit();
            this.SuspendLayout();
            // 
            // newGateButton
            // 
            this.newGateButton.Location = new System.Drawing.Point(12, 168);
            this.newGateButton.Name = "newGateButton";
            this.newGateButton.Size = new System.Drawing.Size(75, 23);
            this.newGateButton.TabIndex = 1;
            this.newGateButton.Text = "New gate";
            this.newGateButton.UseVisualStyleBackColor = true;
            // 
            // deleteButton
            // 
            this.deleteButton.Location = new System.Drawing.Point(93, 168);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            // 
            // gateDataView
            // 
            this.gateDataView.AllowUserToOrderColumns = true;
            this.gateDataView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gateDataView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GateName,
            this.StartTime,
            this.EndTime,
            this.Integrate});
            this.gateDataView.Location = new System.Drawing.Point(12, 12);
            this.gateDataView.Name = "gateDataView";
            this.gateDataView.Size = new System.Drawing.Size(444, 150);
            this.gateDataView.TabIndex = 3;
            // 
            // GateName
            // 
            this.GateName.HeaderText = "Gate name";
            this.GateName.Name = "GateName";
            // 
            // StartTime
            // 
            this.StartTime.HeaderText = "Start time";
            this.StartTime.Name = "StartTime";
            // 
            // EndTime
            // 
            this.EndTime.HeaderText = "End time";
            this.EndTime.Name = "EndTime";
            // 
            // Integrate
            // 
            this.Integrate.HeaderText = "Integrate?";
            this.Integrate.Name = "Integrate";
            // 
            // GateListDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(469, 393);
            this.Controls.Add(this.gateDataView);
            this.Controls.Add(this.deleteButton);
            this.Controls.Add(this.newGateButton);
            this.Name = "GateListDialog";
            this.Text = "GateListDialog";
            ((System.ComponentModel.ISupportInitialize)(this.gateDataView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button newGateButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.DataGridView gateDataView;
        private System.Windows.Forms.DataGridViewTextBoxColumn GateName;
        private System.Windows.Forms.DataGridViewTextBoxColumn StartTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn EndTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn Integrate;
    }
}