using System.Diagnostics;
using System.Linq;

using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Model;
using NzbDrone.Providers;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class MonitoringProviderTest : TestBase
    {

        [Test]
        public void Ensure_priority_doesnt_fail_on_invalid_iis_proccess_id()
        {
            var mocker = new AutoMoqer();

            var processMock = mocker.GetMock<ProcessProvider>();
            processMock.Setup(c => c.GetCurrentProcess())
                .Returns(Builder<ProcessInfo>.CreateNew().With(c => c.Priority == ProcessPriorityClass.Normal).Build());

            processMock.Setup(c => c.GetProcessById(It.IsAny<int>())).Returns((ProcessInfo)null);

            var subject = mocker.Resolve<MonitoringProvider>();


            //Act
            subject.EnsurePriority(null, null);
        }


    }
}
