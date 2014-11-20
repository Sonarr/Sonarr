using System.Linq;
using System.Text.RegularExpressions;

namespace NzbDrone.Common.Instrumentation
{
    public class CleanseLogMessage
    {
        private static readonly Regex[] CleansingRules = new[] 
            {
                // Url
                new Regex(@"(?<=\?|&)(apikey|token|passkey|uid)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"(?<=\?|&)[^=]*?(username|password)=(?<secret>[^&=]+?)(?= |&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"torrentleech\.org/(?<secret>[0-9a-z]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"iptorrents\.com/[/a-z0-9?&;]*?(?:[?&;](u|tp)=(?<secret>[^&=;]+?))+(?= |;|&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // Path
                new Regex(@"""C:\\Users\\(?<secret>[^\""]+?)(\\|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"""/home/(?<secret>[^/""]+?)(/|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // NzbGet
                new Regex(@"""Name""\s*:\s*""[^""]*(username|password)""\s*,\s*""Value""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // Sabnzbd
                new Regex(@"""[^""]*(username|password|api_?key|nzb_key)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"""email_(account|to|from|pwd)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // uTorrent
                new Regex(@"\[""[a-z._]*(|username|password)"",\d,""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"\[""(boss_key|boss_key_salt|proxy\.proxy)"",\d,""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // BroadcastheNet
                new Regex(@"""?method""?\s*:\s*""(getTorrents)"",\s*""?params""?\s*:\s*\[\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"(?<=\?|&)(authkey|torrent_pass)=(?<secret>[^&=]+?)(?=""|&|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

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
                            value = value.Replace(capture.Index - m.Index, capture.Length, "<removed>");
                        }

                        return value;
                    });
            }

            return message;
        }
    }
}
