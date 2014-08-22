using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Test.Common;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class CompletedDownloadServiceFixture : CoreTest<DownloadTrackingService>
    {
        private List<DownloadClientItem> _completed;

        [SetUp]
        public void Setup()
        {
            _completed = Builder<DownloadClientItem>.CreateListOfSize(1)
                                             .All()
                                             .With(h => h.Status = DownloadItemStatus.Completed)
                                             .With(h => h.OutputPath = @"C:\DropFolder\MyDownload".AsOsAgnostic())
                                             .With(h => h.RemoteEpisode = new RemoteEpisode
                                                {
                                                    Episodes = new List<Episode> { new Episode { Id = 1 } }
                                                })
                                             .Build()
                                             .ToList();
            
            Mocker.GetMock<IProvideDownloadClient>()
                  .Setup(c => c.GetDownloadClients())
                  .Returns( new IDownloadClient[] { Mocker.GetMock<IDownloadClient>().Object });

            Mocker.GetMock<IDownloadClient>()
                  .SetupGet(c => c.Definition)
                  .Returns(new Core.Download.DownloadClientDefinition { Id = 1, Name = "testClient" });

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.EnableCompletedDownloadHandling)
                  .Returns(true);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.RemoveCompletedDownloads)
                  .Returns(true);

            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Failed())
                  .Returns(new List<History.History>());
            
            Mocker.SetConstant<ICompletedDownloadService>(Mocker.Resolve<CompletedDownloadService>());
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

        private void GivenNoImportedHistory()
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Imported())
                  .Returns(new List<History.History>());
        }

        private void GivenImportedHistory(List<History.History> importedHistory)
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.Imported())
                  .Returns(importedHistory);
        }

        private void GivenCompletedDownloadClientHistory(bool hasStorage = true)
        {
            Mocker.GetMock<IDownloadClient>()
                  .Setup(s => s.GetItems())
                  .Returns(_completed);
            
            Mocker.GetMock<IDiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(hasStorage);
        }

        private void GivenCompletedImport()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Setup(v => v.ProcessFolder(It.IsAny<DirectoryInfo>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>
                    {
                        new ImportResult(null)
                    });
        }

        private void GivenFailedImport()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Setup(v => v.ProcessFolder(It.IsAny<DirectoryInfo>(), It.IsAny<DownloadClientItem>()))
                .Returns(new List<ImportResult>() 
                    {
                        new ImportResult(new ImportDecision(new LocalEpisode() { Path = @"C:\TestPath\Droned.S01E01.mkv" }, "Test Failure")) 
                    });
        }

        private void VerifyNoImports()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Verify(v => v.ProcessFolder(It.IsAny<DirectoryInfo>(), It.IsAny<DownloadClientItem>()), Times.Never());
        }

        private void VerifyImports()
        {
            Mocker.GetMock<IDownloadedEpisodesImportService>()
                .Verify(v => v.ProcessFolder(It.IsAny<DirectoryInfo>(), It.IsAny<DownloadClientItem>()), Times.Once());
        }

        [Test]
        public void should_process_if_matching_history_is_not_found_but_category_specified()
        {
            _completed.First().Category = "tv";

            GivenCompletedDownloadClientHistory();
            GivenNoGrabbedHistory();
            GivenNoImportedHistory();
            GivenCompletedImport();

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyImports();
        }

        [Test]
        public void should_not_process_if_matching_history_is_not_found_and_no_category_specified()
        {
            _completed.First().Category = null;

            GivenCompletedDownloadClientHistory();
            GivenNoGrabbedHistory();
            GivenNoImportedHistory();

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoImports();
        }

        [Test]
        public void should_not_process_if_grabbed_history_contains_null_downloadclient_id()
        {
            _completed.First().Category = null;

            GivenCompletedDownloadClientHistory();

            var historyGrabbed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyGrabbed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyGrabbed.First().Data.Add("downloadClientId", null);

            GivenGrabbedHistory(historyGrabbed);
            GivenNoImportedHistory();
            GivenFailedImport();

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoImports();
        }

        [Test]
        public void should_process_if_failed_history_contains_null_downloadclient_id()
        {
            GivenCompletedDownloadClientHistory();

            var historyGrabbed = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyGrabbed.First().Data.Add("downloadClient", "SabnzbdClient");
            historyGrabbed.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            GivenGrabbedHistory(historyGrabbed);

            var historyImported = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            historyImported.First().Data.Add("downloadClient", "SabnzbdClient");
            historyImported.First().Data.Add("downloadClientId", null);
            
            GivenImportedHistory(historyImported);
            GivenCompletedImport();

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyImports();
        }

        [Test]
        public void should_not_process_if_already_added_to_history_as_imported()
        {
            GivenCompletedDownloadClientHistory();
            
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();
            
            GivenGrabbedHistory(history);
            GivenImportedHistory(history);

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoImports();
        }

        [Test]
        public void should_process_if_not_already_in_imported_history()
        {
            GivenCompletedDownloadClientHistory();

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoImportedHistory();
            GivenCompletedImport();

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyImports();
        }

        [Test]
        public void should_not_process_if_storage_directory_does_not_exist()
        {
            GivenCompletedDownloadClientHistory(false);
            
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoImportedHistory();

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoImports();

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_not_process_if_storage_directory_in_drone_factory()
        {
            GivenCompletedDownloadClientHistory(true);

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoImportedHistory();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(v => v.DownloadedEpisodesFolder)
                  .Returns(@"C:\DropFolder".AsOsAgnostic());

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoImports();

            ExceptionVerification.IgnoreWarns();
        }

        [Test]
        public void should_process_as_already_imported_if_drone_factory_import_history_exists()
        {
            GivenCompletedDownloadClientHistory(false);

            _completed.Clear();
            _completed.AddRange(Builder<DownloadClientItem>.CreateListOfSize(2)
                                             .All()
                                             .With(h => h.Status = DownloadItemStatus.Completed)
                                             .With(h => h.OutputPath = @"C:\DropFolder\MyDownload".AsOsAgnostic())
                                             .With(h => h.RemoteEpisode = new RemoteEpisode
                                             {
                                                 Episodes = new List<Episode> { new Episode { Id = 1 } }
                                             })
                                             .Build());

            var grabbedHistory = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(d => d.Data["downloadClient"] = "SabnzbdClient")
                                                  .TheFirst(1)
                                                  .With(d => d.Data["downloadClientId"] = _completed.First().DownloadClientId)
                                                  .With(d => d.SourceTitle = "Droned.S01E01.720p-LAZY")
                                                  .TheLast(1)
                                                  .With(d => d.Data["downloadClientId"] = _completed.Last().DownloadClientId)
                                                  .With(d => d.SourceTitle = "Droned.S01E01.Proper.720p-LAZY")
                                                  .Build()
                                                  .ToList();

            var importedHistory = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(d => d.EpisodeId = 1)
                                                  .TheFirst(1)
                                                  .With(d => d.Data["droppedPath"] = @"C:\mydownload\Droned.S01E01.720p-LAZY\lzy-dr101.mkv".AsOsAgnostic())
                                                  .TheLast(1)
                                                  .With(d => d.Data["droppedPath"] = @"C:\mydownload\Droned.S01E01.Proper.720p-LAZY\lzy-dr101.mkv".AsOsAgnostic())
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(grabbedHistory);
            GivenImportedHistory(importedHistory);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            VerifyNoImports();

            Mocker.GetMock<IHistoryService>()
                .Verify(v => v.UpdateHistoryData(It.IsAny<int>(), It.IsAny<Dictionary<String, String>>()), Times.Exactly(2));
        }

        [Test]
        public void should_not_remove_if_config_disabled()
        {
            GivenCompletedDownloadClientHistory();

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoImportedHistory();
            GivenCompletedImport();

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.RemoveCompletedDownloads)
                  .Returns(false);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_not_remove_while_readonly()
        {
            GivenCompletedDownloadClientHistory();

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoImportedHistory();
            GivenCompletedImport();

            _completed.First().IsReadOnly = true;

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);
            
            Subject.Execute(new CheckForFinishedDownloadCommand());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Never());
        }

        [Test]
        public void should_not_remove_if_imported_failed()
        {
            GivenCompletedDownloadClientHistory();

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoImportedHistory();
            GivenFailedImport();

            _completed.First().IsReadOnly = true;

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Never());

            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_remove_if_imported()
        {
            GivenCompletedDownloadClientHistory();

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoImportedHistory();
            GivenCompletedImport();

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Once());
        }

        [Test]
        public void should_not_mark_as_successful_if_no_files_were_imported()
        {
            GivenCompletedDownloadClientHistory();

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .Build()
                                                  .ToList();

            GivenGrabbedHistory(history);
            GivenNoImportedHistory();

            Mocker.GetMock<IDownloadedEpisodesImportService>()
                  .Setup(v => v.ProcessFolder(It.IsAny<DirectoryInfo>(), It.IsAny<DownloadClientItem>()))
                  .Returns(new List<ImportResult>
                           {
                               new ImportResult(
                                   new ImportDecision(new LocalEpisode() {Path = @"C:\TestPath\Droned.S01E01.mkv"}),
                                   "Test Failure")
                           });

            history.First().Data.Add("downloadClient", "SabnzbdClient");
            history.First().Data.Add("downloadClientId", _completed.First().DownloadClientId);

            Subject.Execute(new CheckForFinishedDownloadCommand());

            Mocker.GetMock<IDiskProvider>()
                .Verify(c => c.DeleteFolder(It.IsAny<string>(), true), Times.Never());

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
