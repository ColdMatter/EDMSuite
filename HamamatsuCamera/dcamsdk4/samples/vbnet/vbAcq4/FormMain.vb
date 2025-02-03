Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Drawing.Imaging
Imports vbAcq4.Hamamatsu.DCAM4
Imports vbAcq4.Hamamatsu.subacq4

Public Class FormMain
    Private Enum FormStatus
        Startup             ' After startup or camapi_uninit()
        Initialized         ' After dcamapi_init() or dcamdev_close()
        Opened              ' After dcamdev_open() or dcamcap_stop() without any image
        Acquiring           ' After dcamcap_start()
        Acquired            ' After dcamcap_stop() with image
    End Enum

    Dim mydcam As MyDcam
    Dim mydcamwait As MyDcamWait
    Dim m_image As New MyImage
    Dim m_lut As New MyLut

    Private Structure MyLut
        Dim camerabpp As Integer    ' camera bit per pixel.  This sample code only support MONO.
        Dim cameramax As Integer
        Dim inmax As Integer
        Dim inmin As Integer
    End Structure

    Dim m_indexCamera As Integer    ' index of DCAM device.  This is used at allocating mydcam instance.
    Dim m_nFrameCount As Integer    ' frame count of allocation buffer for DCAM capturing
    Dim m_formstatus As FormStatus  ' Indicate current Form status. For setting, Use MyFormStatus() functions
    Dim m_threadCapture As Thread   ' System.Threading.  Assigned for monitoring updated frames during capturing
    Dim m_bitmap As Bitmap          ' bitmap data for displaying in this Windows Form

    Dim formProperties As FormProperties  ' Properties Form

    ' ---------------- private class MyImage ----------------

    Private Class MyImage
        Public bufframe As New DCAMBUF_FRAME(0)
        Public ReadOnly Property width() As Integer
            Get
                Return bufframe.width
            End Get
        End Property
        Public ReadOnly Property height() As Integer
            Get
                Return bufframe.height
            End Get
        End Property
        Public ReadOnly Property pixeltype() As DCAM_PIXELTYPE
            Get
                Return bufframe.type
            End Get
        End Property
        Public Function isValid() As Boolean
            If width() <= 0 Or height() <= 0 Or pixeltype() = DCAM_PIXELTYPE.NONE Then
                Return False
            Else
                Return True
            End If
        End Function
        Public Sub clear()
            bufframe.width = 0
            bufframe.height = 0
            bufframe.type = DCAM_PIXELTYPE.NONE
        End Sub
        Public Sub set_iFrame(ByVal index As Integer)
            bufframe.iFrame = index
        End Sub
    End Class

    ' ---------------- Local functions ----------------

    ' My Form Status helper
    Private Sub MyFormStatus(ByRef status As FormStatus)
        Dim isStartup As Boolean
        Dim isInitialized As Boolean
        Dim isOpened As Boolean
        Dim isAcquring As Boolean
        Dim isAcquired As Boolean

        If status = FormStatus.Startup Then isStartup = True
        If status = FormStatus.Initialized Then isInitialized = True
        If status = FormStatus.Opened Then isOpened = True
        If status = FormStatus.Acquiring Then isAcquring = True
        If status = FormStatus.Acquired Then isAcquired = True

        Me.PushInit.Enabled = isStartup
        Me.PushOpen.Enabled = isInitialized
        Me.PushInfo.Enabled = isOpened Or isAcquired Or isAcquring
        Me.PushSnap.Enabled = isOpened Or isAcquired
        Me.PushLive.Enabled = isOpened Or isAcquired
        Me.PushFireTrigger.Enabled = isAcquring
        Me.PushIdle.Enabled = isAcquring
        Me.PushBufRelease.Enabled = isAcquired
        Me.PushClose.Enabled = isOpened Or isAcquired
        Me.PushUninit.Enabled = isInitialized

        Me.PushProperties.Enabled = isOpened Or isAcquired

        If isInitialized Or isOpened Then
            ' acquisition is not starting
            MyThreadCapture_Abort()
        End If

        m_formstatus = status
    End Sub
    Private Sub MyFormStatus_Startup()             ' After startup or camapi_uninit()
        MyFormStatus(FormStatus.Startup)
    End Sub
    Private Sub MyFormStatus_Initialized()         ' After dcamapi_init() or dcamdev_close()
        MyFormStatus(FormStatus.Initialized)
    End Sub
    Private Sub MyFormStatus_Opened()              ' After dcamdev_open() or dcamcap_stop() without any image
        MyFormStatus(FormStatus.Opened)
    End Sub
    Private Sub MyFormStatus_Acquiring()           ' After dcamcap_start()
        MyFormStatus(FormStatus.Acquiring)
    End Sub
    Private Sub MyFormStatus_Acquired()            ' After dcamcap_stop() with image
        MyFormStatus(FormStatus.Acquired)
    End Sub

    ' Display status
    Private Sub MyShowStatus(ByVal text As String)
        LabelStatus.Text = text
    End Sub
    Private Sub MyShowStatusOK(ByVal text As String)
        MyShowStatus("OK: " + text)
    End Sub
    Private Sub MyShowStatusNG(ByVal text As String, ByVal err As DCAMERR)
        MyShowStatus("NG: &&H" + Strings.Right("0000000" + Hex(err), 8) + ": " + text)
    End Sub

    ' update LUT condition
    Private Sub update_lut(ByVal bUpdatePicture As Boolean)
        If Not mydcam Is Nothing Then
            Dim prop As New MyDcamProp(mydcam, DCAMIDPROP.BITSPERCHANNEL)
            Dim v As Double

            prop.getvalue(v)
            m_lut.camerabpp = v
            m_lut.cameramax = (1 << m_lut.camerabpp) - 1

            m_lut.inmax = HScrollLutMax.Value
            m_lut.inmin = HScrollLutMin.Value

            HScrollLutMax.Maximum = m_lut.cameramax
            HScrollLutMin.Maximum = m_lut.cameramax

            If (m_lut.inmax > m_lut.cameramax) Then
                m_lut.inmax = m_lut.cameramax
                HScrollLutMax.Value = m_lut.inmax
                bUpdatePicture = True
            End If
            If (m_lut.inmin > m_lut.cameramax) Then
                m_lut.inmin = m_lut.cameramax
                HScrollLutMin.Value = m_lut.inmin
                bUpdatePicture = True
            End If
            If (bUpdatePicture) Then
                MyUpdatePicture()
            End If
        End If
    End Sub

    ' Updating myimage by DCAM frame
    Delegate Sub MyDelegate_UpdateImage(ByVal iFrame As Integer)

    Private Sub MyUpdateImage(ByVal iFrame As Integer)
        If InvokeRequired Then
            ' worker thread calls this function
            Me.Invoke(New MyDelegate_UpdateImage(AddressOf MyUpdateImage), iFrame)
            Return
        End If

        ' lock selected frame by iFrame
        m_image.set_iFrame(iFrame)
        mydcam.buf_lockframe(m_image.bufframe)

        MyUpdatePicture()
    End Sub

    ' Updating bitmap in picture
    Private Sub MyUpdatePicture()
        If m_image.isValid() Then
            m_lut.inmax = HScrollLutMax.Value
            m_lut.inmin = HScrollLutMin.Value

            Dim rc As New Rectangle(0, 0, m_image.width(), m_image.height())
            m_bitmap = New Bitmap(m_image.width(), m_image.height())

            Dim err As SUBACQERR
            err = subacq.copydib(m_bitmap, m_image.bufframe, rc, m_lut.inmax, m_lut.inmin)

            If err = SUBACQERR.SUCCESS Then
                PicDisplay.Image = m_bitmap
            Else
                PicDisplay.Image = Nothing
                MyShowStatus("NG: SUBACQERR: &&H" + Strings.Right("0000000" + Hex(err), 8))
            End If
        End If
    End Sub

    ' Cpaturing Thread helper functions
    Private Sub MyThreadCapture_Start()
        m_threadCapture = New Thread(AddressOf OnThreadCapture)

        m_threadCapture.IsBackground = True
        m_threadCapture.Start()
    End Sub
    Private Sub MyThreadCapture_Abort()
        If Not m_threadCapture Is Nothing Then
            If Not mydcamwait Is Nothing Then
                mydcamwait.abort()
            End If
            m_threadCapture.Abort()
        End If
    End Sub

    Private Sub OnThreadCapture()
        Using mywait As New MyDcamWait(mydcam)
            mydcamwait = mywait

            Do While True
                Dim eventmask As Integer
                Dim eventhappened As Integer
                eventmask = DCAMWAIT_EVENT.CAPEVENT_FRAMEREADY Or DCAMWAIT_EVENT.CAPEVENT_STOPPED
                eventhappened = 0
                If mydcamwait.start(eventmask, eventhappened) Then
                    Select Case eventhappened
                        Case DCAMWAIT_EVENT.CAPEVENT_FRAMEREADY
                            Dim iNewestFrame As Integer
                            Dim iFrameCount As Integer

                            If mydcam.cap_transferinfo(iNewestFrame, iFrameCount) Then
                                MyUpdateImage(iNewestFrame)
                            End If
                        Case DCAMWAIT_EVENT.CAPEVENT_STOPPED
                            Exit Do ' capturing is stopped
                    End Select
                Else
                    Select Case mydcamwait.m_lasterr
                        Case DCAMERR.ABORT
                            Exit Do
                        Case DCAMERR.TIMEOUT
                            ' nothing to do
                    End Select
                End If
            Loop
            mydcamwait = Nothing
        End Using
    End Sub

    ' ---------------- Windows Form Command Handler ----------------
    Private Sub FormMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        ' Initialize form status
        MyFormStatus_Startup()
        m_nFrameCount = 3

        ' Update window title
        If IntPtr.Size = 4 Then
            Me.Text = "vbAcq4 (32 bit)"
        ElseIf IntPtr.Size = 8 Then
            Me.Text = "vbAcq4 (64 bit)"
        End If

    End Sub

    Private Sub FormMain_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        MyThreadCapture_Abort()
    End Sub


    Private Sub PushInit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushInit.Click
        ' dcamapi_init() may takes for a few seconds
        Cursor.Current = Cursors.WaitCursor

        If Not MyDcamApi.init() Then
            MyShowStatusNG("dcamapi_init()", MyDcamApi.m_lasterr)
            Cursor.Current = Cursors.Default
            Return                              ' Fail: dcamapi_init()
        End If

        ' Success: dcamapi_init()

        Cursor.Current = Cursors.Default

        MyShowStatusOK("dcamapi_init()")
        MyFormStatus_Initialized()
    End Sub
    Private Sub PushUninit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushUninit.Click
        If Not MyDcamApi.uninit() Then
            MyShowStatusNG("dcamapi_uninit()", MyDcamApi.m_lasterr)
            Return                              ' Fail: dcamapi_uninit()
        End If

        ' Success: dcamapi_uninit()

        MyShowStatusOK("dcamapi_uninit()")
        MyFormStatus_Startup()                  ' change dialog FormStatus to Startup
    End Sub
    Private Sub PushOpen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushOpen.Click
        If Not mydcam Is Nothing Then
            MyShowStatus("Internal Error: mydcam is already set")
            MyFormStatus_Initialized()          ' FormStatus should be Initialized.
            Return                              ' internal error
        End If

        ' dcamdev_open() may takes for a few seconds
        Cursor.Current = Cursors.WaitCursor

        Dim aMyDcam As New MyDcam
        If Not aMyDcam.dev_open(m_indexCamera) Then
            MyShowStatusNG("dcamdev_open()", mydcam.m_lasterr)
            mydcam = Nothing
            Cursor.Current = Cursors.Default
            Return                              ' Fail: dcamdev_open()
        End If

        ' Success: dcamdev_open()

        mydcam = aMyDcam                        ' store MyDcam instance

        MyShowStatusOK("dcamdev_open()")
        MyFormStatus_Opened()                   ' change dialog FormStatus to Opened
        Cursor.Current = Cursors.Default
    End Sub
    Private Sub PushClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushClose.Click
        If mydcam Is Nothing Then
            MyShowStatus("Internal Error: mydcam is nothing")
            MyFormStatus_Initialized() '        ' FormStatus should be Initialized.
            Return                              ' internal error
        End If

        MyThreadCapture_Abort()                 ' abort capturing thread if exist

        If Not mydcam.dev_close() Then
            MyShowStatusNG("dcamdev_close()", mydcam.m_lasterr)
            Return                              ' Fail: dcamdev_close()
        End If

        ' Success: dcamdev_close()

        mydcam = Nothing

        MyShowStatusOK("dcamdev_close()")
        MyFormStatus_Initialized()              ' change dialog FormStatus to Initialized
    End Sub

    Private Sub PushInfo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushInfo.Click
        Dim FormInfo As New FormInfo

        FormInfo.set_mydcam(mydcam)
        FormInfo.Show()                         ' show FormProperties dialog as modeless
    End Sub
    Private Sub PushProperties_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushProperties.Click
        If formProperties Is Nothing Then
            formProperties = New FormProperties
        ElseIf formProperties.IsDisposed() Then
            formProperties = New FormProperties
        End If
        formProperties.set_mydcam(mydcam)
        formProperties.Show()                   ' show FrmProperties dialog as modeless
        formProperties.update_properties()
    End Sub

    Private Sub PushSnap_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushSnap.Click
        If mydcam Is Nothing Then
            MyShowStatus("Internal Error: mydcam is nothing")
            MyFormStatus_Initialized()          ' FormStatus should be Initialized.
            Return                              ' internal error
        End If

        Dim text As String
        text = ""

        If m_formstatus = FormStatus.Opened Then
            ' if FormStatus is Opened, DCAM buffer is not allocated.
            ' So call dcambuf_alloc() to prepare capturing.

            text = "dcambuf_alloc(" + Str(m_nFrameCount) + ")"

            ' allocate frame buffer
            If Not mydcam.buf_alloc(m_nFrameCount) Then
                ' allocation was failed
                MyShowStatusNG(text, mydcam.m_lasterr)
                Return                          '  Fail: dcambuf_alloc()
            End If

            ' Success: dcambuf_alloc()

            update_lut(False)
        End If

        ' start acquisition
        mydcam.m_capmode = DCAMCAP_START.SNAP    ' one time capturing.  acqusition will stop after capturing m_nFrameCount frames
        If Not mydcam.cap_start() Then
            ' acquisition was failed. In this sample, frame buffer is also released.
            MyShowStatusNG("dcamcap_start()", mydcam.m_lasterr)

            mydcam.buf_release()                ' release unnecessary buffer in DCAM
            MyFormStatus_Opened()               ' change dialog FormStatus to Opened
            Return                              ' Fail: dcamcap_start()
        End If

        ' Success: dcamcap_start()
        ' acquisition has started

        If text.Length > 0 Then
            text = text + " && "
        End If
        MyShowStatusOK(text + "dcamcap_start()")

        MyFormStatus_Acquiring()                ' change dialog FormStatus to Acquiring
        MyThreadCapture_Start()                 ' start monitoring thread
    End Sub
    Private Sub PushLive_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushLive.Click
        If mydcam Is Nothing Then
            MyShowStatus("Internal Error: mydcam is nothing")
            MyFormStatus_Initialized()          ' FormStatus should be Initialized.
            Return                              ' internal error
        End If

        Dim text As String
        text = ""

        If m_formstatus = FormStatus.Opened Then
            ' if FormStatus is Opened, DCAM buffer is not allocated.
            ' So call dcambuf_alloc() to prepare capturing.

            text = "dcambuf_alloc(" + Str(m_nFrameCount) + ")"

            ' allocate frame buffer
            If Not mydcam.buf_alloc(m_nFrameCount) Then
                ' allocation was failed
                MyShowStatusNG(text, mydcam.m_lasterr)
                Return                          '  Fail: dcambuf_alloc()
            End If

            ' Success: dcambuf_alloc()

            update_lut(False)
        End If

        ' start acquisition
        mydcam.m_capmode = DCAMCAP_START.SEQUENCE    ' continuous capturing.  continuously acqusition will be done
        If Not mydcam.cap_start() Then
            ' acquisition was failed. In this sample, frame buffer is also released.
            MyShowStatusNG("dcamcap_start()", mydcam.m_lasterr)

            mydcam.buf_release()                ' release unnecessary buffer in DCAM
            MyFormStatus_Opened()               ' change dialog FormStatus to Opened
            Return                              ' Fail: dcamcap_start()
        End If

        ' Success: dcamcap_start()
        ' acquisition has started

        If text.Length > 0 Then
            text = text + " && "
        End If
        MyShowStatusOK(text + "dcamcap_start()")

        MyFormStatus_Acquiring()                ' change dialog FormStatus to Acquiring
        MyThreadCapture_Start()                 ' start monitoring thread
    End Sub
    Private Sub PushIdle_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushIdle.Click
        If mydcam Is Nothing Then
            MyShowStatus("Internal Error: mydcam is nothing")
            MyFormStatus_Initialized()          ' FormStatus should be Initialized.
            Return                              ' internal error
        End If

        If m_formstatus <> FormStatus.Acquiring Then
            MyShowStatus("Internal Error: Idle button is only available when FormStatus is Acquiring")
            Return                              ' internal error
        End If

        ' stop acquisition
        If Not mydcam.cap_stop() Then
            MyShowStatusNG("dcamcap_stop()", mydcam.m_lasterr)
            Return                              ' Fail: dcamcap_stop()
        End If

        ' Success: dcamcap_stop()

        MyShowStatusOK("dcamcap_stop()")
        MyFormStatus_Acquired()                 ' change dialog FormStatus to Acquired
    End Sub
    Private Sub PushFireTrigger_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushFireTrigger.Click
        If mydcam Is Nothing Then
            MyShowStatus("Internal Error: mydcam is nothing")
            MyFormStatus_Initialized()          ' FormStatus should be Initialized.
            Return                              ' internal error
        End If

        If Not m_formstatus = FormStatus.Acquiring Then
            MyShowStatus("Internal Error: FireTrigger button is only available when FormStatus is Acquiring")
            Return                              ' internal error
        End If

        ' fire software trigger
        If Not mydcam.cap_firetrigger() Then
            MyShowStatusNG("dcamcap_firetrigger()", mydcam.m_lasterr)
            Return                              ' Fail: dcamcap_firetrigger()
        End If

        ' Success: dcamcap_firetrigger()

        MyShowStatusOK("dcamcap_firetrigger()")
    End Sub

    Private Sub PushBufRelease_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PushBufRelease.Click
        If mydcam Is Nothing Then
            MyShowStatus("Internal Error: mydcam is nothing")
            MyFormStatus_Initialized()          ' FormStatus should be Initialized.
            Return                              ' internal error
        End If

        If Not m_formstatus = FormStatus.Acquired Then
            MyShowStatus("Internal Error: BufRelease is only available when FormStatus is Acquired")
            Return                              ' internal error
        End If

        ' release buffer
        If Not mydcam.buf_release() Then
            MyShowStatusNG("dcambuf_release()", mydcam.m_lasterr)
            Return                              ' Fail: dcambuf_release()
        End If

        ' Success: dcambuf_release()

        MyShowStatusOK("dcambuf_release()")

        MyFormStatus_Opened()                   ' change dialog FormStatus to Opened

        m_image.clear()
    End Sub

    Private Sub EditLutMax_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EditLutMax.TextChanged
        m_lut.inmax = Integer.Parse(EditLutMax.Text)
        If HScrollLutMax.Value <> m_lut.inmax And 0 <= m_lut.inmax And m_lut.inmax <= m_lut.cameramax Then
            HScrollLutMax.Value = m_lut.inmax
            update_lut(True)
        End If
    End Sub

    Private Sub EditLutMin_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles EditLutMin.TextChanged
        m_lut.inmin = Integer.Parse(EditLutMin.Text)
        If HScrollLutMin.Value <> m_lut.inmin And 0 <= m_lut.inmin And m_lut.inmin <= m_lut.cameramax Then
            HScrollLutMin.Value = m_lut.inmin
            update_lut(True)
        End If
    End Sub

    Private Sub HScrollLutMax_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HScrollLutMax.ValueChanged
        If m_lut.inmax <> HScrollLutMax.Value Then
            m_lut.inmax = HScrollLutMax.Value
            EditLutMax.Text = m_lut.inmax.ToString()
            update_lut(True)
        End If
    End Sub

    Private Sub HScrollLutMin_ValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HScrollLutMin.ValueChanged
        If m_lut.inmin <> HScrollLutMin.Value Then
            m_lut.inmin = HScrollLutMin.Value
            EditLutMin.Text = m_lut.inmin.ToString()
            update_lut(True)
        End If
    End Sub

End Class
