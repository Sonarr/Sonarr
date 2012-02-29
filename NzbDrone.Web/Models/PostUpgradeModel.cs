using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NzbDrone.Web.Models
{
    public class PostUpgradeModel
    {
        public Version CurrentVersion { get; set; }
        public Version ExpectedVersion { get; set; }
        public bool Success { get; set; }
        public KeyValuePair<DateTime, string> LogFile { get; set; }
    }
}