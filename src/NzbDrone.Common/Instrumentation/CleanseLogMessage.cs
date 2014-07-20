using System.Text.RegularExpressions;

namespace NzbDrone.Common.Instrumentation
{
    public class CleanseLogMessage
    {
        private static readonly Regex[] CleansingRules = new[] 
            {
                // Url
                new Regex(@"(<=\?|&)apikey=(?<secret>\w+?)(?=\W|$|_)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"(<=\?|&)[^=]*?(username|password)=(?<secret>\w+?)(?=\W|$|_)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                
                // NzbGet
                new Regex(@"""Name""\s*:\s*""[^""]*(username|password)""\s*,\s*""Value""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                // Sabnzbd
                new Regex(@"""[^""]*(username|password|api_?key|nzb_key)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase),
                new Regex(@"""email_(account|to|from|pwd)""\s*:\s*""(?<secret>[^""]+?)""", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

        //private static readonly Regex CleansingRegex = new Regex(@"(?<=apikey=)(\w+?)(?=\W|$|_)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string Cleanse(string message)
        {
            if (message.IsNullOrWhiteSpace())
            {
                return message;
            }

            foreach (var regex in CleansingRules)
            {
                message = regex.Replace(message, m => m.Value.Replace(m.Groups["secret"].Index - m.Index, m.Groups["secret"].Length, "<removed>"));
            }

            return message;
        }
    }
}
