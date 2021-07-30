using System;
using System.Runtime.InteropServices;
using Mono.Unix.Native;

namespace NzbDrone.Mono.Interop
{
    internal sealed class SafeUnixHandle : SafeHandle
    {
        private SafeUnixHandle()
            : base(new IntPtr(-1), true)
        {
        }

        public SafeUnixHandle(int fd)
            : base(new IntPtr(-1), true)
        {
            handle = new IntPtr(fd);
        }

        public override bool IsInvalid
        {
            get { return handle == new IntPtr(-1); }
        }

        protected override bool ReleaseHandle()
        {
            return Syscall.close(handle.ToInt32()) != -1;
        }
    }
}
