using System.IO;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Test.Common.Datastore
{
    public static class SqliteDatabase
    {
        public static string GetCachedDb(MigrationType type)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, $"cached_{type}.db");
        }
    }
}
