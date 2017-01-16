using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Http
{
    [TestFixture]
    public class ActivePlayersFixture : CoreTest<HttpApiProvider>
    {
        private XbmcSettings _settings;
        private string _expectedUrl;

        private void WithNoActivePlayers()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(_expectedUrl, _settings.Username, _settings.Password))
                  .Returns("<html><li>Filename:[Nothing Playing]</html>");
        }

        private void WithVideoPlayerActive()
        {
            var activePlayers = @"<html><li>Filename:C:\Test\TV\2 Broke Girls\Season 01\2 Broke Girls - S01E01 - Pilot [SDTV].avi" +
                              "<li>PlayStatus:Playing<li>VideoNo:0<li>Type:Video<li>Thumb:special://masterprofile/Thumbnails/Video/a/auto-a664d5a2.tbn" +
                              "<li>Time:00:06<li>Duration:21:35<li>Percentage:0<li>File size:183182590<li>Changed:True</html>";

            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.DownloadString(_expectedUrl, _settings.Username, _settings.Password))
                  .Returns(activePlayers);
        }

        [SetUp]
        public void Setup()
        {
            _settings = new XbmcSettings
            {
                Host = "localhost",
                Port = 8080,
                Username = "xbmc",
                Password = "xbmc",
                AlwaysUpdate = false,
                CleanLibrary = false,
                UpdateLibrary = true
            };

            _expectedUrl = string.Format("http://{0}/xbmcCmds/xbmcHttp?command={1}", _settings.Address, "getcurrentlyplaying");
        }

        [Test]
        public void _should_be_empty_when_no_active_players()
        {
            WithNoActivePlayers();

            Subject.GetActivePlayers(_settings).Should().BeEmpty();
        }

        [Test]
        public void should_have_active_video_player()
        {
            WithVideoPlayerActive();

            var result = Subject.GetActivePlayers(_settings);

            result.Should().HaveCount(1);
            result.First().Type.Should().Be("video");
        }
    }
}
