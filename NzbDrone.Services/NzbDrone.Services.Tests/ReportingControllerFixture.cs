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
    public class ReportingControllerFixture : ServicesTestBase
    {

        ReportingController Controller
        {
            get
            {
                return Mocker.Resolve<ReportingController>();
            }
        }


        private static ParseErrorReport CreateParseErrorReport()
        {
            return new ParseErrorReport
                                   {
                                       IsProduction = true,
                                       Title = "MyTitle",
                                       Version = "1.1.2.323456",
                                       UGuid = Guid.NewGuid()
                                   };
        }

        private static ExceptionReport CreateExceptionReport()
        {
            return new ExceptionReport
            {
                IsProduction = true,
                Version = "1.1.2.323456",
                UGuid = Guid.NewGuid(),
                Logger = "NzbDrone.Logger.Name",
                LogMessage = "Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message",
                String = "Long message Long message Long messageLong messageLong messageLong messageLong messageLong messageLong messageLong messageLong message",
                Type = typeof(InvalidOperationException).Name
            };
        }


        [Test]
        public void parse_report_should_be_saved()
        {
            var parseReport = CreateParseErrorReport();

            WithRealDb();

            Controller.ParseError(parseReport);

            var reports = Db.Fetch<ParseErrorRow>();
            reports.Should().HaveCount(1);
            reports.Single().Title.Should().Be(parseReport.Title);
            reports.Single().IsProduction.Should().Be(parseReport.IsProduction);
            reports.Single().Version.Should().Be(parseReport.Version);
            reports.Single().Timestamp.Should().BeWithin(TimeSpan.FromSeconds(4)).Before(DateTime.Now);
            reports.Single().UGuid.Should().Be(parseReport.UGuid);
        }

        [Test]
        public void parse_report_should_save_report_if_title_doesnt_exist()
        {
            var parseReport1 = CreateParseErrorReport();
            var parseReport2 = CreateParseErrorReport();

            parseReport1.Title = Guid.NewGuid().ToString();

            WithRealDb();

            Controller.ParseError(parseReport1);
            Controller.ParseError(parseReport2);

            var reports = Db.Fetch<ParseErrorRow>();
            reports.Should().HaveCount(2);
        }

        [Test]
        public void parse_report_should_not_save_report_if_title_exist()
        {
            var parseReport1 = CreateParseErrorReport();
            var parseReport2 = CreateParseErrorReport();

            WithRealDb();

            Controller.ParseError(parseReport1);
            Controller.ParseError(parseReport2);

            var reports = Db.Fetch<ParseErrorRow>();
            reports.Should().HaveCount(1);
        }

        [Test]
        public void exception_report_should_be_saved()
        {
            var exceptionReport = CreateExceptionReport();

            WithRealDb();

            Controller.ReportException(exceptionReport);

            var exceptionRows = Db.Fetch<ExceptionRow>();
            exceptionRows.Should().HaveCount(1);
            exceptionRows.Single().IsProduction.Should().Be(exceptionReport.IsProduction);
            exceptionRows.Single().Version.Should().Be(exceptionReport.Version);
            exceptionRows.Single().Timestamp.Should().BeWithin(TimeSpan.FromSeconds(4)).Before(DateTime.Now);
            exceptionRows.Single().UGuid.Should().Be(exceptionReport.UGuid);

            exceptionRows.Single().Logger.Should().Be(exceptionReport.Logger);
            exceptionRows.Single().LogMessage.Should().Be(exceptionReport.LogMessage);
            exceptionRows.Single().String.Should().Be(exceptionReport.String);
            exceptionRows.Single().Type.Should().Be(exceptionReport.Type);
        }
    }
}
