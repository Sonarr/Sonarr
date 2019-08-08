using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Notifications.Discord.Payloads;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Discord
{
    public class Discord : NotificationBase<DiscordSettings>
    {
        private readonly IDiscordProxy _proxy;

        public Discord(IDiscordProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Discord";
        public override string Link => "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks";

        public override void OnGrab(GrabMessage message)
        {
            var embeds = new List<Embed>
                              {
                                  new Embed
                                  {
                                      Description = message.Message,
                                      Title = message.Series.Title,
                                      Text = message.Message,
                                      Color = (int)DiscordColors.Warning
                                  }
                              };
            var payload = CreatePayload($"Grabbed: {message.Message}", embeds);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var embeds = new List<Embed>
                              {
                                  new Embed
                                  {
                                      Description = message.Message,
                                      Title = message.Series.Title,
                                      Text = message.Message,
                                      Color = (int)DiscordColors.Success
                                  }
                              };
            var payload = CreatePayload($"Imported: {message.Message}", embeds);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnRename(Series series)
        {
            var attachments = new List<Embed>
                              {
                                  new Embed
                                  {
                                      Title = series.Title,
                                  }
                              };

            var payload = CreatePayload("Renamed", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var attachments = new List<Embed>
                              {
                                  new Embed
                                  {
                                      Title = healthCheck.Source.Name,
                                      Text = healthCheck.Message,
                                      Color = healthCheck.Type == HealthCheck.HealthCheckResult.Warning ? (int)DiscordColors.Warning : (int)DiscordColors.Success
                                  }
                              };

            var payload = CreatePayload("Health Issue", attachments);

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
            catch (DiscordException ex)
            {
                return new NzbDroneValidationFailure("Unable to post", ex.Message);
            }

            return null;
        }

        private DiscordPayload CreatePayload(string message, List<Embed> embeds = null)
        {
            var avatar = Settings.Avatar;

            var payload = new DiscordPayload
            {
                Username = Settings.Username,
                Content = message,
                Embeds = embeds
            };

            if (avatar.IsNotNullOrWhiteSpace())
            {
                payload.AvatarUrl = avatar;
            }

            if (Settings.Username.IsNotNullOrWhiteSpace())
            {
                payload.Username = Settings.Username;
            }

            return payload;
        }
    }
}
