using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Contract;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class ReportingService_ReportParseError_Fixture : TestBase
    {
        [SetUp]
        public void SetUp()
        {
            ReportingService.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            ReportingService.ClearCache();
        }

        [Test]
        public void report_parse_error_should_send_report_to_server()
        {
            const string badTitle = "Bad Title";

            ReportingService.ReportParseError(badTitle);
            MockedRestProvider.Verify(p => p.PostData(It.Is<string>(c => c.ToLower().StartsWith("http://service.nzbdrone.com/")), It.Is<ParseErrorReport>(c => c.Title == badTitle)), Times.Once());
        }

        [Test]
        public void report_parse_error_should_send_duplicated_report_once()
        {
            const string badTitle = "Bad Title";

            ReportingService.ReportParseError(badTitle);
            ReportingService.ReportParseError(badTitle);

            MockedRestProvider.Verify(p => p.PostData(It.IsAny<string>(), It.IsAny<ReportBase>()), Times.Once());
        }

        [Test]
        public void report_parse_error_should_send_duplicated_report_once_with_diffrent_casing()
        {
            const string badTitle = "Bad Title";

            ReportingService.ReportParseError(badTitle.ToUpper());
            ReportingService.ReportParseError(badTitle);

            MockedRestProvider.Verify(p => p.PostData(It.IsAny<string>(), It.IsAny<ReportBase>()), Times.Once());
        }

        [Test]
        public void report_parse_error_should_send_multiple_reports_if_titles_are_diffrent()
        {
            ReportingService.ReportParseError("title 1");
            ReportingService.ReportParseError("title 2");

            MockedRestProvider.Verify(p => p.PostData(It.IsAny<string>(), It.IsAny<ReportBase>()), Times.Exactly(2));
            MockedRestProvider.Verify(p => p.PostData(It.IsAny<string>(), It.Is<ParseErrorReport>(c => c.Title == "title 1")), Times.Once());
            MockedRestProvider.Verify(p => p.PostData(It.IsAny<string>(), It.Is<ParseErrorReport>(c => c.Title == "title 2")), Times.Once());
        }

    }
}