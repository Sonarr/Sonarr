using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Contract;
using NzbDrone.Services.Service.Repository.Reporting;
using NzbDrone.Services.Tests.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Services.Tests.ExceptionController
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
            
            Controller.ReportExisting(CreateExceptionReport());
            
            Db.Fetch<ExceptionDetail>().Should().HaveCount(1);
            Db.Fetch<ExceptionInstance>().Should().HaveCount(1);
        }

    }
}