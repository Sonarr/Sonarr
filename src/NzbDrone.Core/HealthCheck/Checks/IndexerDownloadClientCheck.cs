using System.Linq;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Localization;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.HealthCheck.Checks
{
    [CheckOn(typeof(ProviderUpdatedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderDeletedEvent<IIndexer>))]
    [CheckOn(typeof(ProviderUpdatedEvent<IDownloadClient>))]
    [CheckOn(typeof(ProviderDeletedEvent<IDownloadClient>))]
    public class IndexerDownloadClientCheck : HealthCheckBase
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly IDownloadClientFactory _downloadClientFactory;

        public IndexerDownloadClientCheck(IIndexerFactory indexerFactory,
                                          IDownloadClientFactory downloadClientFactory,
                                          ILocalizationService localizationService)
            : base(localizationService)
        {
            _indexerFactory = indexerFactory;
            _downloadClientFactory = downloadClientFactory;
        }

        public override HealthCheck Check()
        {
            var downloadClientsIds = _downloadClientFactory.All().Where(v => v.Enable).Select(v => v.Id).ToList();
            var invalidIndexers = _indexerFactory.All()
                .Where(v => v.Enable && v.DownloadClientId > 0 && !downloadClientsIds.Contains(v.DownloadClientId))
                .ToList();

            if (invalidIndexers.Any())
            {
                return new HealthCheck(GetType(),
                    HealthCheckResult.Warning,
                    string.Format(_localizationService.GetLocalizedString("IndexerDownloadClientHealthCheckMessage"), string.Join(", ", invalidIndexers.Select(v => v.Name).ToArray())),
                    "#invalid-indexer-download-client-setting");
            }

            return new HealthCheck(GetType());
        }
    }
}
