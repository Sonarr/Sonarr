using System.Collections.Generic;

namespace NzbDrone.Common.EnvironmentInfo
{
    public interface IStartupContext
    {
        HashSet<string> Flags { get; }
        Dictionary<string, string> Args { get; }
        bool InstallService { get; }
        bool UninstallService { get; }

        string PreservedArguments { get; }
    }

    public class StartupContext : IStartupContext
    {
        public const string APPDATA = "data";
        public const string NO_BROWSER = "nobrowser";
        internal const string INSTALL_SERVICE = "i";
        internal const string UNINSTALL_SERVICE = "u";
        public const string HELP = "?";
        public const string TERMINATE = "terminateexisting";
        public const string RESTART = "restart";

        public StartupContext(params string[] args)
        {
            Flags = new HashSet<string>();
            Args = new Dictionary<string, string>();

            foreach (var s in args)
            {
                var flag = s.Trim(' ', '/', '-');

                var argParts = flag.Split('=');

                if (argParts.Length == 2)
                {
                    Args.Add(argParts[0].Trim().ToLower(), argParts[1].Trim(' ', '"'));
                }
                else
                {
                    Flags.Add(flag.ToLower());
                }
            }
        }

        public HashSet<string> Flags { get; private set; }
        public Dictionary<string, string> Args { get; private set; }

        public bool InstallService => Flags.Contains(INSTALL_SERVICE);

        public bool UninstallService => Flags.Contains(UNINSTALL_SERVICE);

        public string PreservedArguments
        {
            get
            {
                var args = "";

                if (Args.ContainsKey(APPDATA))
                {
                    args = "/data=" + Args[APPDATA];
                }

                if (Flags.Contains(NO_BROWSER))
                {
                    args += " /" + NO_BROWSER;
                }

                return args.Trim();
            }
        }
    }
}