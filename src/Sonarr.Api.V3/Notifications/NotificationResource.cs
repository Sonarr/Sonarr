using NzbDrone.Core.Notifications;

namespace Sonarr.Api.V3.Notifications
{
    public class NotificationResource : ProviderResource
    {
        public string Link { get; set; }
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnRename { get; set; }
        public bool OnSeriesDelete { get; set; }
        public bool OnEpisodeFileDelete { get; set; }
        public bool OnEpisodeFileDeleteForUpgrade { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool OnApplicationUpdate { get; set; }
        public bool SupportsOnGrab { get; set; }
        public bool SupportsOnDownload { get; set; }
        public bool SupportsOnUpgrade { get; set; }
        public bool SupportsOnRename { get; set; }
        public bool SupportsOnSeriesDelete { get; set; }
        public bool SupportsOnEpisodeFileDelete { get; set; }
        public bool SupportsOnEpisodeFileDeleteForUpgrade { get; set; }
        public bool SupportsOnHealthIssue { get; set; }
        public bool SupportsOnApplicationUpdate { get; set; }
        public bool IncludeHealthWarnings { get; set; }
        public string TestCommand { get; set; }
    }

    public class NotificationResourceMapper : ProviderResourceMapper<NotificationResource, NotificationDefinition>
    {
        public override NotificationResource ToResource(NotificationDefinition definition)
        {
            if (definition == null)
            {
                return default(NotificationResource);
            }

            var resource = base.ToResource(definition);

            resource.OnGrab = definition.OnGrab;
            resource.OnDownload = definition.OnDownload;
            resource.OnUpgrade = definition.OnUpgrade;
            resource.OnRename = definition.OnRename;
            resource.OnSeriesDelete = definition.OnSeriesDelete;
            resource.OnEpisodeFileDelete = definition.OnEpisodeFileDelete;
            resource.OnEpisodeFileDeleteForUpgrade = definition.OnEpisodeFileDeleteForUpgrade;
            resource.OnHealthIssue = definition.OnHealthIssue;
            resource.OnApplicationUpdate = definition.OnApplicationUpdate;
            resource.SupportsOnGrab = definition.SupportsOnGrab;
            resource.SupportsOnDownload = definition.SupportsOnDownload;
            resource.SupportsOnUpgrade = definition.SupportsOnUpgrade;
            resource.SupportsOnRename = definition.SupportsOnRename;
            resource.SupportsOnSeriesDelete = definition.SupportsOnSeriesDelete;
            resource.SupportsOnEpisodeFileDelete = definition.SupportsOnEpisodeFileDelete;
            resource.SupportsOnEpisodeFileDeleteForUpgrade = definition.SupportsOnEpisodeFileDeleteForUpgrade;
            resource.SupportsOnHealthIssue = definition.SupportsOnHealthIssue;
            resource.IncludeHealthWarnings = definition.IncludeHealthWarnings;
            resource.SupportsOnApplicationUpdate = definition.SupportsOnApplicationUpdate;

            return resource;
        }

        public override NotificationDefinition ToModel(NotificationResource resource)
        {
            if (resource == null)
            {
                return default(NotificationDefinition);
            }

            var definition = base.ToModel(resource);

            definition.OnGrab = resource.OnGrab;
            definition.OnDownload = resource.OnDownload;
            definition.OnUpgrade = resource.OnUpgrade;
            definition.OnRename = resource.OnRename;
            definition.OnSeriesDelete = resource.OnSeriesDelete;
            definition.OnEpisodeFileDelete = resource.OnEpisodeFileDelete;
            definition.OnEpisodeFileDeleteForUpgrade = resource.OnEpisodeFileDeleteForUpgrade;
            definition.OnHealthIssue = resource.OnHealthIssue;
            definition.OnApplicationUpdate = resource.OnApplicationUpdate;
            definition.SupportsOnGrab = resource.SupportsOnGrab;
            definition.SupportsOnDownload = resource.SupportsOnDownload;
            definition.SupportsOnUpgrade = resource.SupportsOnUpgrade;
            definition.SupportsOnRename = resource.SupportsOnRename;
            definition.SupportsOnSeriesDelete = resource.SupportsOnSeriesDelete;
            definition.SupportsOnEpisodeFileDelete = resource.SupportsOnEpisodeFileDelete;
            definition.SupportsOnEpisodeFileDeleteForUpgrade = resource.SupportsOnEpisodeFileDeleteForUpgrade;
            definition.SupportsOnHealthIssue = resource.SupportsOnHealthIssue;
            definition.IncludeHealthWarnings = resource.IncludeHealthWarnings;
            definition.SupportsOnApplicationUpdate = resource.SupportsOnApplicationUpdate;

            return definition;
        }
    }
}
