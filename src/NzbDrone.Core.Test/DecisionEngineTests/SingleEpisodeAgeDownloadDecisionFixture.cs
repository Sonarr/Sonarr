using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SingleEpisodeAgeDownloadDecisionFixture : CoreTest<SeasonPackOnlySpecification>
    {
        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private Series _series;
        private List<Episode> _episodes;
        private SeasonSearchCriteria _multiSearch;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                    .With(s => s.Seasons = Builder<Season>.CreateListOfSize(1).Build().ToList())
                                    .With(s => s.SeriesType = SeriesTypes.Standard)
                                    .Build();

            _episodes = new List<Episode>();
            _episodes.Add(CreateEpisodeStub(1, 400));
            _episodes.Add(CreateEpisodeStub(2, 370));
            _episodes.Add(CreateEpisodeStub(3, 340));
            _episodes.Add(CreateEpisodeStub(4, 310));

            _multiSearch = new SeasonSearchCriteria();
            _multiSearch.Episodes = _episodes.ToList();
            _multiSearch.SeasonNumber = 1;

            _parseResultMulti = new RemoteEpisode
            {
                Series = _series,
                Release = new ReleaseInfo(),
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)), FullSeason = true },
                Episodes = _episodes.ToList()
            };

            _parseResultSingle = new RemoteEpisode
            {
                Series = _series,
                Release = new ReleaseInfo(),
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                Episodes = new List<Episode>()
            };
        }

        private Episode CreateEpisodeStub(int number, int age)
        {
            return new Episode()
                   {
                        SeasonNumber = 1,
                        EpisodeNumber = number,
                        AirDateUtc = DateTime.UtcNow.AddDays(-age)
                   };
        }

        [TestCase(1, 200, false)]
        [TestCase(4, 200, false)]
        [TestCase(1, 600, true)]
        [TestCase(1, 365, true)]
        [TestCase(4, 365, true)]
        [TestCase(1, 0, true)]
        public void single_episode_release(int episode, int seasonSearchMaximumSingleEpisodeAge, bool expectedResult)
        {
            _parseResultSingle.Release.SeasonSearchMaximumSingleEpisodeAge = seasonSearchMaximumSingleEpisodeAge;
            _parseResultSingle.Episodes.Clear();
            _parseResultSingle.Episodes.Add(_episodes.Find(e => e.EpisodeNumber == episode));

            Subject.IsSatisfiedBy(_parseResultSingle, _multiSearch).Accepted.Should().Be(expectedResult);
        }

        // should always accept all season packs
        [TestCase(200, true)]
        [TestCase(600, true)]
        [TestCase(365, true)]
        [TestCase(0, true)]
        public void multi_episode_release(int seasonSearchMaximumSingleEpisodeAge, bool expectedResult)
        {
            _parseResultMulti.Release.SeasonSearchMaximumSingleEpisodeAge = seasonSearchMaximumSingleEpisodeAge;

            Subject.IsSatisfiedBy(_parseResultMulti, _multiSearch).Accepted.Should().BeTrue();
        }
    }
}
