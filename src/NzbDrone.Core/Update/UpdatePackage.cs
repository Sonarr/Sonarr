using System;

namespace NzbDrone.Core.Update
{
    public class UpdatePackage
    {
        public Version Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public UpdateChanges Changes { get; set; }
        public string Hash { get; set; }
        public string Branch { get; set; }
    }
}
