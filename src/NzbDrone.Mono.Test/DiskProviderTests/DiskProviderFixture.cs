using Mono.Unix;
using NUnit.Framework;
using NzbDrone.Common.Test.DiskTests;

namespace NzbDrone.Mono.Test.DiskProviderTests
{
    [TestFixture]
    [Platform("Mono")]
    public class DiskProviderFixture : DiskProviderFixtureBase<DiskProvider>
    {
        public DiskProviderFixture()
        {
            MonoOnly();
        }

        protected override void SetWritePermissions(string path, bool writable)
        {
            // Remove Write permissions, we're still owner so we can clean it up, but we'll have to do that explicitly.

            var entry = UnixFileSystemInfo.GetFileSystemEntry(path);

            if (writable)
            {
                entry.FileAccessPermissions |= FileAccessPermissions.UserWrite | FileAccessPermissions.GroupWrite | FileAccessPermissions.OtherWrite;
            }
            else
            {
                entry.FileAccessPermissions &= ~(FileAccessPermissions.UserWrite | FileAccessPermissions.GroupWrite | FileAccessPermissions.OtherWrite);
            }
        }
    }
}
