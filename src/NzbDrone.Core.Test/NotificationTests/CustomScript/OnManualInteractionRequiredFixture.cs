using System.Collections.Generic;
using System.Collections.Specialized;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Notifications.CustomScript;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.NotificationTests.CustomScript
{
    [TestFixture]
    public class OnManualInteractionRequiredFixture : CoreTest<Notifications.CustomScript.CustomScript>
    {
        private ManualInteractionRequiredMessage _manualInteractionRequiredMessage;

        [SetUp]
        public void Setup()
        {
            _manualInteractionRequiredMessage = Builder<ManualInteractionRequiredMessage>.CreateNew()
                .With(m => m.Series = Builder<Series>.CreateNew().Build())
                .With(m => m.TrackedDownload = Builder<TrackedDownload>.CreateNew().Build())
                .Build();

            Subject.Definition = new NotificationDefinition();
            Subject.Definition.Settings = new CustomScriptSettings
                                          {
                                              Path = "test.sh"
                                          };

            Mocker.GetMock<IProcessProvider>()
                .Setup(s => s.StartAndCapture(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<StringDictionary>()))
                .Returns(new ProcessOutput());

            Mocker.GetMock<ITagRepository>()
                .Setup(s => s.GetTags(It.IsAny<HashSet<int>>()))
                .Returns(new List<Tag>());
        }

        [Test]
        public void should_not_throw_when_series_is_null()
        {
            _manualInteractionRequiredMessage.Series = null;

            Subject.OnManualInteractionRequired(_manualInteractionRequiredMessage);

            Mocker.GetMock<IProcessProvider>()
                .Verify(v => v.StartAndCapture(It.IsAny<string>(), It.IsAny<string>(), It.Is<StringDictionary>(d => !d.ContainsKey("Sonarr_Series_Id"))), Times.Once());
        }

        [Test]
        public void should_not_throw_when_tracked_download_item_is_null()
        {
            _manualInteractionRequiredMessage.TrackedDownload.DownloadItem = null;

            Subject.OnManualInteractionRequired(_manualInteractionRequiredMessage);

            Mocker.GetMock<IProcessProvider>()
                .Verify(v => v.StartAndCapture(It.IsAny<string>(), It.IsAny<string>(), It.Is<StringDictionary>(d => d["Sonarr_Download_Title"] == string.Empty && d["Sonarr_Download_Size"] == string.Empty)), Times.Once());
        }
    }
}
