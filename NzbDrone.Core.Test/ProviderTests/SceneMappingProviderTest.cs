using System.Linq;
using System.IO;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ReferenceData;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.TvTests;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    
    public class SceneMappingProviderTest : DbTest
    {
        private const string SceneMappingUrl = "http://services.nzbdrone.com/SceneMapping/Active";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.ServiceRootUrl)
                    .Returns("http://services.nzbdrone.com");

        }

        private void WithValidJson()
        {
            Mocker.GetMock<HttpProvider>()
                .Setup(s => s.DownloadString(SceneMappingUrl))
                .Returns(ReadAllText("Files", "SceneMappings.json"));
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

            
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "laworder")
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SeasonNumber = -1)
                .BuildNew<SceneMapping>();

            Db.Insert(fakeMap);

            
            var sceneName = Mocker.Resolve<SceneMappingService>().GetSceneName(fakeMap.TvdbId);

            
            Assert.AreEqual(fakeMap.SceneName, sceneName);
        }

        [Test]
        public  void GetSeriesId_exists()
        {

            
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();


            Db.Insert(fakeMap);

            
            var seriesId = Mocker.Resolve<SceneMappingService>().GetTvDbId(fakeMap.CleanTitle);

            
            Assert.AreEqual(fakeMap.TvdbId, seriesId);
        }

        [Test]
        public void GetSceneName_null()
        {

            
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();


            Db.Insert(fakeMap);

            
            var sceneName = Mocker.Resolve<SceneMappingService>().GetSceneName(54321);

            
            Assert.AreEqual(null, sceneName);
        }

        [Test]
        public void GetSeriesId_null()
        {

            
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.CleanTitle = "laworder")
                .Build();

            Db.Insert(fakeMap);

            
            var seriesId = Mocker.Resolve<SceneMappingService>().GetTvDbId("notlaworder");

            
            Assert.AreEqual(null, seriesId);
        }

        [Test]
        public void GetSceneName_multiple_clean_names()
        {
            //Test that ensures a series with clean names (office, officeus) can be looked up by seriesId

            
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "office")
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "The Office")
                .With(f => f.SeasonNumber = -1)
                .Build();

            var fakeMap2 = Builder<SceneMapping>.CreateNew()
                .With(f => f.CleanTitle = "officeus")
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "The Office")
                .With(f => f.SeasonNumber = -1)
                .Build();



            Db.Insert(fakeMap);
            Db.Insert(fakeMap2);

            
            var sceneName = Mocker.Resolve<SceneMappingService>().GetSceneName(fakeMap.TvdbId);

            
            Assert.AreEqual(fakeMap.SceneName, sceneName);
        }

        [Test]
        public void GetSceneName_should_be_null_when_seasonNumber_does_not_match()
        {
          

            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .With(f => f.SeasonNumber = 10)
                .Build();

            Db.Insert(fakeMap);

            Mocker.Resolve<SceneMappingService>().GetSceneName(54321, 5).Should().BeNull();
        }

        [Test]
        public void UpdateMappings_should_add_all_mappings_to_database()
        {
            WithValidJson();

            
            Mocker.Resolve<SceneMappingService>().UpdateMappings();

            
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Once());
        }

        [Test]
        public void UpdateMappings_should_overwrite_existing_mappings()
        {
            
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            WithValidJson();
            Db.Insert(fakeMap);

            
            Mocker.Resolve<SceneMappingService>().UpdateMappings();

            
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Once());
        }

        [Test]
        public void UpdateMappings_should_not_delete_if_csv_download_fails()
        {
            
            var fakeMap = Builder<SceneMapping>.CreateNew()
                .With(f => f.TvdbId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            WithErrorDownloadingJson();
            Db.Insert(fakeMap);

            
            Mocker.Resolve<SceneMappingService>().UpdateMappings();

            
            Mocker.Verify<HttpProvider>(v => v.DownloadString(SceneMappingUrl), Times.Once());
        }

    }
}
