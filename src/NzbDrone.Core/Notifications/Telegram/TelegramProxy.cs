using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Localization;

namespace NzbDrone.Core.Notifications.Telegram
{
    public interface ITelegramProxy
    {
        void SendNotification(string title, string message, TelegramSettings settings);
        ValidationFailure Test(TelegramSettings settings);
    }

    public class TelegramProxy : ITelegramProxy
    {
        private const string URL = "https://api.telegram.org";

        private readonly IHttpClient _httpClient;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public TelegramProxy(IHttpClient httpClient, ILocalizationService localizationService,  Logger logger)
        {
            _httpClient = httpClient;
            _localizationService = localizationService;
            _logger = logger;
        }

        public void SendNotification(string title, string message, TelegramSettings settings)
        {
            // Format text to add the title before and bold using markdown
            var text = $"*{HttpUtility.HtmlEncode(title)}*\n{HttpUtility.HtmlEncode(message)}";

            var requestBuilder = new HttpRequestBuilder(URL).Resource("bot{token}/sendmessage").Post();

            var request = requestBuilder.SetSegment("token", settings.BotToken)
                                        .AddFormParameter("chat_id", settings.ChatId)
                                        .AddFormParameter("parse_mode", "MarkdownV2")
                                        .AddFormParameter("text", text)
                                        .AddFormParameter("disable_notification", settings.SendSilently)
                                        .AddFormParameter("message_thread_id", settings.TopicId)
                                        .Build();

            _httpClient.Post(request);
        }

        public ValidationFailure Test(TelegramSettings settings)
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
