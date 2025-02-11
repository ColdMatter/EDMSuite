' subacq4dll.vb

' implementation of subacq.copydib() function
' insgtead of using /unsafe option, this source code requires SUBACQ4.DLL

Imports System.Runtime.InteropServices
Imports System.Drawing.Imaging
Imports vbAcq4.Hamamatsu.DCAM4

#If USE_SUBACQ_DLL Then

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

        <StructLayout(LayoutKind.Sequential, Pack:=4)> _
        Structure SUBACQ_COPYDIB
            Public size As Integer
            Public _option As Integer

            Public src As IntPtr
            Public srcrowbytes As Integer
            Public srcpixeltype As DCAM_PIXELTYPE

            Public dst As IntPtr
            Public dstrowbytes As Integer
            Public dstpixeltype As PixelFormat

            Public width As Integer
            Public height As Integer
            Public left As Integer
            Public top As Integer

            Public lut As IntPtr
            Public lutmax As Integer
            Public lutmin As Integer
        End Structure

        Public Class subacq
            Class subacqdll
                Declare Function subacq_copydib Lib "subacq4.dll" (ByRef param As SUBACQ_COPYDIB) As SUBACQERR

                Private Shared bTried As Boolean = False
                Private Shared bAvailable As Boolean = False
                Private Shared hLibInst As IntPtr = IntPtr.Zero

                <DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)> _
                Private Shared Function LoadLibrary(ByVal lpFileName As String) As IntPtr
                End Function

                <DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)> _
                Private Shared Function FreeLibrary(ByVal hModule As IntPtr) As Boolean
                End Function

                <DllImport("kernel32", CharSet:=CharSet.Ansi, SetLastError:=True)> _
                Private Shared Function GetProcAddress(ByVal hModule As IntPtr, ByVal lpProcName As String) As IntPtr
                End Function

                Shared Function isAvailable() As Boolean
                    If Not bTried Then
                        hLibInst = LoadLibrary("subacq4.dll")
                        bTried = True
                    End If
                    If hLibInst <> IntPtr.Zero Then
                        Dim func As IntPtr
                        func = GetProcAddress(hLibInst, "subacq_copydib")
                        If func <> IntPtr.Zero Then
                            bAvailable = True
                        End If
                    End If

                    Return bAvailable
                End Function
                Public Shared Function copydib(ByRef param As SUBACQ_COPYDIB) As SUBACQERR
                    If Not isAvailable() Then
                        Return SUBACQERR.NO_DLL
                    End If

                    Return subacq_copydib(param)
                End Function
            End Class

            ' ---- call subacq4_copydib function in subacq4.dll ----


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

                Dim param As New SUBACQ_COPYDIB()
                param.size = Marshal.SizeOf(GetType(SUBACQ_COPYDIB))
                param._option = 0

                param.src = src.buf
                param.srcrowbytes = src.rowbytes
                param.srcpixeltype = src.type

                param.dst = dst.Scan0()
                param.dstrowbytes = dst.Stride
                param.dstpixeltype = dst.PixelFormat

                param.width = w
                param.height = h
                param.left = rect.Left
                param.top = rect.Top

                param.lut = 0
                param.lutmax = lutmax
                param.lutmin = lutmin

                Dim err As SUBACQERR
                err = subacqdll.copydib(param)

                bitmap.UnlockBits(dst)

                Return err
            End Function
        End Class
    End Class
End Namespace

#End If ' USE_SUBACQ_DLL
