using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.Notifications.Simplepush
{
    public interface ISimplepushProxy
    {
        void SendNotification(string title, string message, SimplepushSettings settings);
        ValidationFailure Test(SimplepushSettings settings);
    }

    public class SimplepushProxy : ISimplepushProxy
    {
        private const string URL = "https://api.simplepush.io/send";
        private readonly IHttpClient _httpClient;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public SimplepushProxy(IHttpClient httpClient, ILocalizationService localizationService, Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void SendNotification(string title, string message, SimplepushSettings settings)
        {
            var requestBuilder = new HttpRequestBuilder(URL).Post();

            requestBuilder.AddFormParameter("key", settings.Key)
                          .AddFormParameter("event", settings.Event)
                          .AddFormParameter("title", title)
                          .AddFormParameter("msg", message);

            var request = requestBuilder.Build();

            _httpClient.Post(request);
        }

        public ValidationFailure Test(SimplepushSettings settings)
        {
            try
            {
                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr";

                SendNotification(title, body, settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("ApiKey", _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
