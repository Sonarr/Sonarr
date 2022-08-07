using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Parser.Model;
using NUnit.Framework;
using FluentAssertions;
using FizzWare.NBuilder;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SingleEpisodeAgeDownloadDecisionFixture : CoreTest<SeasonPackOnlySpecification>
    {
        private RemoteEpisode parseResultMulti;
        private RemoteEpisode parseResultSingle;
        private Series series;
        private List<Episode> episodes;
        private SeasonSearchCriteria multiSearch;

        [SetUp]
        public void Setup()
        {
            series = Builder<Series>.CreateNew()
                                    .With(s => s.Seasons = Builder<Season>.CreateListOfSize(1).Build().ToList())
                                    .With(s => s.SeriesType = SeriesTypes.Standard)
                                    .Build();

            episodes = new List<Episode>();
            episodes.Add(CreateEpisodeStub(1, 400));
            episodes.Add(CreateEpisodeStub(2, 370));
            episodes.Add(CreateEpisodeStub(3, 340));
            episodes.Add(CreateEpisodeStub(4, 310));

            multiSearch = new SeasonSearchCriteria();
            multiSearch.Episodes = episodes.ToList();
            multiSearch.SeasonNumber = 1;

            parseResultMulti = new RemoteEpisode
            {
                Series = series,
                Release = new ReleaseInfo(),
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)), FullSeason = true },
                Episodes = episodes.ToList()
            };

            parseResultSingle = new RemoteEpisode
            {
                Series = series,
                Release = new ReleaseInfo(),
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                Episodes = new List<Episode>()
            };
        }

        Episode CreateEpisodeStub(int number, int age)
        {
            return new Episode() { 
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
        public void single_episode_release(int episode, int SeasonSearchMaximumSingleEpisodeAge, bool expectedResult)
        {
            parseResultSingle.Release.SeasonSearchMaximumSingleEpisodeAge = SeasonSearchMaximumSingleEpisodeAge;
            parseResultSingle.Episodes.Clear();
            parseResultSingle.Episodes.Add(episodes.Find(e => e.EpisodeNumber == episode));

            Subject.IsSatisfiedBy(parseResultSingle, multiSearch).Accepted.Should().Be(expectedResult);
        }

        // should always accept all season packs
        [TestCase(200, true)]
        [TestCase(600, true)]
        [TestCase(365, true)]
        [TestCase(0, true)]
        public void multi_episode_release(int SeasonSearchMaximumSingleEpisodeAge, bool expectedResult)
        {
            parseResultMulti.Release.SeasonSearchMaximumSingleEpisodeAge = SeasonSearchMaximumSingleEpisodeAge;

            Subject.IsSatisfiedBy(parseResultMulti, multiSearch).Accepted.Should().BeTrue();
        }
    }
}
