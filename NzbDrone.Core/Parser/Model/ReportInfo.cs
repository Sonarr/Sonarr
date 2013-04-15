using System;

namespace NzbDrone.Core.Parser.Model
{
    public class ReportInfo
    {
        public string Title { get; set; }
        public long Size { get; set; }
        public string NzbUrl { get; set; }
        public string NzbInfoUrl { get; set; }
        public String Indexer { get; set; }
        public int Age { get; set; }
        public string ReleaseGroup { get; set; }
    }
}