Imports vbAcq4.Hamamatsu.DCAM4

Public Class FormInfo

    Class datainfo
        Private _infokind As String
        Private _infovalue As String

        Public ReadOnly Property name() As String
            Get
                Return _infokind
            End Get
        End Property
        Public ReadOnly Property value() As String
            Get
                Return _infovalue
            End Get
        End Property
        Public Sub New(ByVal kind As String, ByVal value As String)
            _infokind = kind
            _infovalue = value
        End Sub
    End Class

    ' ----------------

    Sub add_info(ByRef listinfo As List(Of datainfo), ByRef mydcam As MyDcam, ByVal idstr As DCAM_IDSTR, ByVal strkind As String)
        Dim strvalue As String
        strvalue = mydcam.dev_getstring(idstr)
        If (strvalue.Length > 0) Then
            listinfo.Add(New datainfo(strkind, strvalue))
        End If
    End Sub
    Public Sub set_mydcam(ByRef mydcam As MyDcam)
        Dim listinfo As New List(Of datainfo)

        If Not mydcam Is Nothing Then
            add_info(listinfo, mydcam, DCAM_IDSTR.BUS, "BUS")
            add_info(listinfo, mydcam, DCAM_IDSTR.CAMERAID, "CAMERAID")
            add_info(listinfo, mydcam, DCAM_IDSTR.VENDOR, "VENDOR")
            add_info(listinfo, mydcam, DCAM_IDSTR.MODEL, "MODEL")
            add_info(listinfo, mydcam, DCAM_IDSTR.CAMERAVERSION, "CAMERA VERSION")
            add_info(listinfo, mydcam, DCAM_IDSTR.DRIVERVERSION, "DRIVER VERSION")
            add_info(listinfo, mydcam, DCAM_IDSTR.MODULEVERSION, "MODULE VERSION")
            add_info(listinfo, mydcam, DCAM_IDSTR.DCAMAPIVERSION, "DCAM-API VERSION")
            add_info(listinfo, mydcam, DCAM_IDSTR.CAMERA_SERIESNAME, "SERIES NAME")
        End If

        DataGridViewInfo.DataSource = listinfo
    End Sub
End Class