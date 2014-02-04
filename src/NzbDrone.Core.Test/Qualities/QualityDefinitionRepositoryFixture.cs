using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using FluentAssertions;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityDefinitionRepositoryFixture : DbTest<QualityDefinitionRepository, QualityDefinition>
    {
        [SetUp]
        public void Setup()
        {
            foreach (var qualityDefault in Quality.DefaultQualityDefinitions)
            {
                qualityDefault.Id = 0;
                Storage.Insert(qualityDefault);
            }
        }

        [Test]
        public void should_get_qualitydefinition_by_id()
        {
            var size = Subject.GetByQualityId((int)Quality.Bluray1080p);

            size.Should().NotBeNull();
        }

        [Test]
        public void should_throw_with_id_if_not_exist()
        {
            var id = 123;
            Assert.Throws<ModelNotFoundException>(()=> Subject.GetByQualityId(id)).Message.Contains(id.ToString());
        }

    }
}