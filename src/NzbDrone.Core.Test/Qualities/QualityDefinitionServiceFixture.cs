using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Qualities
{
    [TestFixture]
    public class QualityDefinitionServiceFixture : CoreTest<QualityDefinitionService>
    {
        [Test]
        public void init_should_add_all_definitions()
        {
            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.Insert(It.IsAny<QualityDefinition>()), Times.Exactly(Quality.All.Count));
        }

        [Test]
        public void init_should_insert_any_missing_definitions()
        {
            Mocker.GetMock<IQualityDefinitionRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<QualityDefinition>
                      {
                              new QualityDefinition(Quality.SDTV) { Weight = 1, MinSize = 0, MaxSize = 100, Id = 20 }
                      });

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.Insert(It.IsAny<QualityDefinition>()), Times.Exactly(Quality.All.Count - 1));
        }

        [Test]
        public void init_should_insert_missing_definitions_preserving_weight()
        {
            // User moved HDTV1080p to a higher weight.
            var currentQualities = new List<QualityDefinition>
            {
                new QualityDefinition(Quality.SDTV)        { Id = 5, Title = "SDTV",         Weight = 1,  MinSize=0, MaxSize=100 },
                new QualityDefinition(Quality.WEBDL720p)   { Id = 2, Title = "720p WEB-DL",  Weight = 2,  MinSize=0, MaxSize=100 },
                new QualityDefinition(Quality.HDTV1080p)   { Id = 4, Title = "1080p HDTV",   Weight = 3,  MinSize=0, MaxSize=100 },
                new QualityDefinition(Quality.WEBDL1080p)  { Id = 8, Title = "1080p WEB-DL", Weight = 4,  MinSize=0, MaxSize=100 },
            };

            // Expected to insert Bluray720p above HDTV1080p.
            // Expected to insert Bluray1080p above WEBDL1080p.
            var addBluray1080p = new List<QualityDefinition>
            {
                new QualityDefinition(Quality.SDTV)        { Title = "SDTV",         Weight = 1,  MinSize=0, MaxSize=100 },
                new QualityDefinition(Quality.HDTV1080p)   { Title = "1080p HDTV",   Weight = 2,  MinSize=0, MaxSize=100 },
                new QualityDefinition(Quality.WEBDL720p)   { Title = "720p WEB-DL",  Weight = 3,  MinSize=0, MaxSize=100 },
                new QualityDefinition(Quality.Bluray720p)  { Title = "720p BluRay",  Weight = 4,  MinSize=0, MaxSize=100 },
                new QualityDefinition(Quality.WEBDL1080p)  { Title = "1080p WEB-DL", Weight = 5,  MinSize=0, MaxSize=100 },
                new QualityDefinition(Quality.Bluray1080p) { Title = "1080p BluRay", Weight = 6,  MinSize=0, MaxSize=100 }
            };

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Setup(v => v.All())
                .Returns(currentQualities);
                        
            Subject.InsertMissingDefinitions(addBluray1080p);

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.Insert(It.Is<QualityDefinition>(p => p.Quality == Quality.Bluray720p && p.Weight == 4)), Times.Once());

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.Update(It.Is<QualityDefinition>(p => p.Quality == Quality.WEBDL1080p && p.Weight == 5)), Times.Once());
            
            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.Insert(It.Is<QualityDefinition>(p => p.Quality == Quality.Bluray1080p && p.Weight == 6)), Times.Once());           
            
        }
    }
}