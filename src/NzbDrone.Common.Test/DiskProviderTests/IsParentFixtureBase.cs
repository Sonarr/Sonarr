using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskProviderTests
{
    public class IsParentFixtureBase<TSubject> : TestBase<TSubject> where TSubject : class, IDiskProvider
    public class IsParentFixture : TestBase
    {
        private string _parent = @"C:\Test".AsOsAgnostic();

        [Test]
        public void should_return_false_when_not_a_child()
        {
            var path = @"C:\Another Folder".AsOsAgnostic();

            DiskProvider.IsParent(_parent, path).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_folder_is_parent_of_another_folder()
        {
            var path = @"C:\Test\TV".AsOsAgnostic();

            DiskProvider.IsParent(_parent, path).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_folder_is_parent_of_a_file()
        {
            var path = @"C:\Test\30.Rock.S01E01.Pilot.avi".AsOsAgnostic();

            DiskProvider.IsParent(_parent, path).Should().BeTrue();
        }
    }
}
