using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using NzbDrone.Core.Notifications.NotificationTemplates;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.NotificationTemplates
{
    public class NotificationTemplateResource : RestResource
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public override int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public bool OnGrab { get; set; }
        public bool OnDownload { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnImportComplete { get; set; }
        public bool OnRename { get; set; }
        public bool OnSeriesAdd { get; set; }
        public bool OnSeriesDelete { get; set; }
        public bool OnEpisodeFileDelete { get; set; }
        public bool OnEpisodeFileDeleteForUpgrade { get; set; }
        public bool OnHealthIssue { get; set; }
        public bool IncludeHealthWarnings { get; set; }
        public bool OnHealthRestored { get; set; }
        public bool OnApplicationUpdate { get; set; }
        public bool OnManualInteractionRequired { get; set; }
    }

    public static class NotificationTemplateResourceMapper
    {
        public static NotificationTemplateResource ToResource(this NotificationTemplate model)
        {
            var resource = new NotificationTemplateResource
            {
                Id = model.Id,
                Name = model.Name,
                Title = model.Title,
                Body = model.Body,
                OnGrab = model.OnGrab,
                OnDownload = model.OnDownload,
                OnUpgrade = model.OnUpgrade,
                OnImportComplete = model.OnImportComplete,
                OnRename = model.OnRename,
                OnSeriesAdd = model.OnSeriesAdd,
                OnSeriesDelete = model.OnSeriesDelete,
                OnEpisodeFileDelete = model.OnEpisodeFileDelete,
                OnEpisodeFileDeleteForUpgrade = model.OnEpisodeFileDeleteForUpgrade,
                OnHealthIssue = model.OnHealthIssue,
                OnHealthRestored = model.OnHealthRestored,
                OnApplicationUpdate = model.OnApplicationUpdate,
                OnManualInteractionRequired = model.OnManualInteractionRequired
            };

            return resource;
        }

        public static List<NotificationTemplateResource> ToResource(this IEnumerable<NotificationTemplate> models)
        {
            return models.Select(m => m.ToResource()).ToList();
        }

        public static NotificationTemplate ToModel(this NotificationTemplateResource resource)
        {
            return new NotificationTemplate
            {
                Id = resource.Id,
                Name = resource.Name,
                Title = resource.Title,
                Body = resource.Body,
                OnGrab = resource.OnGrab,
                OnDownload = resource.OnDownload,
                OnUpgrade = resource.OnUpgrade,
                OnImportComplete = resource.OnImportComplete,
                OnRename = resource.OnRename,
                OnSeriesAdd = resource.OnSeriesAdd,
                OnSeriesDelete = resource.OnSeriesDelete,
                OnEpisodeFileDelete = resource.OnEpisodeFileDelete,
                OnEpisodeFileDeleteForUpgrade = resource.OnEpisodeFileDeleteForUpgrade,
                OnHealthIssue = resource.OnHealthIssue,
                OnHealthRestored = resource.OnHealthRestored,
                OnApplicationUpdate = resource.OnApplicationUpdate,
                OnManualInteractionRequired = resource.OnManualInteractionRequired
            };
        }
    }
}
