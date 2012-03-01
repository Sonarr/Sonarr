using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Contract;
using NzbDrone.Services.Service.Repository.Reporting;
using NzbDrone.Services.Tests.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Services.Tests.ExceptionControllerTests
{
    [TestFixture]
    public class ReportExistingFixture : ServicesTestBase
    {

        Service.Controllers.ExceptionController Controller
        {
            get
            {
                return Mocker.Resolve<Service.Controllers.ExceptionController>();
            }
        }

        private static ExistingExceptionReport CreateExceptionReport()
        {
            return new ExistingExceptionReport
            {
                IsProduction = true,
                Version = "1.1.2.323456",
                UGuid = Guid.NewGuid(),
                LogMessage = @"Log message",
                Hash = "ABC123"
            };
        }

        [Test]
        public void should_log_warn_if_hash_doesnt_exist()
        {
            WithRealDb();

            Controller.ReportExisting(CreateExceptionReport());

            Db.Fetch<ExceptionDetail>().Should().BeEmpty();
            Db.Fetch<ExceptionInstance>().Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }

        [Test]
        public void should_save_instance_if_hash_is_valid()
        {
            WithRealDb();

            var existing = CreateExceptionReport();
      

            Db.Insert(Builder<ExceptionDetail>.CreateNew().With(c => c.Hash = existing.Hash).Build());

            Controller.ReportExisting(existing);
            
            Db.Fetch<ExceptionDetail>().Should().HaveCount(1);
            var exceptionInstance = Db.Fetch<ExceptionInstance>();
            exceptionInstance.Should().HaveCount(1);
            exceptionInstance.Single().Id.Should().BeGreaterThan(0);
            exceptionInstance.Single().ExceptionHash.Should().NotBeBlank();
            exceptionInstance.Single().IsProduction.Should().Be(existing.IsProduction);
            exceptionInstance.Single().Timestamp.Should().BeWithin(TimeSpan.FromSeconds(4)).Before(DateTime.Now);
            exceptionInstance.Single().LogMessage.Should().Be(existing.LogMessage);
            exceptionInstance.Single().UGuid.Should().Be(existing.UGuid);
        }

    }
}