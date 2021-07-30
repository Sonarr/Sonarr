using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using NUnit.Framework;
using NzbDrone.Common.Test.DiskTests;
using NzbDrone.Windows.Disk;

namespace NzbDrone.Windows.Test.DiskProviderTests
{
    [TestFixture]
    [Platform("Win")]
    public class DiskProviderFixture : DiskProviderFixtureBase<DiskProvider>
    {
        public DiskProviderFixture()
        {
            WindowsOnly();
        }

        protected override void SetWritePermissions(string path, bool writable)
        {
            // Remove Write permissions, we're owner and have Delete permissions, so we can still clean it up.

            var owner = WindowsIdentity.GetCurrent().Owner;
            var accessControlType = writable ? AccessControlType.Allow : AccessControlType.Deny;

            if (Directory.Exists(path))
            {
                var directory = new DirectoryInfo(path);
                var ds = directory.GetAccessControl();
                ds.SetAccessRule(new FileSystemAccessRule(owner, FileSystemRights.Write, accessControlType));
                directory.SetAccessControl(ds);
            }
            else
            {
                var file = new FileInfo(path);
                var fs = file.GetAccessControl();
                fs.SetAccessRule(new FileSystemAccessRule(owner, FileSystemRights.Write, accessControlType));
                file.SetAccessControl(fs);
            }
        }
    }
}
