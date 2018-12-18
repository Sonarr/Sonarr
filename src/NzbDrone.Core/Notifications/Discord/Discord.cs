using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Discord.Payloads;
using NzbDrone.Core.Rest;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;
using RestSharp;


namespace NzbDrone.Core.Notifications.Discord
{
    public class Discord : NotificationBase<DiscordSettings>
    {
        private readonly IDiscordProxy _proxy;
        private readonly Logger _logger;

        public Discord(IDiscordProxy proxy, Logger logger)
        {
            _proxy = proxy;
            _logger = logger;
        }

        public override string Name => "Discord";
        public override string Link => "";

        public override void OnGrab(GrabMessage message)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Fallback = message.Message,
                                      Title = message.Series.Title,
                                      Text = message.Message,
                                      Color = "warning"
                                  }
                              };
            var payload = CreatePayload($"Grabbed: {message.Message}", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Fallback = message.Message,
                                      Title = message.Series.Title,
                                      Text = message.Message,
                                      Color = "good"
                                  }
                              };
            var payload = CreatePayload($"Imported: {message.Message}", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnRename(Series series)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = series.Title,
                                  }
                              };

            var payload = CreatePayload("Renamed", attachments);

            _proxy.SendPayload(payload, Settings);
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
                var payload = CreatePayload(message);

                _proxy.SendPayload(payload, Settings);

            }
            catch (DiscordExeption ex)
            {
                return new NzbDroneValidationFailure("Unable to post", ex.Message);
            }

            return null;
        }

        private DiscordPayload CreatePayload(string message, List<Attachment> attachments = null)
        {
            var icon = Settings.Icon;

            var payload = new DiscordPayload
            {
                Username = Settings.Username,
                Text = message,
                Attachments = attachments
            };

            if (icon.IsNotNullOrWhiteSpace())
            {
                // Set the correct icon based on the value
                if (icon.StartsWith(":") && icon.EndsWith(":"))
                {
                    payload.IconEmoji = icon;
                }
                else
                {
                    payload.IconUrl = icon;
                }
            }

            if (Settings.Username != "")
            {
                payload.Username = Settings.Username;
            }
            else
            {
                payload.Username = "";
            }

            return payload;
        }
    }
}
