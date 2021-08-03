using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class ImportListStatusCheckFixture : CoreTest<ImportListStatusCheck>
    {
        private List<IImportList> _importLists = new List<IImportList>();
        private List<ImportListStatus> _blockedImportLists = new List<ImportListStatus>();

        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IImportListFactory>()
                  .Setup(v => v.GetAvailableProviders())
                  .Returns(_importLists);

            Mocker.GetMock<IImportListStatusService>()
                   .Setup(v => v.GetBlockedProviders())
                   .Returns(_blockedImportLists);
        }

        private Mock<IImportList> GivenImportList(int id, double backoffHours, double failureHours)
        {
            var mockImportList = new Mock<IImportList>();
            mockImportList.SetupGet(s => s.Definition).Returns(new ImportListDefinition { Id = id });

            _importLists.Add(mockImportList.Object);

            if (backoffHours != 0.0)
            {
                _blockedImportLists.Add(new ImportListStatus
                {
                    ProviderId = id,
                    InitialFailure = DateTime.UtcNow.AddHours(-failureHours),
                    MostRecentFailure = DateTime.UtcNow.AddHours(-0.1),
                    EscalationLevel = 5,
                    DisabledTill = DateTime.UtcNow.AddHours(backoffHours)
                });
            }

            return mockImportList;
        }

        [Test]
        public void should_not_return_error_when_no_import_lists()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_warning_if_import_list_unavailable()
        {
            GivenImportList(1, 10.0, 24.0);
            GivenImportList(2, 0.0, 0.0);

            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_error_if_all_import_lists_unavailable()
        {
            GivenImportList(1, 10.0, 24.0);

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_warning_if_few_import_lists_unavailable()
        {
            GivenImportList(1, 10.0, 24.0);
            GivenImportList(2, 10.0, 24.0);
            GivenImportList(3, 0.0, 0.0);

            Subject.Check().ShouldBeWarning();
        }
    }
}
