using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetHistoryItem
    {
        private string _nzbName;
        public Int32 Id { get; set; }
        public String Name { get; set; }
        public String Category { get; set; }
        public Int32 FileSizeMb { get; set; }
        public String ParStatus { get; set; }
        public String ScriptStatus { get; set; }
        public String DestDir { get; set; }
        public List<NzbgetParameter> Parameters { get; set; }
    }
}
