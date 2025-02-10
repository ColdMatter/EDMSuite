' MyDcam.vb

Imports System.Runtime.InteropServices
Imports vbacq4.Hamamatsu.DCAM4

' ================================ MyDcamApi ================================

''' <summary>
''' Manager class for DCAM-API.  All memebers and functions are Shared
''' </summary>
''' <remarks>remark of MyDcamApi </remarks>
Public Class MyDcamApi
    Public Shared m_lasterr As DCAMERR
    Public Shared m_devcount As Integer

    Public Shared Function init() As Boolean
        Dim param As New DCAMAPI_INIT(0)

        m_lasterr = dcamapi.init(param)
        If failed(m_lasterr) Then
            m_devcount = 0
        Else
            m_devcount = param.iDeviceCount
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Shared Function uninit() As Boolean
        m_lasterr = dcamapi.uninit()
        m_devcount = 0

        Return Not failed(m_lasterr)
    End Function
End Class

' ================================ MyDcam ================================

''' <summary>
''' Helper class for HDCAM.
''' </summary>
''' <remarks></remarks>
Public Class MyDcam
    Public m_lasterr As DCAMERR
    Public m_hdcam As IntPtr
    Public m_capmode As DCAMCAP_START      ' used for dcamcap_start

    ' ---------------- open, close and information ----------------

    Public Function dev_open(ByVal iCamera As Integer) As Boolean
        Dim param As New DCAMDEV_OPEN(iCamera)

        m_lasterr = dcamdev.open(param)
        If failed(m_lasterr) Then
            m_hdcam = 0
        Else
            If m_hdcam <> 0 Then
                m_lasterr = dcamdev.close(m_hdcam)
            End If

            m_hdcam = param.hdcam
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function dev_close() As Boolean
        If m_hdcam = 0 Then
            Return True ' already closed
        End If

        m_lasterr = dcamdev.close(m_hdcam)
        If Not failed(m_lasterr) Then
            m_hdcam = 0
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function dev_getstring(ByVal iString As Integer) As String
        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
        Else
            Dim ret As String
            ret = ""

            m_lasterr = dcamdev.getstring(m_hdcam, iString, ret)
            If Not failed(m_lasterr) Then
                Return ret
            End If
        End If

        Return ""   ' return empty string when error happened.
    End Function
    ' dcamdev_getcapability() is not supported in this code
    ' dev_getcapability( ByRef param As DCAMDEV_CAPABILITY) As Integer


    ' ---------------- buffer allocation and get ----------------

    Public Function buf_alloc(ByVal framecount As Integer) As Boolean
        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
        Else
            m_lasterr = dcambuf.alloc(m_hdcam, framecount)
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function buf_release() As Boolean
        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
        Else
            m_lasterr = dcambuf.release(m_hdcam, 0)
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function buf_lockframe(ByRef aFrame As DCAMBUF_FRAME) As Boolean
        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
        Else
            m_lasterr = dcambuf.lockframe(m_hdcam, aFrame)
        End If

        Return Not failed(m_lasterr)
    End Function
    'dcambuf_copyframe(ByVal hdcam As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As Integer
    'dcambuf_copymetadata(ByVal hdcam As IntPtr, ByRef hdr As DCAM_METADATAHDR) As Integer

    ' ---------------- capture control ----------------

    Public Function cap_start() As Boolean
        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
        Else
            m_lasterr = dcamcap.start(m_hdcam, m_capmode)
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function cap_stop() As Boolean   ' Note: Is it better to use cap_idle() instead of cap_stop() because dcamcap class member function is idle ?
        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
        Else
            m_lasterr = dcamcap.idle(m_hdcam)
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function cap_status() As DCAMCAP_STATUS
        Dim stat As DCAMCAP_STATUS

        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
            stat = DCAMCAP_STATUS._ERROR
        Else
            m_lasterr = dcamcap.status(m_hdcam, stat)
        End If

        Return stat
    End Function
    Public Function cap_transferinfo(ByRef nNewestFrameIndex As Integer, ByRef nFrameCount As Integer) As Boolean
        Dim param As New DCAMCAP_TRANSFERINFO(0)
        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
            nNewestFrameIndex = -1
            nFrameCount = 0
        Else
            m_lasterr = dcamcap.transferinfo(m_hdcam, param)
            nNewestFrameIndex = param.nNewestFrameIndex
            nFrameCount = param.nFrameCount
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function cap_firetrigger() As Boolean
        If m_hdcam = 0 Then
            m_lasterr = DCAMERR.INVALIDHANDLE
        Else
            m_lasterr = dcamcap.firetrigger(m_hdcam, 0)
        End If

        Return Not failed(m_lasterr)
    End Function
    'dcamcap_record(ByVal hdcam As IntPtr, ByVal hrec As IntPtr) As Integer
End Class

' ================================ MyDcamWait ================================

''' <summary>
''' helper class for HDCAMWAIT and dcamwait functions
''' </summary>
''' <remarks></remarks>
Public Class MyDcamWait
    Implements IDisposable                  ' for Dispose()
    Public m_lasterr As DCAMERR
    Public m_hwait As IntPtr
    Public m_supportevent As Integer        ' [out] filled with supported event events
    Public m_timeout As Integer

    Public Sub New(ByRef mydcam As MyDcam)
        If mydcam.m_hdcam = 0 Then
            ' mydcam should have valid HDCAM handle.
            m_lasterr = DCAMERR.INVALIDHANDLE
            m_hwait = 0
        Else
            Dim param As New DCAMWAIT_OPEN(mydcam.m_hdcam)

            m_lasterr = dcamwait.open(param)
            If Not failed(m_lasterr) Then
                m_hwait = param.hwait
                m_supportevent = param.supportevent
            End If
        End If

        m_timeout = 1000        ' 1 second
    End Sub

    Protected Overrides Sub Finalize()
        If m_hwait <> 0 Then
            dcamwait.close(m_hwait)
            m_hwait = 0
        End If

        MyBase.Finalize()
    End Sub
    Sub Dispose() Implements IDisposable.Dispose
        If m_hwait <> 0 Then
            dcamwait.close(m_hwait)
            m_hwait = 0
        End If
    End Sub

    Public Function start(ByVal eventmask As Integer) As Boolean
        If m_hwait = 0 Then
            m_lasterr = DCAMERR.INVALIDWAITHANDLE
        Else
            Dim param As New DCAMWAIT_START(eventmask)
            m_lasterr = dcamwait.start(m_hwait, param)
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function start(ByVal eventmask As Integer, ByRef eventhappened As Integer) As Boolean
        If m_hwait = 0 Then
            m_lasterr = DCAMERR.INVALIDWAITHANDLE
        Else
            Dim param As New DCAMWAIT_START(eventmask, m_timeout)
            m_lasterr = dcamwait.start(m_hwait, param)
            If Not failed(m_lasterr) Then
                eventhappened = param.eventhappened
            End If
        End If

        Return Not failed(m_lasterr)
    End Function
    Public Function abort() As Boolean
        If m_hwait = 0 Then
            m_lasterr = DCAMERR.INVALIDWAITHANDLE
        Else
            m_lasterr = dcamwait.abort(m_hwait)
        End If

        Return Not failed(m_lasterr)
    End Function
End Class

' ================================ MyDcamRec ================================

''' <summary>
''' helper class for HDCAMREC and dcamrec functions
''' </summary>
''' <remarks></remarks>
Public Class MyDcamRec
    'dcamrec_openA(ByRef param As DCAMREC_OPEN) As Integer
    'dcamrec_status(ByVal hrec As IntPtr, ByRef param As DCAMREC_STATUS) As Integer
    'dcamrec_close(ByVal hrec As IntPtr) As Integer
    'dcamrec_lockframe(ByVal hrec As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As Integer
    'dcamrec_copyframe(ByVal hrec As IntPtr, ByRef pFrame As DCAMBUF_FRAME) As Integer
    'dcamrec_writemetadata(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As Integer
    'dcamrec_lockmetadata(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As Integer
    'dcamrec_copymetadata(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATAHDR) As Integer
    'dcamrec_lockmetadatablock(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATABLOCKHDR) As Integer
    'dcamrec_copymetadatablock(ByVal hrec As IntPtr, ByRef hdr As DCAM_METADATABLOCKHDR) As Integer
End Class

' ================================ MyDcamProp ================================

''' <summary>
''' helper function for DCAM properties
''' </summary>
''' <remarks></remarks>
Public Class MyDcamProp
    Public m_lasterr As DCAMERR
    Public m_hdcam As IntPtr
    Public m_idProp As Integer
    Public m_attr As DCAMPROP_ATTR

    Public Sub New(ByRef mydcam As MyDcam, ByVal idProp As Integer)
        m_hdcam = mydcam.m_hdcam
        m_idProp = idProp
        m_attr = New DCAMPROP_ATTR(idProp)
    End Sub
    Private Sub New(ByRef hdcam As IntPtr, ByVal idProp As Integer)
        m_hdcam = hdcam
        m_idProp = idProp
    End Sub

    Public Function Clone() As MyDcamProp
        Dim ret As New MyDcamProp(m_hdcam, m_idProp)
        ret.m_attr = m_attr
        Return ret
    End Function

    Public Function update_attr() As Boolean
        m_attr.iProp = m_idProp
        m_lasterr = dcamprop.getattr(m_hdcam, m_attr)
        Return Not failed(m_lasterr)
    End Function

    Public Function getvalue(ByRef value As Double) As Boolean
        m_lasterr = dcamprop.getvalue(m_hdcam, m_idProp, value)
        Return Not failed(m_lasterr)
    End Function
    Public Function setvalue(ByVal value As Double) As Boolean
        m_lasterr = dcamprop.setvalue(m_hdcam, m_idProp, value)
        Return Not failed(m_lasterr)
    End Function
    Public Function setgetvalue(ByRef value As Double) As Boolean
        Dim _option As Integer
        m_lasterr = dcamprop.setgetvalue(m_hdcam, m_idProp, value, _option)
        Return Not failed(m_lasterr)
    End Function
    Public Function queryvalue(ByRef value As Double, ByVal _option As DCAMPROPOPTION) As Boolean
        m_lasterr = dcamprop.queryvalue(m_hdcam, m_idProp, value, _option)
        Return Not failed(m_lasterr)
    End Function
    Public Function queryvalue_next(ByRef value As Double) As Boolean
        m_lasterr = dcamprop.queryvalue(m_hdcam, m_idProp, value, DCAMPROPOPTION._NEXT)
        Return Not failed(m_lasterr)
    End Function
    Public Function nextid() As Boolean
        m_lasterr = dcamprop.getnextid(m_hdcam, m_idProp, 0)
        Return Not failed(m_lasterr)
    End Function
    Public Function getname() As String
        Dim name As String
        name = ""
        m_lasterr = dcamprop.getname(m_hdcam, m_idProp, name)
        If failed(m_lasterr) Then
            name = ""
        End If
        Return name
    End Function
    Public Function getvaluetext(ByVal value As Double) As String
        Dim ret As String
        ret = ""

        m_lasterr = dcamprop.getvaluetext(m_hdcam, m_idProp, value, ret)
        If failed(m_lasterr) Then
            ret = Str(value)
        End If
        Return ret
    End Function

    Public Function is_attrtype_mode() As Boolean
        Select Case m_attr.attribute And DCAMPROPATTRIBUTE.TYPE_MASK
            Case DCAMPROPATTRIBUTE.TYPE_MODE
                Return True
            Case Else
                Return False
        End Select
    End Function
    Public Function is_attr_readonly() As Boolean
        Select Case m_attr.attribute And DCAMPROPATTRIBUTE.WRITABLE
            Case DCAMPROPATTRIBUTE.WRITABLE
                Return False    ' not readonly
            Case Else
                Return True     ' readonly
        End Select
    End Function
End Class

