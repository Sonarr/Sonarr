using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.Oracle.Resource
{
    public class SeasonResource
    {
        public SeasonResource()
        {
            Images = new List<ImageResource>();
        }
        
        public int SeasonNumber { get; set; }
        public List<ImageResource> Images { get; set; }
    }
}