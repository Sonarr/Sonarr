using System;
using System.Linq;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders
{
    public class TransferProviderDefinition : ProviderDefinition
    {
        public int DownloadClientId { get; set; }
        // OR
        // Path could be extracted from download client.
        //public string DownloadClientRootPath { get; set; }
    }
}
