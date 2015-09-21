using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Parser.Model
{
    public abstract class RemoteItem
    {
        public abstract Media Media { get; }
        public abstract IEnumerable<MediaModelBase> MediaFiles { get; }

        public ReleaseInfo Release { get; set; }
        public bool DownloadAllowed { get; set; }
        public ParsedInfo ParsedInfo { get; set; }


        public override string ToString()
        {
            return Release.Title;
        }
    }
}
