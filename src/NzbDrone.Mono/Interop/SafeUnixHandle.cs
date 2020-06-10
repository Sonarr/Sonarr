using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Mono.Unix.Native;

namespace NzbDrone.Mono.Interop
{
    [SecurityPermission(SecurityAction.InheritanceDemand, UnmanagedCode = true)]
    [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
    internal sealed class SafeUnixHandle : SafeHandle
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        private SafeUnixHandle()
            : base(new IntPtr(-1), true)
        {
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public SafeUnixHandle(int fd)
            : base(new IntPtr(-1), true)
        {
            handle = new IntPtr(fd);
        }

        public override bool IsInvalid
        {
            get { return this.handle == new IntPtr(-1); }
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            return Syscall.close(this.handle.ToInt32()) != -1;
        }
    }
}
