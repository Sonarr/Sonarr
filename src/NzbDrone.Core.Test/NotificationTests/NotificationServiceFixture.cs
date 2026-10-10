using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class NotificationServiceFixture : CoreTest<NotificationService>
    {
        private Series _series;
        private Mock<INotification> _notification;

        [SetUp]
        public void Setup()
        {
            _series = new Series { Id = 1, Title = "Series Title", Path = @"C:\TV\Series Title".AsOsAgnostic() };

            _notification = new Mock<INotification>();
            _notification.SetupGet(n => n.Definition).Returns(new NotificationDefinition { Id = 1, Name = "Test", OnImportComplete = true });

            Mocker.GetMock<INotificationFactory>()
                  .Setup(s => s.OnGrabEnabled(It.IsAny<bool>()))
                  .Returns([_notification.Object]);

            Mocker.GetMock<INotificationFactory>()
                  .Setup(s => s.OnImportCompleteEnabled(It.IsAny<bool>()))
                  .Returns([_notification.Object]);
        }

        private RemoteEpisode GivenRemoteEpisode(string title, params int[] seasonNumbers)
        {
            return new RemoteEpisode
            {
                Series = _series,
                ParsedEpisodeInfo = Parser.Parser.ParseTitle(title),
                Episodes = seasonNumbers
                    .SelectMany(s => Enumerable.Range(1, 2).Select(e => new Episode { SeasonNumber = s, EpisodeNumber = e, Title = $"Episode {e}" }))
                    .ToList()
            };
        }

        private string GivenImportCompleteMessage(RemoteEpisode remoteEpisode)
        {
            ImportCompleteMessage sent = null;
            _notification.Setup(n => n.OnImportComplete(It.IsAny<ImportCompleteMessage>())).Callback<ImportCompleteMessage>(m => sent = m);

            var trackedDownload = new TrackedDownload
            {
                RemoteEpisode = remoteEpisode,
                DownloadItem = new DownloadClientItem
                {
                    DownloadClientInfo = new DownloadClientItemClientInfo(),
                    OutputPath = new OsPath(@"C:\Downloads\Series.Title".AsOsAgnostic())
                }
            };

            Subject.Handle(new DownloadCompletedEvent(trackedDownload, _series.Id, [new EpisodeFile { RelativePath = "Season 1/file.mkv" }], null));

            return sent.Message;
        }

        [Test]
        public void should_show_season_range_when_grabbing_multi_season_pack()
        {
            GrabMessage sent = null;
            _notification.Setup(n => n.OnGrab(It.IsAny<GrabMessage>())).Callback<GrabMessage>(m => sent = m);

            Subject.Handle(new EpisodeGrabbedEvent(GivenRemoteEpisode("Series.Title.S01-S03.1080p.BluRay.x264-RlsGrp", 1, 2, 3)));

            sent.Message.Should().Be("Series Title - Seasons 1-3 [Bluray-1080p]");
        }

        [Test]
        public void should_list_episodes_when_grabbing_single_season()
        {
            GrabMessage sent = null;
            _notification.Setup(n => n.OnGrab(It.IsAny<GrabMessage>())).Callback<GrabMessage>(m => sent = m);

            Subject.Handle(new EpisodeGrabbedEvent(GivenRemoteEpisode("Series.Title.S02E01E02.1080p.BluRay.x264-RlsGrp", 2)));

            sent.Message.Should().Be("Series Title - 2x01x02 - Episode 1 + Episode 2 [Bluray-1080p]");
        }

        [Test]
        public void should_show_season_range_when_multi_season_pack_is_imported()
        {
            GivenImportCompleteMessage(GivenRemoteEpisode("Series.Title.S01-S03.1080p.BluRay.x264-RlsGrp", 1, 2, 3))
                .Should().Be("Series Title - Seasons 1-3 [Bluray-1080p]");
        }

        [Test]
        public void should_show_season_when_season_pack_is_imported()
        {
            GivenImportCompleteMessage(GivenRemoteEpisode("Series.Title.S02.1080p.BluRay.x264-RlsGrp", 2))
                .Should().Be("Series Title - Season 2 [Bluray-1080p]");
        }
    }
}
