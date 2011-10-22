using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Model
{
    public class UpdatePackage
    {
        public string Url { get; set; }
        public string FileName { get; set; }
        public Version Version { get; set; }
    }
}
