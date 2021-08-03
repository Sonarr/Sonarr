using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class Newznab : HttpIndexerBase<NewznabSettings>
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;

        public override string Name => "Newznab";

        public override DownloadProtocol Protocol => DownloadProtocol.Usenet;

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
            return new NewznabRssParser();
        }

        public override IEnumerable<ProviderDefinition> DefaultDefinitions
        {
            get
            {
                yield return GetDefinition("DOGnzb", GetSettings("https://api.dognzb.cr"));
                yield return GetDefinition("DrunkenSlug", GetSettings("https://api.drunkenslug.com"));
                yield return GetDefinition("Nzb.su", GetSettings("https://api.nzb.su"));
                yield return GetDefinition("NZBCat", GetSettings("https://nzb.cat"));
                yield return GetDefinition("NZBFinder.ws", GetSettings("https://nzbfinder.ws", categories: new[] { 5010, 5030, 5040, 5045, 5090 }));
                yield return GetDefinition("NZBgeek", GetSettings("https://api.nzbgeek.info"));
                yield return GetDefinition("nzbplanet.net", GetSettings("https://api.nzbplanet.net"));
                yield return GetDefinition("omgwtfnzbs", GetSettings("https://api.omgwtfnzbs.me"));
                yield return GetDefinition("OZnzb.com", GetSettings("https://api.oznzb.com"));
                yield return GetDefinition("SimplyNZBs", GetSettings("https://simplynzbs.com"));
                yield return GetDefinition("Tabula Rasa", GetSettings("https://www.tabula-rasa.pw", apiPath: @"/api/v1/api"));
                yield return GetDefinition("AnimeTosho Usenet", GetSettings("https://feed.animetosho.org", apiPath: @"/nabapi", categories: new int[0], animeCategories: new[] { 5070 }));
            }
        }

        public Newznab(INewznabCapabilitiesProvider capabilitiesProvider, IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, indexerStatusService, configService, parsingService, logger)
        {
            _capabilitiesProvider = capabilitiesProvider;
        }

        private IndexerDefinition GetDefinition(string name, NewznabSettings settings)
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

        private NewznabSettings GetSettings(string url, string apiPath = null, int[] categories = null, int[] animeCategories = null)
        {
            var settings = new NewznabSettings { BaseUrl = url };

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
            if (failures.HasErrors())
            {
                return;
            }

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

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "newznabCategories")
            {
                List<NewznabCategory> categories = null;
                try
                {
                    if (Settings.BaseUrl.IsNotNullOrWhiteSpace() && Settings.ApiPath.IsNotNullOrWhiteSpace())
                    {
                        categories = _capabilitiesProvider.GetCapabilities(Settings).Categories;
                    }
                }
                catch
                {
                    // Use default categories
                }

                return new
                {
                    options = NewznabCategoryFieldOptionsConverter.GetFieldSelectOptions(categories)
                };
            }

            return base.RequestAction(action, query);
        }
    }
}
