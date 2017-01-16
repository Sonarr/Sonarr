using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Notifications.Slack.Payloads;
using NzbDrone.Core.Rest;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;
using RestSharp;


namespace NzbDrone.Core.Notifications.Slack
{
    public class Slack : NotificationBase<SlackSettings>
    {
        private readonly Logger _logger;

        public Slack(Logger logger)
        {
            _logger = logger;
        }

        public override string Name => "Slack";
        public override string Link => "https://my.slack.com/services/new/incoming-webhook/";

        public override void OnGrab(GrabMessage message)
        {
            var payload = new SlackPayload
            {
                IconEmoji = Settings.Icon,
                Username = Settings.Username,
                Text = $"Grabbed: {message.Message}",
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

            NotifySlack(payload);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var payload = new SlackPayload
            {
                IconEmoji = Settings.Icon,
                Username = Settings.Username,
                Text = $"Imported: {message.Message}",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Fallback = message.Message,
                        Title = message.Series.Title,
                        Text = message.Message,
                        Color = "good"
                    }
                }
            };

            NotifySlack(payload);
        }

        public override void OnRename(Series series)
        {
            var payload = new SlackPayload
            {
                IconEmoji = Settings.Icon,
                Username = Settings.Username,
                Text = "Renamed",
                Attachments = new List<Attachment>
                {
                    new Attachment
                    {
                        Title = series.Title,
                    }
                }
            };

            NotifySlack(payload);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(TestMessage());

            return new ValidationResult(failures);
        }

        public ValidationFailure TestMessage()
        {
            try
            {
                var message = $"Test message from Sonarr posted at {DateTime.Now}";
                var payload = new SlackPayload
                {
                    IconEmoji = Settings.Icon,
                    Username = Settings.Username,
                    Text = message
                };

                NotifySlack(payload);

            }
            catch (SlackExeption ex)
            {
                return new NzbDroneValidationFailure("Unable to post", ex.Message);
            }

            return null;
        }

        private void NotifySlack(SlackPayload payload)
        {
            try
            {
                var client = RestClientFactory.BuildClient(Settings.WebHookUrl);
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
