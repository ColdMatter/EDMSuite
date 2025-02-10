// subacq4.cs

// implementation of subacq.copydib() function. SUBACQ4.DLL is not necessary.

using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using Hamamatsu.DCAM4;

#if ! USE_SUBACQ_DLL

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

    public class subacq
    {
        public static SUBACQERR copydib(ref Bitmap bitmap, DCAMBUF_FRAME src, ref Rectangle rect, int lutmax, int lutmin, int bpp)
        {
            int w = rect.Width;
            int h = rect.Height;
            if (w > bitmap.Width) w = bitmap.Width;
            if (h > bitmap.Height) h = bitmap.Height;
            if (w > src.width) w = src.width;
            if (h > src.height) h = src.height;

            SUBACQERR err;
            BitmapData dst = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            if (src.type == DCAM_PIXELTYPE.MONO16)
                err = copydib_rgb24_from_mono16(dst.Scan0, dst.Stride, src.buf, src.rowbytes, w, h, lutmax, lutmin, bpp);
            else
                err = SUBACQERR.INVALID_SRCPIXELTYPE;

            bitmap.UnlockBits(dst);

            return err;
        }

        //public static SUBACQERR copydib(ref Bitmap bitmap, DCAMBUF_FRAME src, ref Rectangle rect, int lutmax, int lutmin, int bpp)
        //{
        //    int w = rect.Width;
        //    int h = rect.Height;
        //    if (w > bitmap.Width) w = bitmap.Width;
        //    if (h > bitmap.Height) h = bitmap.Height;
        //    if (w > src.width) w = src.width;
        //    if (h > src.height) h = src.height;

        //    SUBACQERR err;
        //    BitmapData dst = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);

        //    if (src.type == DCAM_PIXELTYPE.MONO16)
        //        err = copydib_gray8_from_mono16(dst.Scan0, dst.Stride, src.buf, src.rowbytes, w, h, lutmax, lutmin, bpp);
        //    else
        //        err = SUBACQERR.INVALID_SRCPIXELTYPE;

        //    bitmap.UnlockBits(dst);

        //    // Apply grayscale palette (required for 8-bit images)
        //    ColorPalette pal = bitmap.Palette;
        //    for (int i = 0; i < 256; i++)
        //        pal.Entries[i] = Color.FromArgb(i, i, i);
        //    bitmap.Palette = pal;

        //    return err;
        //}

    //    private static SUBACQERR copydib_gray8_from_mono16(
    //IntPtr dst, Int32 dstrowbytes, IntPtr src, Int32 srcrowbytes, Int32 width, Int32 height,
    //Int32 lutmax, Int32 lutmin, Int32 bpp)
    //    {
    //        Int16[] s = new Int16[width];
    //        byte[] d = new byte[dstrowbytes];

    //        double gain = 256.0 / (lutmax - lutmin + 1);
    //        double inBase = lutmin;

    //        for (int y = 0; y < height; y++)
    //        {
    //            Int32 offsetSrc = srcrowbytes * y;
    //            Marshal.Copy((IntPtr)(src.ToInt64() + offsetSrc), s, 0, width);

    //            for (int x = 0; x < width; x++)
    //            {
    //                UInt16 u = (UInt16)s[x];
    //                double v = gain * (u - inBase);

    //                d[x] = (Byte)(v > 255 ? 255 : (v < 0 ? 0 : v)); // Clamp to 0-255
    //            }

    //            Int32 offsetDst = dstrowbytes * y;
    //            Marshal.Copy(d, 0, (IntPtr)(dst.ToInt64() + offsetDst), width);
    //        }
    //        return SUBACQERR.SUCCESS;
    //    }

        private static SUBACQERR copydib_rgb24_from_mono16(IntPtr dst, Int32 dstrowbytes, IntPtr src, Int32 srcrowbytes, Int32 width, Int32 height, Int32 lutmax, Int32 lutmin, Int32 bpp)
        {
            Int16[] s = new Int16[width];
            byte[] d = new byte[dstrowbytes];

            double gain = 0;
            double inBase = 0;

            if (lutmax != lutmin)
            {
                if (lutmin < lutmax)
                {
                    gain = 256.0 / (lutmax - lutmin + 1);
                    inBase = lutmin;
                }
                else
                if (lutmin > lutmax)
                {
                    gain = 256.0 / (lutmax - lutmin - 1);
                    inBase = lutmax;
                }
            }
            else
            if (lutmin > 0)    // binary threshold
            {
                gain = 0;
                inBase = lutmin;
            }

            Int16 y;
            for (y = 0; y < height; y++)
            {
                Int32 offset;

                offset = srcrowbytes * y;
                Marshal.Copy((IntPtr)(src.ToInt64() + offset), s, 0, width);

                copydibline_rgb24_from_mono16(d, s, width, gain, inBase, bpp);

                offset = dstrowbytes * y;
                Marshal.Copy(d, 0, (IntPtr)(dst.ToInt64() + offset), dstrowbytes);
            }
            return SUBACQERR.SUCCESS;
        }

        private static void copydibline_rgb24_from_mono16(byte[] d, Int16[] s, Int32 width, double gain, double inBase, Int32 bpp)
        {
            Int16 x;
            Int16 i = 0;
            if (gain != 0)
            {
                for (x = 0; x < width; x++)
                {
                    UInt16 u = (UInt16)s[x];

                    double v = gain * (u - inBase);

                    Byte c;
                    if (v > 255)
                        c = 255;
                    else
                    if (v < 0)
                        c = 0;
                    else
                        c = (Byte)v;

                    d[i++] = c;
                    d[i++] = c;
                    d[i++] = c;
                }
            }
            else
            if (inBase > 0)    // binary threshold
            {
                for (x = 0; x < width; x++)
                {
                    UInt16 u = (UInt16)s[x];

                    Byte c = (Byte)(u >= inBase ? 255 : 0);

                    d[i++] = c;
                    d[i++] = c;
                    d[i++] = c;
                }
            }
            else
            {
                for (x = 0; x < width; x++)
                {
                    UInt16 u = (UInt16)s[x];

                    Byte c = (Byte)(u >> (bpp - 8));

                    d[i++] = c;
                    d[i++] = c;
                    d[i++] = c;
                }
            }
        }
    }
}

#endif // ! USE_SUBACQ_DLL
