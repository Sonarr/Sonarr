using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Instrumentation
{
    public class CleanseLogMessage
    {
        private static readonly Regex[] CleansingRules = new[]
            {
                // Url
                new Regex(@"(?<=\?|&)(apikey|token|passkey|auth|authkey|user|uid|api|[a-z_]*apikey|account|passwd)=(?<secret>[^&=""]+?)(?=[ ""&=]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"(?<=\?|&)[^=]*?(username|password)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"torrentleech\.org/(?!rss)(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"torrentleech\.org/rss/download/[0-9]+/(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"iptorrents\.com/[/a-z0-9?&;]*?(?:[?&;](u|tp)=(?<secret>[^&=;]+?))+(?= |;|&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"/fetch/[a-z0-9]{32}/(?<secret>[a-z0-9]{32})", RegexOptions.Compiled),
                new Regex(@"getnzb.*?(?<=\?|&)(r)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // Trackers Announce Keys; Designed for Qbit Json; should work for all in theory
                new Regex(@"announce(\.php)?(/|%2f|%3fpasskey%3d)(?<secret>[a-z0-9]{16,})|(?<secret>[a-z0-9]{16,})(/|%2f)announce"),

                // Path
                new Regex(@"C:\\Users\\(?<secret>[^\""]+?)(\\|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"/home/(?<secret>[^/""]+?)(/|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // NzbGet
                new Regex(@"""Name""\s*:\s*""[^""]*(username|password)""\s*,\s*""Value""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // Sabnzbd
                new Regex(@"""[^""]*(username|password|api_?key|nzb_key)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"""email_(account|to|from|pwd)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // uTorrent
                new Regex(@"\[""[a-z._]*(username|password)"",\d,""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\[""(boss_key|boss_key_salt|proxy\.proxy)"",\d,""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // Deluge
                new Regex(@"auth.login\(""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // BroadcastheNet
                new Regex(@"""?method""?\s*:\s*""(getTorrents)"",\s*""?params""?\s*:\s*\[\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"getTorrents\(""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"(?<=\?|&)(authkey|torrent_pass)=(?<secret>[^&=]+?)(?=""|&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // Plex
                new Regex(@"(?<=\?|&)(X-Plex-Client-Identifier|X-Plex-Token)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // Webhooks
                // Notifiarr
                new Regex(@"api/v[0-9]/notification/sonarr/(?<secret>[\w-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

        private static readonly Regex CleanseRemoteIPRegex = new Regex(@"(?:Auth-\w+(?<!Failure|Unauthorized) ip|from) (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})", RegexOptions.Compiled);

        public static string Cleanse(string message)
        {
            if (message.IsNullOrWhiteSpace())
            {
                return message;
            }

            foreach (var regex in CleansingRules)
            {
                message = regex.Replace(message, m =>
                    {
                        var value = m.Value;
                        foreach (var capture in m.Groups["secret"].Captures.OfType<Capture>().Reverse())
                        {
                            value = value.Replace(capture.Index - m.Index, capture.Length, "(removed)");
                        }

                        return value;
                    });
            }

            message = CleanseRemoteIPRegex.Replace(message, CleanseRemoteIP);

            return message;
        }

        private static string CleanseRemoteIP(Match match)
        {
            var group = match.Groups[1];
            var valueAll = match.Value;
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
