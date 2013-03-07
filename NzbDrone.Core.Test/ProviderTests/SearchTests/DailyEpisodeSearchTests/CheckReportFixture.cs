using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Search;

using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.SearchTests.DailyEpisodeSearchTests
{
    [TestFixture]
    public class CheckReportFixture : TestBase
    {
        private Series _series;
        private Episode _episode;
        private EpisodeParseResult _episodeParseResult;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _episode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.SeriesId = _series.Id)
                    .With(e => e.Series = _series)
                    .Build();

            _episodeParseResult = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.AirDate = _episode.AirDate)
                    .With(p => p.Episodes = new List<Episode> { _episode })
                    .With(p => p.Series = _series)
                    .Build();
        }

        [Test]
        public void should_return_WrongEpisode_is_parseResult_doesnt_have_airdate()
        {
            _episodeParseResult.AirDate = null;

            Mocker.Resolve<DailyEpisodeSearch>()
                  .IsEpisodeMatch(_series, new { Episode = _episode }, _episodeParseResult).Should().BeFalse();
        }

        [Test]
        public void should_return_WrongEpisode_is_parseResult_airdate_doesnt_match_episode()
        {
            _episodeParseResult.AirDate = _episode.AirDate.Value.AddDays(-10);

            Mocker.Resolve<DailyEpisodeSearch>()
                  .IsEpisodeMatch(_series, new { Episode = _episode }, _episodeParseResult)
                  .Should()
                  .BeFalse();
        }

        [Test]
        public void should_not_return_error_when_airDates_match()
        {
            Mocker.Resolve<DailyEpisodeSearch>()
                  .IsEpisodeMatch(_series, new { Episode = _episode }, _episodeParseResult)
                  .Should().BeFalse();
        }
    }
}
