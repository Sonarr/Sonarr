using System;
using NUnit.Framework;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using FluentAssertions;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]

    public class QualitySizeRepositoryFixture : DbTest<QualitySizeRepository, QualitySize>
    {



        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IQualitySizeRepository>(Subject);
            Mocker.Resolve<QualitySizeService>().Handle(new ApplicationStartedEvent());
        }


        [Test]
        public void should_get_quality_size_by_id()
        {
            var size = Subject.GetByQualityId(Quality.Bluray1080p.Id);

            size.Should().NotBeNull();
        }

        [Test]
        public void should_throw_with_id_if_not_exist()
        {
            var id = 123;
            Assert.Throws<InvalidOperationException>(()=> Subject.GetByQualityId(id)).Message.Contains(id.ToString());
        }

    }
}