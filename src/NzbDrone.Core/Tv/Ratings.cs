using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class Ratings : IEmbeddedDocument
    {
        public Decimal Percentage { get; set; }
        public Int32 Votes { get; set; }
        public Int32 Loved { get; set; }
        public Int32 Hated { get; set; }
    }
}
