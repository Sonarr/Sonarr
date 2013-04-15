using System;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbGetQueueItem
    {
        private string _nzbName;

        public Int32 NzbId { get; set; }

        public string NzbName { get; set; }

        public String Category { get; set; }
        public Int32 FileSizeMb { get; set; }
        public Int32 RemainingSizeMb { get; set; }
    }
}
