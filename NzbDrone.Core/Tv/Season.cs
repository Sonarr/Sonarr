using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class Season : IEmbeddedDocument
    {
        public int SeasonNumber { get; set; }
        public Boolean Monitored { get; set; }
    }
}