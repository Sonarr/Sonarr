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
        [SetUp]
        public void Setup()
        {
            WithRealDb();
            Mocker.Resolve<ExceptionController>();
        }

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
        public void parse_report_should_be_saved()
        {
            var parseReport = CreateParseErrorReport();


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


            Controller.ParseError(parseReport1);
            Controller.ParseError(parseReport2);

            var reports = Db.Fetch<ParseErrorRow>();
            reports.Should().HaveCount(1);
        }
    }
}
