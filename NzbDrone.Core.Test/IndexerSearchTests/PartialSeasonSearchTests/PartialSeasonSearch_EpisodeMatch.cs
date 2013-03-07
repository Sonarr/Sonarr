using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerSearchTests.PartialSeasonSearchTests
{
    [TestFixture]
    public class PartialSeasonSearch_EpisodeMatch : TestBase
    {
        private Series _series;
        private List<Episode> _episodes;
        private EpisodeParseResult _episodeParseResult;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _episodes = Builder<Episode>
                    .CreateListOfSize(10)
                    .All()
                    .With(e => e.SeriesId = _series.Id)
                    .With(e => e.Series = _series)
                    .Build()
                    .ToList();

            _episodeParseResult = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.SeasonNumber = 1)
                    .Build();

        }

        [Test]
        public void should_return_wrongSeason_when_season_does_not_match()
        {
            Mocker.Resolve<PartialSeasonSearch>()
                  .IsEpisodeMatch(_series, new { SeasonNumber = 2, Episodes = _episodes }, _episodeParseResult)
                  .Should().BeFalse();
        }

        [Test]
        public void should_not_return_error_when_season_matches()
        {
            Mocker.Resolve<PartialSeasonSearch>()
                  .IsEpisodeMatch(_series, new { SeasonNumber = 1, Episodes = _episodes }, _episodeParseResult)
                  .Should().BeFalse();
        }
    }
}
