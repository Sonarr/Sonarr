using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Tags;
using Sonarr.Api.V3.Notifications;
using Sonarr.Api.V3.Profiles.Delay;
using Sonarr.Api.V3.Restrictions;
using Sonarr.Http.REST;

namespace Sonarr.Api.V3.Tags
{
    public class TagDetailsResource : RestResource
    {
        public string Label { get; set; }
        public List<DelayProfileResource> DelayProfiles { get; set; }
        public List<NotificationResource> Notifications { get; set; }
        public List<RestrictionResource> Restrictions { get; set; }
        public List<int> SeriesIds { get; set; }
    }

    public static class TagDetailsResourceMapper
    {
        private static readonly NotificationResourceMapper NotificationResourceMapper = new NotificationResourceMapper();

        public static TagDetailsResource ToResource(this TagDetails model)
        {
            if (model == null) return null;

            return new TagDetailsResource
            {
                Id = model.Id,
                Label = model.Label,
                DelayProfiles = model.DelayProfiles.ToResource(),
                Notifications = model.Notifications.Select(NotificationResourceMapper.ToResource).ToList(),
                Restrictions = model.Restrictions.ToResource(),
                SeriesIds = model.Series.Select(s => s.Id).ToList()
            };
        }

        public static List<TagDetailsResource> ToResource(this IEnumerable<TagDetails> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
