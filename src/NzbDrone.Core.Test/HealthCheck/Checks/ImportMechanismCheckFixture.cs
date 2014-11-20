using System;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class ImportMechanismCheckFixture : CoreTest<ImportMechanismCheck>
    {
        private const string DRONE_FACTORY_FOLDER = @"C:\Test\Unsorted";

        private IList<TrackedDownload> _completed;

        private void GivenCompletedDownloadHandling(bool? enabled = null)
        {
            if (enabled.HasValue)
            {
                Mocker.GetMock<IConfigService>()
                      .Setup(s => s.IsDefined("EnableCompletedDownloadHandling"))
                      .Returns(true);

                Mocker.GetMock<IConfigService>()
                      .SetupGet(s => s.EnableCompletedDownloadHandling)
                      .Returns(enabled.Value);
            }

            _completed = Builder<TrackedDownload>.CreateListOfSize(1)
                .All()
                .With(v => v.State == TrackedDownloadState.Downloading)
                .With(v => v.DownloadItem = new DownloadClientItem())
                .With(v => v.DownloadItem.Status = DownloadItemStatus.Completed)
                .With(v => v.DownloadItem.OutputPath = new OsPath(@"C:\Test\DropFolder\myfile.mkv".AsOsAgnostic()))
                .Build();

            Mocker.GetMock<IDownloadTrackingService>()
                .Setup(v => v.GetCompletedDownloads())
                .Returns(_completed.ToArray());
        }

        private void GivenDroneFactoryFolder(bool exists = false)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.DownloadedEpisodesFolder)
                  .Returns(DRONE_FACTORY_FOLDER.AsOsAgnostic());

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(DRONE_FACTORY_FOLDER.AsOsAgnostic()))
                  .Returns(exists);
        }

        [Test]
        public void should_return_warning_when_completed_download_handling_not_configured()
        {
            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_warning_when_both_completeddownloadhandling_and_dronefactory_are_not_configured()
        {
            GivenCompletedDownloadHandling(false);
            
            Subject.Check().ShouldBeWarning();
        }

        [Test]
        public void should_return_warning_when_downloadclient_drops_in_dronefactory_folder()
        {
            GivenCompletedDownloadHandling(true);
            GivenDroneFactoryFolder(true);

            _completed.First().DownloadItem.OutputPath = new OsPath((DRONE_FACTORY_FOLDER + @"\myfile.mkv").AsOsAgnostic());

            Subject.Check().ShouldBeWarning();
        }
        
        [Test]
        public void should_return_ok_when_no_issues_found()
        {
            GivenCompletedDownloadHandling(true);
            GivenDroneFactoryFolder(true);

            Subject.Check().ShouldBeOk();
        }
    }
}
