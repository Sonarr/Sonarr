// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;

using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.InventoryProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class IsUpgradeFixture : CoreTest
    {
        [Test]
        public void IsUpgrade_should_return_true_if_new_is_proper_and_current_isnt_even_if_cutoff_is_met()
        {
            var currentQuality = new Quality(QualityTypes.SDTV, false);
            var newQuality = new Quality(QualityTypes.SDTV, true);
            var cutoff = QualityTypes.SDTV;

            var result = InventoryProvider.IsUpgrade(currentQuality, newQuality, cutoff);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsUpgrade_should_return_true_if_new_quality_is_better_than_current_and_cutoff_is_not_met()
        {
            var currentQuality = new Quality(QualityTypes.SDTV, false);
            var newQuality = new Quality(QualityTypes.DVD, true);
            var cutoff = QualityTypes.DVD;

            var result = InventoryProvider.IsUpgrade(currentQuality, newQuality, cutoff);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void IsUpgrade_should_return_false_if_new_quality_is_same_as_current_and_cutoff_is_met()
        {
            var currentQuality = new Quality(QualityTypes.SDTV, false);
            var newQuality = new Quality(QualityTypes.SDTV, false);
            var cutoff = QualityTypes.SDTV;

            var result = InventoryProvider.IsUpgrade(currentQuality, newQuality, cutoff);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsUpgrade_should_return_false_if_new_quality_is_better_than_current_and_cutoff_is_met()
        {
            var currentQuality = new Quality(QualityTypes.SDTV, false);
            var newQuality = new Quality(QualityTypes.DVD, true);
            var cutoff = QualityTypes.SDTV;

            var result = InventoryProvider.IsUpgrade(currentQuality, newQuality, cutoff);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsUpgrade_should_return_false_if_new_quality_is_worse_than_current_and_cutoff_is_not_met()
        {
            var currentQuality = new Quality(QualityTypes.WEBDL, false);
            var newQuality = new Quality(QualityTypes.HDTV, true);
            var cutoff = QualityTypes.Bluray720p;

            var result = InventoryProvider.IsUpgrade(currentQuality, newQuality, cutoff);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsUpgrade_should_return_false_if_new_quality_is_worse_than_current_and_cutoff_is_met()
        {
            var currentQuality = new Quality(QualityTypes.WEBDL, false);
            var newQuality = new Quality(QualityTypes.HDTV, true);
            var cutoff = QualityTypes.WEBDL;

            var result = InventoryProvider.IsUpgrade(currentQuality, newQuality, cutoff);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsUpgrade_should_return_false_if_new_quality_is_the_same_as_current_and_cutoff_is_met()
        {
            var currentQuality = new Quality(QualityTypes.WEBDL, false);
            var newQuality = new Quality(QualityTypes.WEBDL, false);
            var cutoff = QualityTypes.WEBDL;

            var result = InventoryProvider.IsUpgrade(currentQuality, newQuality, cutoff);

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void IsUpgrade_should_return_true_if_new_quality_is_a_proper_with_the_same_quality_as_current_and_cutoff_is_not_met()
        {
            var currentQuality = new Quality(QualityTypes.WEBDL, false);
            var newQuality = new Quality(QualityTypes.WEBDL, true);
            var cutoff = QualityTypes.Bluray720p;

            var result = InventoryProvider.IsUpgrade(currentQuality, newQuality, cutoff);

            //Assert
            result.Should().BeTrue();
        }
    }
}