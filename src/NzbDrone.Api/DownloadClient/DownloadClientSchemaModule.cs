using System.Collections.Generic;
using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.Download;
using Omu.ValueInjecter;

namespace NzbDrone.Api.DownloadClient
{
    public class DownloadClientSchemaModule : NzbDroneRestModule<DownloadClientResource>
    {
        private readonly IDownloadClientFactory _notificationFactory;

        public DownloadClientSchemaModule(IDownloadClientFactory notificationFactory)
            : base("downloadclient/schema")
        {
            _notificationFactory = notificationFactory;
            GetResourceAll = GetSchema;
        }

        private List<DownloadClientResource> GetSchema()
        {
            var notifications = _notificationFactory.Templates();

            var result = new List<DownloadClientResource>(notifications.Count);

            foreach (var notification in notifications)
            {
                var notificationResource = new DownloadClientResource();
                notificationResource.InjectFrom(notification);
                notificationResource.Fields = SchemaBuilder.ToSchema(notification.Settings);

                result.Add(notificationResource);
            }

            return result;
        }
    }
}