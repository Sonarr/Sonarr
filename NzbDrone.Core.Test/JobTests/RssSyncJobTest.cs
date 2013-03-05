using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.JobTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RssSyncJobTest : CoreTest
    {
        public void WithMinutes(int minutes)
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.RssSyncInterval).Returns(minutes);
        }

        [TestCase(10)]
        [TestCase(15)]
        [TestCase(25)]
        [TestCase(60)]
        [TestCase(120)]
        public void should_use_value_from_config_provider(int minutes)
        {
            WithMinutes(minutes);
            Mocker.Resolve<RssSyncJob>().DefaultInterval.Should().Be(TimeSpan.FromMinutes(minutes));
        }
    }
}