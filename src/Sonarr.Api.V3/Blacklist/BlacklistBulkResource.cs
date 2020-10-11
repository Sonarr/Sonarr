using System.Collections.Generic;

namespace NzbDrone.Api.V3.Blacklist
{
    public class BlacklistBulkResource
    {
        public List<int> Ids { get; set; }
    }
}
