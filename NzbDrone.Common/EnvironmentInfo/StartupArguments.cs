using System.Collections.Generic;

namespace NzbDrone.Common.EnvironmentInfo
{
    public class StartupArguments
    {

        public const string NO_BROWSER = "no-browser";
        public const string INSTALL_SERVICE = "i";
        public const string UNINSTALL_SERVICE = "u";
        public const string HELP = "?";

        public StartupArguments(string[] args)
        {
            Flags = new HashSet<string>();

            foreach (var s in args)
            {
                var flag = s.Trim(' ', '/', '-').ToLower();
                Flags.Add(flag);
            }
        }

        public HashSet<string> Flags { get; private set; }
    }
}