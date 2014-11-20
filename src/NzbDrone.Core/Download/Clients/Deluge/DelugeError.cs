using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeError
    {
        public String Message { get; set; }
        public Int32 Code { get; set; }
    }
}
