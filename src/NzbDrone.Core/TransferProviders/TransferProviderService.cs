using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.TransferProviders.Providers;

namespace NzbDrone.Core.TransferProviders
{
    public interface ITransferProviderService
    {
        ITransferProvider GetProvider(int downloadClientId);
    }

    public class TransferProviderService : ITransferProviderService
    {
        private readonly ITransferProviderFactory _transferProviderFactory;
        private readonly Logger _logger;

        public TransferProviderService(ITransferProviderFactory transferProviderFactory, Logger logger)
        {
            _transferProviderFactory = transferProviderFactory;
            _logger = logger;
        }

        public ITransferProvider GetProvider(int downloadClientId)
        {
            var definition = _transferProviderFactory.All().FirstOrDefault(v => v.DownloadClientId == downloadClientId);

            if (definition == null)
            {
                definition = _transferProviderFactory.GetDefaultDefinitions().First(v => v.ImplementationName == nameof(DefaultTransfer));
            }

            return _transferProviderFactory.GetInstance(definition);
        }
    }
}
