using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.DataAugmentation.Xem.Model;

namespace NzbDrone.Core.DataAugmentation.Xem
{
    public interface IXemProxy
    {
        List<int> GetXemSeriesIds();
        List<XemSceneTvdbMapping> GetSceneTvdbMappings(int id);
        List<SceneMapping> GetSceneTvdbNames();
    }

    public class XemProxy : IXemProxy
    {
        private const string ROOT_URL = "https://thexem.info/map/";

        private readonly Logger _logger;
        private readonly IHttpClient _httpClient;
        private readonly IHttpRequestBuilderFactory _xemRequestBuilder;

        private static readonly string[] IgnoredErrors = { "no single connection", "no show with the tvdb_id" };

        public XemProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            _xemRequestBuilder = new HttpRequestBuilder(ROOT_URL)
                .AddSuffixQueryParam("origin", "tvdb")
                .CreateFactory();
        }

        public List<int> GetXemSeriesIds()
        {
            _logger.Debug("Fetching Series IDs from");

            var request = _xemRequestBuilder.Create()
                                            .Resource("/havemap")
                                            .Build();

            var response = _httpClient.Get<XemResult<List<string>>>(request).Resource;
            CheckForFailureResult(response);

            return response.Data.Select(d =>
            {
                int tvdbId = 0;
                int.TryParse(d, out tvdbId);

                return tvdbId;
            }).Where(t => t > 0).ToList();
        }

        public List<XemSceneTvdbMapping> GetSceneTvdbMappings(int id)
        {
            _logger.Debug("Fetching Mappings for: {0}", id);

            var request = _xemRequestBuilder.Create()
                                            .Resource("/all")
                                            .AddQueryParam("id", id)
                                            .Build();

            var response = _httpClient.Get<XemResult<List<XemSceneTvdbMapping>>>(request).Resource;

            return response.Data.Where(c => c.Scene != null).ToList();
        }

        public List<SceneMapping> GetSceneTvdbNames()
        {
            _logger.Debug("Fetching alternate names");

            var request = _xemRequestBuilder.Create()
                                            .Resource("/allNames")
                                            .AddQueryParam("seasonNumbers", true)
                                            .Build();

            var response = _httpClient.Get<XemResult<Dictionary<int, List<JObject>>>>(request).Resource;

            var result = new List<SceneMapping>();

            foreach (var series in response.Data)
            {
                foreach (var name in series.Value)
                {
                    foreach (var n in name)
                    {
                        int seasonNumber;
                        if (!int.TryParse(n.Value.ToString(), out seasonNumber))
                        {
                            continue;
                        }

                        //hack to deal with Fate/Zero
                        if (series.Key == 79151 && seasonNumber > 1)
                        {
                            continue;
                        }

                        result.Add(new SceneMapping
                                   {
                                       Title = n.Key,
                                       SearchTerm = n.Key,
                                       SceneSeasonNumber = seasonNumber,
                                       TvdbId = series.Key
                                   });
                    }
                }
            }

            return result;
        }

        private static void CheckForFailureResult<T>(XemResult<T> response)
        {
            if (response.Result.Equals("failure", StringComparison.InvariantCultureIgnoreCase) &&
                !IgnoredErrors.Any(knowError => response.Message.Contains(knowError)))
            {
                throw new Exception("Error response received from Xem: " + response.Message);
            }
        }
    }
}
