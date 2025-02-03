' subacq4.vb

' implementation of subacq.copydib() function
' instead of using DLL, this source code requires /unsafe option.

Imports System.Runtime.InteropServices
Imports System.Drawing.Imaging
Imports vbAcq4.Hamamatsu.DCAM4

#If Not USE_SUBACQ_DLL Then

Namespace Hamamatsu
    Public Class subacq4

        Public Enum SUBACQERR
            ' error
            NO_DLL = &H80000000
            INVALID_ARG = &H80000001
            INVALID_DST = &H80000011
            INVALID_DSTPIXELFORMAT = &H80000012
            INVALID_SRC = &H80000021
            INVALID_SRCPIXELTYPE = &H80000022

            NOSUPPORT_LUTARRAY = &H80000101

            ' success
            SUCCESS = 1
        End Enum

        Public Class subacq

            Public Declare Sub CopyMemory_UShortarrayFromIntPtr Lib "kernel32" Alias "RtlMoveMemory" (ByVal destination() As UShort, ByVal Source As IntPtr, ByVal Length As Integer)

            Public Shared Function copydib(ByRef bitmap As Bitmap, ByVal src As DCAMBUF_FRAME, ByRef rect As Rectangle, ByVal lutmax As Integer, ByVal lutmin As Integer) As SUBACQERR
                Dim w As Integer
                Dim h As Integer

                w = rect.Width
                h = rect.Height
                If w > bitmap.Width Then w = bitmap.Width
                If h > bitmap.Height Then h = bitmap.Height
                If w > src.width Then w = src.width
                If h > src.height Then h = src.height

                Dim dst As BitmapData
                dst = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb)

                Dim err As SUBACQERR
                If src.type = DCAM_PIXELTYPE.MONO16 Then
                    err = copydib_rgb32_from_mono16(dst.Scan0, dst.Stride, src.buf, src.rowbytes, w, h, lutmax, lutmin)
                Else
                    err = SUBACQERR.INVALID_SRCPIXELTYPE
                End If

                bitmap.UnlockBits(dst)
                Return err
            End Function

            Private Shared Function copydib_rgb32_from_mono16(ByVal dst As IntPtr, ByVal dstrowbytes As Integer, ByVal src As IntPtr, ByVal srcrowbytes As Int32, ByVal width As Integer, ByVal height As Integer, ByVal lutmax As Integer, ByVal lutmin As Integer) As SUBACQERR
                Dim s As UShort()
                Dim d As Int32()
                s = New UShort(width) {}
                d = New Int32(width) {}

                Dim gain As Double
                Dim inBase As Double
                gain = 0
                inBase = 0

                If lutmax <> lutmin Then
                    If lutmin < lutmax Then
                        gain = 256.0 / (lutmax - lutmin + 1)
                        inBase = lutmin
                    ElseIf lutmin > lutmax Then
                        gain = 256.0 / (lutmax - lutmin - 1)
                        inBase = lutmax
                    End If
                ElseIf lutmin > 0 Then  ' binary threshold
                    gain = 0
                    inBase = lutmin
                End If

                Dim y As Integer
                For y = 0 To height - 1
                    Dim offset As Integer

                    offset = srcrowbytes * y
                    CopyMemory_UShortarrayFromIntPtr(s, src.ToInt64() + offset, srcrowbytes)

                    copydibline_rgb32_from_mono16(d, s, width, gain, inBase)

                    offset = dstrowbytes * y
                    Marshal.Copy(d, 0, dst.ToInt64() + offset, width)
                Next
                Return SUBACQERR.SUCCESS
            End Function

            Private Shared Sub copydibline_rgb32_from_mono16(ByVal d() As Int32, ByVal s() As UShort, ByVal width As Integer, ByVal gain As Double, ByVal inBase As Double)
                Dim x As Integer
                If gain <> 0 Then
                    For x = 0 To width - 1
                        Dim u As UInt16
                        u = s(x)

                        Dim v As Double
                        v = gain * (u - inBase)

                        Dim c As Byte
                        If v > 255 Then
                            c = 255
                        ElseIf v < 0 Then
                            c = 0
                        Else
                            c = v
                        End If

                        d(x) = Color.FromArgb(c, c, c).ToArgb()
                    Next
                ElseIf inBase > 0 Then
                    For x = 0 To width - 1
                        Dim u As UInt16
                        u = s(x)

                        Dim c As Byte
                        If u >= inBase Then
                            c = 255
                        Else
                            c = 0
                        End If

                        d(x) = Color.FromArgb(c, c, c).ToArgb()
                    Next
                Else
                    For x = 0 To width - 1
                        Dim u As UInt16
                        u = s(x)

                        Dim c As Byte
                        c = (u >> 8)

                        d(x) = Color.FromArgb(c, c, c).ToArgb()
                    Next
                End If
            End Sub
        End Class
    End Class
End Namespace

#End If ' Not USE_SUBACQ_DLL
