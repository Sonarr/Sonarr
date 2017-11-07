using System;
using System.Linq;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.TransferProviders
{
    public interface ITransferProviderRepository : IProviderRepository<TransferProviderDefinition>
    {

    }
}
