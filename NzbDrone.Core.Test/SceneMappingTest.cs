using AutoMoq;
using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SceneMappingTest : TestBase
    {
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

            var emptyDatabase = MockLib.GetEmptyDatabase();
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

            var emptyDatabase = MockLib.GetEmptyDatabase();
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

            var emptyDatabase = MockLib.GetEmptyDatabase();
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

            var emptyDatabase = MockLib.GetEmptyDatabase();
            mocker.SetConstant(emptyDatabase);
            emptyDatabase.Insert(fakeMap);

            //Act
            var seriesId = mocker.Resolve<SceneMappingProvider>().GetSeriesId("notlaworder");

            //Assert
            Assert.AreEqual(null, seriesId);
        }
    }
}
