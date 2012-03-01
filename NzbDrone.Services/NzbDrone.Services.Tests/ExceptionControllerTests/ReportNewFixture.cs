using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Contract;
using NzbDrone.Services.Service.Repository.Reporting;
using NzbDrone.Services.Tests.Framework;

namespace NzbDrone.Services.Tests.ExceptionControllerTests
{
    [TestFixture]
    public class ReportNewFixture : ServicesTestBase
    {

        Service.Controllers.ExceptionController Controller
        {
            get
            {
                return Mocker.Resolve<Service.Controllers.ExceptionController>();
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
            exceptionInstance.Single().ExceptionHash.Should().NotBeBlank();
            exceptionInstance.Single().IsProduction.Should().Be(exceptionReport.IsProduction);
            exceptionInstance.Single().Timestamp.Should().BeWithin(TimeSpan.FromSeconds(4)).Before(DateTime.Now);
            exceptionInstance.Single().LogMessage.Should().Be(exceptionReport.LogMessage);
            exceptionInstance.Single().UGuid.Should().Be(exceptionReport.UGuid);
        }

        [Test]
        public void ReportNew_should_save_detail()
        {
            var exceptionReport = CreateExceptionReport();

            WithRealDb();

            Controller.ReportNew(exceptionReport);

            var exceptionDetails = Db.Fetch<ExceptionDetail>();
            exceptionDetails.Should().HaveCount(1);
            exceptionDetails.Single().Hash.Should().NotBeBlank();
            exceptionDetails.Single().Logger.Should().Be(exceptionReport.Logger);
            exceptionDetails.Single().Type.Should().Be(exceptionReport.Type);
            exceptionDetails.Single().String.Should().Be(exceptionReport.String);
            exceptionDetails.Single().Version.Should().Be(exceptionReport.Version);
        }

        [Test]
        public void ReportNew_should_return_exception_id()
        {
            var exceptionReport = CreateExceptionReport();

            WithRealDb();

            var response = Controller.ReportNew(exceptionReport);

            response.Data.Should().BeOfType<ExceptionReportResponse>();
            ((ExceptionReportResponse)response.Data).ExceptionHash.Should().NotBeBlank();
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

            instances.Should().OnlyContain(c => c.ExceptionHash == detail.Single().Hash);
        }

        [Test]
        public void Reporting_exception_with_diffrent_version_should_create_new_detail()
        {
            var exceptionReport1 = CreateExceptionReport();
            exceptionReport1.Version = "0.1.1";

            var exceptionReport2 = CreateExceptionReport();
            exceptionReport2.Version = "0.2.1";

            WithRealDb();

            Controller.ReportNew(exceptionReport1);
            Controller.ReportNew(exceptionReport2);

            var detail = Db.Fetch<ExceptionDetail>();
            var instances = Db.Fetch<ExceptionInstance>();

            detail.Should().HaveCount(2);
            instances.Should().HaveCount(2);
        }

        [Test]
        public void Reporting_exception_with_diffrent_strting_should_create_new_detail()
        {
            var exceptionReport1 = CreateExceptionReport();
            exceptionReport1.String = "Error1";

            var exceptionReport2 = CreateExceptionReport();
            exceptionReport2.String = "Error2";

            WithRealDb();

            Controller.ReportNew(exceptionReport1);
            Controller.ReportNew(exceptionReport2);

            var detail = Db.Fetch<ExceptionDetail>();
            var instances = Db.Fetch<ExceptionInstance>();

            detail.Should().HaveCount(2);
            instances.Should().HaveCount(2);
        }

        [Test]
        public void Reporting_exception_with_diffrent_logger_should_create_new_detail()
        {
            var exceptionReport1 = CreateExceptionReport();
            exceptionReport1.Logger = "logger1";

            var exceptionReport2 = CreateExceptionReport();
            exceptionReport2.Logger = "logger2";

            WithRealDb();

            Controller.ReportNew(exceptionReport1);
            Controller.ReportNew(exceptionReport2);

            var detail = Db.Fetch<ExceptionDetail>();
            var instances = Db.Fetch<ExceptionInstance>();

            detail.Should().HaveCount(2);
            instances.Should().HaveCount(2);
        }
    }
}