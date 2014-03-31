using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetQueueItem
    {
        private string _nzbName;
        public Int32 NzbId { get; set; }
        public Int32 FirstId { get; set; }
        public Int32 LastId { get; set; }
        public string NzbName { get; set; }
        public String Category { get; set; }
        public Int32 FileSizeMb { get; set; }
        public Int32 RemainingSizeMb { get; set; }
        public Int32 PausedSizeMb { get; set; }
        public List<NzbgetParameter> Parameters { get; set; }
    }
}
