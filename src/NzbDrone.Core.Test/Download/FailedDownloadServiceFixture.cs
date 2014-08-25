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
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class FailedDownloadServiceFixture : CoreTest<DownloadTrackingService>
    {
        private List<DownloadClientItem> _completed;
        private List<DownloadClientItem> _failed;

        [SetUp]
        public void Setup()
        {
            _completed = Builder<DownloadClientItem>.CreateListOfSize(5)
                                             .All()
                                             .With(h => h.Status = DownloadItemStatus.Completed)
                                             .Build()
                                             .ToList();

            _failed = Builder<DownloadClientItem>.CreateListOfSize(1)
                                          .All()
                                          .With(h => h.Status = DownloadItemStatus.Failed)
                                          .Build()
                                          .ToList();

            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClients())
                  .Returns( new IDownloadClient[] { Mocker.GetMock<IDownloadClient>().Object });

            Mocker.GetMock<IDownloadClient>()
                  .SetupGet(c => c.Definition)
                  .Returns(new Core.Download.DownloadClientDefinition { Id = 1, Name = "testClient" });

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableFailedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Imported())
                  .Returns(new List<History.History>());

            Mocker.SetConstant<IFailedDownloadService>(Mocker.Resolve<FailedDownloadService>());
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
                  .Setup(s => s.GetItems())
                  .Returns(_failed);
        }

        private void GivenGracePeriod(int hours)
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.BlacklistGracePeriod).Returns(hours);
        }

        private void GivenRetryLimit(int count)
        {
            Mocker.GetMock<IConfigService>().SetupGet(s => s.BlacklistRetryLimit).Returns(count);
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
                  .Setup(s => s.GetItems())
                  .Returns(new List<DownloadClientItem>());

            Subject.Execute(new CheckForFinishedDownloadCommand());

            Mocker.GetMock<IHistoryService>()
                  .Verify(s => s.BetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>(), HistoryEventType.Grabbed),
                      Times.Never());

            VerifyNoFailedDownloads();
        }

        [Test]
        public void should_not_process_if_no_failed_items_in_download_client_history()
        {
            GivenNoGrabbedHistory();
            GivenNoFailedHistory();

            Mocker.GetMock<IDownloadClient>()
                  .Setup(s => s.GetItems())
                  .Returns(_completed);

            Subject.Execute(new CheckForFinishedDownloadCommand());

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

            Subject.Execute(new CheckForFinishedDownloadCommand());

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

            Subject.Execute(new CheckForFinishedDownloadCommand());

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
            historyGrabbed.First().Data.Add("downloadClientId", _failed.First().DownloadClientId);

            GivenGrabbedHistory(historyGrabbed);

            var historyFailed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyFailed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyFailed.First().Data.Add("downloadClientId", null);
            
            GivenFailedHistory(historyFailed);

            Subject.Execute(new CheckForFinishedDownloadCommand());

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
            history.First().Data.Add("downloadClientId", _failed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

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
            history.First().Data.Add("downloadClientId", _failed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

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
                h.Data.Add("downloadClientId", _failed.First().DownloadClientId);
            });

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyFailedDownloads(2);
        }

        [Test]
        public void should_skip_if_enable_failed_download_handling_is_off()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableFailedDownloadHandling)
                  .Returns(false);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoFailedDownloads();
        }

        [Test]
        public void should_not_process_if_failed_due_to_lack_of_disk_space()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenFailedDownloadClientHistory();

            _failed.First().Message = "Unpacking failed, write error or disk is full?";

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoFailedDownloads();
        }

        [Test]
        public void should_process_if_ageHours_is_not_set()
        {
            GivenFailedDownloadClientHistory();

            var historyGrabbed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyGrabbed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyGrabbed.First().Data.Add("downloadClientId", _failed.First().DownloadClientId);

            GivenGrabbedHistory(historyGrabbed);
            GivenNoFailedHistory();

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyFailedDownloads();
        }

        [Test]
        public void should_process_if_age_is_greater_than_grace_period()
        {
            GivenFailedDownloadClientHistory();

            var historyGrabbed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyGrabbed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyGrabbed.First().Data.Add("downloadClientId", _failed.First().DownloadClientId);
            historyGrabbed.First().Data.Add("ageHours", "48");

            GivenGrabbedHistory(historyGrabbed);
            GivenNoFailedHistory();

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyFailedDownloads();
        }

        [Test]
        public void should_process_if_retry_count_is_greater_than_grace_period()
        {
            GivenFailedDownloadClientHistory();

            var historyGrabbed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyGrabbed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyGrabbed.First().Data.Add("downloadClientId", _failed.First().DownloadClientId);
            historyGrabbed.First().Data.Add("ageHours", "48");

            GivenGrabbedHistory(historyGrabbed);
            GivenNoFailedHistory();
            GivenGracePeriod(6);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyFailedDownloads();
        }

        [Test]
        public void should_not_process_if_age_is_less_than_grace_period()
        {
            GivenFailedDownloadClientHistory();

            var historyGrabbed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyGrabbed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyGrabbed.First().Data.Add("downloadClientId", _failed.First().DownloadClientId);
            historyGrabbed.First().Data.Add("ageHours", "1");

            GivenGrabbedHistory(historyGrabbed);
            GivenNoFailedHistory();
            GivenGracePeriod(6);
            GivenRetryLimit(1);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoFailedDownloads();

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_manual_mark_all_episodes_of_release_as_failed()
        {
            var historyFailed = Builder<History.History>.CreateListOfSize(2)
                .All()
                .With(v => v.EventType == HistoryEventType.Grabbed)
                .Do(v => v.Data.Add("downloadClient", "SabnzbdClient"))
                .Do(v => v.Data.Add("downloadClientId", "test"))
                .Build()
                .ToList();

            GivenGrabbedHistory(historyFailed);

            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.Get(It.IsAny<Int32>()))
                .Returns<Int32>(i => historyFailed.FirstOrDefault(v => v.Id == i));

            Subject.MarkAsFailed(1);

            VerifyFailedDownloads(2);
        }
    }
}
