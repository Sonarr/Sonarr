using System;

namespace NzbDrone.Core.Update
{
    public class UpdatePackage
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public Version Version { get; set; }
    }
}
