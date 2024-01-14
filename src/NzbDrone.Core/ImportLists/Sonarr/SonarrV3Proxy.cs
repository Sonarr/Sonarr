using System;
using System.Collections.Generic;
using System.Net;
using FluentValidation.Results;
using Newtonsoft.Json;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.ImportLists.Sonarr
{
    public interface ISonarrV3Proxy
    {
        List<SonarrSeries> GetSeries(SonarrSettings settings);
        List<SonarrProfile> GetQualityProfiles(SonarrSettings settings);
        List<SonarrProfile> GetLanguageProfiles(SonarrSettings settings);
        List<SonarrRootFolder> GetRootFolders(SonarrSettings settings);
        List<SonarrTag> GetTags(SonarrSettings settings);
        ValidationFailure Test(SonarrSettings settings);
    }

    public class SonarrV3Proxy : ISonarrV3Proxy
    {
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;
        private readonly ILocalizationService _localizationService;

        public SonarrV3Proxy(IHttpClient httpClient, ILocalizationService localizationService, Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public List<SonarrSeries> GetSeries(SonarrSettings settings)
        {
            return Execute<SonarrSeries>("/api/v3/series", settings);
        }

        public List<SonarrProfile> GetQualityProfiles(SonarrSettings settings)
        {
            return Execute<SonarrProfile>("/api/v3/qualityprofile", settings);
        }

        public List<SonarrProfile> GetLanguageProfiles(SonarrSettings settings)
        {
            return Execute<SonarrProfile>("/api/v3/languageprofile", settings);
        }

        public List<SonarrRootFolder> GetRootFolders(SonarrSettings settings)
        {
            return Execute<SonarrRootFolder>("api/v3/rootfolder", settings);
        }

        public List<SonarrTag> GetTags(SonarrSettings settings)
        {
            return Execute<SonarrTag>("/api/v3/tag", settings);
        }

        public ValidationFailure Test(SonarrSettings settings)
        {
            try
            {
                GetSeries(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "API Key is invalid");
                    return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("ImportListsValidationInvalidApiKey"));
                }

                if (ex.Response.HasHttpRedirect)
                {
                    _logger.Error(ex, "Sonarr returned redirect and is invalid");
                    return new ValidationFailure("BaseUrl", _localizationService.GetLocalizedString("ImportListsSonarrValidationInvalidUrl"));
                }

                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("ImportListsValidationUnableToConnectException", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("ImportListsValidationUnableToConnectException", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private List<TResource> Execute<TResource>(string resource, SonarrSettings settings)
        {
            if (settings.BaseUrl.IsNullOrWhiteSpace() || settings.ApiKey.IsNullOrWhiteSpace())
            {
                return new List<TResource>();
            }

            var baseUrl = settings.BaseUrl.TrimEnd('/');

            var request = new HttpRequestBuilder(baseUrl).Resource(resource)
                .Accept(HttpAccept.Json)
                .SetHeader("X-Api-Key", settings.ApiKey)
                .Build();

            var response = _httpClient.Get(request);

            if ((int)response.StatusCode >= 300)
            {
                throw new HttpException(response);
            }

            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);

            return results;
        }
    }
}
