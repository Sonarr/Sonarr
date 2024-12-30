﻿using System.Text.RegularExpressions;
using Workarr.Datastore;
using Workarr.Http;

namespace Workarr.HealthCheck
{
    public class HealthCheck : ModelBase
    {
        private static readonly Regex CleanFragmentRegex = new Regex("[^a-z ]", RegexOptions.Compiled);

        public Type Source { get; set; }
        public HealthCheckResult Type { get; set; }
        public string Message { get; set; }
        public HttpUri WikiUrl { get; set; }

        public HealthCheck()
        {
        }

        public HealthCheck(Type source)
        {
            Source = source;
            Type = HealthCheckResult.Ok;
        }

        public HealthCheck(Type source, HealthCheckResult type, string message, string wikiFragment = null)
        {
            Source = source;
            Type = type;
            Message = message;
            WikiUrl = MakeWikiUrl(wikiFragment ?? MakeWikiFragment(message));
        }

        private static string MakeWikiFragment(string message)
        {
            return "#" + CleanFragmentRegex.Replace(message.ToLower(), string.Empty).Replace(' ', '-');
        }

        private static HttpUri MakeWikiUrl(string fragment)
        {
            return new HttpUri("https://wiki.servarr.com/sonarr/system") + new HttpUri(fragment);
        }
    }

    public enum HealthCheckResult
    {
        Ok = 0,
        Notice = 1,
        Warning = 2,
        Error = 3
    }
}
