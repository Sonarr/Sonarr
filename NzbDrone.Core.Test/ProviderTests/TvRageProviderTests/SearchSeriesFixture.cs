// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using TvdbLib.Data;
using TvdbLib.Exceptions;

namespace NzbDrone.Core.Test.ProviderTests.TvRageProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SearchSeriesFixture : CoreTest
    {
        private const string search = "http://services.tvrage.com/feeds/full_search.php?show=";

        private void WithEmptyResults()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadStream(It.Is<String>(u => u.StartsWith(search)), null))
                    .Returns(new FileStream(@".\Files\TVRage\SearchResults_empty.xml", FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        private void WithManyResults()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadStream(It.Is<String>(u => u.StartsWith(search)), null))
                    .Returns(new FileStream(@".\Files\TVRage\SearchResults_many.xml", FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        private void WithOneResult()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadStream(It.Is<String>(u => u.StartsWith(search)), null))
                    .Returns(new FileStream(@".\Files\TVRage\SearchResults_one.xml", FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        [Test]
        public void should_be_empty_when_no_results_are_found()
        {
            WithEmptyResults();
            Mocker.Resolve<TvRageProvider>().SearchSeries("asdasdasdasdas").Should().BeEmpty();
        }

        [Test]
        public void should_be_have_more_than_one_when_multiple_results_are_returned()
        {
            WithManyResults();
            Mocker.Resolve<TvRageProvider>().SearchSeries("top+gear").Should().NotBeEmpty();
        }

        [Test]
        public void should_have_one_when_only_one_result_is_found()
        {
            WithOneResult();
            Mocker.Resolve<TvRageProvider>().SearchSeries("suits").Should().HaveCount(1);
        }

        [Test]
        public void ended_should_not_have_a_value_when_series_has_not_ended()
        {
            WithOneResult();
            Mocker.Resolve<TvRageProvider>().SearchSeries("suits").First().Ended.HasValue.Should().BeFalse();
        }

        [Test]
        public void started_should_match_series()
        {
            WithOneResult();
            Mocker.Resolve<TvRageProvider>().SearchSeries("suits").First().Started.Should().Be(new DateTime(2011, 6, 23));
        }
    }
}