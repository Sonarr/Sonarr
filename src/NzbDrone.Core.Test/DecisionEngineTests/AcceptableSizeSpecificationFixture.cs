using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class AcceptableSizeSpecificationFixture : CoreTest<AcceptableSizeSpecification>
    {
        private RemoteEpisode _parseResultMultiSet;
        private RemoteEpisode _parseResultMulti;
        private RemoteEpisode _parseResultSingle;
        private Series _series;
        private List<Episode> _episodes;
        private QualityDefinition _qualityType;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                    .With(s => s.Seasons = Builder<Season>.CreateListOfSize(2).Build().ToList())
                                    .Build();

            _episodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(s => s.SeasonNumber = 1)
                .BuildList();

            _parseResultMultiSet = new RemoteEpisode
                                    {
                                        Series = _series,
                                        Release = new ReleaseInfo(),
                                        ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Episodes = Builder<Episode>.CreateListOfSize(6).All().With(s => s.SeasonNumber = 1).BuildList()
                                    };

            _parseResultMulti = new RemoteEpisode
                                    {
                                        Series = _series,
                                        Release = new ReleaseInfo(),
                                        ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Episodes = Builder<Episode>.CreateListOfSize(2).All().With(s => s.SeasonNumber = 1).BuildList()
                                    };

            _parseResultSingle = new RemoteEpisode
                                    {
                                        Series = _series,
                                        Release = new ReleaseInfo(),
                                        ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Episodes = new List<Episode>
                                        {
                                            Builder<Episode>.CreateNew()
                                                .With(s => s.SeasonNumber = 1)
                                                .With(s => s.EpisodeNumber = 1)
                                                .Build()
                                        }
                                    };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            _qualityType = Builder<QualityDefinition>.CreateNew()
                .With(q => q.MinSize = 2)
                .With(q => q.MaxSize = 10)
                .With(q => q.Quality = Quality.SDTV)
                .Build();

            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.Get(Quality.SDTV)).Returns(_qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(_episodes);
        }

        [TestCase(30, 50, false)]
        [TestCase(30, 250, true)]
        [TestCase(30, 500, false)]
        [TestCase(60, 100, false)]
        [TestCase(60, 500, true)]
        [TestCase(60, 1000, false)]
        public void single_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();
            _parseResultSingle.Episodes.First().Id = 5;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 500, true)]
        [TestCase(30, 1000, false)]
        [TestCase(60, 1000, true)]
        [TestCase(60, 2000, false)]
        public void should_return_expected_result_for_first_episode_of_season(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();
            _parseResultSingle.Episodes.First().Id = _episodes.First().Id;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 500, true)]
        [TestCase(30, 1000, false)]
        [TestCase(60, 1000, true)]
        [TestCase(60, 2000, false)]
        public void should_return_expected_result_for_last_episode_of_season(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();
            _parseResultSingle.Episodes.First().Id = _episodes.Last().Id;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 50 * 2, false)]
        [TestCase(30, 250 * 2, true)]
        [TestCase(30, 500 * 2, false)]
        [TestCase(60, 100 * 2, false)]
        [TestCase(60, 500 * 2, true)]
        [TestCase(60, 1000 * 2, false)]
        public void multi_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultMulti.Series = _series;
            _parseResultMulti.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 50 * 6, false)]
        [TestCase(30, 250 * 6, true)]
        [TestCase(30, 500 * 6, false)]
        [TestCase(60, 100 * 6, false)]
        [TestCase(60, 500 * 6, true)]
        [TestCase(60, 1000 * 6, false)]
        public void multiset_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            _series.Runtime = runtime;
            _parseResultMultiSet.Series = _series;
            _parseResultMultiSet.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(_parseResultMultiSet, null).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void should_return_true_if_size_is_zero()
        {
            _series.Runtime = 30;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Release.Size = 0;
            _qualityType.MinSize = 10;
            _qualityType.MaxSize = 20;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_30_minute()
        {
            _series.Runtime = 30;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Release.Size = 18457280000;
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_60_minute()
        {
            _series.Runtime = 60;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Release.Size = 36857280000;
            _qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_treat_daily_series_as_single_episode()
        {
            _series.Runtime = 60;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Series.SeriesType = SeriesTypes.Daily;
            _parseResultSingle.Release.Size = 300.Megabytes();

            _qualityType.MaxSize = 10;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_RAWHD()
        {
            _parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.RAWHD);

            _series.Runtime = 45;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Series.SeriesType = SeriesTypes.Daily;
            _parseResultSingle.Release.Size = 8000.Megabytes();

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_special()
        {
            _parseResultSingle.ParsedEpisodeInfo.Special = true;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_series_runtime_is_zero_and_single_episode_is_not_from_first_season()
        {
            _series.Runtime = 0;
            _parseResultSingle.Series = _series;
            _parseResultSingle.Episodes.First().Id = 5;
            _parseResultSingle.Release.Size = 200.Megabytes();
            _parseResultSingle.Episodes.First().SeasonNumber = 2;

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().Be(false);
        }

        [Test]
        public void should_return_false_if_series_runtime_is_zero_and_single_episode_aired_more_than_24_hours_after_first_aired_episode()
        {
            _series.Runtime = 0;

            _parseResultSingle.Series = _series;
            _parseResultSingle.Release.Size = 200.Megabytes();
            _parseResultSingle.Episodes.First().Id = 5;
            _parseResultSingle.Episodes.First().SeasonNumber = 1;
            _parseResultSingle.Episodes.First().EpisodeNumber = 2;
            _parseResultSingle.Episodes.First().AirDateUtc = _episodes.First().AirDateUtc.Value.AddDays(7);

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().Be(false);
        }

        [Test]
        public void should_return_true_if_series_runtime_is_zero_and_single_episode_aired_less_than_24_hours_after_first_aired_episode()
        {
            _series.Runtime = 0;

            _parseResultSingle.Series = _series;
            _parseResultSingle.Release.Size = 200.Megabytes();
            _parseResultSingle.Episodes.First().Id = 5;
            _parseResultSingle.Episodes.First().SeasonNumber = 1;
            _parseResultSingle.Episodes.First().EpisodeNumber = 2;
            _parseResultSingle.Episodes.First().AirDateUtc = _episodes.First().AirDateUtc.Value.AddHours(1);

            Subject.IsSatisfiedBy(_parseResultSingle, null).Accepted.Should().Be(true);
        }

        [Test]
        public void should_return_false_if_series_runtime_is_zero_and_multi_episode_is_not_from_first_season()
        {
            _series.Runtime = 0;
            _parseResultMulti.Series = _series;
            _parseResultMulti.Release.Size = 200.Megabytes();
            _parseResultMulti.Episodes.ForEach(e => e.SeasonNumber = 2);

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().Be(false);
        }

        [Test]
        public void should_return_false_if_series_runtime_is_zero_and_multi_episode_aired_more_than_24_hours_after_first_aired_episode()
        {
            var airDateUtc = _episodes.First().AirDateUtc.Value.AddDays(7);

            _series.Runtime = 0;

            _parseResultMulti.Series = _series;
            _parseResultMulti.Release.Size = 200.Megabytes();
            _parseResultMulti.Episodes.ForEach(e =>
            {
                e.SeasonNumber = 1;
                e.AirDateUtc = airDateUtc;
            });

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().Be(false);
        }

        [Test]
        public void should_return_true_if_series_runtime_is_zero_and_multi_episode_aired_less_than_24_hours_after_first_aired_episode()
        {
            var airDateUtc = _episodes.First().AirDateUtc.Value.AddHours(1);

            _series.Runtime = 0;

            _parseResultMulti.Series = _series;
            _parseResultMulti.Release.Size = 200.Megabytes();
            _parseResultMulti.Episodes.ForEach(e =>
            {
                e.SeasonNumber = 1;
                e.AirDateUtc = airDateUtc;
            });

            Subject.IsSatisfiedBy(_parseResultMulti, null).Accepted.Should().Be(true);
        }
    }
}
