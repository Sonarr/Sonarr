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
        [Test]
        public void should_return_false_when_not_a_child()
        {
            Subject.IsParent(@"C:\Test", @"C:\Another Folder").Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_folder_is_parent_of_another_folder()
        {
            Subject.IsParent(@"C:\Test", @"C:\Test\TV").Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_folder_is_parent_of_a_file()
        {
            Subject.IsParent(@"C:\Test", @"C:\Test\30.Rock.S01E01.Pilot.avi").Should().BeTrue();
        }
    }
}
