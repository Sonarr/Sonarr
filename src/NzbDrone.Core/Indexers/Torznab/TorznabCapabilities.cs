using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class TorznabCapabilities
    {
        public string[] SupportedSearchParameters { get; set; }
        public string[] SupportedTvSearchParameters { get; set; }
        public List<TorznabCategory> Categories { get; set; }

        public TorznabCapabilities()
        {
            SupportedSearchParameters = new[] { "q", "offset", "limit" };
            SupportedTvSearchParameters = new[] { "q", "rid", "season", "ep", "offset", "limit" };
            Categories = new List<TorznabCategory>();
        }
    }

    public class TorznabCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<TorznabCategory> Subcategories { get; set; }
    }
}
