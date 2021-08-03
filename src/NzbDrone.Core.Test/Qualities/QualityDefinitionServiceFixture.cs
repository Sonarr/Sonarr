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
                .Verify(v => v.InsertMany(It.Is<List<QualityDefinition>>(d => d.Count == Quality.All.Count)), Times.Once());
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
                .Verify(v => v.InsertMany(It.Is<List<QualityDefinition>>(d => d.Count == Quality.All.Count - 1)), Times.Once());
        }

        [Test]
        public void init_should_update_existing_definitions()
        {
            Mocker.GetMock<IQualityDefinitionRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<QualityDefinition>
                      {
                              new QualityDefinition(Quality.SDTV) { Weight = 1, MinSize = 0, MaxSize = 100, Id = 20 }
                      });

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.UpdateMany(It.Is<List<QualityDefinition>>(d => d.Count == 1)), Times.Once());
        }

        [Test]
        public void init_should_remove_old_definitions()
        {
            Mocker.GetMock<IQualityDefinitionRepository>()
                  .Setup(s => s.All())
                  .Returns(new List<QualityDefinition>
                      {
                              new QualityDefinition(new Quality { Id = 100, Name = "Test" }) { Weight = 1, MinSize = 0, MaxSize = 100, Id = 20 }
                      });

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.DeleteMany(It.Is<List<QualityDefinition>>(d => d.Count == 1)), Times.Once());
        }
    }
}
