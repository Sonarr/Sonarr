// ReSharper disable RedundantUsingDirective

using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RetentionSpecificationFixture : CoreTest
    {
        private RetentionSpecification retentionSpecification;

        private EpisodeParseResult parseResult;

        [SetUp]
        public void Setup()
        {
            retentionSpecification = Mocker.Resolve<RetentionSpecification>();

            parseResult = new EpisodeParseResult
            {
                Age = 100
            };
        }

        private void WithUnlimitedRetention()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(0);
        }

        private void WithLongRetention()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(1000);
        }

        private void WithShortRetention()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(10);
        }

        private void WithEqualRetention()
        {
            Mocker.GetMock<IConfigService>().SetupGet(c => c.Retention).Returns(100);
        }

        [Test]
        public void unlimited_retention_should_return_true()
        {
            WithUnlimitedRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void longer_retention_should_return_true()
        {
            WithLongRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void equal_retention_should_return_true()
        {
            WithEqualRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeTrue();
        }

        [Test]
        public void shorter_retention_should_return_false()
        {
            WithShortRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeFalse();
        }

        [Test]
        public void zeroDay_report_should_return_true()
        {
            WithUnlimitedRetention();
            retentionSpecification.IsSatisfiedBy(parseResult).Should().BeTrue();
        }
    }
}