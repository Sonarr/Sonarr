using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class SeasonTypeResource
    {
        public SeasonTypeResource()
        {
            SeasonNumbers = new List<int>();
        }

        public string Name { get; set; }
        public string Type { get; set; }
        public List<int> SeasonNumbers { get; set; }
        public int EpisodeCount { get; set; }
    }
}
