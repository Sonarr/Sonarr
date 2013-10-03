using System.Collections.Generic;
using System.Net;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using FluentAssertions;

namespace NzbDrone.Core.Test.DataAugmentationFixture.Scene
{
    [TestFixture]

    public class SceneMappingServiceFixture : CoreTest<SceneMappingService>
    {

        private List<SceneMapping> _fakeMappings;

        [SetUp]
        public void Setup()
        {
            _fakeMappings = Builder<SceneMapping>.CreateListOfSize(5).BuildListOfNew();

            _fakeMappings[0].SearchTerm = "Words";
            _fakeMappings[1].SearchTerm = "That";
            _fakeMappings[2].SearchTerm = "Can";
            _fakeMappings[3].SearchTerm = "Be";
            _fakeMappings[4].SearchTerm = "Cleaned";

            _fakeMappings[0].ParseTerm = "Words";
            _fakeMappings[1].ParseTerm = "That";
            _fakeMappings[2].ParseTerm = "Can";
            _fakeMappings[3].ParseTerm = "Be";
            _fakeMappings[4].ParseTerm = "Cleaned";
        }

   

        [Test]
        public void UpdateMappings_purge_existing_mapping_and_add_new_ones()
        {
            Mocker.GetMock<ISceneMappingProxy>().Setup(c => c.Fetch()).Returns(_fakeMappings);
            Mocker.GetMock<ISceneMappingRepository>().Setup(c => c.All()).Returns(_fakeMappings);

            Subject.Execute(new UpdateSceneMappingCommand());

            AssertMappingUpdated();
        }



        [Test]
        public void UpdateMappings_should_not_delete_if_fetch_fails()
        {

            Mocker.GetMock<ISceneMappingProxy>().Setup(c => c.Fetch()).Throws(new WebException());

            Subject.Execute(new UpdateSceneMappingCommand());

            AssertNoUpdate();

            ExceptionVerification.ExpectedErrors(1);

        }

        [Test]
        public void UpdateMappings_should_not_delete_if_fetch_returns_empty_list()
        {

            Mocker.GetMock<ISceneMappingProxy>().Setup(c => c.Fetch()).Returns(new List<SceneMapping>());

            Subject.Execute(new UpdateSceneMappingCommand());

            AssertNoUpdate();

            ExceptionVerification.ExpectedWarns(1);
        }

        private void AssertNoUpdate()
        {
            Mocker.GetMock<ISceneMappingProxy>().Verify(c => c.Fetch(), Times.Once());
            Mocker.GetMock<ISceneMappingRepository>().Verify(c => c.Purge(), Times.Never());
            Mocker.GetMock<ISceneMappingRepository>().Verify(c => c.InsertMany(_fakeMappings), Times.Never());
        }

        private void AssertMappingUpdated()
        {
            Mocker.GetMock<ISceneMappingProxy>().Verify(c => c.Fetch(), Times.Once());
            Mocker.GetMock<ISceneMappingRepository>().Verify(c => c.Purge(), Times.Once());
            Mocker.GetMock<ISceneMappingRepository>().Verify(c => c.InsertMany(_fakeMappings), Times.Once());


            foreach (var sceneMapping in _fakeMappings)
            {
                Subject.GetSceneName(sceneMapping.TvdbId).Should().Be(sceneMapping.SearchTerm);
                Subject.GetTvDbId(sceneMapping.ParseTerm).Should().Be(sceneMapping.TvdbId);
            }
        }



    }
}
