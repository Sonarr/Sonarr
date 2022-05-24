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

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class SingleEpisodeAgeDownloadDecisionFixture : CoreTest<SeasonPackOnlySpecification>
    {
        private RemoteEpisode parseResultMulti;
        private RemoteEpisode parseResultSingle;
        private Series series;

        [SetUp]
        public void Setup()
        {
            series = Builder<Series>.CreateNew()
                                    .With(s => s.Seasons = Builder<Season>.CreateListOfSize(2).Build().ToList())
                                    .Build();

            parseResultMulti = new RemoteEpisode
            {
                Series = series,
                Release = new ReleaseInfo(),
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                Episodes = Builder<Episode>.CreateListOfSize(6).All().With(s => s.SeasonNumber = 1).BuildList()
            };

            parseResultSingle = new RemoteEpisode
            {
                Series = series,
                Release = new ReleaseInfo(),
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                Episodes = new List<Episode> {
                                            Builder<Episode>.CreateNew()
                                                .With(s => s.SeasonNumber = 1)
                                                .With(s => s.EpisodeNumber = 1)
                                                .Build()
                                        }

            };

        }


        [TestCase(500, 365, false)]
        [TestCase(10, 365, true)]
        [TestCase(500, 0, true)]
        public void single_episode_release(int airTimeAge, int MaximumSingleEpisodeAge, bool expectedResult)
        {
            parseResultSingle.Release.MaximumSingleEpisodeAge = MaximumSingleEpisodeAge;
            parseResultSingle.Episodes.First().AirDateUtc = DateTime.UtcNow.AddDays(-airTimeAge);

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void multi_episode_release()
        {
            parseResultMulti.Release.MaximumSingleEpisodeAge = 365;
            parseResultMulti.Episodes.First().AirDateUtc = DateTime.UtcNow.AddDays(-530);
            parseResultMulti.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddDays(-500);

            Subject.IsSatisfiedBy(parseResultMulti, null).Accepted.Should().BeTrue();
        }

    }
}
