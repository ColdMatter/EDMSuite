<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormProperties
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.DataGridViewProp = New System.Windows.Forms.DataGridView
        CType(Me.DataGridViewProp, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'DataGridViewProp
        '
        Me.DataGridViewProp.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridViewProp.Dock = System.Windows.Forms.DockStyle.Fill
        Me.DataGridViewProp.Location = New System.Drawing.Point(0, 0)
        Me.DataGridViewProp.Name = "DataGridViewProp"
        Me.DataGridViewProp.RowTemplate.Height = 21
        Me.DataGridViewProp.Size = New System.Drawing.Size(435, 315)
        Me.DataGridViewProp.TabIndex = 1
        '
        'FormProperties
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(435, 315)
        Me.Controls.Add(Me.DataGridViewProp)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FormProperties"
        Me.ShowInTaskbar = False
        Me.Text = "Properties"
        CType(Me.DataGridViewProp, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents DataGridViewProp As System.Windows.Forms.DataGridView
End Class
