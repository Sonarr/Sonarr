using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AutoMoq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DiskProviderTests
{
    [TestFixture]
    public class FreeDiskSpaceTest : CoreTest
    {
        [Test]
        public void FreeDiskSpace()
        {
            //Setup
            var mocker = new AutoMoqer();

            //Act
            var di = new DirectoryInfo(Directory.GetCurrentDirectory());
            var result = mocker.Resolve<DiskProvider>().FreeDiskSpace(di);

            //Asert
            //Checks to ensure that the free space on the first is greater than 0 (It should be in 99.99999999999999% of cases... I hope)
            result.Should().BeGreaterThan(0);
        }
    }
}
