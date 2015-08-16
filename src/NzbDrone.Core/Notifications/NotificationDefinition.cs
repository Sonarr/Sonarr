using System;
using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public class NotificationDefinition : ProviderDefinition
    {
        public NotificationDefinition()
        {
            Tags = new HashSet<Int32>();
        }

        public bool OnGrab { get; set; }
        public bool OnGrabMovie { get; set; }
        public bool OnDownload { get; set; }
        public bool OnDownloadMovie { get; set; }
        public bool OnUpgrade { get; set; }
        public bool OnRename { get; set; }
        public bool OnRenameMovie { get; set; }
        public bool SupportsOnGrab { get; set; }
        public bool SupportsOnGrabMovie { get; set; }
        public bool SupportsOnDownload { get; set; }
        public bool SupportsOnDownloadMovie { get; set; }
        public bool SupportsOnUpgrade { get; set; }
        public bool SupportsOnRename { get; set; }
        public bool SupportsOnRenameMovie { get; set; }
        public HashSet<int> Tags { get; set; }

        public override bool Enable
        {
            get
            {
                return OnGrab || OnGrabMovie || OnDownload || OnDownloadMovie || (OnDownload && OnUpgrade);
            }
        }
    }
}
