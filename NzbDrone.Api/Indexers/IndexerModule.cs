using System.Collections.Generic;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Indexers
{
    public class IndexerModule : NzbDroneRestModule<IndexerResource>
    {
        private readonly IIndexerService _indexerService;

        public IndexerModule(IIndexerService indexerService)
        {
            _indexerService = indexerService;
            GetResourceAll = GetAll;
        }

        private List<IndexerResource> GetAll()
        {
            return ApplyToList(_indexerService.All);
        }
    }
}