using System;
using System.Collections.Generic;
using System.Net;

using FluentValidation.Results;

using Newtonsoft.Json;

using NLog;

using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.ImportLists.Custom
{
    public interface ICustomImportProxy
    {
        List<CustomSeries> GetSeries(CustomSettings settings);
        ValidationFailure Test(CustomSettings settings);
    }

    public class CustomImportProxy : ICustomImportProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public CustomImportProxy(IHttpClient httpClient, ILocalizationService localizationService, Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public List<CustomSeries> GetSeries(CustomSettings settings)
        {
            return Execute<CustomSeries>(settings);
        }

        public ValidationFailure Test(CustomSettings settings)
        {
            try
            {
                GetSeries(settings);
            }
            catch (HttpException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    _logger.Error(ex, "There was an authorization issue. We cannot get the list from the provider.");
                    return new ValidationFailure("BaseUrl", _localizationService.GetLocalizedString("ImportListsCustomListValidationAuthenticationFailure"));
                }

                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure("BaseUrl",  _localizationService.GetLocalizedString("ImportListsCustomListValidationConnectionError", new Dictionary<string, object> { { "exceptionStatusCode", ex.Response.StatusCode } }));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to import list.");
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("ImportListsValidationUnableToConnectException", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private List<TResource> Execute<TResource>(CustomSettings settings)
        {
            if (settings.BaseUrl.IsNullOrWhiteSpace())
            {
                return new List<TResource>();
            }

            var baseUrl = settings.BaseUrl.TrimEnd('/');
            var request = new HttpRequestBuilder(baseUrl).Accept(HttpAccept.Json).Build();
            var response = _httpClient.Get(request);
            var results = JsonConvert.DeserializeObject<List<TResource>>(response.Content);

            return results;
        }
    }
}
