

using System.Linq;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
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
            Subject.Init();
            
            Mocker.GetMock<IQualitySizeRepository>()
                .Verify(v => v.Insert(It.IsAny<QualitySize>()), Times.Exactly(Quality.All().Count - 1));

            //Todo: Should we validate each was inserted exactly as configured?

            //var types = Mocker.Resolve<QualitySizeService>().All();

            //types.Should().HaveCount(10);
            //types.Should().Contain(e => e.Name == "SDTV" && e.QualityId == 1);
            //types.Should().Contain(e => e.Name == "DVD" && e.QualityId == 2);
            //types.Should().Contain(e => e.Name == "WEBDL-480p" && e.QualityId == 8);
            //types.Should().Contain(e => e.Name == "HDTV-720p" && e.QualityId == 4);
            //types.Should().Contain(e => e.Name == "HDTV-1080p" && e.QualityId == 9);
            //types.Should().Contain(e => e.Name == "Raw-HD" && e.QualityId == 10);
            //types.Should().Contain(e => e.Name == "WEBDL-720p" && e.QualityId == 5);
            //types.Should().Contain(e => e.Name == "WEBDL-1080p" && e.QualityId == 3);
            //types.Should().Contain(e => e.Name == "Bluray720p" && e.QualityId == 6);
            //types.Should().Contain(e => e.Name == "Bluray1080p" && e.QualityId == 7);
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

            Subject.Init();

            Mocker.GetMock<IQualitySizeRepository>()
                .Verify(v => v.Insert(It.IsAny<QualitySize>()), Times.Exactly(Quality.All().Count - 2));
        }
    }
}