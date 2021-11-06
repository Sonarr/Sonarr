using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using NzbDrone.Common.EnvironmentInfo;

namespace NzbDrone.Common.Composition
{
    public class AssemblyLoader
    {
        static AssemblyLoader()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ContainerResolveEventHandler);
        }

        public static IEnumerable<Assembly> Load(IEnumerable<string> assemblyNames)
        {
            var toLoad = assemblyNames.ToList();
            toLoad.Add("Sonarr.Common");
            toLoad.Add(OsInfo.IsWindows ? "Sonarr.Windows" : "Sonarr.Mono");

            var toRegisterResolver = new List<string> { "System.Data.SQLite" };
            toRegisterResolver.AddRange(assemblyNames.Intersect(new[] { "Sonarr.Core" }));
            RegisterNativeResolver(toRegisterResolver);

            var startupPath = AppDomain.CurrentDomain.BaseDirectory;

            return toLoad.Select(x =>
                AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(startupPath, $"{x}.dll")));
        }

        private static Assembly ContainerResolveEventHandler(object sender, ResolveEventArgs args)
        {
            var resolver = new AssemblyDependencyResolver(args.RequestingAssembly.Location);
            var assemblyPath = resolver.ResolveAssemblyToPath(new AssemblyName(args.Name));

            if (assemblyPath == null)
            {
                return null;
            }

            return AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        }

        public static void RegisterNativeResolver(IEnumerable<string> assemblyNames)
        {
            foreach (var name in assemblyNames)
            {
                // This ensures we look for sqlite3 using libsqlite3.so.0 on Linux and not libsqlite3.so which
                // is less likely to exist.
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{name}.dll"));

                try
                {
                    NativeLibrary.SetDllImportResolver(assembly, LoadNativeLib);
                }
                catch (InvalidOperationException)
                {
                    // This can only be set once per assembly
                    // Catch required for NzbDrone.Host tests
                }
            }
        }

        private static IntPtr LoadNativeLib(string libraryName, Assembly assembly, DllImportSearchPath? dllImportSearchPath)
        {
            var mappedName = libraryName;
            if (OsInfo.IsLinux)
            {
                if (libraryName == "sqlite3")
                {
                    mappedName = "libsqlite3.so.0";
                }
                else if (libraryName == "mediainfo")
                {
                    mappedName = "libmediainfo.so.0";
                }
            }

            return NativeLibrary.Load(mappedName, assembly, dllImportSearchPath);
        }
    }
}
