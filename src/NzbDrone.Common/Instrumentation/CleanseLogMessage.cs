using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Instrumentation
{
    public class CleanseLogMessage
    {
        private const string TimedOutMessage = "(removed, cleansing the log message timed out)";
        private const int TruncatedMessageLength = 4096;
        private static readonly TimeSpan MatchTimeout = TimeSpan.FromSeconds(1);

        private static readonly Regex[] CleansingRules =
        {
            // Url
            new(@"(?<=\?|&)(apikey|token|passkey|auth|authkey|user|uid|api|[a-z_]*apikey|account|passwd)=(?<secret>[^&=""]+?)(?=[ ""&=]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"(?<=\?|&)[^=&?]*?(username|password)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"rss(24h)?\.torrentleech\.org/(?!rss)(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"torrentleech\.org/rss/download/[0-9]+/(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"iptorrents\.com/[/a-z0-9?&;]*?(?:[?&;](u|tp)=(?<secret>[^&=;]+?))+(?= |;|&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"/fetch/[a-z0-9]{32}/(?<secret>[a-z0-9]{32})", RegexOptions.Compiled, MatchTimeout),
            new(@"(getnzb|rss)(?:(?!getnzb|rss).)*?(?<=\?|&)(r)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"\b(\w*)?(_?(?<!use|get_)token|username|passwo?rd)=(?<secret>[^&=]+?)(?= |&|$|;)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"-hd.me/torrent/[a-z0-9-]\.[0-9]+\.(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Trackers Announce Keys; Designed for Qbit Json; should work for all in theory
            new(@"announce(\.php)?(/|%2f|%3fpasskey%3d)(?<secret>[a-z0-9]{16,})|(?:(?<![a-z0-9])|(?<=%2f))(?<secret>[a-z0-9]{16,})(/|%2f)announce", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Path
            new(@"C:\\Users\\(?<secret>[^\""]+?)(\\|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"/(home|Users)/(?<secret>[^/""]+?)(/|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Email
            new(@"(?<![a-z0-9._%+-])(?<secret>[a-z0-9._%+-]+)@[a-z0-9.-]+\.[a-z]{2,}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // NzbGet
            new(@"""Name""\s*:\s*""[^""]*(username|password)""\s*,\s*""Value""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Sabnzbd
            new(@"""[^""]*(username|password|api_?key|nzb_key)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"""email_(account|to|from|pwd)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // uTorrent
            new(@"\[""[a-z._]*(username|password)"",\d,""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"\[""(boss_key|boss_key_salt|proxy\.proxy)"",\d,""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Deluge
            new(@"auth.login\(""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // BroadcastheNet
            new(@"""?method""?\s*:\s*""(getTorrents)"",\s*""?params""?\s*:\s*\[\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"getTorrents\(""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),
            new(@"(?<=\?|&)(authkey|torrent_pass)=(?<secret>[^&=]+?)(?=""|&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Plex
            new(@"(?<=\?|&)(X-Plex-Client-Identifier|X-Plex-Token)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Webhooks
            // Notifiarr
            new(@"api/v[0-9]/notification/sonarr/(?<secret>[\w-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Discord
            new(@"discord.com/api/webhooks/((?<secret>[\w-]+)/)?(?<secret>[\w-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout),

            // Telegram
            new(@"api.telegram.org/bot(?<id>[\d]+):(?<secret>[\w-]+)/", RegexOptions.Compiled | RegexOptions.IgnoreCase, MatchTimeout)
        };

        private static readonly Regex CleanseRemoteIPRegex = new(@"(?:Auth-\w+(?<!Failure|Unauthorized) ip|from) (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})", RegexOptions.Compiled, MatchTimeout);

        public static string Cleanse(string message)
        {
            return TryCleanse(message, out var cleansed) ? cleansed : TimedOutMessage;
        }

        public static string Cleanse(LogEventInfo logEvent, Func<LogEventInfo, string> render)
        {
            if (TryCleanse(render(logEvent), out var cleansed))
            {
                return cleansed;
            }

            var message = logEvent.FormattedMessage ?? string.Empty;

            if (message.Length > TruncatedMessageLength)
            {
                message = message[..TruncatedMessageLength];
            }

            var truncatedEvent = new LogEventInfo(logEvent.Level, logEvent.LoggerName, $"{message}... {TimedOutMessage}")
            {
                TimeStamp = logEvent.TimeStamp
            };

            return Cleanse(render(truncatedEvent));
        }

        private static bool TryCleanse(string message, out string cleansed)
        {
            cleansed = message;

            if (message.IsNullOrWhiteSpace())
            {
                return true;
            }

            try
            {
                foreach (var regex in CleansingRules)
                {
                    cleansed = regex.Replace(cleansed, m =>
                    {
                        var value = m.Value;

                        foreach (var capture in m.Groups["secret"].Captures.OfType<Capture>().Reverse())
                        {
                            value = value.Replace(capture.Index - m.Index, capture.Length, "(removed)");
                        }

                        return value;
                    });
                }

                cleansed = CleanseRemoteIPRegex.Replace(cleansed, CleanseRemoteIP);
            }
            catch (RegexMatchTimeoutException)
            {
                cleansed = null;

                return false;
            }

            return true;
        }

        private static string CleanseRemoteIP(Match match)
        {
            var group = match.Groups[1];
            var valueIP = group.Value;

            if (IPAddress.TryParse(valueIP, out var address) && !address.IsLocalAddress())
            {
                var prefix = match.Value.Substring(0, group.Index - match.Index);
                var postfix = match.Value.Substring(group.Index + group.Length - match.Index);
                var items = valueIP.Split('.');

                return $"{prefix}{items[0]}.*.*.{items[3]}{postfix}";
            }

            return match.Value;
        }
    }
}
