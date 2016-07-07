using System.IO;
using NzbDrone.Common.Disk.Abstractions;

namespace NzbDrone.Common.Disk {
    public static class FileSystemInfoFactory {
        public static IFileSystemInfo CreateFrom(FileSystemInfo fileSystemInfo) {
            if (fileSystemInfo == null)
                return null;


            return (IFileSystemInfo)CreateFrom(fileSystemInfo as FileInfo) ??
                                    CreateFrom(fileSystemInfo as DirectoryInfo);
        }
        public static IFileInfo CreateFrom(FileInfo fileInfo) {
            if (fileInfo == null)
                return null;

            return new FileInfoWrapper(fileInfo);
        }

        public static IDirectoryInfo CreateFrom(DirectoryInfo directoryInfo) {
            if (directoryInfo == null)
                return null;

            return new DirectoryInfoWrapper(directoryInfo);
        }
    }
}
