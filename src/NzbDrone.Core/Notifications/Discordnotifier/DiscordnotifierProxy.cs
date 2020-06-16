using System;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using RestSharp;
using NzbDrone.Core.Rest;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Core.Notifications.Discordnotifier
{
    public interface IDiscordnotifierProxy
    {
        void SendNotification(StringDictionary message, DiscordnotifierSettings settings);
        ValidationFailure Test(DiscordnotifierSettings settings);
    }

    public class DiscordnotifierProxy : IDiscordnotifierProxy
    {
        private readonly IRestClientFactory _restClientFactory;
        private readonly Logger _logger;
        private const string URL = "https://discordnotifier.com/notifier.php";

        public DiscordnotifierProxy(IRestClientFactory restClientFactory, Logger logger)
        {
            _restClientFactory = restClientFactory;
            _logger = logger;
        }

        public void SendNotification(StringDictionary message, DiscordnotifierSettings settings)
        {
            var request = new RestRequest(Method.POST);

            try
            {
                SendNotification(message, request, settings);
            }
            catch (DiscordnotifierException ex)
            {
                _logger.Error(ex, "Unable to send notification");
                throw new DiscordnotifierException("Unable to send notification");
            }
        }

        public ValidationFailure Test(DiscordnotifierSettings settings)
        {
            try
            {
				var variables = new StringDictionary();
				variables.Add("Sonarr_EventType", "Test");

                SendNotification(variables, settings);
                return null;
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.Error(ex, "API key is invalid");
                    return new ValidationFailure("APIKey", "API key is invalid");
                }

                _logger.Error(ex, "Unable to send test notification");
                return new ValidationFailure("APIKey", "Unable to send test notification");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to send test message");
                return new ValidationFailure("", "Unable to send test notification");
            }
        }

        private void SendNotification(StringDictionary message, RestRequest request, DiscordnotifierSettings settings)
        {
            try
            {
                var client = _restClientFactory.BuildClient(URL);

                request.AddParameter("api", settings.APIKey);
				foreach (string key in message.Keys)
				{
					request.AddParameter(key, message[key]);
				}				

                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
            {
                if (ex.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    _logger.Error(ex, "API key is invalid");
                    throw;
                }

                throw new DiscordnotifierException("Unable to send notification", ex);
            }
        }
    }
}
