using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
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
        public override int PageSize => GetProviderPageSize();

        public Newznab(INewznabCapabilitiesProvider capabilitiesProvider, IHttpClient httpClient, IIndexerStatusService indexerStatusService, IConfigService configService, IParsingService parsingService, Logger logger, ILocalizationService localizationService)
            : base(httpClient, indexerStatusService, configService, parsingService, logger, localizationService)
        {
            _capabilitiesProvider = capabilitiesProvider;
        }

        public override IIndexerRequestGenerator GetRequestGenerator()
        {
            return new NewznabRequestGenerator(_capabilitiesProvider)
            {
                Definition = Definition,
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
                yield return GetDefinition("DrunkenSlug", GetSettings("https://drunkenslug.com"));
                yield return GetDefinition("Nzb.life", GetSettings("https://api.nzb.life"));
                yield return GetDefinition("NZBCat", GetSettings("https://nzb.cat"));
                yield return GetDefinition("NZBFinder.ws", GetSettings("https://nzbfinder.ws", categories: new[] { 5030, 5040, 5045 }));
                yield return GetDefinition("NZBgeek", GetSettings("https://api.nzbgeek.info"));
                yield return GetDefinition("nzbplanet.net", GetSettings("https://api.nzbplanet.net"));
                yield return GetDefinition("SimplyNZBs", GetSettings("https://simplynzbs.com"));
                yield return GetDefinition("Tabula Rasa", GetSettings("https://www.tabula-rasa.pw", apiPath: @"/api/v1/api"));
            }
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

        protected override async Task Test(List<ValidationFailure> failures)
        {
            await base.Test(failures);

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

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationSearchParametersNotSupported"));
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to indexer: " + ex.Message);

                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("IndexerValidationUnableToConnect", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
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

        private int GetProviderPageSize()
        {
            try
            {
                return Math.Min(100, Math.Max(_capabilitiesProvider.GetCapabilities(Settings).DefaultPageSize, _capabilitiesProvider.GetCapabilities(Settings).MaxPageSize));
            }
            catch
            {
                return 100;
            }
        }
    }
}
