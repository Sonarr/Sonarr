using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Instrumentation
{
    public class CleanseLogMessage
    {
        private static readonly Regex[] CleansingRules =
        {
            // Url
            new(@"(?<=\?|&)(apikey|token|passkey|auth|authkey|user|uid|api|[a-z_]*apikey|account|passwd)=(?<secret>[^&=""]+?)(?=[ ""&=]|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"(?<=\?|&)[^=]*?(username|password)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"rss(24h)?\.torrentleech\.org/(?!rss)(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"torrentleech\.org/rss/download/[0-9]+/(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"iptorrents\.com/[/a-z0-9?&;]*?(?:[?&;](u|tp)=(?<secret>[^&=;]+?))+(?= |;|&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"/fetch/[a-z0-9]{32}/(?<secret>[a-z0-9]{32})", RegexOptions.Compiled),
            new(@"(getnzb|rss).*?(?<=\?|&)(r)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"\b(\w*)?(_?(?<!use|get_)token|username|passwo?rd)=(?<secret>[^&=]+?)(?= |&|$|;)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"-hd.me/torrent/[a-z0-9-]\.[0-9]+\.(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Trackers Announce Keys; Designed for Qbit Json; should work for all in theory
            new(@"announce(\.php)?(/|%2f|%3fpasskey%3d)(?<secret>[a-z0-9]{16,})|(?<secret>[a-z0-9]{16,})(/|%2f)announce", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Path
            new(@"C:\\Users\\(?<secret>[^\""]+?)(\\|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"/(home|Users)/(?<secret>[^/""]+?)(/|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // NzbGet
            new(@"""Name""\s*:\s*""[^""]*(username|password)""\s*,\s*""Value""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Sabnzbd
            new(@"""[^""]*(username|password|api_?key|nzb_key)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"""email_(account|to|from|pwd)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // uTorrent
            new(@"\[""[a-z._]*(username|password)"",\d,""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"\[""(boss_key|boss_key_salt|proxy\.proxy)"",\d,""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Deluge
            new(@"auth.login\(""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // BroadcastheNet
            new(@"""?method""?\s*:\s*""(getTorrents)"",\s*""?params""?\s*:\s*\[\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"getTorrents\(""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            new(@"(?<=\?|&)(authkey|torrent_pass)=(?<secret>[^&=]+?)(?=""|&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Plex
            new(@"(?<=\?|&)(X-Plex-Client-Identifier|X-Plex-Token)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Webhooks
            // Notifiarr
            new(@"api/v[0-9]/notification/sonarr/(?<secret>[\w-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Discord
            new(@"discord.com/api/webhooks/((?<secret>[\w-]+)/)?(?<secret>[\w-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

            // Telegram
            new(@"api.telegram.org/bot(?<id>[\d]+):(?<secret>[\w-]+)/", RegexOptions.Compiled | RegexOptions.IgnoreCase)
        };

        private static readonly Regex CleanseRemoteIPRegex = new(@"(?:Auth-\w+(?<!Failure|Unauthorized) ip|from) (\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})", RegexOptions.Compiled);

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
