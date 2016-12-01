using System;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.Notifications.Telegram
{
    public interface ITelegramProxy
    {
        void SendNotification(string title, string message, TelegramSettings settings);
        ValidationFailure Test(TelegramSettings settings);
    }

    public class TelegramProxy : ITelegramProxy
    {
        private readonly Logger _logger;
        private const string URL = "https://api.telegram.org";
        public TelegramProxy(Logger logger)
        {
            _logger = logger;
        }

        public void SendNotification(string title, string message, TelegramSettings settings)
        {
            //Format text to add the title before and bold using markdown
            string text = string.Concat("*", title, "*\n", message);
            var client = RestClientFactory.BuildClient(URL);
            var request = new RestRequest("bot{token}/sendmessage", Method.POST);
            request.AddUrlSegment("token", settings.BotToken);
            request.AddParameter("chat_id", settings.ChatID);
            request.AddParameter("parse_mode", "Markdown");
            request.AddParameter("text", text);

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
                _logger.Error(ex, "Unable to send test message: " + ex.Message);
                return new ValidationFailure("BotToken", "Unable to send test message");
            }

            return null;
        }
    }
}
