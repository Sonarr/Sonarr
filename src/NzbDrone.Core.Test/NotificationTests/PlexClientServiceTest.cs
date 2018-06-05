using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Plex.HomeTheater;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    
    public class PlexClientServiceTest : CoreTest
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
        public void Notify_should_send_notification()
        {

            const string header = "Test Header";
            const string message = "Test Message";

            var expectedUrl = string.Format("http://localhost:3000/xbmcCmds/xbmcHttp?command=ExecBuiltIn(Notification({0}, {1}))", header, message);

            var fakeHttp = Mocker.GetMock<IHttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(expectedUrl))
                    .Returns("ok");

            
            Mocker.Resolve<PlexClientService>().Notify(_clientSettings, header, message);

            
            fakeHttp.Verify(v => v.DownloadString(expectedUrl), Times.Once());
        }

        [Test]
        public void Notify_should_send_notification_with_credentials_when_configured()
        {
            WithClientCredentials();

            const string header = "Test Header";
            const string message = "Test Message";

            var expectedUrl = string.Format("http://localhost:3000/xbmcCmds/xbmcHttp?command=ExecBuiltIn(Notification({0}, {1}))", header, message);

            var fakeHttp = Mocker.GetMock<IHttpProvider>();
            fakeHttp.Setup(s => s.DownloadString(expectedUrl, "plex", "plex"))
                    .Returns("ok");


            Mocker.Resolve<PlexClientService>().Notify(_clientSettings, header, message);

            
            fakeHttp.Verify(v => v.DownloadString(expectedUrl, "plex", "plex"), Times.Once());
        }
    }
}
