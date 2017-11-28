using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class Torznab : HttpIndexerBase<TorznabSettings>
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;

        public override string Name => "Torznab";

        public override DownloadProtocol Protocol => DownloadProtocol.Torrent;
        public override int PageSize => _capabilitiesProvider.GetCapabilities(Settings).DefaultPageSize;

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new NewznabRequestGenerator(_capabilitiesProvider)
            {
                PageSize = PageSize,
                Settings = Settings
            };
        }

        public override IParseIndexerResponse GetParser()
        {
            return new TorznabRssParser();
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return GetDefinition("HD4Free.xyz", GetSettings("http://hd4free.xyz"));
                yield return GetDefinition("AnimeTosho Torrents", GetSettings("https://feed.animetosho.org", apiPath: @"/nabapi", categories: new int[0], animeCategories: new[] { 5070 }));
                yield return GetDefinition("Nyaa Pantsu", GetSettings("https://nyaa.pantsu.cat", apiPath: @"/feed/torznab", categories: new int[0], animeCategories: new[] { 5070 }));
            }
        }

        public Torznab(INewznabCapabilitiesProvider capabilitiesProvider, IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _capabilitiesProvider = capabilitiesProvider;
        }

        private IndexerDefinition GetDefinition(string name, TorznabSettings settings)
        {
            return new IndexerDefinition
                   {
                       EnableRss = false,
                       EnableAutomaticSearch = false,
                       EnableInteractiveSearch = false,
                       Name = name,
                       Implementation = GetType().Name,
                       Settings = settings,
                       Protocol = DownloadProtocol.Usenet,
                       SupportsRss = SupportsRss,
                       SupportsSearch = SupportsSearch
                   };
        }

        private TorznabSettings GetSettings(string url, string apiPath = null, int[] categories = null, int[] animeCategories = null)
        {
            var settings = new TorznabSettings { BaseUrl = url };

            if (categories != null)
            {
                settings.Categories = categories;
            }

            if (animeCategories != null)
            {
                settings.AnimeCategories = animeCategories;
            }

            if (apiPath.IsNotNullOrWhiteSpace())
            {
                settings.ApiPath = apiPath;
            }

            return settings;
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            base.Test(failures);
            if (failures.Any()) return;
            failures.AddIfNotNull(TestCapabilities());
        }

        protected virtual ValidationFailure TestCapabilities()
        {
            try
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                if (capabilities.SupportedSearchParameters != null && capabilities.SupportedSearchParameters.Contains("q"))
                {
                    return null;
                }

                if (capabilities.SupportedTvSearchParameters != null &&
                    new[] { "q", "tvdbid", "rid" }.Any(v => capabilities.SupportedTvSearchParameters.Contains(v)) &&
                    new[] { "season", "ep" }.All(v => capabilities.SupportedTvSearchParameters.Contains(v)))
                {
                    return null;
                }

                return new ValidationFailure(string.Empty, "Indexer does not support required search parameters");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer: " + ex.Message);

                return new ValidationFailure(string.Empty, "Unable to connect to indexer, check the log for more details");
            }
        }
    }
}
