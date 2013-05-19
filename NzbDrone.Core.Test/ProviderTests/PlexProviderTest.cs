using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Notifications.Plex;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests
{
    [TestFixture]
    
    public class PlexProviderTest : CoreTest
    {
        private PlexClientSettings _clientSettings;

        public void WithClientCredentials()
        {
            _clientSettings.Username = "plex";
            _clientSettings.Password = "plex";
        }

        [SetUp]
        public void Setup()
        {
            _clientSettings = new PlexClientSettings
                                  {
                                      Host = "localhost",
                                      Port = 3000
                                  };
        }

        [Test]
        public void GetSectionKeys_should_return_single_section_key_when_only_one_show_section()
        {           
            var response = "<MediaContainer size=\"1\" mediaTagPrefix=\"/system/bundle/media/flags/\" mediaTagVersion=\"1329809559\" title1=\"Plex Library\" identifier=\"com.plexapp.plugins.library\"><Directory refreshing=\"0\" key=\"5\" type=\"show\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory></MediaContainer>";
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(response));

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadStream("http://localhost:32400/library/sections", null))
                    .Returns(stream);

            
            var result = Mocker.Resolve<PlexProvider>().GetSectionKeys("localhost:32400");

            
            result.Should().HaveCount(1);
            result.First().Should().Be(5);
        }

        [Test]
        public void GetSectionKeys_should_return_single_section_key_when_only_one_show_section_with_other_sections()
        {
            

            var response = "<MediaContainer size=\"1\" mediaTagPrefix=\"/system/bundle/media/flags/\" mediaTagVersion=\"1329809559\" title1=\"Plex Library\" identifier=\"com.plexapp.plugins.library\"><Directory refreshing=\"0\" key=\"5\" type=\"show\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory><Directory refreshing=\"0\" key=\"7\" type=\"movie\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory></MediaContainer>";
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(response));

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadStream("http://localhost:32400/library/sections", null))
                    .Returns(stream);

            
            var result = Mocker.Resolve<PlexProvider>().GetSectionKeys("localhost:32400");

            
            result.Should().HaveCount(1);
            result.First().Should().Be(5);
        }

        [Test]
        public void GetSectionKeys_should_return_multiple_section_keys_when_there_are_multiple_show_sections()
        {
            

            var response = "<MediaContainer size=\"1\" mediaTagPrefix=\"/system/bundle/media/flags/\" mediaTagVersion=\"1329809559\" title1=\"Plex Library\" identifier=\"com.plexapp.plugins.library\"><Directory refreshing=\"0\" key=\"5\" type=\"show\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory><Directory refreshing=\"0\" key=\"6\" type=\"show\" title=\"TV Shows\" art=\"/:/resources/show-fanart.jpg\" agent=\"com.plexapp.agents.thetvdb\" scanner=\"Plex Series Scanner\" language=\"en\" updatedAt=\"1329810350\"><Location path=\"C:/Test/TV\"/></Directory></MediaContainer>";
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(response));

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadStream("http://localhost:32400/library/sections", null))
                    .Returns(stream);

            
            var result = Mocker.Resolve<PlexProvider>().GetSectionKeys("localhost:32400");

            
            result.Should().HaveCount(2);
            result.First().Should().Be(5);
            result.Last().Should().Be(6);
        }

        [Test]
        public void UpdateSection_should_update_section()
        {
            

            var response = "";
            Stream stream = new MemoryStream(ASCIIEncoding.Default.GetBytes(response));

            Mocker.GetMock<IHttpProvider>().Setup(s => s.DownloadString("http://localhost:32400/library/sections/5/refresh"))
                    .Returns(response);

            
            Mocker.Resolve<PlexProvider>().UpdateSection("localhost:32400", 5);

            
            
        }

        [Test]
        public void Notify_should_send_notification()
        {

            const string header = "Test Header";
            const string message = "Test Message";

            var expectedUrl = String.Format("http://localhost:3000/xbmcCmds/xbmcHttp?command=ExecBuiltIn(Notification({0}, {1}))", header, message);

            var fakeHttp = Mocker.GetMock<IHttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(expectedUrl))
                    .Returns("ok");

            
            Mocker.Resolve<PlexProvider>().Notify(_clientSettings, header, message);

            
            fakeHttp.Verify(v => v.DownloadString(expectedUrl), Times.Once());
        }

        [Test]
        public void Notify_should_send_notification_with_credentials_when_configured()
        {
            WithClientCredentials();

            const string header = "Test Header";
            const string message = "Test Message";

            var expectedUrl = String.Format("http://localhost:3000/xbmcCmds/xbmcHttp?command=ExecBuiltIn(Notification({0}, {1}))", header, message);

            var fakeHttp = Mocker.GetMock<IHttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(expectedUrl, "plex", "plex"))
                    .Returns("ok");

            
            Mocker.Resolve<PlexProvider>().Notify(_clientSettings, header, message);

            
            fakeHttp.Verify(v => v.DownloadString(expectedUrl, "plex", "plex"), Times.Once());
        }
    }
}