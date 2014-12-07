using System;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeError
    {
        public String Message { get; set; }
        public Int32 Code { get; set; }
    }
}
