using System.Linq;
using System.IO;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SceneMappingProviderTest : CoreTest
    {
        private const string SceneMappingUrl = "http://www.nzbdrone.com/SceneMappings.csv";

        private void WithValidCsv()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString(SceneMappingUrl))
                .Returns(File.ReadAllText(@".\Files\SceneMappings.csv"));
        }

        private void WithErrorDownloadingCsv()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString(SceneMappingUrl))
                    .Throws(new WebException());
        }

        [Test]
        public  void GetSceneName_exists()
        {
            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "laworder")
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .Build();

            var mocker = new AutoMoqer();

            var emptyDatabase = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(emptyDatabase);
            emptyDatabase.Insert(fakeMap);

            //Act
            var sceneName = mocker.Resolve<SceneMappingProvider>().GetSceneName(fakeMap.SeriesId);

            //Assert
            Assert.AreEqual(fakeMap.SceneName, sceneName);
        }

        [Test]
        public  void GetSeriesId_exists()
        {
            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            var mocker = new AutoMoqer();

            var emptyDatabase = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(emptyDatabase);
            emptyDatabase.Insert(fakeMap);

            //Act
            var seriesId = mocker.Resolve<SceneMappingProvider>().GetSeriesId(fakeMap.CleanTitle);

            //Assert
            Assert.AreEqual(fakeMap.SeriesId, seriesId);
        }

        [Test]
        public void GetSceneName_null()
        {
            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            var mocker = new AutoMoqer();

            var emptyDatabase = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(emptyDatabase);
            emptyDatabase.Insert(fakeMap);

            //Act
            var sceneName = mocker.Resolve<SceneMappingProvider>().GetSceneName(54321);

            //Assert
            Assert.AreEqual(null, sceneName);
        }

        [Test]
        public void GetSeriesId_null()
        {
            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.CleanTitle = "laworder")
                .Build();

            var mocker = new AutoMoqer();

            var emptyDatabase = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(emptyDatabase);
            emptyDatabase.Insert(fakeMap);

            //Act
            var seriesId = mocker.Resolve<SceneMappingProvider>().GetSeriesId("notlaworder");

            //Assert
            Assert.AreEqual(null, seriesId);
        }

        [Test]
        public void GetSceneName_multiple_clean_names()
        {
            //Test that ensures a series with clean names (office, officeus) can be looked up by seriesId

            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "office")
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "The Office")
                .Build();

            var fakeMap2 = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "officeus")
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "The Office")
                .Build();

            var mocker = new AutoMoqer();

            var db = TestDbHelper.GetEmptyDatabase();
            mocker.SetConstant(db);

            db.Insert(fakeMap);
            db.Insert(fakeMap2);

            //Act
            var sceneName = mocker.Resolve<SceneMappingProvider>().GetSceneName(fakeMap.SeriesId);

            //Assert
            Assert.AreEqual(fakeMap.SceneName, sceneName);
        }

        [Test]
        public void UpdateMappings_should_add_all_mappings_to_database()
        {
            //Setup
            WithRealDb();
            WithValidCsv();

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateMappings();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(It.IsAny<string>()), Times.Once());
            var result = Db.Fetch<SceneMapping>();
            result.Should().HaveCount(5);
        }

        [Test]
        public void UpdateMappings_should_overwrite_existing_mappings()
        {
            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            WithRealDb();
            WithValidCsv();
            Db.Insert(fakeMap);

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateMappings();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(It.IsAny<string>()), Times.Once());
            var result = Db.Fetch<SceneMapping>();
            result.Should().HaveCount(5);
        }

        [Test]
        public void UpdateMappings_should_not_delete_if_csv_download_fails()
        {
            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            WithRealDb();
            WithErrorDownloadingCsv();
            Db.Insert(fakeMap);

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateMappings();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(It.IsAny<string>()), Times.Once());
            var result = Db.Fetch<SceneMapping>();
            result.Should().HaveCount(1);
        }

        [Test]
        public void UpdateIfEmpty_should_not_update_if_count_is_not_zero()
        {
            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            WithRealDb();
            Db.Insert(fakeMap);

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateIfEmpty();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void UpdateIfEmpty_should_update_if_count_is_zero()
        {
            //Setup
            WithRealDb();
            WithValidCsv();

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateIfEmpty();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Once());
            var result = Db.Fetch<SceneMapping>();
            result.Should().HaveCount(5);
        }
    }
}
