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
        private RemoteEpisode parseResultMultiSet;
        private RemoteEpisode parseResultMulti;
        private RemoteEpisode parseResultSingle;
        private Series series;
        private QualityDefinition qualityType;

        [SetUp]
        public void Setup()
        {
            series = Builder<Series>.CreateNew()
                .Build();

            parseResultMultiSet = new RemoteEpisode
                                    {
                                        Series = series,
                                        Release = new ReleaseInfo(),
                                        ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Episodes = new List<Episode> { new Episode(), new Episode(), new Episode(), new Episode(), new Episode(), new Episode() }
                                    };

            parseResultMulti = new RemoteEpisode
                                    {
                                        Series = series,
                                        Release = new ReleaseInfo(),
                                        ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Episodes = new List<Episode> { new Episode(), new Episode() }
                                    };

            parseResultSingle = new RemoteEpisode
                                    {
                                        Series = series,
                                        Release = new ReleaseInfo(),
                                        ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = new QualityModel(Quality.SDTV, new Revision(version: 2)) },
                                        Episodes = new List<Episode> { new Episode() { Id = 2 } }

                                    };

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));

            qualityType = Builder<QualityDefinition>.CreateNew()
                .With(q => q.MinSize = 2)
                .With(q => q.MaxSize = 10)
                .With(q => q.Quality = Quality.SDTV)
                .Build();

            Mocker.GetMock<IQualityDefinitionService>().Setup(s => s.Get(Quality.SDTV)).Returns(qualityType);

            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<Episode>() {
                    new Episode(), new Episode(), new Episode(), new Episode(), new Episode(),
                    new Episode(), new Episode(), new Episode(), new Episode() { Id = 2 }, new Episode() });
        }

        private void GivenLastEpisode()
        {
            Mocker.GetMock<IEpisodeService>().Setup(
                s => s.GetEpisodesBySeason(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(new List<Episode>() {
                    new Episode(), new Episode(), new Episode(), new Episode(), new Episode(),
                    new Episode(), new Episode(), new Episode(), new Episode(), new Episode() { Id = 2 } });
        }

        [TestCase(30, 50, false)]
        [TestCase(30, 250, true)]
        [TestCase(30, 500, false)]
        [TestCase(60, 100, false)]
        [TestCase(60, 500, true)]
        [TestCase(60, 1000, false)]
        public void single_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {           
            series.Runtime = runtime;
            parseResultSingle.Series = series;
            parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 500, true)]
        [TestCase(30, 1000, false)]
        [TestCase(60, 1000, true)]
        [TestCase(60, 2000, false)]
        public void single_episode_first_or_last(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            GivenLastEpisode();

            series.Runtime = runtime;
            parseResultSingle.Series = series;
            parseResultSingle.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 50 * 2, false)]
        [TestCase(30, 250 * 2, true)]
        [TestCase(30, 500 * 2, false)]
        [TestCase(60, 100 * 2, false)]
        [TestCase(60, 500 * 2, true)]
        [TestCase(60, 1000 * 2, false)]
        public void multi_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            series.Runtime = runtime;
            parseResultMulti.Series = series;
            parseResultMulti.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(parseResultMulti, null).Accepted.Should().Be(expectedResult);
        }

        [TestCase(30, 50 * 6, false)]
        [TestCase(30, 250 * 6, true)]
        [TestCase(30, 500 * 6, false)]
        [TestCase(60, 100 * 6, false)]
        [TestCase(60, 500 * 6, true)]
        [TestCase(60, 1000 * 6, false)]
        public void multiset_episode(int runtime, int sizeInMegaBytes, bool expectedResult)
        {
            series.Runtime = runtime;
            parseResultMultiSet.Series = series;
            parseResultMultiSet.Release.Size = sizeInMegaBytes.Megabytes();

            Subject.IsSatisfiedBy(parseResultMultiSet, null).Accepted.Should().Be(expectedResult);
        }

        [Test]
        public void should_return_true_if_size_is_zero()
        {
            GivenLastEpisode();

            series.Runtime = 30;
            parseResultSingle.Series = series;
            parseResultSingle.Release.Size = 0;
            qualityType.MinSize = 10;
            qualityType.MaxSize = 20;

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_unlimited_30_minute()
        {
            GivenLastEpisode();

            series.Runtime = 30;
            parseResultSingle.Series = series;
            parseResultSingle.Release.Size = 18457280000;
            qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().BeTrue();
        }
        
        [Test]
        public void should_return_true_if_unlimited_60_minute()
        {
            GivenLastEpisode();

            series.Runtime = 60;
            parseResultSingle.Series = series;
            parseResultSingle.Release.Size = 36857280000;
            qualityType.MaxSize = null;

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_treat_daily_series_as_single_episode()
        {
            GivenLastEpisode();

            series.Runtime = 60;
            parseResultSingle.Series = series;
            parseResultSingle.Series.SeriesType = SeriesTypes.Daily;
            parseResultSingle.Release.Size = 300.Megabytes();

            qualityType.MaxSize = 10;

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_RAWHD()
        {
            parseResultSingle.ParsedEpisodeInfo.Quality = new QualityModel(Quality.RAWHD);
            
            series.Runtime = 45;
            parseResultSingle.Series = series;
            parseResultSingle.Series.SeriesType = SeriesTypes.Daily;
            parseResultSingle.Release.Size = 8000.Megabytes();

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_special()
        {
            parseResultSingle.ParsedEpisodeInfo.Special = true;

            Subject.IsSatisfiedBy(parseResultSingle, null).Accepted.Should().BeTrue();
        }
    }
}
