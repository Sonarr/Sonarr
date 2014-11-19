using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tags
{
    public class Tag : ModelBase
    {
        public String Label { get; set; }
    }
}
