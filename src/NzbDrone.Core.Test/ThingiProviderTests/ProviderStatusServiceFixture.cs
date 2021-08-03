using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Test.ThingiProviderTests
{
    public class MockProviderStatus : ProviderStatusBase
    {
    }

    public interface IMockProvider : IProvider
    {
    }

    public interface IMockProviderStatusRepository : IProviderStatusRepository<MockProviderStatus>
    {
    }

    public class MockProviderStatusService : ProviderStatusServiceBase<IMockProvider, MockProviderStatus>
    {
        public MockProviderStatusService(IMockProviderStatusRepository providerStatusRepository, IEventAggregator eventAggregator, IRuntimeInfo runtimeInfo, Logger logger)
            : base(providerStatusRepository, eventAggregator, runtimeInfo, logger)
        {
        }
    }

    public class ProviderStatusServiceFixture : CoreTest<MockProviderStatusService>
    {
        private DateTime _epoch;

        [SetUp]
        public void SetUp()
        {
            _epoch = DateTime.UtcNow;

            Mocker.GetMock<IRuntimeInfo>()
                .SetupGet(v => v.StartTime)
                .Returns(_epoch - TimeSpan.FromHours(1));
        }

        private void GivenRecentStartup()
        {
            Mocker.GetMock<IRuntimeInfo>()
                .SetupGet(v => v.StartTime)
                .Returns(_epoch - TimeSpan.FromMinutes(12));
        }

        private MockProviderStatus WithStatus(MockProviderStatus status)
        {
            Mocker.GetMock<IMockProviderStatusRepository>()
                .Setup(v => v.FindByProviderId(1))
                .Returns(status);

            Mocker.GetMock<IMockProviderStatusRepository>()
                .Setup(v => v.All())
                .Returns(new[] { status });

            return status;
        }

        private void VerifyUpdate()
        {
            Mocker.GetMock<IMockProviderStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<MockProviderStatus>()), Times.Once());
        }

        private void VerifyNoUpdate()
        {
            Mocker.GetMock<IMockProviderStatusRepository>()
                .Verify(v => v.Upsert(It.IsAny<MockProviderStatus>()), Times.Never());
        }

        [Test]
        public void should_start_backoff_on_first_failure()
        {
            WithStatus(new MockProviderStatus());

            Subject.RecordFailure(1);

            VerifyUpdate();

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().NotBeNull();
            status.DisabledTill.Should().HaveValue();
            status.DisabledTill.Value.Should().BeCloseTo(_epoch + TimeSpan.FromMinutes(5), 500);
        }

        [Test]
        public void should_cancel_backoff_on_success()
        {
            WithStatus(new MockProviderStatus { EscalationLevel = 2 });

            Subject.RecordSuccess(1);

            VerifyUpdate();

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().BeNull();
        }

        [Test]
        public void should_not_store_update_if_already_okay()
        {
            WithStatus(new MockProviderStatus { EscalationLevel = 0 });

            Subject.RecordSuccess(1);

            VerifyNoUpdate();
        }

        [Test]
        public void should_preserve_escalation_on_intermittent_success()
        {
            WithStatus(new MockProviderStatus
            {
                InitialFailure = _epoch - TimeSpan.FromSeconds(20),
                MostRecentFailure = _epoch - TimeSpan.FromSeconds(4),
                EscalationLevel = 3
            });

            Subject.RecordSuccess(1);
            Subject.RecordSuccess(1);
            Subject.RecordFailure(1);

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().NotBeNull();
            status.DisabledTill.Should().HaveValue();
            status.DisabledTill.Value.Should().BeCloseTo(_epoch + TimeSpan.FromMinutes(15), 500);
        }

        [Test]
        public void should_not_escalate_further_till_after_5_minutes_since_startup()
        {
            GivenRecentStartup();

            var origStatus = WithStatus(new MockProviderStatus
            {
                InitialFailure = _epoch - TimeSpan.FromMinutes(6),
                MostRecentFailure = _epoch - TimeSpan.FromSeconds(120),
                EscalationLevel = 3
            });

            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);
            Subject.RecordFailure(1);

            var status = Subject.GetBlockedProviders().FirstOrDefault();
            status.Should().NotBeNull();

            origStatus.EscalationLevel.Should().Be(3);
            status.DisabledTill.Should().BeCloseTo(_epoch + TimeSpan.FromMinutes(5), 500);
        }
    }
}
