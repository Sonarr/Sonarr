using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Contract;
using NzbDrone.Services.Service.Controllers;
using NzbDrone.Services.Service.Repository.Reporting;
using NzbDrone.Services.Tests.Framework;

namespace NzbDrone.Services.Tests
{
    [TestFixture]
    public class ExceptionControllerFixture : ServicesTestBase
    {

        ExceptionController Controller
        {
            get
            {
                return Mocker.Resolve<ExceptionController>();
            }
        }

        private static ExceptionReport CreateExceptionReport()
        {
            return new ExceptionReport
            {
                IsProduction = true,
                Version = "1.1.2.323456",
                UGuid = Guid.NewGuid(),
                Logger = "NzbDrone.Logger.Name",
                LogMessage = @"Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message",
                String = @"Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message
                            Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message",

                Type = typeof(InvalidOperationException).Name
            };
        }


        [Test]
        public void ReportNew_should_save_instance()
        {
            var exceptionReport = CreateExceptionReport();

            WithRealDb();

            Controller.ReportNew(exceptionReport);

            var exceptionInstance = Db.Fetch<ExceptionInstance>();
            exceptionInstance.Should().HaveCount(1);
            exceptionInstance.Single().Id.Should().BeGreaterThan(0);
            exceptionInstance.Single().ExceptionDetail.Should().BeGreaterThan(0);
            exceptionInstance.Single().IsProduction.Should().Be(exceptionReport.IsProduction);
            exceptionInstance.Single().Timestamp.Should().BeWithin(TimeSpan.FromSeconds(4)).Before(DateTime.Now);
            exceptionInstance.Single().LogMessage.Should().Be(exceptionReport.LogMessage);
        }

        [Test]
        public void ReportNew_should_return_exception_id()
        {
            var exceptionReport = CreateExceptionReport();

            WithRealDb();

            var response = Controller.ReportNew(exceptionReport);

            response.Data.Should().BeOfType<ExceptionReportResponse>();
            ((ExceptionReportResponse)response.Data).ExceptionId.Should().BeGreaterThan(0);
        }


        [Test]
        public void Reporting_exception_more_than_once_should_create_single_detail_with_multiple_instances()
        {
            var exceptionReport = CreateExceptionReport();

            WithRealDb();

            var response1 = Controller.ReportNew(exceptionReport);
            var response2 = Controller.ReportNew(exceptionReport);
            var response3 = Controller.ReportNew(exceptionReport);
            
            var detail = Db.Fetch<ExceptionDetail>();
            var instances = Db.Fetch<ExceptionInstance>();

            detail.Should().HaveCount(1);
            instances.Should().HaveCount(3);

            instances.Should().OnlyContain(c => c.ExceptionDetail == detail.Single().Id);

        }
    }
}