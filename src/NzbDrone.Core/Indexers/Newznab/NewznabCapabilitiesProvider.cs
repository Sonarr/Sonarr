using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Indexers.Newznab
{
    public interface INewznabCapabilitiesProvider
    {
        NewznabCapabilities GetCapabilities(NewznabSettings settings);
    }

    public class NewznabCapabilitiesProvider : INewznabCapabilitiesProvider
    {
        private readonly ICached<NewznabCapabilities> _capabilitiesCache;
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public NewznabCapabilitiesProvider(ICacheManager cacheManager, IHttpClient httpClient, Logger logger)
        {
            _capabilitiesCache = cacheManager.GetCache<NewznabCapabilities>(GetType());
            _httpClient = httpClient;
            _logger = logger;
        }

        public NewznabCapabilities GetCapabilities(NewznabSettings indexerSettings)
        {
            var key = indexerSettings.ToJson();
            var capabilities = _capabilitiesCache.Get(key, () => FetchCapabilities(indexerSettings), TimeSpan.FromDays(7));

            return capabilities;
        }

        private NewznabCapabilities FetchCapabilities(NewznabSettings indexerSettings)
        {
            var capabilities = new NewznabCapabilities();

            var url = string.Format("{0}/api?t=caps", indexerSettings.Url.TrimEnd('/'));

            if (indexerSettings.ApiKey.IsNotNullOrWhiteSpace())
            {
                url += "&apikey=" + indexerSettings.ApiKey;
            }

            var request = new HttpRequest(url, HttpAccept.Rss);

            try
            {
                var response = _httpClient.Get(request);

                capabilities = ParseCapabilities(response);
            }
            catch (Exception ex)
            {
                _logger.DebugException(string.Format("Failed to get capabilities from {0}: {1}", indexerSettings.Url, ex.Message), ex);
            }

            return capabilities;
        }

        private NewznabCapabilities ParseCapabilities(HttpResponse response)
        {
            var capabilities = new NewznabCapabilities();

            var xmlRoot = XDocument.Parse(response.Content).Element("caps");

            var xmlSearching = xmlRoot.Element("searching");
            if (xmlSearching != null)
            {
                var xmlBasicSearch = xmlSearching.Element("search");
                if (xmlBasicSearch == null || xmlBasicSearch.Attribute("available").Value != "yes")
                {
                    capabilities.SupportedSearchParameters = null;
                }
                else if (xmlBasicSearch.Attribute("supportedParams") != null)
                {
                    capabilities.SupportedSearchParameters = xmlBasicSearch.Attribute("supportedParams").Value.Split(',');
                }

                var xmlTvSearch = xmlSearching.Element("tv-search");
                if (xmlTvSearch == null || xmlTvSearch.Attribute("available").Value != "yes")
                {
                    capabilities.SupportedTvSearchParameters = null;
                }
                else if (xmlTvSearch.Attribute("supportedParams") != null)
                {
                    capabilities.SupportedTvSearchParameters = xmlTvSearch.Attribute("supportedParams").Value.Split(',');
                    capabilities.SupportsAggregateIdSearch = true;
                }
            }

            var xmlCategories = xmlRoot.Element("categories");
            if (xmlCategories != null)
            {
                foreach (var xmlCategory in xmlCategories.Elements("category"))
                {
                    var cat = new NewznabCategory
                    {
                        Id = int.Parse(xmlCategory.Attribute("id").Value),
                        Name = xmlCategory.Attribute("name").Value,
                        Description = xmlCategory.Attribute("description") != null ? xmlCategory.Attribute("description").Value : string.Empty,
                        Subcategories = new List<NewznabCategory>()
                    };

                    foreach (var xmlSubcat in xmlCategory.Elements("subcat"))
                    {
                        cat.Subcategories.Add(new NewznabCategory
                        {
                            Id = int.Parse(xmlSubcat.Attribute("id").Value),
                            Name = xmlSubcat.Attribute("name").Value,
                            Description = xmlSubcat.Attribute("description") != null ? xmlCategory.Attribute("description").Value : string.Empty

                        });
                    }

                    capabilities.Categories.Add(cat);
                }
            }

            return capabilities;
        }
    }
}
