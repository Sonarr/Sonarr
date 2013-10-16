using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.REST;
using NzbDrone.Common.Reflection;
using NzbDrone.Core.Notifications;
using Omu.ValueInjecter;

namespace NzbDrone.Api.Notifications
{
    public class IndexerModule : ProviderModuleBase<NotificationResource, INotification, NotificationDefinition>
    {
        public IndexerModule(NotificationFactory notificationrFactory)
            : base(notificationrFactory, "notification")
        {
        }

        protected override void Validate(NotificationDefinition definition)
        {
            if (!definition.OnGrab && !definition.OnDownload) return;
            base.Validate(definition);
        }
    }
}