using System;
using System.Diagnostics;
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
        public void Ensure_priority_doesnt_fail_on_invalid_iis_proccess_id()
        {
            Mocker.GetMock<ProcessProvider>().Setup(c => c.GetCurrentProcess())
                .Returns(Builder<ProcessInfo>.CreateNew().With(c => c.Priority == ProcessPriorityClass.Normal).Build());

            Mocker.GetMock<ProcessProvider>().Setup(c => c.GetProcessById(It.IsAny<int>())).Returns((ProcessInfo)null);

            Subject.EnsurePriority(null);
        }

        [Test]
        public void Ensure_should_log_warn_exception_rather_than_throw()
        {
            Mocker.GetMock<ProcessProvider>().Setup(c => c.GetCurrentProcess()).Throws<InvalidOperationException>();
            Subject.EnsurePriority(null);

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
