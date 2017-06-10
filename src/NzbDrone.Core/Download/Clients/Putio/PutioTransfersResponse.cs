using System.Collections.Generic;
namespace NzbDrone.Core.Download.Clients.Putio
{
    public class PutioTransfersResponse : PutioGenericResponse
    {
        public List<PutioTorrent> Transfers { get; set; }
    }
}
