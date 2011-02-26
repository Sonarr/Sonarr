using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model
{
    public class EpisodeSortingType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Pattern { get; set; }
    }
}
