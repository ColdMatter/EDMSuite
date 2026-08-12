using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SpectrumDDS
{
    /// <summary>Raised when the Spectrum driver returns a non-zero error code.</summary>
    public class SpcmException : Exception
    {
        public uint ErrorCode { get; private set; }

        public SpcmException(uint errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    /// <summary>
    /// Thin, checked wrapper around one open handle to a Spectrum card.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bound to <c>spcm_win64.dll</c> by name, which the Spectrum installer puts in
    /// System32, so this builds and runs on any machine with the driver installed.
    /// (The project this replaces referenced <c>SpcmDrv64.NET.dll</c> by an absolute
    /// <c>HintPath</c> into someone's E: drive and was unbuildable anywhere else.)
    /// </para>
    /// <para>
    /// Every call is checked. This matters more than usual on this card: an
    /// out-of-range register write is <em>rejected</em>, not clamped -- writing 30 s
    /// to <c>SPC_DDS_TRG_TIMER</c> returns error 257 and leaves the previous value
    /// in place -- so code that ignores return codes runs happily with stale
    /// settings and no indication anything went wrong.
    /// </para>
    /// </remarks>
    public class SpcmCard : IDisposable
    {
        public const string DefaultDevice = "/dev/spcm0";

        // Verified against dumpbin /EXPORTS: the x64 build exports undecorated
        // names (no leading underscore, no @N suffix -- that decoration is a
        // 32-bit stdcall thing and spcm_win64.dll is 64-bit only). Anything
        // loading this assembly must therefore run as x64.
        private const string Dll = "spcm_win64.dll";

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private static extern IntPtr spcm_hOpen([MarshalAs(UnmanagedType.LPStr)] string deviceName);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi)]
        private static extern void spcm_vClose(IntPtr device);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi)]
        private static extern uint spcm_dwSetParam_i32(IntPtr device, int register, int value);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi)]
        private static extern uint spcm_dwSetParam_i64(IntPtr device, int register, long value);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi)]
        private static extern uint spcm_dwSetParam_d64(IntPtr device, int register, double value);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi)]
        private static extern uint spcm_dwGetParam_i32(IntPtr device, int register, out int value);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi)]
        private static extern uint spcm_dwGetParam_i64(IntPtr device, int register, out long value);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi)]
        private static extern uint spcm_dwGetParam_d64(IntPtr device, int register, out double value);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Ansi)]
        private static extern uint spcm_dwGetErrorInfo_i32(IntPtr device, IntPtr errorReg, IntPtr errorValue, StringBuilder errorText);

        [DllImport(Dll, CallingConvention = CallingConvention.Winapi)]
        private static extern uint spcm_dwSetParam_ptr(IntPtr device, int register, byte[] buffer, ulong length);

        private IntPtr handle = IntPtr.Zero;

        // Serialises individual driver calls, so that a status poll from the GUI
        // thread cannot land in the middle of one from the shot thread. It does
        // *not* make a multi-register sequence atomic -- queueing a pattern is over
        // a hundred writes that must not be interleaved with a manual EXEC_NOW, and
        // that is the caller's business.
        private readonly object gate = new object();

        public string Device { get; private set; }
        public bool IsOpen { get { return handle != IntPtr.Zero; } }

        public SpcmCard() : this(DefaultDevice) { }

        public SpcmCard(string device)
        {
            Device = device;
        }

        public void Open()
        {
            if (IsOpen) return;
            handle = spcm_hOpen(Device);
            if (handle == IntPtr.Zero)
                throw new SpcmException(0, "could not open " + Device +
                    " -- is the card present and the Spectrum driver installed?");
        }

        public void Close()
        {
            if (!IsOpen) return;
            spcm_vClose(handle);
            handle = IntPtr.Zero;
        }

        public void Dispose()
        {
            Close();
            GC.SuppressFinalize(this);
        }

        ~SpcmCard()
        {
            Close();
        }

        // -- checked register access -----------------------------------------

        private void Check(uint err, string what)
        {
            if (err == 0) return;
            StringBuilder text = new StringBuilder(SpcmRegs.ERRORTEXTLEN);
            spcm_dwGetErrorInfo_i32(handle, IntPtr.Zero, IntPtr.Zero, text);
            throw new SpcmException(err, string.Format(
                "{0} failed (error {1}): {2}", what, err, text.ToString().Trim()));
        }

        private void RequireOpen()
        {
            if (!IsOpen) throw new InvalidOperationException("the card is not open");
        }

        public void SetInt(int register, int value)
        {
            lock (gate)
            {
                RequireOpen();
                Check(spcm_dwSetParam_i32(handle, register, value), Describe("SetInt", register));
            }
        }

        public void SetLong(int register, long value)
        {
            lock (gate)
            {
                RequireOpen();
                Check(spcm_dwSetParam_i64(handle, register, value), Describe("SetLong", register));
            }
        }

        public void SetDouble(int register, double value)
        {
            lock (gate)
            {
                RequireOpen();
                Check(spcm_dwSetParam_d64(handle, register, value), Describe("SetDouble", register));
            }
        }

        public int GetInt(int register)
        {
            lock (gate)
            {
                RequireOpen();
                int value;
                Check(spcm_dwGetParam_i32(handle, register, out value), Describe("GetInt", register));
                return value;
            }
        }

        public long GetLong(int register)
        {
            lock (gate)
            {
                RequireOpen();
                long value;
                Check(spcm_dwGetParam_i64(handle, register, out value), Describe("GetLong", register));
                return value;
            }
        }

        public double GetDouble(int register)
        {
            lock (gate)
            {
                RequireOpen();
                double value;
                Check(spcm_dwGetParam_d64(handle, register, out value), Describe("GetDouble", register));
                return value;
            }
        }

        private static string Describe(string op, int register)
        {
            return string.Format("{0}(register {1})", op, register);
        }

        /// <summary>
        /// Write a custom line to the driver's debug log (SPC_WRITE_TO_LOG), for
        /// marking connection/pattern boundaries when the debug log is on.
        /// </summary>
        /// <remarks>
        /// The debug log is driver-global, not tied to a device handle -- the
        /// manual's own example calls this with a NULL handle -- so this works
        /// whether or not this card is open, and deliberately does not go through
        /// <see cref="Check"/>: a logging call failing (for example because debug
        /// logging is off) must never be allowed to break card operation.
        /// </remarks>
        public static void WriteLogLine(string text)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(text);
                spcm_dwSetParam_ptr(IntPtr.Zero, SpcmRegs.SPC_WRITE_TO_LOG, bytes, (ulong)bytes.Length);
            }
            catch
            {
                // Best-effort diagnostics only.
            }
        }
    }
}
