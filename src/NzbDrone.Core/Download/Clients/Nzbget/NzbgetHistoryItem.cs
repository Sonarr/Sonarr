using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetHistoryItem
    {
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public String Category { get; set; }
        public UInt32 FileSizeLo { get; set; }
        public UInt32 FileSizeHi { get; set; }
        public String ParStatus { get; set; }
        public String ScriptStatus { get; set; }
        public String DestDir { get; set; }
        public List<NzbgetParameter> Parameters { get; set; }
    }
}
