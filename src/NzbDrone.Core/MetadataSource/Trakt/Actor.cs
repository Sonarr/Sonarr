using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MetadataSource.Trakt
{
    public class Actor
    {
        public string name { get; set; }
        public string character { get; set; }
        public Images images { get; set; }
    }
}
