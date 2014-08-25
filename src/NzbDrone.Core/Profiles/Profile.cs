using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Profiles
{
    public class Profile : ModelBase
    {
        public String Name { get; set; }
        public Quality Cutoff { get; set; }
        public List<ProfileQualityItem> Items { get; set; }
        public Language Language { get; set; }
        public Int32 GrabDelay { get; set; }
        public GrabDelayMode GrabDelayMode { get; set; }
    }
}