using System;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetCategory
    {
        public String Name { get; set; }
        public String DestDir { get; set; }
        public Boolean Unpack { get; set; }
        public String DefScript { get; set; }
        public String Aliases { get; set; }
    }
}
