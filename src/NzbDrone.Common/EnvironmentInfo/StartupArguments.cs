using System.Collections.Generic;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IStartupArguments
    {
        HashSet<string> Flags { get; }
        Dictionary<string, string> Args { get; }
    }

    public class StartupArguments : IStartupArguments
    {
        public const string APPDATA = "data";
        public const string NO_BROWSER = "nobrowser";
        public const string INSTALL_SERVICE = "i";
        public const string UNINSTALL_SERVICE = "u";
        public const string HELP = "?";

        public StartupArguments(params string[] args)
        {
            Flags = new HashSet<string>();
            Args = new Dictionary<string, string>();

            foreach (var s in args)
            {
                var flag = s.Trim(' ', '/', '-').ToLower();

                var argParts = flag.Split('=');

                if (argParts.Length == 2)
                {
                    Args.Add(argParts[0].Trim(), argParts[1].Trim(' ', '"'));
                }
                else
                {
                    Flags.Add(flag);
                }
            }
        }

        public HashSet<string> Flags { get; private set; }
        public Dictionary<string, string> Args { get; private set; }
    }
}