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
            var env = Environment.GetEnvironmentVariable("DISABLE_RUNTIMEPATCHES");
            if (env != "1")
            {
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
