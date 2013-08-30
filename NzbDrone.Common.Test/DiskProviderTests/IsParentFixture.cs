using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.DiskProviderTests
{
    [TestFixture]
    public class FreeSpaceFixture : TestBase<DiskProvider>
    {
        [Test]
        public void should_get_free_space_for_folder()
        {
            var path = @"C:\".AsOsAgnostic();

            Subject.GetAvilableSpace(path).Should().NotBe(0);
        }

        [Test]
        public void should_get_free_space_for_folder_that_doesnt_exist()
        {
            var path = @"C:\".AsOsAgnostic();

            Subject.GetAvilableSpace(Path.Combine(path, "invalidFolder")).Should().NotBe(0);
        }


        [Test]
        public void should_get_free_space_for_drive_that_doesnt_exist()
        {
            WindowsOnly();

            Subject.GetAvilableSpace("J:\\").Should().NotBe(0);
        }
    }
}
