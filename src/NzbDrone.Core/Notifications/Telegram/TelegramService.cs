using System;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Common.Http.Proxy;
using RestSharp;
using NzbDrone.Core.Rest;
using System.Web;

namespace NzbDrone.Core.Notifications.Telegram
{
    public interface ITelegramProxy
    {
        void SendNotification(string title, string message, TelegramSettings settings);
        ValidationFailure Test(TelegramSettings settings);
    }

    public class TelegramProxy : ITelegramProxy
    {
        private readonly IRestClientFactory _restClientFactory;
        private readonly Logger _logger;
        private const string URL = "https://api.telegram.org";

        public TelegramProxy(IRestClientFactory restClientFactory, Logger logger)
        {
            _restClientFactory = restClientFactory;
            _logger = logger;
        }

        public void SendNotification(string title, string message, TelegramSettings settings)
        {
            //Format text to add the title before and bold using markdown
            var text = $"<b>{HttpUtility.HtmlEncode(title)}</b>\n{HttpUtility.HtmlEncode(message)}";
            var client = _restClientFactory.BuildClient(URL);

            var request = new RestRequest("bot{token}/sendmessage", Method.POST);

            request.AddUrlSegment("token", settings.BotToken);
            request.AddParameter("chat_id", settings.ChatId);
            request.AddParameter("parse_mode", "HTML");
            request.AddParameter("text", text);
            request.AddParameter("disable_notification", settings.SendSilently);

            client.ExecuteAndValidate(request);
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
                    return new ValidationFailure("Connection", $"{webException.Status.ToString()}: {webException.Message}");
                }
                else if (ex is RestException restException && restException.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var error = Json.Deserialize<TelegramError>(restException.Response.Content);
                    var property = error.Description.ContainsIgnoreCase("chat not found") ? "ChatId" : "BotToken";

                    return new ValidationFailure(property, error.Description);
                }

                return new ValidationFailure("BotToken", "Unable to send test message");
            }

            return null;
        }
    }
}
