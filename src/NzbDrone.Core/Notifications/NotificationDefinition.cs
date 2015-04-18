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

        public Boolean OnGrab { get; set; }
        public Boolean OnDownload { get; set; }
        public Boolean OnUpgrade { get; set; }
        public HashSet<Int32> Tags { get; set; }

        public override Boolean Enable
        {
            get
            {
                return OnGrab || (OnDownload && OnUpgrade);
            }
        }
    }
}
