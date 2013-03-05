using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabJsonError
    {
        public string Status { get; set; }
        public string Error { get; set; }
    }
}
