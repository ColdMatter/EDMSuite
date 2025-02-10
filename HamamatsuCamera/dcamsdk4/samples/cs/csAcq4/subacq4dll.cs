// subacq4dll.cs

// implementation of subacq.copydib() function. SUBACQ4.DLL is necessary.

using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using Hamamatsu.DCAM4;

#if USE_SUBACQ_DLL

namespace Hamamatsu.subacq4
{
    public enum SUBACQERR : uint
    {
        // error
        NO_DLL = 0x80000000,
        INVALID_ARG = 0x80000001,
        INVALID_DST = 0x80000011,
        INVALID_DSTPIXELFORMAT = 0x80000012,
        INVALID_SRC = 0x80000021,
        INVALID_SRCPIXELTYPE = 0x80000022,

        NOSUPPORT_LUTARRAY = 0x80000101,

        // success
        SUCCESS = 1
    }

    [StructLayout(LayoutKind.Sequential,Pack=4)]
    struct SUBACQ_COPYDIB
    {
        public Int32 size;					// [in] size of whole structure, not only this.
        public Int32 option;				// [in] 0 reserved

        public IntPtr src;
        public Int32 srcrowbytes;
        public DCAM_PIXELTYPE srcpixeltype;

        public IntPtr dst;
        public Int32 dstrowbytes;
        public PixelFormat dstpixelformat;		// .PixelFormat

        public Int32 width;
        public Int32 height;
        public Int32 left;
        public Int32 top;

        public IntPtr lut;
        public Int32 lutmax;
        public Int32 lutmin;
    }

    public class subacq
    {
        public static SUBACQERR copydib(ref Bitmap bitmap, DCAMBUF_FRAME src, ref Rectangle rect, int lutmax, int lutmin)
        {
            int w = rect.Width;
            int h = rect.Height;
            if (w > bitmap.Width) w = bitmap.Width;
            if (h > bitmap.Height) h = bitmap.Height;
            if (w > src.width) w = src.width;
            if (h > src.height) h = src.height;

            BitmapData dst = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            SUBACQ_COPYDIB param = new SUBACQ_COPYDIB();

            param.size = Marshal.SizeOf(typeof(SUBACQ_COPYDIB));
            param.option = 0;

            param.src = src.buf;
            param.srcrowbytes = src.rowbytes;
            param.srcpixeltype = src.type;

            param.dst = dst.Scan0;
            param.dstrowbytes = dst.Stride;
            param.dstpixelformat = dst.PixelFormat;

            param.width = w;
            param.height = h;
            param.left = rect.Left;
            param.top = rect.Top;

            param.lut = IntPtr.Zero;
            param.lutmax = lutmax;
            param.lutmin = lutmin;

            // ---- call subacq4_copydib function in subacq4.dll ----

            SUBACQERR   err = subacqdll.copydib(ref param);
            bitmap.UnlockBits(dst);

            return err;
        }

        // ---- helper funciton for dynamic load library ----

        static class subacqdll
        {
            [DllImport("subacq4")]
            public static extern SUBACQERR subacq_copydib(ref SUBACQ_COPYDIB param);
            static Boolean bTried = false;
            static Boolean bAvailable = false;
            static IntPtr hLibInst;

            [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern IntPtr LoadLibrary(string lpFileName);
            [DllImport("kernel32", SetLastError = true)]
            internal static extern bool FreeLibrary(IntPtr hModule);
            [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
            internal static Boolean isAvailable()
            {
                if( ! bTried )
                {
                    hLibInst = LoadLibrary("subacq4.dll");
                    bTried = true;
                }
                if( hLibInst != IntPtr.Zero )
                {
                    IntPtr func = GetProcAddress(hLibInst, "subacq_copydib");
                    if( func != IntPtr.Zero )
                        bAvailable = true;
                }
                return bAvailable;
            }
            public static SUBACQERR copydib(ref SUBACQ_COPYDIB param)
            {
                if (!isAvailable())
                    return SUBACQERR.NO_DLL;

                return subacq_copydib(ref param);
            }
        }
    }
}

#endif // USE_SUBACQ_DLL
