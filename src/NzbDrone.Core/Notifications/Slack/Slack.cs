using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Notifications.Slack.Payloads;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Slack
{
    public class Slack : NotificationBase<SlackSettings>
    {
        private readonly ISlackProxy _proxy;

        public Slack(ISlackProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Name => "Slack";
        public override string Link => "https://my.slack.com/services/new/incoming-webhook/";

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

        public override void OnRename(Series series, List<RenamedEpisodeFile> renamedFiles)
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

        public override void OnEpisodeFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = GetTitle(deleteMessage.Series, deleteMessage.EpisodeFile.Episodes),
                                  }
                              };

            var payload = CreatePayload("Episode Deleted", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = deleteMessage.Series.Title,
                                      Text = deleteMessage.DeletedFilesMessage
                                  }
                              };

            var payload = CreatePayload("Series Deleted", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = healthCheck.Source.Name,
                                      Text = healthCheck.Message,
                                      Color = healthCheck.Type == HealthCheck.HealthCheckResult.Warning ? "warning" : "danger"
                                  }
                              };

            var payload = CreatePayload("Health Issue", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var attachments = new List<Attachment>
                              {
                                  new Attachment
                                  {
                                      Title = Environment.MachineName,
                                      Text = updateMessage.Message,
                                      Color = "good"
                                  }
                              };

            var payload = CreatePayload("Application Updated", attachments);

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
            catch (SlackExeption ex)
            {
                return new NzbDroneValidationFailure("Unable to post", ex.Message);
            }

            return null;
        }

        private SlackPayload CreatePayload(string message, List<Attachment> attachments = null)
        {
            var icon = Settings.Icon;
            var channel = Settings.Channel;

            var payload = new SlackPayload
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

            if (channel.IsNotNullOrWhiteSpace())
            {
                payload.Channel = channel;
            }

            return payload;
        }

        private string GetTitle(Series series, List<Episode> episodes)
        {
            if (series.SeriesType == SeriesTypes.Daily)
            {
                var episode = episodes.First();

                return $"{series.Title} - {episode.AirDate} - {episode.Title}";
            }

            var episodeNumbers = string.Concat(episodes.Select(e => e.EpisodeNumber)
                                                       .Select(i => string.Format("x{0:00}", i)));

            var episodeTitles = string.Join(" + ", episodes.Select(e => e.Title));

            return $"{series.Title} - {episodes.First().SeasonNumber}{episodeNumbers} - {episodeTitles}";
        }
    }
}
