using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using AutoMoq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Providers;

namespace NzbDrone.App.Test
{
    [TestFixture]
    public class MonitoringProviderTest
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
