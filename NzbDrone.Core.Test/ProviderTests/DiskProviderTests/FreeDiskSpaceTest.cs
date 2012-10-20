using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.DiskProviderTests
{
    [TestFixture]
    public class FreeDiskSpaceTest : CoreTest
    {
        [Test]
        public void should_return_free_disk_space()
        {
            var di = new DirectoryInfo(Directory.GetCurrentDirectory());
            var result = Mocker.Resolve<DiskProvider>().FreeDiskSpace(di);

            //Checks to ensure that the free space on the first is greater than 0 (It should be in 99.99999999999999% of cases... I hope)
            result.Should().BeGreaterThan(0);
        }

        [Test]
        public void should_throw_if_directoy_does_not_exist()
        {
            var di = new DirectoryInfo(@"Z:\NOT_A_REAL_PATH\DOES_NOT_EXIST");
            Assert.Throws<DirectoryNotFoundException>(() => Mocker.Resolve<DiskProvider>().FreeDiskSpace(di));
        }
    }
}
