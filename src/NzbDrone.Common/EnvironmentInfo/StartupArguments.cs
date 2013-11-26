using System.Collections.Generic;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IStartupArguments
    {
        HashSet<string> Flags { get; }
        Dictionary<string, string> Args { get; }
        bool InstallService { get; }
        bool UninstallService { get; }
    }

    public class StartupArguments : IStartupArguments
    {
        public const string APPDATA = "data";
        public const string NO_BROWSER = "nobrowser";
        internal const string INSTALL_SERVICE = "i";
        internal const string UNINSTALL_SERVICE = "u";
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

        public bool InstallService
        {
            get
            {
                return Flags.Contains(INSTALL_SERVICE);
            }
        }

        public bool UninstallService
        {
            get
            {
                return Flags.Contains(UNINSTALL_SERVICE);
            }
        }
    }
}