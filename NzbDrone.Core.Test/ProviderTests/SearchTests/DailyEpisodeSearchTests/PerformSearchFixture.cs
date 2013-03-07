using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Notification;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Search;

using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests.DailyEpisodeSearchTests
{
    [TestFixture]
    public class PerformSearchFixture : PerformSearchTestBase
    {
        [Test]
        public void should_throw_if_episode_is_null()
        {
            Episode nullEpisode = null;
            Assert.Throws<ArgumentException>(() => 
                                                Mocker.Resolve<DailyEpisodeSearch>()
                                                      .PerformSearch(_series, new { Episode = nullEpisode }, notification));
        }

        [Test]
        public void should_fetch_results_from_indexers()
        {
            WithValidIndexers();

            Mocker.Resolve<DailyEpisodeSearch>()
                  .PerformSearch(_series, new {Episode = _episode}, notification)
                  .Should()
                  .HaveCount(20);
        }

        [Test]
        public void should_log_error_when_fetching_from_indexer_fails()
        {
            WithInvalidIndexers();

            Mocker.Resolve<DailyEpisodeSearch>()
                  .PerformSearch(_series, new { Episode = _episode }, notification)
                  .Should()
                  .HaveCount(0);

            ExceptionVerification.ExpectedErrors(2);
        }
    }
}
