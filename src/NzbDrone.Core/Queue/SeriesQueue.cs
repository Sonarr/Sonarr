using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Queue
{
    public class SeriesQueue : Queue
    {
        public Episode Episode { get; set; }
    }
}
