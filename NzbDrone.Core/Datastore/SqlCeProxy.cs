using System;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace NzbDrone.Core.Datastore
{
    public static class SqlCeProxy
    {
        private static readonly object instance;
        private static readonly Type proxyType;

        static SqlCeProxy()
        {
            proxyType = Assembly.Load("NzbDrone.SqlCe").GetExportedTypes()[0];
            instance = Activator.CreateInstance(proxyType);
        }

        public static DbConnection EnsureDatabase(string connectionString)
        {
            var factoryMethod = proxyType.GetMethod("EnsureDatabase");
            return (DbConnection)factoryMethod.Invoke(instance, new object[] { connectionString });
        }

        public static DbProviderFactory GetSqlCeProviderFactory()
        {
            var factoryMethod = proxyType.GetMethod("GetSqlCeProviderFactory");
            return (DbProviderFactory)factoryMethod.Invoke(instance, null);
        }
    }

}
