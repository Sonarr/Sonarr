﻿using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;
using System;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBullet : NotificationBase<PushBulletSettings>
    {
        private readonly IPushBulletProxy _proxy;

        public PushBullet(IPushBulletProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link
        {
            get { return "https://www.pushbullet.com/"; }
        }

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string title = "Sonarr - Episode Grabbed";

            _proxy.SendNotification(title, grabMessage.Message, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string title = "Sonarr - Episode Downloaded";

            _proxy.SendNotification(title, message.Message, Settings);
        }

        public override void OnRename(Series series)
        {
        }

        public override void OnUpdateAvailable(UpdatePackage package)
        {
            const string title = "Sonarr - New System Update";
            var body = String.Format("New update is available - {0}", package.Version.ToString());

            _proxy.SendNotification(title, body, Settings);
        }

        public override string Name
        {
            get
            {
                return "Pushbullet";
            }
        }

        public override bool SupportsOnRename
        {
            get
            {
                return false;
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
