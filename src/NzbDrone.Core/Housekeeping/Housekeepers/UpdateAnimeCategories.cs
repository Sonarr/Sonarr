using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Newznab;

namespace NzbDrone.Core.Housekeeping.Housekeepers
{
    public class UpdateAnimeCategories : IHousekeepingTask
    {
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        private const int NZBS_ORG_ANIME_ID = 7040;
        private const int NEWZNAB_ANIME_ID = 5070;

        public UpdateAnimeCategories(IIndexerFactory indexerFactory, Logger logger)
        {
            _indexerFactory = indexerFactory;
            _logger = logger;
        }

        public void Clean()
        {
            //TODO: We should remove this before merging it into develop
            _logger.Debug("Updating Anime Categories for newznab indexers");

            var indexers = _indexerFactory.All().Where(i => i.Implementation == typeof (Newznab).Name);

            foreach (var indexer in indexers)
            {
                var settings = indexer.Settings as NewznabSettings;

                if (settings.Url.ContainsIgnoreCase("nzbs.org") && settings.Categories.Contains(NZBS_ORG_ANIME_ID))
                {
                    var animeCategories = new List<int>(settings.AnimeCategories);
                    animeCategories.Add(NZBS_ORG_ANIME_ID);

                    settings.AnimeCategories = animeCategories;

                    settings.Categories = settings.Categories.Where(c => c != NZBS_ORG_ANIME_ID);

                    indexer.Settings = settings;
                    _indexerFactory.Update(indexer);
                }

                else if (settings.Categories.Contains(NEWZNAB_ANIME_ID))
                {
                    var animeCategories = new List<int>(settings.AnimeCategories);
                    animeCategories.Add(NEWZNAB_ANIME_ID);

                    settings.AnimeCategories = animeCategories;

                    settings.Categories = settings.Categories.Where(c => c != NEWZNAB_ANIME_ID);

                    indexer.Settings = settings;
                    _indexerFactory.Update(indexer);
                }
            }
        }
    }
}
