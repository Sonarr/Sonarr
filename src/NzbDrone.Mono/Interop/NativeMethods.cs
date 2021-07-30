using System.Runtime.InteropServices;
using Mono.Unix.Native;

namespace NzbDrone.Mono.Interop
{
    internal enum IoctlRequest : uint
    {
        // Hardcoded ioctl for FICLONE on a typical linux system
        // #define FICLONE _IOW(0x94, 9, int)
        FICLONE = 0x40049409
    }

    internal static class NativeMethods
    {
        [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
        private static extern int Ioctl(SafeUnixHandle dst_fd, IoctlRequest request, SafeUnixHandle src_fd);

        public static SafeUnixHandle open(string pathname, OpenFlags flags)
        {
            return new SafeUnixHandle(Syscall.open(pathname, flags, FilePermissions.DEFFILEMODE));
        }

        internal static int clone_file(SafeUnixHandle link_fd, SafeUnixHandle src_fd)
        {
            return Ioctl(link_fd, IoctlRequest.FICLONE, src_fd);
        }
    }
}
