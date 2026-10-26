using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Notifications.Trakt.Resource;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.NotificationTests.TraktTests
{
    [TestFixture]
    public class TraktFixture : CoreTest<Trakt>
    {
        private DownloadMessage _downloadMessage;
        private NotificationDefinition _traktDefinition;

        [SetUp]
        public void Setup()
        {
            _downloadMessage = new DownloadMessage
            {
                Series = new Series(),
                EpisodeFile = new EpisodeFile
                {
                    MediaInfo = null,
                    Quality = new QualityModel
                    {
                        Quality = Quality.Unknown
                    },
                    Episodes = new List<Episode>
                    {
                        new Episode { EpisodeNumber = 1 }
                    }
                }
            };

            _traktDefinition = new NotificationDefinition
            {
                Settings = new TraktSettings
                {
                    AccessToken = "",
                    RefreshToken = "",
                    Expires = DateTime.Now.AddDays(1)
                }
            };

            Subject.Definition = _traktDefinition;
        }

        private void GivenValidMediaInfo(Quality quality, string channelPositions, int channels, string audioFormat, string scanType, HdrFormat hdrFormat = HdrFormat.None)
        {
            _downloadMessage.EpisodeFile.MediaInfo = new MediaInfoModel
            {
                AudioStreams =
                [
                    new MediaInfoAudioStreamModel
                    {
                        ChannelPositions = channelPositions,
                        Channels = channels,
                        Format = audioFormat
                    }
                ],
                ScanType = scanType,
                VideoHdrFormat = hdrFormat
            };

            _downloadMessage.EpisodeFile.Quality.Quality = quality;
        }

        [Test]
        public void should_add_collection_episode_if_null_mediainfo()
        {
            Subject.OnDownload(_downloadMessage);

            Mocker.GetMock<ITraktProxy>()
                  .Verify(v => v.AddToCollection(It.IsAny<TraktCollectShowsResource>(), It.IsAny<string>()), Times.Once());
        }

        [TestCase("en-US")]
        [TestCase("fr-FR")]
        [TestCase("nb-NO")]
        [TestCase("nn-NO")]
        public void should_add_collection_episode_if_valid_mediainfo(string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

            GivenValidMediaInfo(Quality.Bluray2160p, "5.1", 6, "dts", "Progressive", HdrFormat.DolbyVisionHdr10);

            Subject.OnDownload(_downloadMessage);

            Mocker.GetMock<ITraktProxy>()
                  .Verify(v => v.AddToCollection(It.Is<TraktCollectShowsResource>(t =>
                    t.Shows.First().Seasons.First().Episodes.First().Audio == "dts" &&
                    t.Shows.First().Seasons.First().Episodes.First().AudioChannels == "5.1" &&
                    t.Shows.First().Seasons.First().Episodes.First().Resolution == "uhd_4k" &&
                    t.Shows.First().Seasons.First().Episodes.First().MediaType == "bluray" &&
                    t.Shows.First().Seasons.First().Episodes.First().Hdr == "hdr10"),
                  It.IsAny<string>()),
                      Times.Once());
        }

        [TestCase("en-US")]
        [TestCase("fr-FR")]
        [TestCase("nb-NO")]
        [TestCase("nn-NO")]
        public void should_format_audio_channels_to_one_decimal_when_adding_collection_episode(string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

            GivenValidMediaInfo(Quality.Bluray2160p, null, 2, "dts", "Progressive", HdrFormat.DolbyVisionHdr10);

            Subject.OnDownload(_downloadMessage);

            Mocker.GetMock<ITraktProxy>()
                  .Verify(v => v.AddToCollection(It.Is<TraktCollectShowsResource>(t =>
                    t.Shows.First().Seasons.First().Episodes.First().AudioChannels == "2.0"),
                  It.IsAny<string>()),
                      Times.Once());
        }

        [TestCase("en-US")]
        [TestCase("fr-FR")]
        [TestCase("nb-NO")]
        [TestCase("nn-NO")]
        public void should_send_empty_audio_channels_if_unknown(string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

            GivenValidMediaInfo(Quality.Bluray2160p, null, 0, "dts", "Progressive", HdrFormat.DolbyVisionHdr10);

            Subject.OnDownload(_downloadMessage);

            Mocker.GetMock<ITraktProxy>()
                  .Verify(v => v.AddToCollection(It.Is<TraktCollectShowsResource>(t =>
                    t.Shows.First().Seasons.First().Episodes.First().AudioChannels == string.Empty),
                  It.IsAny<string>()),
                      Times.Once());
        }
    }
}
