using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Repository
{
    public class FeedItem
    {
        //Item from the NZB Feed waiting tp be processed.
        public string Description { get; set; }
        public string Title { get; set; }
        public string TitleFix { get; set; }
        public string NzbId { get; set; }
    }
}
