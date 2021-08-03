using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AlreadyImportedSpecificationFixture : CoreTest<AlreadyImportedSpecification>
    {
        private const int FIRST_EPISODE_ID = 1;
        private const string TITLE = "Series.Title.S01E01.720p.HDTV.x264-Sonarr";

        private Series _series;
        private QualityModel _hdtv720p;
        private QualityModel _hdtv1080p;
        private RemoteEpisode _remoteEpisode;
        private List<EpisodeHistory> _history;

        [SetUp]
        public void Setup()
        {
            var singleEpisodeList = new List<Episode>
                                    {
                                        new Episode
                                        {
                                            Id = FIRST_EPISODE_ID,
                                            SeasonNumber = 12,
                                            EpisodeNumber = 3,
                                            EpisodeFileId = 1
                                        }
                                    };

            _series = Builder<Series>.CreateNew()
                                     .Build();

            _hdtv720p = new QualityModel(Quality.HDTV720p, new Revision(version: 1));
            _hdtv1080p = new QualityModel(Quality.HDTV1080p, new Revision(version: 1));

            _remoteEpisode = new RemoteEpisode
            {
                Series = _series,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = _hdtv720p },
                Episodes = singleEpisodeList,
                Release = Builder<ReleaseInfo>.CreateNew()
                                              .Build()
            };

            _history = new List<EpisodeHistory>();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByEpisodeId(It.IsAny<int>()))
                  .Returns(_history);
        }

        private void GivenCdhDisabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(false);
        }

        private void GivenHistoryItem(string downloadId, string sourceTitle, QualityModel quality, EpisodeHistoryEventType eventType)
        {
            _history.Add(new EpisodeHistory
                         {
                             DownloadId = downloadId,
                             SourceTitle = sourceTitle,
                             Quality = quality,
                             Date = DateTime.UtcNow,
                             EventType = eventType
                         });
        }

        [Test]
        public void should_be_accepted_if_CDH_is_disabled()
        {
            GivenCdhDisabled();

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_episode_does_not_have_a_file()
        {
            _remoteEpisode.Episodes.First().EpisodeFileId = 0;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_episode_does_not_have_grabbed_event()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_episode_does_not_have_imported_event()
        {
            GivenHistoryItem(Guid.NewGuid().ToString().ToUpper(), TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_and_imported_quality_is_the_same()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.DownloadFolderImported);

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_download_id_and_release_torrent_hash_is_unknown()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, EpisodeHistoryEventType.DownloadFolderImported);

            _remoteEpisode.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = null)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_download_does_not_have_an_id()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(null, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, EpisodeHistoryEventType.DownloadFolderImported);

            _remoteEpisode.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_grabbed_download_id_matches_release_torrent_hash()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, EpisodeHistoryEventType.DownloadFolderImported);

            _remoteEpisode.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_release_title_matches_grabbed_event_source_title()
        {
            var downloadId = Guid.NewGuid().ToString().ToUpper();

            GivenHistoryItem(downloadId, TITLE, _hdtv720p, EpisodeHistoryEventType.Grabbed);
            GivenHistoryItem(downloadId, TITLE, _hdtv1080p, EpisodeHistoryEventType.DownloadFolderImported);

            _remoteEpisode.Release = Builder<TorrentInfo>.CreateNew()
                                                         .With(t => t.DownloadProtocol = DownloadProtocol.Torrent)
                                                         .With(t => t.InfoHash = downloadId)
                                                         .Build();

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }
    }
}
