using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace Our.Umbraco.FileSystemProviders.Samba.Net {
    
    /// <summary>
    /// Source: http://stackoverflow.com/a/39540451/1387407
    /// </summary>
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private bool _isHandleReleased = false;

        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            if (this._isHandleReleased) return false;

            this._isHandleReleased = true;
            return CloseHandle(handle);
        }

        protected override void Dispose(bool disposing)
        {
            this.ReleaseHandle();

            base.Dispose(disposing);
        }
    }
}
