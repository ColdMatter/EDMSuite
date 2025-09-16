<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormMain
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
        Me.LabelStatus = New System.Windows.Forms.Label
        Me.LabelLutMin = New System.Windows.Forms.Label
        Me.LabelLutMax = New System.Windows.Forms.Label
        Me.EditLutMin = New System.Windows.Forms.TextBox
        Me.EditLutMax = New System.Windows.Forms.TextBox
        Me.PushAsterisk = New System.Windows.Forms.Button
        Me.HScrollLutMin = New System.Windows.Forms.HScrollBar
        Me.HScrollLutMax = New System.Windows.Forms.HScrollBar
        Me.PicDisplay = New System.Windows.Forms.PictureBox
        Me.PushInit = New System.Windows.Forms.Button
        Me.PushOpen = New System.Windows.Forms.Button
        Me.PushInfo = New System.Windows.Forms.Button
        Me.PushProperties = New System.Windows.Forms.Button
        Me.PushSnap = New System.Windows.Forms.Button
        Me.PushLive = New System.Windows.Forms.Button
        Me.PushFireTrigger = New System.Windows.Forms.Button
        Me.PushIdle = New System.Windows.Forms.Button
        Me.PushBufRelease = New System.Windows.Forms.Button
        Me.PushClose = New System.Windows.Forms.Button
        Me.PushUninit = New System.Windows.Forms.Button
        CType(Me.PicDisplay, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LabelStatus
        '
        Me.LabelStatus.BackColor = System.Drawing.SystemColors.Control
        Me.LabelStatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.LabelStatus.Cursor = System.Windows.Forms.Cursors.Default
        Me.LabelStatus.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LabelStatus.Location = New System.Drawing.Point(14, 14)
        Me.LabelStatus.Name = "LabelStatus"
        Me.LabelStatus.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LabelStatus.Size = New System.Drawing.Size(517, 17)
        Me.LabelStatus.TabIndex = 0
        '
        'LabelLutMin
        '
        Me.LabelLutMin.BackColor = System.Drawing.SystemColors.Control
        Me.LabelLutMin.Cursor = System.Windows.Forms.Cursors.Default
        Me.LabelLutMin.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LabelLutMin.Location = New System.Drawing.Point(14, 60)
        Me.LabelLutMin.Name = "LabelLutMin"
        Me.LabelLutMin.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LabelLutMin.Size = New System.Drawing.Size(52, 17)
        Me.LabelLutMin.TabIndex = 4
        Me.LabelLutMin.Text = "LUT Min"
        '
        'LabelLutMax
        '
        Me.LabelLutMax.BackColor = System.Drawing.SystemColors.Control
        Me.LabelLutMax.Cursor = System.Windows.Forms.Cursors.Default
        Me.LabelLutMax.ForeColor = System.Drawing.SystemColors.ControlText
        Me.LabelLutMax.Location = New System.Drawing.Point(14, 38)
        Me.LabelLutMax.Name = "LabelLutMax"
        Me.LabelLutMax.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.LabelLutMax.Size = New System.Drawing.Size(52, 17)
        Me.LabelLutMax.TabIndex = 1
        Me.LabelLutMax.Text = "LUT Max"
        '
        'EditLutMin
        '
        Me.EditLutMin.AcceptsReturn = True
        Me.EditLutMin.BackColor = System.Drawing.SystemColors.Window
        Me.EditLutMin.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.EditLutMin.ForeColor = System.Drawing.SystemColors.WindowText
        Me.EditLutMin.Location = New System.Drawing.Point(67, 60)
        Me.EditLutMin.MaxLength = 0
        Me.EditLutMin.Name = "EditLutMin"
        Me.EditLutMin.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.EditLutMin.Size = New System.Drawing.Size(50, 20)
        Me.EditLutMin.TabIndex = 5
        '
        'EditLutMax
        '
        Me.EditLutMax.AcceptsReturn = True
        Me.EditLutMax.BackColor = System.Drawing.SystemColors.Window
        Me.EditLutMax.Cursor = System.Windows.Forms.Cursors.IBeam
        Me.EditLutMax.ForeColor = System.Drawing.SystemColors.WindowText
        Me.EditLutMax.Location = New System.Drawing.Point(67, 38)
        Me.EditLutMax.MaxLength = 0
        Me.EditLutMax.Name = "EditLutMax"
        Me.EditLutMax.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.EditLutMax.Size = New System.Drawing.Size(50, 20)
        Me.EditLutMax.TabIndex = 2
        '
        'PushAsterisk
        '
        Me.PushAsterisk.BackColor = System.Drawing.SystemColors.Control
        Me.PushAsterisk.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushAsterisk.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushAsterisk.Location = New System.Drawing.Point(484, 40)
        Me.PushAsterisk.Name = "PushAsterisk"
        Me.PushAsterisk.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushAsterisk.Size = New System.Drawing.Size(34, 34)
        Me.PushAsterisk.TabIndex = 7
        Me.PushAsterisk.Text = "*"
        Me.PushAsterisk.UseVisualStyleBackColor = False
        '
        'HScrollLutMin
        '
        Me.HScrollLutMin.Cursor = System.Windows.Forms.Cursors.Default
        Me.HScrollLutMin.LargeChange = 1
        Me.HScrollLutMin.Location = New System.Drawing.Point(122, 60)
        Me.HScrollLutMin.Maximum = 32767
        Me.HScrollLutMin.Name = "HScrollLutMin"
        Me.HScrollLutMin.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.HScrollLutMin.Size = New System.Drawing.Size(354, 17)
        Me.HScrollLutMin.TabIndex = 6
        Me.HScrollLutMin.TabStop = True
        '
        'HScrollLutMax
        '
        Me.HScrollLutMax.Cursor = System.Windows.Forms.Cursors.Default
        Me.HScrollLutMax.LargeChange = 1
        Me.HScrollLutMax.Location = New System.Drawing.Point(122, 38)
        Me.HScrollLutMax.Maximum = 32767
        Me.HScrollLutMax.Name = "HScrollLutMax"
        Me.HScrollLutMax.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.HScrollLutMax.Size = New System.Drawing.Size(354, 17)
        Me.HScrollLutMax.TabIndex = 3
        Me.HScrollLutMax.TabStop = True
        '
        'PicDisplay
        '
        Me.PicDisplay.BackColor = System.Drawing.SystemColors.Control
        Me.PicDisplay.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D
        Me.PicDisplay.Cursor = System.Windows.Forms.Cursors.Default
        Me.PicDisplay.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PicDisplay.Location = New System.Drawing.Point(139, 95)
        Me.PicDisplay.Name = "PicDisplay"
        Me.PicDisplay.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PicDisplay.Size = New System.Drawing.Size(392, 463)
        Me.PicDisplay.TabIndex = 8
        Me.PicDisplay.TabStop = False
        '
        'PushInit
        '
        Me.PushInit.BackColor = System.Drawing.SystemColors.Control
        Me.PushInit.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushInit.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushInit.Location = New System.Drawing.Point(14, 95)
        Me.PushInit.Name = "PushInit"
        Me.PushInit.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushInit.Size = New System.Drawing.Size(113, 33)
        Me.PushInit.TabIndex = 8
        Me.PushInit.Text = "Init"
        Me.PushInit.UseVisualStyleBackColor = False
        '
        'PushOpen
        '
        Me.PushOpen.BackColor = System.Drawing.SystemColors.Control
        Me.PushOpen.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushOpen.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushOpen.Location = New System.Drawing.Point(14, 134)
        Me.PushOpen.Name = "PushOpen"
        Me.PushOpen.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushOpen.Size = New System.Drawing.Size(113, 33)
        Me.PushOpen.TabIndex = 9
        Me.PushOpen.Text = "Open"
        Me.PushOpen.UseVisualStyleBackColor = False
        '
        'PushInfo
        '
        Me.PushInfo.BackColor = System.Drawing.SystemColors.Control
        Me.PushInfo.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushInfo.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushInfo.Location = New System.Drawing.Point(14, 173)
        Me.PushInfo.Name = "PushInfo"
        Me.PushInfo.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushInfo.Size = New System.Drawing.Size(113, 33)
        Me.PushInfo.TabIndex = 10
        Me.PushInfo.Text = "Information"
        Me.PushInfo.UseVisualStyleBackColor = False
        '
        'PushProperties
        '
        Me.PushProperties.BackColor = System.Drawing.SystemColors.Control
        Me.PushProperties.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushProperties.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushProperties.Location = New System.Drawing.Point(14, 212)
        Me.PushProperties.Name = "PushProperties"
        Me.PushProperties.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushProperties.Size = New System.Drawing.Size(113, 33)
        Me.PushProperties.TabIndex = 11
        Me.PushProperties.Text = "Properties..."
        Me.PushProperties.UseVisualStyleBackColor = False
        '
        'PushSnap
        '
        Me.PushSnap.BackColor = System.Drawing.SystemColors.Control
        Me.PushSnap.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushSnap.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushSnap.Location = New System.Drawing.Point(14, 272)
        Me.PushSnap.Name = "PushSnap"
        Me.PushSnap.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushSnap.Size = New System.Drawing.Size(113, 33)
        Me.PushSnap.TabIndex = 12
        Me.PushSnap.Text = "Snap"
        Me.PushSnap.UseVisualStyleBackColor = False
        '
        'PushLive
        '
        Me.PushLive.BackColor = System.Drawing.SystemColors.Control
        Me.PushLive.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushLive.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushLive.Location = New System.Drawing.Point(14, 311)
        Me.PushLive.Name = "PushLive"
        Me.PushLive.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushLive.Size = New System.Drawing.Size(113, 33)
        Me.PushLive.TabIndex = 13
        Me.PushLive.Text = "Live"
        Me.PushLive.UseVisualStyleBackColor = False
        '
        'PushFireTrigger
        '
        Me.PushFireTrigger.BackColor = System.Drawing.SystemColors.Control
        Me.PushFireTrigger.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushFireTrigger.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushFireTrigger.Location = New System.Drawing.Point(14, 350)
        Me.PushFireTrigger.Name = "PushFireTrigger"
        Me.PushFireTrigger.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushFireTrigger.Size = New System.Drawing.Size(113, 33)
        Me.PushFireTrigger.TabIndex = 14
        Me.PushFireTrigger.Text = "Fire Trigger"
        Me.PushFireTrigger.UseVisualStyleBackColor = False
        '
        'PushIdle
        '
        Me.PushIdle.BackColor = System.Drawing.SystemColors.Control
        Me.PushIdle.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushIdle.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushIdle.Location = New System.Drawing.Point(14, 389)
        Me.PushIdle.Name = "PushIdle"
        Me.PushIdle.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushIdle.Size = New System.Drawing.Size(113, 33)
        Me.PushIdle.TabIndex = 15
        Me.PushIdle.Text = "Idle"
        Me.PushIdle.UseVisualStyleBackColor = False
        '
        'PushBufRelease
        '
        Me.PushBufRelease.BackColor = System.Drawing.SystemColors.Control
        Me.PushBufRelease.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushBufRelease.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushBufRelease.Location = New System.Drawing.Point(14, 428)
        Me.PushBufRelease.Name = "PushBufRelease"
        Me.PushBufRelease.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushBufRelease.Size = New System.Drawing.Size(113, 33)
        Me.PushBufRelease.TabIndex = 16
        Me.PushBufRelease.Text = "Buf Release"
        Me.PushBufRelease.UseVisualStyleBackColor = False
        '
        'PushClose
        '
        Me.PushClose.BackColor = System.Drawing.SystemColors.Control
        Me.PushClose.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushClose.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushClose.Location = New System.Drawing.Point(14, 486)
        Me.PushClose.Name = "PushClose"
        Me.PushClose.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushClose.Size = New System.Drawing.Size(113, 33)
        Me.PushClose.TabIndex = 17
        Me.PushClose.Text = "Close"
        Me.PushClose.UseVisualStyleBackColor = False
        '
        'PushUninit
        '
        Me.PushUninit.BackColor = System.Drawing.SystemColors.Control
        Me.PushUninit.Cursor = System.Windows.Forms.Cursors.Default
        Me.PushUninit.ForeColor = System.Drawing.SystemColors.ControlText
        Me.PushUninit.Location = New System.Drawing.Point(14, 525)
        Me.PushUninit.Name = "PushUninit"
        Me.PushUninit.RightToLeft = System.Windows.Forms.RightToLeft.No
        Me.PushUninit.Size = New System.Drawing.Size(113, 33)
        Me.PushUninit.TabIndex = 18
        Me.PushUninit.Text = "Uninit"
        Me.PushUninit.UseVisualStyleBackColor = False
        '
        'FormMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(544, 573)
        Me.Controls.Add(Me.LabelLutMax)
        Me.Controls.Add(Me.EditLutMax)
        Me.Controls.Add(Me.HScrollLutMax)
        Me.Controls.Add(Me.LabelStatus)
        Me.Controls.Add(Me.LabelLutMin)
        Me.Controls.Add(Me.EditLutMin)
        Me.Controls.Add(Me.PushAsterisk)
        Me.Controls.Add(Me.HScrollLutMin)
        Me.Controls.Add(Me.PicDisplay)
        Me.Controls.Add(Me.PushInit)
        Me.Controls.Add(Me.PushOpen)
        Me.Controls.Add(Me.PushInfo)
        Me.Controls.Add(Me.PushProperties)
        Me.Controls.Add(Me.PushSnap)
        Me.Controls.Add(Me.PushLive)
        Me.Controls.Add(Me.PushFireTrigger)
        Me.Controls.Add(Me.PushIdle)
        Me.Controls.Add(Me.PushBufRelease)
        Me.Controls.Add(Me.PushClose)
        Me.Controls.Add(Me.PushUninit)
        Me.Name = "FormMain"
        Me.Text = "vbAcq4"
        CType(Me.PicDisplay, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Public WithEvents LabelStatus As System.Windows.Forms.Label
    Public WithEvents LabelLutMin As System.Windows.Forms.Label
    Public WithEvents LabelLutMax As System.Windows.Forms.Label
    Public WithEvents EditLutMin As System.Windows.Forms.TextBox
    Public WithEvents EditLutMax As System.Windows.Forms.TextBox
    Public WithEvents PushAsterisk As System.Windows.Forms.Button
    Public WithEvents HScrollLutMin As System.Windows.Forms.HScrollBar
    Public WithEvents HScrollLutMax As System.Windows.Forms.HScrollBar
    Public WithEvents PicDisplay As System.Windows.Forms.PictureBox
    Public WithEvents PushInit As System.Windows.Forms.Button
    Public WithEvents PushOpen As System.Windows.Forms.Button
    Public WithEvents PushInfo As System.Windows.Forms.Button
    Public WithEvents PushProperties As System.Windows.Forms.Button
    Public WithEvents PushSnap As System.Windows.Forms.Button
    Public WithEvents PushLive As System.Windows.Forms.Button
    Public WithEvents PushFireTrigger As System.Windows.Forms.Button
    Public WithEvents PushIdle As System.Windows.Forms.Button
    Public WithEvents PushBufRelease As System.Windows.Forms.Button
    Public WithEvents PushClose As System.Windows.Forms.Button
    Public WithEvents PushUninit As System.Windows.Forms.Button

End Class
