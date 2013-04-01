using System.Collections.Generic;
using System.Net;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DataAugmentationFixture.Scene
{
    [TestFixture]

    public class SceneMappingServiceFixture : DbTest<SceneMappingService, SceneMapping>
    {

        private List<SceneMapping> _fakeMappings;

        [SetUp]
        public void Setup()
        {
            _fakeMappings = Builder<SceneMapping>.CreateListOfSize(5).BuildListOfNew();
        }

   

        [Test]
        public void UpdateMappings_purge_existing_mapping_and_add_new_ones()
        {
            Mocker.GetMock<ISceneMappingProxy>().Setup(c => c.Fetch()).Returns(_fakeMappings);

            Subject.UpdateMappings();

            AssertMappingUpdated();
        }



        [Test]
        public void UpdateMappings_should_not_delete_if_fetch_fails()
        {

            Mocker.GetMock<ISceneMappingProxy>().Setup(c => c.Fetch()).Throws(new WebException());

            Subject.UpdateMappings();

            AssertNoUpdate();

            ExceptionVerification.ExpectedErrors(1);

        }

        [Test]
        public void UpdateMappings_should_not_delete_if_fetch_returns_empty_list()
        {

            Mocker.GetMock<ISceneMappingProxy>().Setup(c => c.Fetch()).Returns(new List<SceneMapping>());

            Subject.UpdateMappings();

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
        }

    }
}
