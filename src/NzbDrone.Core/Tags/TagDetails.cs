using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tags
{
    public class TagDetails : ModelBase
    {
        public string Label { get; set; }
        public List<int> SeriesIds { get; set; }
        public List<int> NotificationIds { get; set; }
        public List<int> RestrictionIds { get; set; }
        public List<int> DelayProfileIds { get; set; }
    }
}
