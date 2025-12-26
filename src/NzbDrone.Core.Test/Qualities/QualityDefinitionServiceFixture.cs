using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        public async Task init_should_add_all_definitions()
        {
            await Subject.HandleAsync(new ApplicationStartedEvent(), CancellationToken.None);

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.InsertManyAsync(It.Is<List<QualityDefinition>>(d => d.Count == Quality.All.Count)), Times.Once());
        }

        [Test]
        public async Task init_should_insert_any_missing_definitions()
        {
            Mocker.GetMock<IQualityDefinitionRepository>()
                  .Setup(s => s.AllAsync())
                  .ReturnsAsync(new List<QualityDefinition>
                      {
                              new QualityDefinition(Quality.SDTV) { Weight = 1, MinSize = 0, MaxSize = 100, Id = 20 }
                      });

            await Subject.HandleAsync(new ApplicationStartedEvent(), CancellationToken.None);

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.InsertManyAsync(It.Is<List<QualityDefinition>>(d => d.Count == Quality.All.Count - 1)), Times.Once());
        }

        [Test]
        public async Task init_should_update_existing_definitions()
        {
            Mocker.GetMock<IQualityDefinitionRepository>()
                  .Setup(s => s.AllAsync())
                  .ReturnsAsync(new List<QualityDefinition>
                      {
                              new QualityDefinition(Quality.SDTV) { Weight = 1, MinSize = 0, MaxSize = 100, Id = 20 }
                      });

            await Subject.HandleAsync(new ApplicationStartedEvent(), CancellationToken.None);

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.UpdateManyAsync(It.Is<List<QualityDefinition>>(d => d.Count == 1)), Times.Once());
        }

        [Test]
        public async Task init_should_remove_old_definitions()
        {
            Mocker.GetMock<IQualityDefinitionRepository>()
                  .Setup(s => s.AllAsync())
                  .ReturnsAsync(new List<QualityDefinition>
                      {
                              new QualityDefinition(new Quality { Id = 100, Name = "Test" }) { Weight = 1, MinSize = 0, MaxSize = 100, Id = 20 }
                      });

            await Subject.HandleAsync(new ApplicationStartedEvent(), CancellationToken.None);

            Mocker.GetMock<IQualityDefinitionRepository>()
                .Verify(v => v.DeleteManyAsync(It.Is<List<QualityDefinition>>(d => d.Count == 1)), Times.Once());
        }
    }
}
