using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Notifications.Xbmc;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.NotificationTests.Xbmc.Json
{
    [TestFixture]
    public class ActivePlayersFixture : CoreTest<JsonApiProvider>
    {
        private XbmcSettings _settings;

        private void WithNoActivePlayers()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.IsAny<string>()))
                  .Returns("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[]}");
        }

        private void WithVideoPlayerActive()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.IsAny<string>()))
                  .Returns("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[{\"playerid\":1,\"type\":\"video\"}]}");
        }

        private void WithAudioPlayerActive()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.IsAny<string>()))
                  .Returns("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[{\"playerid\":1,\"type\":\"audio\"}]}");
        }

        private void WithPicturePlayerActive()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.IsAny<string>()))
                  .Returns("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[{\"playerid\":1,\"type\":\"picture\"}]}");
        }

        private void WithAllPlayersActive()
        {
            Mocker.GetMock<IHttpProvider>()
                  .Setup(s => s.PostCommand(_settings.Address, _settings.Username, _settings.Password, It.IsAny<string>()))
                  .Returns("{\"id\":10,\"jsonrpc\":\"2.0\",\"result\":[{\"playerid\":1,\"type\":\"audio\"},{\"playerid\":2,\"type\":\"picture\"},{\"playerid\":3,\"type\":\"video\"}]}");
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

        [Test]
        public void should_have_active_audio_player()
        {
            WithAudioPlayerActive();
            
            var result = Subject.GetActivePlayers(_settings);

            result.Should().HaveCount(1);
            result.First().Type.Should().Be("audio");
        }

        [Test]
        public void should_have_active_picture_player()
        {
            WithPicturePlayerActive();

            var result = Subject.GetActivePlayers(_settings);
            
            result.Should().HaveCount(1);
            result.First().Type.Should().Be("picture");
        }

        [Test]
        public void should_have_all_players_active()
        {
            WithAllPlayersActive();

            var result = Subject.GetActivePlayers(_settings);

            result.Should().HaveCount(3);
            result.Select(a => a.PlayerId).Distinct().Should().HaveCount(3);
            result.Select(a => a.Type).Distinct().Should().HaveCount(3);
        }
    }
}
