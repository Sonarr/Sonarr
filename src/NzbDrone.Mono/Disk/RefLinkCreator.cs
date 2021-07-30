using System;
using Mono.Unix;
using Mono.Unix.Native;
using NLog;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Mono.Interop;

namespace NzbDrone.Mono.Disk
{
    public interface ICreateRefLink
    {
        bool TryCreateRefLink(string srcPath, string linkPath);
    }

    public class RefLinkCreator : ICreateRefLink
    {
        private readonly Logger _logger;
        private readonly bool _supported;

        public RefLinkCreator(Logger logger)
        {
            _logger = logger;

            // Only support x86_64 because we know the FICLONE value is valid for it
            _supported = OsInfo.IsLinux && (Syscall.uname(out var results) == 0 && results.machine == "x86_64");
        }

        public bool TryCreateRefLink(string srcPath, string linkPath)
        {
            if (!_supported)
            {
                return false;
            }

            try
            {
                using (var srcHandle = NativeMethods.open(srcPath, OpenFlags.O_RDONLY))
                {
                    if (srcHandle.IsInvalid)
                    {
                        _logger.Trace("Failed to create reflink at '{0}' to '{1}': Couldn't open source file", linkPath, srcPath);
                        return false;
                    }

                    using (var linkHandle = NativeMethods.open(linkPath, OpenFlags.O_WRONLY | OpenFlags.O_CREAT | OpenFlags.O_TRUNC))
                    {
                        if (linkHandle.IsInvalid)
                        {
                            _logger.Trace("Failed to create reflink at '{0}' to '{1}': Couldn't create new link file", linkPath, srcPath);
                            return false;
                        }

                        if (NativeMethods.clone_file(linkHandle, srcHandle) == -1)
                        {
                            var error = new UnixIOException();
                            linkHandle.Dispose();
                            Syscall.unlink(linkPath);
                            _logger.Trace("Failed to create reflink at '{0}' to '{1}': {2}", linkPath, srcPath, error.Message);
                            return false;
                        }

                        _logger.Trace("Created reflink at '{0}' to '{1}'", linkPath, srcPath);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Syscall.unlink(linkPath);
                _logger.Trace(ex, "Failed to create reflink at '{0}' to '{1}'", linkPath, srcPath);
                return false;
            }
        }
    }
}
