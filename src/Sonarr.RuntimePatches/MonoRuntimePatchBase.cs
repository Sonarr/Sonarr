using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NzbDrone.RuntimePatches
{
    public abstract class MonoRuntimePatchBase : RuntimePatchBase
    {
        private static readonly Regex VersionRegex = new Regex(@"(?<=\W|^)(?<version>\d+\.\d+(\.\d+)?(\.\d+)?)(?=\W)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Version MonoVersion;
        public virtual Version MonoMinVersion => new Version(0, 0);
        public virtual Version MonoMaxVersion => new Version(100, 0);

        static MonoRuntimePatchBase()
        {
            // Copied from MonoPlatformInfo, coz we want to load as little as possible at this stage.
            try
            {
                var type = Type.GetType("Mono.Runtime");

                if (type != null)
                {
                    var displayNameMethod = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    if (displayNameMethod != null)
                    {
                        var displayName = displayNameMethod.Invoke(null, null).ToString();
                        var versionMatch = VersionRegex.Match(displayName);

                        if (versionMatch.Success)
                        {
                            MonoVersion = new Version(versionMatch.Groups["version"].Value);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public override bool ShouldPatch()
        {
            if (MonoVersion == null)
            {
                return false;
            }

            return MonoMinVersion <= MonoVersion && MonoMaxVersion > MonoVersion;
        }

        protected override void Log(string log)
        {
            base.Log($"{log} (Mono {MonoVersion})");
        }
    }
}
