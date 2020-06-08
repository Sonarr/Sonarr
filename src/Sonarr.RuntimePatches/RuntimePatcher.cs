using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace NzbDrone.RuntimePatches
{
    public static class RuntimePatcher
    {
        public static void Initialize()
        {
            var envDisableRuntimePatches = Environment.GetEnvironmentVariable("DISABLE_RUNTIMEPATCHES");
            var envDebugRuntimePatches = Environment.GetEnvironmentVariable("DEBUG_RUNTIMEPATCHES");

            if (envDisableRuntimePatches != "1")
            {
                if (envDebugRuntimePatches == "1")
                {
                    RuntimePatchBase.IsDebug = true;
                }
                else if (envDebugRuntimePatches == "0")
                {
                    RuntimePatchBase.IsDebug = false;
                }
                else
                {
#if DEBUG
                    RuntimePatchBase.IsDebug = true;
#endif
                }

                try
                {
                    ApplyPatches();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to apply runtime patches, attempting to continue normally.\r\n" + ex.ToString());
                }
            }
        }

        private static void ApplyPatches()
        {
            var patches = Assembly.GetExecutingAssembly()
                .GetExportedTypes()
                .Where(type => !type.IsAbstract && typeof(RuntimePatchBase).IsAssignableFrom(type))
                .Select(Activator.CreateInstance)
                .Cast<RuntimePatchBase>()
                .Where(patch => patch.ShouldPatch())
                .ToList();

            if (patches.Any())
            {
                var harmony = new Harmony("tv.sonarr");

                foreach (var patch in patches)
                {
                    patch.Patch(harmony);
                }
            }
        }
    }
}
