using System.Linq;
using System.IO;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
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
        private const string SceneMappingUrl = "http://services.nzbdrone.com/SceneMapping/Active";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ConfigProvider>().SetupGet(s => s.ServiceRootUrl)
                    .Returns("http://services.nzbdrone.com");
        }

        private void WithValidJson()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString(SceneMappingUrl))
                .Returns(File.ReadAllText(@".\Files\SceneMappings.json"));
        }

        private void WithErrorDownloadingJson()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString(SceneMappingUrl))
                    .Throws(new WebException());
        }

        [Test]
        public  void GetSceneName_exists()
        {
            WithRealDb();

            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "laworder")
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SeasonNumber = -1)
                .Build();

            Db.Insert(fakeMap);

            //Act
            var sceneName = Mocker.Resolve<SceneMappingProvider>().GetSceneName(fakeMap.SeriesId);

            //Assert
            Assert.AreEqual(fakeMap.SceneName, sceneName);
        }

        [Test]
        public  void GetSeriesId_exists()
        {
            WithRealDb();

            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            Db.Insert(fakeMap);

            //Act
            var seriesId = Mocker.Resolve<SceneMappingProvider>().GetSeriesId(fakeMap.CleanTitle);

            //Assert
            Assert.AreEqual(fakeMap.SeriesId, seriesId);
        }

        [Test]
        public void GetSceneName_null()
        {
            WithRealDb();

            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            Db.Insert(fakeMap);

            //Act
            var sceneName = Mocker.Resolve<SceneMappingProvider>().GetSceneName(54321);

            //Assert
            Assert.AreEqual(null, sceneName);
        }

        [Test]
        public void GetSeriesId_null()
        {
            WithRealDb();

            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.CleanTitle = "laworder")
                .Build();

            Db.Insert(fakeMap);

            //Act
            var seriesId = Mocker.Resolve<SceneMappingProvider>().GetSeriesId("notlaworder");

            //Assert
            Assert.AreEqual(null, seriesId);
        }

        [Test]
        public void GetSceneName_multiple_clean_names()
        {
            WithRealDb();
            //Test that ensures a series with clean names (office, officeus) can be looked up by seriesId

            //Setup
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "office")
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "The Office")
                .With(f => f.SeasonNumber = -1)
                .Build();

            var fakeMap2 = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "officeus")
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "The Office")
                .With(f => f.SeasonNumber = -1)
                .Build();

            Db.Insert(fakeMap);
            Db.Insert(fakeMap2);

            //Act
            var sceneName = Mocker.Resolve<SceneMappingProvider>().GetSceneName(fakeMap.SeriesId);

            //Assert
            Assert.AreEqual(fakeMap.SceneName, sceneName);
        }

        [Test]
        public void GetSceneName_should_be_null_when_seasonNumber_does_not_match()
        {
            WithRealDb();

            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .With(f => f.SeasonNumber = 10)
                .Build();

            Db.Insert(fakeMap);

            Mocker.Resolve<SceneMappingProvider>().GetSceneName(54321, 5).Should().BeNull();
        }

        [Test]
        public void UpdateMappings_should_add_all_mappings_to_database()
        {
            //Setup
            WithRealDb();
            WithValidJson();

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateMappings();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Once());
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
            WithValidJson();
            Db.Insert(fakeMap);

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateMappings();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Once());
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
            WithErrorDownloadingJson();
            Db.Insert(fakeMap);

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateMappings();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Once());
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
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Never());
        }

        [Test]
        public void UpdateIfEmpty_should_update_if_count_is_zero()
        {
            //Setup
            WithRealDb();
            WithValidJson();

            //Act
            Mocker.Resolve<SceneMappingProvider>().UpdateIfEmpty();

            //Assert
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Once());
            var result = Db.Fetch<SceneMapping>();
            result.Should().HaveCount(5);
        }
    }
}
