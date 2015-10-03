using System;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeResponse<TResult>
    {
        public int Id { get; set; }
        public TResult Result { get; set; }
        public DelugeError Error { get; set; }
    }
}
