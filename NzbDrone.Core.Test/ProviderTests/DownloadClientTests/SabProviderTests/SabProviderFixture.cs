// ReSharper disable RedundantUsingDirective

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Model.Sabnzbd;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Providers.DownloadClients;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ProviderTests.DownloadClientTests.SabProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class SabProviderFixture : CoreTest
    {
        private const string url = "http://www.nzbclub.com/nzb_download.aspx?mid=1950232";
        private const string title = "My Series Name - 5x2-5x3 - My title [Bluray720p] [Proper]";

        [SetUp]
        public void Setup()
        {
            var fakeConfig = Mocker.GetMock<ConfigProvider>();

            fakeConfig.SetupGet(c => c.SabHost).Returns("192.168.5.55");
            fakeConfig.SetupGet(c => c.SabPort).Returns(2222);
            fakeConfig.SetupGet(c => c.SabApiKey).Returns("5c770e3197e4fe763423ee7c392c25d1");
            fakeConfig.SetupGet(c => c.SabUsername).Returns("admin");
            fakeConfig.SetupGet(c => c.SabPassword).Returns("pass");
            fakeConfig.SetupGet(c => c.SabTvCategory).Returns("tv");
        }

        private void WithFailResponse()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString(It.IsAny<String>())).Returns("{ \"status\": false, \"error\": \"API Key Required\" }");
        }

        [Test]
        public void add_url_should_format_request_properly()
        {
            Mocker.GetMock<HttpProvider>(MockBehavior.Strict)
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=0&pp=3&cat=tv&nzbname=My+Series+Name+-+5x2-5x3+-+My+title+%5bBluray720p%5d+%5bProper%5d&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns("{ \"status\": true }");

            //Act
            Mocker.Resolve<SabProvider>().DownloadNzb(url, title, false).Should().BeTrue();
        }

        [Test]
        public void newzbin_add_url_should_format_request_properly()
        {
            Mocker.GetMock<HttpProvider>(MockBehavior.Strict)
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addid&name=6107863&priority=0&pp=3&cat=tv&nzbname=My+Series+Name+-+5x2-5x3+-+My+title+%5bBluray720p%5d+%5bProper%5d&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns("{ \"status\": true }");


            //Act
            bool result = Mocker.Resolve<SabProvider>().DownloadNzb("http://www.newzbin.com/browse/post/6107863/nzb", title, false);

            //Assert
            result.Should().BeTrue();
        }

        [Test]
        public void add_by_url_should_detect_and_handle_sab_errors()
        {
            WithFailResponse();

            //Act
            Assert.Throws<ApplicationException>(() => Mocker.Resolve<SabProvider>().DownloadNzb(url, title, false).Should().BeFalse());
            //ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_be_able_to_get_categories_when_config_is_passed_in()
        {
            //Setup
            const string host = "192.168.5.22";
            const int port = 1111;
            const string apikey = "5c770e3197e4fe763423ee7c392c25d2";
            const string username = "admin2";
            const string password = "pass2";



            Mocker.GetMock<HttpProvider>(MockBehavior.Strict)
                    .Setup(s => s.DownloadString("http://192.168.5.22:1111/api?mode=get_cats&output=json&apikey=5c770e3197e4fe763423ee7c392c25d2&ma_username=admin2&ma_password=pass2"))
                    .Returns(File.ReadAllText(@".\Files\Categories_json.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetCategories(host, port, apikey, username, password);

            //Assert
            result.Should().NotBeNull();
            result.categories.Should().NotBeEmpty();
        }

        [Test]
        public void should_be_able_to_get_categories_using_config()
        {
            Mocker.GetMock<HttpProvider>(MockBehavior.Strict)
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=get_cats&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(File.ReadAllText(@".\Files\Categories_json.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetCategories();

            //Assert
            result.Should().NotBeNull();
            result.categories.Should().NotBeEmpty();
        }

        [Test]
        public void GetHistory_should_return_a_list_with_items_when_the_history_has_items()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=history&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(File.ReadAllText(@".\Files\History.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetHistory();

            //Assert
            result.Should().HaveCount(1);
        }

        [Test]
        public void GetHistory_should_return_an_empty_list_when_the_queue_is_empty()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=history&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(File.ReadAllText(@".\Files\HistoryEmpty.txt"));

            //Act
            var result = Mocker.Resolve<SabProvider>().GetHistory();

            //Assert
            result.Should().BeEmpty();
        }

        [Test]
        public void GetHistory_should_return_an_empty_list_when_there_is_an_error_getting_the_queue()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=history&output=json&start=0&limit=0&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(File.ReadAllText(@".\Files\JsonError.txt"));

            //Act
            Assert.Throws<ApplicationException>(() => Mocker.Resolve<SabProvider>().GetHistory(), "API Key Incorrect");
        }

        [Test]
        public void GetVersion_should_return_the_version_using_passed_in_values()
        {
            var response = "{ \"version\": \"0.6.9\" }";

            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=version&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(response);

            //Act
            var result = Mocker.Resolve<SabProvider>().GetVersion("192.168.5.55", 2222, "5c770e3197e4fe763423ee7c392c25d1", "admin", "pass");

            //Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("0.6.9");
        }

        [Test]
        public void GetVersion_should_return_the_version_using_saved_values()
        {
            var response = "{ \"version\": \"0.6.9\" }";

            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=version&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(response);

            //Act
            var result = Mocker.Resolve<SabProvider>().GetVersion();

            //Assert
            result.Should().NotBeNull();
            result.Version.Should().Be("0.6.9");
        }

        [Test]
        public void Test_should_return_version_as_a_string()
        {
            var response = "{ \"version\": \"0.6.9\" }";

            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=version&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns(response);

            //Act
            var result = Mocker.Resolve<SabProvider>().Test("192.168.5.55", 2222, "5c770e3197e4fe763423ee7c392c25d1", "admin", "pass");

            //Assert
            result.Should().Be("0.6.9");
        }

        [Test]
        public void should_return_false_when_WebException_is_thrown()
        {
            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString(It.IsAny<String>())).Throws(new WebException());

            Mocker.Resolve<SabProvider>().DownloadNzb(url, title, false).Should().BeFalse();
            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void downloadNzb_should_use_sabRecentTvPriority_when_recentEpisode_is_true()
        {
            Mocker.GetMock<ConfigProvider>()
                  .SetupGet(s => s.SabRecentTvPriority)
                  .Returns(SabPriorityType.High);

            Mocker.GetMock<ConfigProvider>()
                  .SetupGet(s => s.SabBacklogTvPriority)
                  .Returns(SabPriorityType.Low);

            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=1&pp=3&cat=tv&nzbname=My+Series+Name+-+5x2-5x3+-+My+title+%5bBluray720p%5d+%5bProper%5d&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns("{ \"status\": true }");

            //Act
            Mocker.Resolve<SabProvider>().DownloadNzb(url, title, true).Should().BeTrue();

            Mocker.GetMock<HttpProvider>()
                    .Verify(v => v.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=1&pp=3&cat=tv&nzbname=My+Series+Name+-+5x2-5x3+-+My+title+%5bBluray720p%5d+%5bProper%5d&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"), Times.Once());
        }

        [Test]
        public void downloadNzb_should_use_sabBackogTvPriority_when_recentEpisode_is_false()
        {
            Mocker.GetMock<ConfigProvider>()
                  .SetupGet(s => s.SabRecentTvPriority)
                  .Returns(SabPriorityType.High);

            Mocker.GetMock<ConfigProvider>()
                  .SetupGet(s => s.SabBacklogTvPriority)
                  .Returns(SabPriorityType.Low);

            Mocker.GetMock<HttpProvider>()
                    .Setup(s => s.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=-1&pp=3&cat=tv&nzbname=My+Series+Name+-+5x2-5x3+-+My+title+%5bBluray720p%5d+%5bProper%5d&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"))
                    .Returns("{ \"status\": true }");

            //Act
            Mocker.Resolve<SabProvider>().DownloadNzb(url, title, false).Should().BeTrue();

            Mocker.GetMock<HttpProvider>()
                    .Verify(v => v.DownloadString("http://192.168.5.55:2222/api?mode=addurl&name=http://www.nzbclub.com/nzb_download.aspx?mid=1950232&priority=-1&pp=3&cat=tv&nzbname=My+Series+Name+-+5x2-5x3+-+My+title+%5bBluray720p%5d+%5bProper%5d&output=json&apikey=5c770e3197e4fe763423ee7c392c25d1&ma_username=admin&ma_password=pass"), Times.Once());
        }
    }
}