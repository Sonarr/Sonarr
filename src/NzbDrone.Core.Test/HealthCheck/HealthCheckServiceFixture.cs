using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.HealthCheck
{
    public class HealthCheckServiceFixture : CoreTest<HealthCheckService>
    {
        private FakeHealthCheck _healthCheck;

        [SetUp]
        public void SetUp()
        {
            _healthCheck = new FakeHealthCheck();

            Mocker.SetConstant<IEnumerable<IProvideHealthCheck>>(new[] { _healthCheck });
            Mocker.SetConstant<ICacheManager>(Mocker.Resolve<CacheManager>());
        }

        [Test]
        public void should_not_execute_conditional()
        {
            Subject.HandleAsync(new FakeEvent());

            _healthCheck.Executed.Should().BeFalse();
        }

        [Test]
        public void should_execute_conditional()
        {
            Subject.HandleAsync(new FakeEvent() { ShouldExecute = true });

            _healthCheck.Executed.Should().BeTrue();
        }

        [Test]
        public void should_execute_unconditional()
        {
            Subject.HandleAsync(new FakeEvent2());

            _healthCheck.Executed.Should().BeTrue();
        }
    }

    public class FakeEvent : IEvent
    {
        public bool ShouldExecute { get; set; }
    }

    public class FakeEvent2 : IEvent
    {
        public bool ShouldExecute { get; set; }
    }

    [CheckOn(typeof(FakeEvent))]
    [CheckOn(typeof(FakeEvent2))]
    public class FakeHealthCheck : IProvideHealthCheck, ICheckOnCondition<FakeEvent>
    {
        public bool CheckOnStartup => false;
        public bool CheckOnSchedule => false;

        public bool Executed { get; set; }
        public bool Checked { get; set; }

        public Core.HealthCheck.HealthCheck Check()
        {
            Executed = true;

            return new Core.HealthCheck.HealthCheck(GetType());
        }

        public bool ShouldCheckOnEvent(FakeEvent message)
        {
            return message.ShouldExecute;
        }
    }
}
