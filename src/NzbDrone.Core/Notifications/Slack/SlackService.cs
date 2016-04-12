using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Notifications.Slack.Payloads;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Rest;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;
using RestSharp;

namespace NzbDrone.Core.Notifications.Slack
{
    public interface ISlackService
    {
        void OnDownload(DownloadMessage message, SlackSettings settings);
        void OnRename(Series series, SlackSettings settings);
        void OnGrab(GrabMessage message, SlackSettings settings);
        ValidationFailure Test(SlackSettings settings);
    }

    public class SlackService : ISlackService
    {
        private readonly Logger _logger;

        public SlackService(Logger logger)
        {
            _logger = logger;
        }

        public void OnDownload(DownloadMessage message, SlackSettings settings)
        {
            var payload = new SlackPayload
            {
                IconEmoji = settings.Icon,
                Username = settings.Username,
                Text = "Downloaded",
                Attachments = new List<Attachment>
                {
                    new Attachment()
                    {
                        Fallback = message.Message,
                        Title = message.Series.Title,
                        Text = message.Message,
                        Color = "good"
                    }
                }
            };

            NotifySlack(payload, settings);
        }

        public void OnRename(Series series, SlackSettings settings)
        {
            var payload = new SlackPayload
            {
                IconEmoji = settings.Icon,
                Username = settings.Username,
                Text = "Renamed",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Title = series.Title,
                    }
                }
            };

            NotifySlack(payload, settings);
        }

        public void OnGrab(GrabMessage message, SlackSettings settings)
        {
            var payload = new SlackPayload
            {
                IconEmoji = settings.Icon,
                Username = settings.Username,
                Text = "Grabbed",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Fallback = message.Message,
                        Title = message.Series.Title,
                        Text = message.Message,
                        Color = "warning"
                    }
                }
            };

            NotifySlack(payload, settings);
        }

        public ValidationFailure Test(SlackSettings settings)
        {
            try
            {
                var message = $"Test message from Sonarr posted at {DateTime.Now}";
                var payload = new SlackPayload
                {
                    IconEmoji = settings.Icon,
                    Username = settings.Username,
                    Text = message
                };

                NotifySlack(payload, settings);
                
            }
            catch (SlackExeption ex)
            {
                return new NzbDroneValidationFailure("Unable to post", ex.Message);
            }

            return null;
        }

        private void NotifySlack(SlackPayload payload, SlackSettings settings)
        {
            try
            {
                var client = RestClientFactory.BuildClient(settings.WebHookUrl);
                var request = new RestRequest(Method.POST)
                {
                    RequestFormat = DataFormat.Json,
                    JsonSerializer = new JsonNetSerializer()
                };
                request.AddBody(payload);
                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
            {
                _logger.Error(ex, "Unable to post payload {0}", payload);
                throw new SlackExeption("Unable to post payload", ex);
            }
        }
    }
}
