using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Rest;
using NzbDrone.Core.Validation;
using RestSharp;

namespace NzbDrone.Core.Notifications.Slack
{
    public interface ISlackService
    {
        void OnDownload(Dictionary<string,string> mapping, SlackSettings settings);
        void OnRename(Dictionary<string, string> mapping, SlackSettings settings);
        void OnGrab(Dictionary<string, string> mapping, SlackSettings settings);
        ValidationFailure Test(SlackSettings settings);
    }

    public class SlackService : ISlackService
    {
        private readonly Logger _logger;

        public SlackService(Logger logger)
        {
            _logger = logger;
        }

        public void OnDownload(Dictionary<string, string> mapping, SlackSettings settings)
        {
            var text = ConvertUserInput(settings.OnDownloadPayload, mapping);
            NotifySlack(text, settings);
        }

        public void OnRename(Dictionary<string, string> mapping, SlackSettings settings)
        {
            var text = ConvertUserInput(settings.OnRenamePayload, mapping);
            NotifySlack(text, settings);
        }

        public void OnGrab(Dictionary<string, string> mapping, SlackSettings settings)
        {
            var text = ConvertUserInput(settings.OnGrabPayload, mapping);
            NotifySlack(text, settings);
        }

        public ValidationFailure Test(SlackSettings settings)
        {
            try
            {
                // Mock OnRenamePayload
                var data = new Dictionary<string, string>
                {
                    {"$EventType", "Rename"},
                    {"$Series_Id", "1"},
                    {"$Series_Title", "How I met your mother"},
                    {"$Series_Path", @"C:\path to somthing.mp4"},
                    {"$Series_TvdbId", "123123123123"},
                    {"$Series_Type", "Standard"}
                };


                this.OnRename(data, settings);
            }
            catch (SlackExeption ex)
            {
                return new NzbDroneValidationFailure("Unable to post", ex.Message);
            }
            return null;
        }

        private void NotifySlack(string text, SlackSettings settings)
        {
            var payload = new SlackPayload()
            {
                IconEmoji = ":ghost:",
                Text = text,
                Username = settings.BotName
            };

            try
            {
                var client = RestClientFactory.BuildClient(settings.WebHookUrl);
                var request = new RestRequest(Method.POST) { RequestFormat = DataFormat.Json };
                request.JsonSerializer = new JsonNetSerializer();
                request.AddBody(payload);
                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
            {
                _logger.Error(ex, "Unable to post payload {0}", text);
                throw new SlackExeption("Unable to post payload", ex);
            }
        }


        private string ConvertUserInput(string payloadText, Dictionary<string, string> mapping)
        {

            var text = payloadText;
            foreach (var variable in mapping)
            {
                text = Replace(text, variable.Key, variable.Value, StringComparison.OrdinalIgnoreCase);
            }
            return text;
        }

        private string Replace(string str, string old, string @new, StringComparison comparison)
        {
            @new = @new ?? "";
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(old) || old.Equals(@new, comparison))
                return str;
            int foundAt;
            while ((foundAt = str.IndexOf(old, 0, StringComparison.CurrentCultureIgnoreCase)) != -1)
                str = str.Remove(foundAt, old.Length).Insert(foundAt, @new);
            return str;
        }
    }
}
