using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]

    public class QualitySizeServiceFixture : CoreTest<QualitySizeService>
    {
        [Test]
        public void Init_should_add_all_sizes()
        {
            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualitySizeRepository>()
                .Verify(v => v.Insert(It.IsAny<QualitySize>()), Times.Exactly(Quality.All().Count));
        }

        [Test]
        public void Init_should_insert_any_missing_sizes()
        {
            Mocker.GetMock<IQualitySizeRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<QualitySize>
                      {
                              new QualitySize { QualityId = 1, Name = "SDTV", MinSize = 0, MaxSize = 100 }
                      });

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualitySizeRepository>()
                .Verify(v => v.Insert(It.IsAny<QualitySize>()), Times.Exactly(Quality.All().Count - 1));
        }
    }
}