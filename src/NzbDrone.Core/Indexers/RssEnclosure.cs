using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Indexers
{
    public class RssEnclosure
    {
        public string Url { get; set; }
        public string Type { get; set; }
        public long Length { get; set; }
    }
}
