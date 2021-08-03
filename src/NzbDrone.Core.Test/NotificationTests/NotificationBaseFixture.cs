using System;
using System.Collections.Generic;
using FluentAssertions;
using FluentValidation.Results;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests
{
    [TestFixture]
    public class NotificationBaseFixture : TestBase
    {
        private class TestSetting : IProviderConfig
        {
            public NzbDroneValidationResult Validate()
            {
                return new NzbDroneValidationResult();
            }
        }

        private class TestNotificationWithOnDownload : NotificationBase<TestSetting>
        {
            public override string Name => "TestNotification";
            public override string Link => "";

            public override ValidationResult Test()
            {
                throw new NotImplementedException();
            }

            public override void OnDownload(DownloadMessage downloadMessage)
            {
                TestLogger.Info("OnDownload was called");
            }
        }

        private class TestNotificationWithAllEvents : NotificationBase<TestSetting>
        {
            public override string Name => "TestNotification";
            public override string Link => "";

            public override ValidationResult Test()
            {
                throw new NotImplementedException();
            }

            public override void OnGrab(GrabMessage grabMessage)
            {
                TestLogger.Info("OnGrab was called");
            }

            public override void OnDownload(DownloadMessage message)
            {
                TestLogger.Info("OnDownload was called");
            }

            public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
            {
                TestLogger.Info("OnRename was called");
            }

            public override void OnEpisodeFileDelete(EpisodeDeleteMessage message)
            {
                TestLogger.Info("Episode OnDelete was called");
            }

            public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
            {
                TestLogger.Info("Series OnDelete was called");
            }

            public override void OnHealthIssue(NzbDrone.Core.HealthCheck.HealthCheck artist)
            {
                TestLogger.Info("OnHealthIssue was called");
            }

            public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
            {
                TestLogger.Info("OnApplicationUpdate was called");
            }
        }

        private class TestNotificationWithNoEvents : NotificationBase<TestSetting>
        {
            public override string Name => "TestNotification";
            public override string Link => "";

            public override ValidationResult Test()
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        public void should_support_OnUpgrade_should_link_to_OnDownload()
        {
            var notification = new TestNotificationWithOnDownload();

            notification.SupportsOnDownload.Should().BeTrue();
            notification.SupportsOnUpgrade.Should().BeTrue();

            notification.SupportsOnGrab.Should().BeFalse();
            notification.SupportsOnRename.Should().BeFalse();
        }

        [Test]
        public void should_support_all_if_implemented()
        {
            var notification = new TestNotificationWithAllEvents();

            notification.SupportsOnGrab.Should().BeTrue();
            notification.SupportsOnDownload.Should().BeTrue();
            notification.SupportsOnUpgrade.Should().BeTrue();
            notification.SupportsOnRename.Should().BeTrue();
            notification.SupportsOnSeriesDelete.Should().BeTrue();
            notification.SupportsOnEpisodeFileDelete.Should().BeTrue();
            notification.SupportsOnEpisodeFileDeleteForUpgrade.Should().BeTrue();
            notification.SupportsOnHealthIssue.Should().BeTrue();
            notification.SupportsOnApplicationUpdate.Should().BeTrue();
        }

        [Test]
        public void should_support_none_if_none_are_implemented()
        {
            var notification = new TestNotificationWithNoEvents();

            notification.SupportsOnGrab.Should().BeFalse();
            notification.SupportsOnDownload.Should().BeFalse();
            notification.SupportsOnUpgrade.Should().BeFalse();
            notification.SupportsOnRename.Should().BeFalse();
            notification.SupportsOnSeriesDelete.Should().BeFalse();
            notification.SupportsOnEpisodeFileDelete.Should().BeFalse();
            notification.SupportsOnEpisodeFileDeleteForUpgrade.Should().BeFalse();
            notification.SupportsOnHealthIssue.Should().BeFalse();
            notification.SupportsOnApplicationUpdate.Should().BeFalse();
        }
    }
}
