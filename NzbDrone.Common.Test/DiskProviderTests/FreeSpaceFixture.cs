using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskProviderTests
{
    [TestFixture]
    public class IsParentFixture : TestBase<DiskProvider>
    {
        private string _parent = @"C:\Test".AsOsAgnostic();

        [Test]
        public void should_return_false_when_not_a_child()
        {
            var path = @"C:\Another Folder".AsOsAgnostic();

            Subject.IsParent(_parent, path).Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_folder_is_parent_of_another_folder()
        {
            var path = @"C:\Test\TV".AsOsAgnostic();

            Subject.IsParent(_parent, path).Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_folder_is_parent_of_a_file()
        {
            var path = @"C:\Test\30.Rock.S01E01.Pilot.avi".AsOsAgnostic();

            Subject.IsParent(_parent, path).Should().BeTrue();
        }
    }
}
