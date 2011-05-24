#region License

//The contents of this file are subject to the Mozilla Public License
//Version 1.1 (the "License"); you may not use this file except in
//compliance with the License. You may obtain a copy of the License at
//http://www.mozilla.org/MPL/
//Software distributed under the License is distributed on an "AS IS"
//basis, WITHOUT WARRANTY OF ANY KIND, either express or implied. See the
//License for the specific language governing rights and limitations
//under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using Migrator.Framework;
using Migrator.Providers;

namespace Migrator
{
    /// <summary>
    /// Handles loading Provider implementations
    /// </summary>
    public class ProviderFactory
    {
        private static readonly Assembly providerAssembly;
        private static readonly Dictionary<String, object> dialects = new Dictionary<string, object>();
        static ProviderFactory()
        {
            
            //string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            //string fullPath = Path.Combine(directory, "Migrator.Providers.dll");
            //if (fullPath.StartsWith("file:\\"))
            //    fullPath = fullPath.Substring(6);
            //else if (fullPath.StartsWith("file:"))
            //    fullPath = fullPath.Substring(5);
            providerAssembly = Assembly.GetAssembly(typeof(TransformationProvider));
            //providerAssembly = Assembly.LoadFrom("Migrator.Providers.dll");
            LoadDialects();
        }

        public static ITransformationProvider Create(string providerName, string connectionString)
        {
            object dialectInstance = DialectForProvider(providerName);
            MethodInfo mi = dialectInstance.GetType().GetMethod("NewProviderForDialect", new Type[] {typeof (String)});
            return (ITransformationProvider)mi.Invoke(dialectInstance, new object[] { connectionString });
        }

        public static object DialectForProvider(string providerName)
        {
            if (String.IsNullOrEmpty(providerName))
                return null;

            foreach (string key in dialects.Keys)
            {
                if (0 < key.IndexOf(providerName, StringComparison.InvariantCultureIgnoreCase))
                    return dialects[key];
            }
            return null;
        }

        public static void LoadDialects()
        {
            Type dialectType = providerAssembly.GetType("Migrator.Providers.Dialect");
            foreach (Type t in providerAssembly.GetTypes())
            {
                if (t.IsSubclassOf(dialectType))
                {
                    dialects.Add(t.FullName, Activator.CreateInstance(t, null));
                }
            }
        }
    }
}
