using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.Nzbget
{
    public class NzbgetHistoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public uint FileSizeLo { get; set; }
        public uint FileSizeHi { get; set; }
        public string ParStatus { get; set; }
        public string UnpackStatus { get; set; }
        public string MoveStatus { get; set; }
        public string ScriptStatus { get; set; }
        public string DeleteStatus { get; set; }
        public string MarkStatus { get; set; }
        public string DestDir { get; set; }
        public List<NzbgetParameter> Parameters { get; set; }
    }
}
