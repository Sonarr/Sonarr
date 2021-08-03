using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadAlreadyImportedFixture : CoreTest<TrackedDownloadAlreadyImported>
    {
        private List<Episode> _episodes;
        private TrackedDownload _trackedDownload;
        private List<EpisodeHistory> _historyItems;

        [SetUp]
        public void Setup()
        {
            _episodes = new List<Episode>();

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(r => r.Episodes = _episodes)
                                                      .Build();

            var downloadItem = Builder<DownloadClientItem>.CreateNew()
                                                         .Build();

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                                                       .With(t => t.RemoteEpisode = remoteEpisode)
                                                       .With(t => t.DownloadItem = downloadItem)
                                                       .Build();

            _historyItems = new List<EpisodeHistory>();
        }

        public void GivenEpisodes(int count)
        {
            _episodes.AddRange(Builder<Episode>.CreateListOfSize(count)
                                               .BuildList());
        }

        public void GivenHistoryForEpisode(Episode episode, params EpisodeHistoryEventType[] eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                _historyItems.Add(
                    Builder<EpisodeHistory>.CreateNew()
                                            .With(h => h.EpisodeId = episode.Id)
                                            .With(h => h.EventType = eventType)
                                            .Build());
            }
        }

        [Test]
        public void should_return_false_if_there_is_no_history()
        {
            GivenEpisodes(1);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_single_episode_download_is_not_imported()
        {
            GivenEpisodes(1);

            GivenHistoryForEpisode(_episodes[0], EpisodeHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_no_episode_in_multi_episode_download_is_imported()
        {
            GivenEpisodes(2);

            GivenHistoryForEpisode(_episodes[0], EpisodeHistoryEventType.Grabbed);
            GivenHistoryForEpisode(_episodes[1], EpisodeHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_should_return_false_if_only_one_episode_in_multi_episode_download_is_imported()
        {
            GivenEpisodes(2);

            GivenHistoryForEpisode(_episodes[0], EpisodeHistoryEventType.DownloadFolderImported, EpisodeHistoryEventType.Grabbed);
            GivenHistoryForEpisode(_episodes[1], EpisodeHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_true_if_single_episode_download_is_imported()
        {
            GivenEpisodes(1);

            GivenHistoryForEpisode(_episodes[0], EpisodeHistoryEventType.DownloadFolderImported, EpisodeHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_multi_episode_download_is_imported()
        {
            GivenEpisodes(2);

            GivenHistoryForEpisode(_episodes[0], EpisodeHistoryEventType.DownloadFolderImported, EpisodeHistoryEventType.Grabbed);
            GivenHistoryForEpisode(_episodes[1], EpisodeHistoryEventType.DownloadFolderImported, EpisodeHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeTrue();
        }
    }
}
