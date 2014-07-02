using System;
using System.Collections.Generic;
using System.Linq;
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

        private Mock<ISceneMappingProvider> _provider1;
        private Mock<ISceneMappingProvider> _provider2;
            
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

            _provider1 = new Mock<ISceneMappingProvider>();
            _provider1.Setup(s => s.GetSceneMappings()).Returns(_fakeMappings);

            _provider2 = new Mock<ISceneMappingProvider>();
            _provider2.Setup(s => s.GetSceneMappings()).Returns(_fakeMappings);
        }

        private void GivenProviders(IEnumerable<Mock<ISceneMappingProvider>> providers)
        {
            Mocker.SetConstant<IEnumerable<ISceneMappingProvider>>(providers.Select(s => s.Object));
        }

        [Test]
        public void should_purge_existing_mapping_and_add_new_ones()
        {
            GivenProviders(new [] { _provider1 });

            Mocker.GetMock<ISceneMappingRepository>().Setup(c => c.All()).Returns(_fakeMappings);

            Subject.Execute(new UpdateSceneMappingCommand());

            AssertMappingUpdated();
        }

        [Test]
        public void should_not_delete_if_fetch_fails()
        {
            GivenProviders(new[] { _provider1 });

            _provider1.Setup(c => c.GetSceneMappings()).Throws(new WebException());

            Subject.Execute(new UpdateSceneMappingCommand());

            AssertNoUpdate();

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_not_delete_if_fetch_returns_empty_list()
        {
            GivenProviders(new[] { _provider1 });

            _provider1.Setup(c => c.GetSceneMappings()).Returns(new List<SceneMapping>());

            Subject.Execute(new UpdateSceneMappingCommand());

            AssertNoUpdate();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_get_mappings_for_all_providers()
        {
            GivenProviders(new[] { _provider1, _provider2 });

            Mocker.GetMock<ISceneMappingRepository>().Setup(c => c.All()).Returns(_fakeMappings);

            Subject.Execute(new UpdateSceneMappingCommand());

            _provider1.Verify(c => c.GetSceneMappings(), Times.Once());
            _provider2.Verify(c => c.GetSceneMappings(), Times.Once());
        }

        private void AssertNoUpdate()
        {
            _provider1.Verify(c => c.GetSceneMappings(), Times.Once());
            Mocker.GetMock<ISceneMappingRepository>().Verify(c => c.Clear(It.IsAny<String>()), Times.Never());
            Mocker.GetMock<ISceneMappingRepository>().Verify(c => c.InsertMany(_fakeMappings), Times.Never());
        }

        private void AssertMappingUpdated()
        {
            _provider1.Verify(c => c.GetSceneMappings(), Times.Once());
            Mocker.GetMock<ISceneMappingRepository>().Verify(c => c.Clear(It.IsAny<String>()), Times.Once());
            Mocker.GetMock<ISceneMappingRepository>().Verify(c => c.InsertMany(_fakeMappings), Times.Once());

            foreach (var sceneMapping in _fakeMappings)
            {
                Subject.GetSceneNames(sceneMapping.TvdbId, _fakeMappings.Select(m => m.SeasonNumber)).Should().Contain(sceneMapping.SearchTerm);
                Subject.GetTvDbId(sceneMapping.ParseTerm).Should().Be(sceneMapping.TvdbId);
            }
        }
    }
}
