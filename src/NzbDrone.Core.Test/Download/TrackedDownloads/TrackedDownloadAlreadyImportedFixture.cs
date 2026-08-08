using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadAlreadyImportedFixture : CoreTest<TrackedDownloadAlreadyImported>
    {
        private readonly OsPath _outputPath = new OsPath(@"C:\DropFolder\MyDownload".AsOsAgnostic());
        private readonly OsPath _otherDownloadOutputPath = new OsPath(@"C:\DropFolder\SomeOtherDownload".AsOsAgnostic());
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
                                                         .With(d => d.OutputPath = _outputPath)
                                                         .Build();

            _trackedDownload = Builder<TrackedDownload>.CreateNew()
                                                       .With(t => t.RemoteEpisode = remoteEpisode)
                                                       .With(t => t.DownloadItem = downloadItem)
                                                       .With(t => t.ImportItem = downloadItem)
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
            GivenHistoryForEpisode(episode, _outputPath, eventTypes);
        }

        public void GivenHistoryForEpisode(Episode episode, OsPath droppedFromPath, params EpisodeHistoryEventType[] eventTypes)
        {
            foreach (var eventType in eventTypes)
            {
                var history = Builder<EpisodeHistory>.CreateNew()
                                            .With(h => h.EpisodeId = episode.Id)
                                            .With(h => h.EventType = eventType)
                                            .Build();

                if (eventType == EpisodeHistoryEventType.DownloadFolderImported)
                {
                    history.Data["DroppedPath"] = Path.Combine(droppedFromPath.FullPath, "episode.mkv");
                }

                _historyItems.Add(history);
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

        [Test]
        public void should_return_false_if_download_folder_imported_history_came_from_a_different_download()
        {
            // Reproduces a data-loss scenario: a download client's own duplicate
            // detection (e.g. NZBGet recognizing repeat grabs of the same
            // release) can cause DownloadItem.DownloadId to be reused across
            // genuinely distinct download attempts. HistoryService.FindByDownloadId
            // then surfaces an OLDER, unrelated attempt's DownloadFolderImported
            // event for this episode, even though the CURRENT download's own
            // files (at _outputPath) were never imported. Matching by EpisodeId
            // alone falsely reports this download as complete - which, combined
            // with removeCompletedDownloads, deletes its never-imported files.
            GivenEpisodes(1);

            GivenHistoryForEpisode(_episodes[0], _otherDownloadOutputPath, EpisodeHistoryEventType.DownloadFolderImported, EpisodeHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }

        [Test]
        public void should_return_false_if_only_one_episode_in_multi_episode_download_has_matching_import_path()
        {
            GivenEpisodes(2);

            GivenHistoryForEpisode(_episodes[0], _outputPath, EpisodeHistoryEventType.DownloadFolderImported, EpisodeHistoryEventType.Grabbed);
            GivenHistoryForEpisode(_episodes[1], _otherDownloadOutputPath, EpisodeHistoryEventType.DownloadFolderImported, EpisodeHistoryEventType.Grabbed);

            Subject.IsImported(_trackedDownload, _historyItems)
                   .Should()
                   .BeFalse();
        }
    }
}
