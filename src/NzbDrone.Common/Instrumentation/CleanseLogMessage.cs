using System.Text.RegularExpressions;

namespace NzbDrone.Common.Instrumentation
{
    public class CleanseLogMessage
    {
        //TODO: remove password=
        private static readonly Regex CleansingRegex = new Regex(@"(?<=apikey=)(\w+?)(?=\W|$|_)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static string Cleanse(string message)
        {
            if (message.IsNullOrWhiteSpace())
            {
                return message;
            }

            return CleansingRegex.Replace(message, "<removed>");
        }
    }
}
