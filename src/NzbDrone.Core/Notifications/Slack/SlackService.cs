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
        private readonly IBuildFileNames _buildFileNames;

        public SlackService(Logger logger, IBuildFileNames buildFileNames)
        {
            _logger = logger;
            _buildFileNames = buildFileNames;
        }

        public void OnDownload(DownloadMessage message, SlackSettings settings)
        {
            var fileName = _buildFileNames.BuildFileName(message.EpisodeFile.Episodes, message.Series, message.EpisodeFile);

            var payload = new SlackPayload()
            {
                IconEmoji = settings.Icon,
                Username = settings.BotName,
                Text = "Downloaded",
                Attachments = new List<Attachment>()
                {
                    new Attachment()
                    {
                        Fallback = fileName,
                        Title = message.Series.Title,
                        TitleLink = $"http://www.imdb.com/title/{message.Series.ImdbId}/",
                        Text = fileName,
                        Color = "good"
                    }
                }
            };

            NotifySlack(payload, settings);
        }

        public void OnRename(Series series, SlackSettings settings)
        {
            var payload = new SlackPayload()
            {
                IconEmoji = settings.Icon,
                Username = settings.BotName,
                Text = "Renamed",
                Attachments = new List<Attachment>()
                {
                    new Attachment()
                    {
                        Title = series.Title,
                        TitleLink = $"http://www.imdb.com/title/{series.ImdbId}/",
                    }
                }
            };

            NotifySlack(payload, settings);
        }

        public void OnGrab(GrabMessage message, SlackSettings settings)
        {
            var payload = new SlackPayload()
            {
                IconEmoji = settings.Icon,
                Username = settings.BotName,
                Text = "Grabbed",
                Attachments = new List<Attachment>()
                {
                    new Attachment()
                    {
                        Fallback = message.Message,
                        Title = message.Series.Title,
                        TitleLink = $"http://www.imdb.com/title/{message.Series.ImdbId}/",
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
                var message = string.Format("Test message from Sonarr posted at {0}", DateTime.Now);
                var payload = new SlackPayload()
                {
                    IconEmoji = settings.Icon,
                    Username = settings.BotName,
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

        private void NotifySlack(SlackPayload text, SlackSettings settings)
        {
            try
            {
                var client = RestClientFactory.BuildClient(settings.WebHookUrl);
                var request = new RestRequest(Method.POST) { RequestFormat = DataFormat.Json };
                request.JsonSerializer = new JsonNetSerializer();
                request.AddBody(text);
                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
            {
                _logger.Error(ex, "Unable to post payload {0}", text);
                throw new SlackExeption("Unable to post payload", ex);
            }
        }
    }
}
