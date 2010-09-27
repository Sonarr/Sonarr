using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Repository
{
    public class ItemInfo
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public Site Site { get; set; }
        public Uri Link { get; set; }
        public string Description { get; set; }

        public bool IsPassworded()
        {
            return Title.EndsWith("(Passworded)", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
