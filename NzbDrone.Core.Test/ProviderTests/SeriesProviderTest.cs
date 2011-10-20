using System;
using System.Linq;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

// ReSharper disable InconsistentNaming
namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    public class SeriesProviderTest : TestBase
    {
        [TestCase(true)]
        [TestCase(false)]
        public void Add_new_series(bool useSeasonFolder)
        {
            var mocker = new AutoMoqer();

            mocker.GetMock<ConfigProvider>()
                .Setup(c => c.UseSeasonFolder).Returns(useSeasonFolder);

            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeProfiles = Builder<QualityProfile>.CreateListOfSize(2).Build();

            db.InsertMany(fakeProfiles);

            const string path = "C:\\Test\\";
            const int tvDbId = 1234;
            const int qualityProfileId = 2;

            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
            seriesProvider.AddSeries(path, tvDbId, qualityProfileId);

            //Assert
            var series = seriesProvider.GetAllSeries();
            series.Should().HaveCount(1);
            Assert.AreEqual(path, series.First().Path);
            Assert.AreEqual(tvDbId, series.First().SeriesId);
            Assert.AreEqual(qualityProfileId, series.First().QualityProfileId);
            series.First().SeasonFolder.Should().Be(useSeasonFolder);
        }

        [Test]
        public void find_series_empty_repo()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyDatabase());

            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
            var series = seriesProvider.FindSeries("My Title");


            //Assert
            Assert.IsNull(series);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException), ExpectedMessage = "Sequence contains no elements")]
        public void Get_series_invalid_series_id_should_return_null()
        {
            var mocker = new AutoMoqer();
            mocker.SetConstant(MockLib.GetEmptyDatabase());

            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
            var series = seriesProvider.GetSeries(2);


            //Assert
            Assert.IsNull(series);
        }

        [Test]
        public void Get_series_by_id()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.QualityProfileId = 1)
                .With(c => c.EpisodeCount = 0)
                .With(c => c.EpisodeFileCount = 0)
                .With(c => c.SeasonCount = 0)
                .Build();
            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetSeries(1);

            //Assert
            series.ShouldHave().AllPropertiesBut(s => s.QualityProfile, s => s.SeriesId, s => s.NextAiring).EqualTo(fakeSeries);
            series.QualityProfile.Should().NotBeNull();
            series.QualityProfile.ShouldHave().Properties(q => q.Name, q => q.SonicAllowed, q => q.Cutoff, q => q.SonicAllowed).EqualTo(fakeQuality);

        }

        [Test]
        public void Find_series_by_cleanName_mapped()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeSeries = Builder<Series>.CreateNew()
                .With(c => c.QualityProfileId = 1)
                .With(c => c.CleanTitle = "laworder")
                .Build();
            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();

            var id = db.Insert(fakeSeries);
            db.Insert(fakeQuality);

            //Act
            mocker.Resolve<QualityProvider>();
            mocker.GetMock<SceneMappingProvider>().Setup(s => s.GetSeriesId("laworder")).Returns(1);

            var series = mocker.Resolve<SeriesProvider>().FindSeries("laworder");

            //Assert
            series.ShouldHave().AllPropertiesBut(s => s.QualityProfile, s => s.SeriesId);
            series.QualityProfile.Should().NotBeNull();
            series.QualityProfile.ShouldHave().Properties(q => q.Name, q => q.SonicAllowed, q => q.Cutoff, q => q.AllowedString);
        }

        [Test]
        public void find_series_empty_match()
        {
            var mocker = new AutoMoqer();
            var emptyRepository = MockLib.GetEmptyDatabase();
            mocker.SetConstant(emptyRepository);
            emptyRepository.Insert(MockLib.GetFakeSeries(1, "MyTitle"));
            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
            var series = seriesProvider.FindSeries("WrongTitle");


            //Assert
            Assert.IsNull(series);
        }

        [TestCase("The Test", "Test")]
        [TestCase("Through the Wormhole", "Through.the.Wormhole")]
        public void find_series_match(string title, string searchTitle)
        {
            var mocker = new AutoMoqer();
            var emptyRepository = MockLib.GetEmptyDatabase();
            mocker.SetConstant(emptyRepository);
            emptyRepository.Insert(MockLib.GetFakeSeries(1, title));
            emptyRepository.Insert(Builder<QualityProfile>.CreateNew().Build());
            mocker.Resolve<QualityProvider>();

            //Act
            var seriesProvider = mocker.Resolve<SeriesProvider>();
            var series = seriesProvider.FindSeries(searchTitle);

            //Assert
            Assert.IsNotNull(series);
            Assert.AreEqual(title, series.Title);
            series.QualityProfile.Should().NotBeNull();
            series.QualityProfile.ShouldHave().Properties(q => q.Name, q => q.SonicAllowed, q => q.Cutoff, q => q.AllowedString);
        }

        [Test]
        public void is_monitored()
        {
            var mocker = new AutoMoqer();

            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            mocker.SetConstant(db);

            db.Insert(Builder<Series>.CreateNew()
                                                  .With(c => c.Monitored = true)
                                                  .With(c => c.SeriesId = 12)
                                                  .Build());

            db.Insert(Builder<Series>.CreateNew()
                                                 .With(c => c.Monitored = false)
                                                 .With(c => c.SeriesId = 11)
                                                 .Build());

            db.InsertMany(Builder<QualityProfile>.CreateListOfSize(3).Build());

            //Act, Assert
            var provider = mocker.Resolve<SeriesProvider>();
            provider.IsMonitored(12).Should().BeTrue();
            Assert.IsFalse(provider.IsMonitored(11));
            Assert.IsFalse(provider.IsMonitored(1));
        }

        [Test]
        public void Get_Series_With_Count()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .All().With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today)
                .TheFirst(5)
                .With(e => e.EpisodeFileId = 0)
                .WhereTheLast(2)
                .With(e => e.AirDate = DateTime.Today.AddDays(1))
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            Assert.AreEqual(8, series[0].EpisodeCount);
            Assert.AreEqual(3, series[0].EpisodeFileCount);
        }

        [Test]
        public void Get_Series_With_Count_AllIgnored()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10).All().With(e => e.SeriesId = fakeSeries.SeriesId).With(e => e.Ignored = true).WhereRandom(5).With(e => e.EpisodeFileId = 0).Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            Assert.AreEqual(0, series[0].EpisodeCount);
            Assert.AreEqual(0, series[0].EpisodeFileCount);
        }

        [Test]
        public void Get_Series_With_Count_AllDownloaded()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            Assert.AreEqual(10, series[0].EpisodeCount);
            Assert.AreEqual(10, series[0].EpisodeFileCount);
        }

        [Test]
        public void Get_Series_With_Count_Half_Ignored()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .TheFirst(5)
                .With(e => e.Ignored = false)
                .WhereTheLast(5)
                .With(e => e.Ignored = true)
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            Assert.AreEqual(5, series[0].EpisodeCount);
            Assert.AreEqual(5, series[0].EpisodeFileCount);
        }

        [Test]
        public void Get_Single_Series()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew()
                .With(e => e.QualityProfileId = fakeQuality.QualityProfileId)
                .With(e => e.SeriesId = 1)
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetSeries(1);

            //Assert
            series.QualityProfile.Should().NotBeNull();
            series.QualityProfileId.Should().Be(fakeQuality.QualityProfileId);
        }

        [Test]
        public void SeriesPathExists_exact_match()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var path = @"C:\Test\TV\30 Rock";

            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .All()
                .With(c => c.QualityProfileId = 1)
                .TheFirst(1)
                .With(c => c.Path = path)
                .Build();
            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();

            db.InsertMany(fakeSeries);
            db.Insert(fakeQuality);

            //Act
            mocker.Resolve<QualityProvider>();
            //mocker.GetMock<IDatabase>().Setup(s => s.Fetch<Series, QualityProfile>(It.IsAny<string>())).Returns(
            //fakeSeries.ToList());

            var result = mocker.Resolve<SeriesProvider>().SeriesPathExists(path);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SeriesPathExists_match()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var path = @"C:\Test\TV\30 Rock";

            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .All()
                .With(c => c.QualityProfileId = 1)
                .TheFirst(1)
                .With(c => c.Path = path)
                .Build();
            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();

            db.InsertMany(fakeSeries);
            db.Insert(fakeQuality);

            //Act
            mocker.Resolve<QualityProvider>();
            //mocker.GetMock<IDatabase>().Setup(s => s.Fetch<Series, QualityProfile>(It.IsAny<string>())).Returns(
            //fakeSeries.ToList());

            var result = mocker.Resolve<SeriesProvider>().SeriesPathExists(path.ToUpper());

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SeriesPathExists_match_alt()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var path = @"C:\Test\TV\The Simpsons";

            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .All()
                .With(c => c.QualityProfileId = 1)
                .TheFirst(1)
                .With(c => c.Path = path)
                .Build();
            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();

            db.InsertMany(fakeSeries);
            db.Insert(fakeQuality);

            //Act
            mocker.Resolve<QualityProvider>();
            //mocker.GetMock<IDatabase>().Setup(s => s.Fetch<Series, QualityProfile>(It.IsAny<string>())).Returns(
            //fakeSeries.ToList());

            var result = mocker.Resolve<SeriesProvider>().SeriesPathExists(@"c:\Test\Tv\the sIMpsons");

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void SeriesPathExists_match_false()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var path = @"C:\Test\TV\30 Rock";

            var fakeSeries = Builder<Series>.CreateListOfSize(10)
                .All()
                .With(c => c.QualityProfileId = 1)
                .TheFirst(1)
                .With(c => c.Path = path)
                .Build();
            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();

            db.InsertMany(fakeSeries);
            db.Insert(fakeQuality);

            //Act
            mocker.Resolve<QualityProvider>();
            //mocker.GetMock<IDatabase>().Setup(s => s.Fetch<Series, QualityProfile>(It.IsAny<string>())).Returns(
            //fakeSeries.ToList());

            var result = mocker.Resolve<SeriesProvider>().SeriesPathExists(@"C:\Test\TV\Not A match");

            //Assert
            result.Should().BeFalse();
        }

        [Test]
        public void Get_Series_NextAiring_Today()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddDays(1))
                .TheFirst(1)
                .With(e => e.AirDate = DateTime.Today)
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            series[0].NextAiring.Should().Be(DateTime.Today);
        }

        [Test]
        public void Get_Series_NextAiring_Tomorrow_Last_Aired_Yesterday()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddDays(1))
                .TheFirst(1)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            series[0].NextAiring.Should().Be(DateTime.Today.AddDays(1));
        }

        [Test]
        public void Get_Series_NextAiring_Unknown()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.AirDate = null)
                .With(e => e.Ignored = false)
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            series[0].NextAiring.Should().NotHaveValue();
        }

        [Test]
        public void Get_Series_NextAiring_1_month()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.Ignored = false)
                .With(e => e.AirDate = DateTime.Today.AddMonths(1))
                .TheFirst(1)
                .With(e => e.AirDate = DateTime.Today.AddDays(-1))
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            series[0].NextAiring.Should().Be(DateTime.Today.AddMonths(1));
        }

        [Test]
        public void Get_Series_NextAiring_skip_ignored()
        {
            var mocker = new AutoMoqer(MockBehavior.Strict);
            var db = MockLib.GetEmptyDatabase();
            mocker.SetConstant(db);

            var fakeQuality = Builder<QualityProfile>.CreateNew().Build();
            var fakeSeries = Builder<Series>.CreateNew().With(e => e.QualityProfileId = fakeQuality.QualityProfileId).Build();
            var fakeEpisodes = Builder<Episode>.CreateListOfSize(2)
                .All()
                .With(e => e.SeriesId = fakeSeries.SeriesId)
                .With(e => e.AirDate = DateTime.Today.AddMonths(1))
                .With(e => e.Ignored = false)
                .TheFirst(1)
                .With(e => e.AirDate = DateTime.Today.AddDays(1))
                .With(e => e.Ignored = true)
                .Build();

            db.Insert(fakeSeries);
            db.Insert(fakeQuality);
            db.InsertMany(fakeEpisodes);

            //Act
            mocker.Resolve<QualityProvider>();
            var series = mocker.Resolve<SeriesProvider>().GetAllSeriesWithEpisodeCount();

            //Assert
            series.Should().HaveCount(1);
            series[0].NextAiring.Should().Be(DateTime.Today.AddMonths(1));
        }
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             