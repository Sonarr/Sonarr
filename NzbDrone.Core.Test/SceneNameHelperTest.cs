using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Helpers;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using SubSonic.Repository;
using TvdbLib.Data;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SceneNameHelperTest : TestBase
    {
        [Test]
        public  void GetSceneName_exists()
        {
            //Setup
            var fakeMap = Builder<SceneNameMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .Build();

            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(f => f.Single<SceneNameMapping>(It.IsAny<Expression<Func<SceneNameMapping, bool>>>()))
                .Returns(fakeMap);

            //Act
            var sceneName = mocker.Resolve<SceneNameMappingProvider>().GetSceneName(fakeMap.SeriesId);

            //Assert
            Assert.AreEqual(fakeMap.SceneName, sceneName);
        }

        [Test]
        public  void GetSeriesId_exists()
        {
            //Setup
            var fakeMap = Builder<SceneNameMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(f => f.Single<SceneNameMapping>(It.IsAny<Expression<Func<SceneNameMapping, bool>>>()))
                .Returns(fakeMap);

            //Act
            var seriesId = mocker.Resolve<SceneNameMappingProvider>().GetSeriesId(fakeMap.SceneCleanName);

            //Assert
            Assert.AreEqual(fakeMap.SeriesId, seriesId);
        }

        [Test]
        public void GetSceneName_null()
        {
            //Setup
            var fakeMap = Builder<SceneNameMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .Build();

            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(f => f.Single<SceneNameMapping>(It.IsAny<Expression<Func<SceneNameMapping, bool>>>()));

            //Act
            var sceneName = mocker.Resolve<SceneNameMappingProvider>().GetSceneName(fakeMap.SeriesId);

            //Assert
            Assert.AreEqual(null, sceneName);
        }

        [Test]
        public void GetSeriesId_null()
        {
            //Setup
            var fakeMap = Builder<SceneNameMapping>.CreateNew()
                .With(f => f.SeriesId = 12345)
                .With(f => f.SceneName = "Law and Order")
                .With(f => f.SceneName = "laworder")
                .Build();

            var mocker = new AutoMoqer();

            mocker.GetMock<IRepository>()
                .Setup(f => f.Single<SceneNameMapping>(It.IsAny<Expression<Func<SceneNameMapping, bool>>>()));

            //Act
            var seriesId = mocker.Resolve<SceneNameMappingProvider>().GetSeriesId(fakeMap.SceneCleanName);

            //Assert
            Assert.AreEqual(null, seriesId);
        }
    }
}
