using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeException : DownloadClientException
    {
        public Int32 Code { get; set; }

        public DelugeException(String message, Int32 code)
            :base (message)
        {
            Code = code;
        }
    }
}
