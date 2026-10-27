using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.Notifications.Telegram
{
    public interface ITelegramProxy
    {
        void SendNotification(TelegramMessage message, List<NotificationMetadataLink> links, TelegramSettings settings);
        ValidationFailure Test(TelegramSettings settings);
    }

    public class TelegramProxy : ITelegramProxy
    {
        private const string URL = "https://api.telegram.org";

        private readonly IHttpClient _httpClient;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public TelegramProxy(IHttpClient httpClient, IConfigFileProvider configFileProvider, ILocalizationService localizationService,  Logger logger)
        {
            _httpClient = httpClient;
            _configFileProvider = configFileProvider;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void SendNotification(TelegramMessage message, List<NotificationMetadataLink> links, TelegramSettings settings)
        {
            // sendPhoto has no link preview to configure - the image is the attachment.
            var resource = message.HasPhoto ? "bot{token}/sendPhoto" : "bot{token}/sendmessage";

            var request = new HttpRequestBuilder(URL).Resource(resource)
                                                     .Post()
                                                     .SetSegment("token", settings.BotToken)
                                                     .Accept(HttpAccept.Json)
                                                     .Build();

            request.Headers.ContentType = "application/json";

            object payload = message.HasPhoto
                ? new TelegramPhotoPayload
                {
                    ChatId = settings.ChatId,
                    Photo = message.Photo,
                    Caption = message.Text,
                    DisableNotification = settings.SendSilently,
                    MessageThreadId = settings.TopicId
                }
                : new TelegramPayload
                {
                    ChatId = settings.ChatId,
                    Text = message.Text,
                    DisableNotification = settings.SendSilently,
                    MessageThreadId = settings.TopicId,
                    LinkPreviewOptions = new TelegramLinkPreviewOptions(links, settings)
                };

            request.SetContent(payload.ToJson());

            _httpClient.Post(request);
        }

        public ValidationFailure Test(TelegramSettings settings)
        {
            try
            {
                const string brandedTitle = "Sonarr - Test Notification";
                const string title = "Test Notification";
                const string body = "This is a test message from Sonarr";

                var links = new List<NotificationMetadataLink>
                    {
                        new NotificationMetadataLink(null, "Sonarr.tv", "https://sonarr.tv")
                    };

                var testMessageTitle = settings.IncludeAppNameInTitle ? brandedTitle : title;
                testMessageTitle = settings.IncludeInstanceNameInTitle ? $"{testMessageTitle} - {_configFileProvider.InstanceName}" : testMessageTitle;

                SendNotification(TelegramMessageBuilder.BuildSimpleMessage(testMessageTitle, body, links), links, settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");

                if (ex is WebException webException)
                {
                    return new ValidationFailure("Connection",  _localizationService.GetLocalizedString("NotificationsValidationUnableToConnectToApi", new Dictionary<string, object> { { "service", "Telegram" }, { "responseCode", webException.Status.ToString() }, { "exceptionMessage", webException.Message } }));
                }
                else if (ex is Common.Http.HttpException restException && restException.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var error = Json.Deserialize<TelegramError>(restException.Response.Content);
                    var property = "BotToken";

                    if (error.Description.ContainsIgnoreCase("chat not found") || error.Description.ContainsIgnoreCase("group chat was upgraded to a supergroup chat"))
                    {
                        property = "ChatId";
                    }
                    else if (error.Description.ContainsIgnoreCase("message thread not found"))
                    {
                        property = "TopicId";
                    }

                    return new ValidationFailure(property, _localizationService.GetLocalizedString("NotificationsValidationUnableToConnect", new Dictionary<string, object> { { "exceptionMessage", error.Description } }));
                }

                return new ValidationFailure("BotToken", _localizationService.GetLocalizedString("NotificationsValidationUnableToConnect", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }
    }
}
