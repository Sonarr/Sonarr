using System;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Test.Common;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class MonitoringProviderTest : TestBase<PriorityMonitor>
    {
        [Test]
        public void Ensure_priority_doesnt_fail_on_invalid_process_id()
        {
            Mocker.GetMock<IProcessProvider>().Setup(c => c.GetCurrentProcess())
                .Returns(Builder<ProcessInfo>.CreateNew().Build());

            Mocker.GetMock<IProcessProvider>().Setup(c => c.GetProcessById(It.IsAny<int>())).Returns((ProcessInfo)null);

            Subject.EnsurePriority(null);
        }

        [Test]
        public void Ensure_should_log_warn_exception_rather_than_throw()
        {
            Mocker.GetMock<IProcessProvider>().Setup(c => c.GetCurrentProcess()).Throws<InvalidOperationException>();
            Subject.EnsurePriority(null);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
