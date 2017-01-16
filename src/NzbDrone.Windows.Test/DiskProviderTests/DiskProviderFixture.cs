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
                var ds = Directory.GetAccessControl(path);
                ds.SetAccessRule(new FileSystemAccessRule(owner, FileSystemRights.Write, accessControlType));
                Directory.SetAccessControl(path, ds);
            }
            else
            {
                var fs = File.GetAccessControl(path);
                fs.SetAccessRule(new FileSystemAccessRule(owner, FileSystemRights.Write, accessControlType));
                File.SetAccessControl(path, fs);
            }
        }
    }
}
