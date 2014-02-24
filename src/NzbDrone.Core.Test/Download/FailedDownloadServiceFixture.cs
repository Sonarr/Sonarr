using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class FailedDownloadServiceFixture : CoreTest<FailedDownloadService>
    {
        private List<HistoryItem> _completed;
        private List<HistoryItem> _failed;

        [SetUp]
        public void Setup()
        {
            _completed = Builder<HistoryItem>.CreateListOfSize(5)
                                             .All()
                                             .With(h => h.Status = HistoryStatus.Completed)
                                             .Build()
                                             .ToList();

            _failed = Builder<HistoryItem>.CreateListOfSize(1)
                                          .All()
                                          .With(h => h.Status = HistoryStatus.Failed)
                                          .Build()
                                          .ToList();

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClient()).Returns(Mocker.GetMock<IDownloadClient>().Object);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableFailedDownloadHandling)
                  .Returns(true);
        }

        private void GivenNoGrabbedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Grabbed())
                  .Returns(new List<History.History>());
        }

        private void GivenGrabbedHistory(List<History.History> history)
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Grabbed())
                  .Returns(history);
        }

        private void GivenNoFailedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Failed())
                  .Returns(new List<History.History>());
        }

        private void GivenFailedHistory(List<History.History> failedHistory)
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Failed())
                  .Returns(failedHistory);
        }

        private void GivenFailedDownloadClientHistory()
        {
            Mocker.GetMock<IDownloadClient>()
                  .Setup(s => s.GetHistory(0, 20))
                  .Returns(_failed);
        }

        private void VerifyNoFailedDownloads()
        {
            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.IsAny<DownloadFailedEvent>()), Times.Never());
        }

        private void VerifyFailedDownloads(int count = 1)
        {
            Mocker.GetMock<IEventAggregator>()
                .Verify(v => v.PublishEvent(It.Is<DownloadFailedEvent>(d => d.EpisodeIds.Count == count)), Times.Once());
        }

        [Test]
        public void should_not_process_if_no_download_client_history()
        {
            Mocker.GetMock<IDownloadClient>()
                  .Setup(s => s.GetHistory(0, 20))
                  .Returns(new List<HistoryItem>());

            Subject.Execute(new CheckForFailedDownloadCommand());

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.BetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>(), HistoryEventType.Grabbed),
                      Times.Never());

            VerifyNoFailedDownloads();
        }

        [Test]
        public void should_not_process_if_no_failed_items_in_download_client_history()
        {
            Mocker.GetMock<IDownloadClient>()
                  .Setup(s => s.GetHistory(0, 20))
                  .Returns(_completed);

            Subject.Execute(new CheckForFailedDownloadCommand());

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.BetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>(), HistoryEventType.Grabbed),
                      Times.Never());

            VerifyNoFailedDownloads();
        }

        [Test]
        public void should_not_process_if_matching_history_is_not_found()
        {
            GivenNoGrabbedHistory();
            GivenFailedDownloadClientHistory();

            Subject.Execute(new CheckForFailedDownloadCommand());

            VerifyNoFailedDownloads();
        }

        [Test]
        public void should_not_process_if_grabbed_history_contains_null_downloadclient_id()
        {
            GivenFailedDownloadClientHistory();

            var historyGrabbed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyGrabbed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyGrabbed.First().Data.Add("downloadClientId", null);

            GivenGrabbedHistory(historyGrabbed);
            GivenNoFailedHistory();

            Subject.Execute(new CheckForFailedDownloadCommand());

            VerifyNoFailedDownloads();
        }

        [Test]
        public void should_process_if_failed_history_contains_null_downloadclient_id()
        {
            GivenFailedDownloadClientHistory();

            var historyGrabbed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyGrabbed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyGrabbed.First().Data.Add("downloadClientId", _failed.First().Id);

            GivenGrabbedHistory(historyGrabbed);

            var historyFailed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyFailed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyFailed.First().Data.Add("downloadClientId", null);
            
            GivenFailedHistory(historyFailed);

            Subject.Execute(new CheckForFailedDownloadCommand());

            VerifyFailedDownloads();
        }

        [Test]
        public void should_not_process_if_already_added_to_history_as_failed()
        {
            GivenFailedDownloadClientHistory();
            
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();
            
            GivenGrabbedHistory(history);
            GivenFailedHistory(history);

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _failed.First().Id);

            Subject.Execute(new CheckForFailedDownloadCommand());

            VerifyNoFailedDownloads();
        }

        [Test]
        public void should_process_if_not_already_in_failed_history()
        {
            GivenFailedDownloadClientHistory();

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoFailedHistory();

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _failed.First().Id);

            Subject.Execute(new CheckForFailedDownloadCommand());

            VerifyFailedDownloads();
        }

        [Test]
        public void should_have_multiple_episode_ids_when_multi_episode_release_fails()
        {
            GivenFailedDownloadClientHistory();

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoFailedHistory();

            history.ForEach(h =>
            {
                h.Data.Add("downloadClient", "SabnzbdClient");
                h.Data.Add("downloadClientId", _failed.First().Id);
            });

            Subject.Execute(new CheckForFailedDownloadCommand());

            VerifyFailedDownloads(2);
        }

        [Test]
        public void should_skip_if_enable_failed_download_handling_is_off()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableFailedDownloadHandling)
                  .Returns(false);

            Subject.Execute(new CheckForFailedDownloadCommand());

            VerifyNoFailedDownloads();
        }
    }
}
