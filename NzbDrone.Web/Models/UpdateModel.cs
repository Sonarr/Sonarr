using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NzbDrone.Core.Model;

namespace NzbDrone.Web.Models
{
    public class UpdateModel
    {
        public UpdatePackage UpdatePackage { get; set; }
        public Dictionary<DateTime, string> LogFiles { get; set; }
        public String LogFolder { get; set; }
    }
}