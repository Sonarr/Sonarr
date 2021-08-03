using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Trakt.Resource
{
    public class TraktSeasonResource
    {
        public int Number { get; set; }
        public List<TraktEpisodeResource> Episodes { get; set; }
    }
}
