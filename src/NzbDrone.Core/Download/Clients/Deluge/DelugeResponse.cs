using System;
using Newtonsoft.Json.Linq;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeResponse<TResult>
    {
        public Int32 Id { get; set; }
        public TResult Result { get; set; }
        public DelugeError Error { get; set; }
    }
}
