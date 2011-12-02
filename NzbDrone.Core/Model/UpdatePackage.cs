using System;
using System.Linq;

namespace NzbDrone.Core.Model
{
    public class UpdatePackage
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public Version Version { get; set; }
    }
}
