using System.IO;
using NUnit.Framework;

namespace NzbDrone.Core.Test
{
    [SetUpFixture]
    public class RemoveCachedDatabase
    {
        [OneTimeSetUp]
        [OneTimeTearDown]
        public void ClearCachedDatabase()
        {
            var mainCache = Path.Combine(TestContext.CurrentContext.TestDirectory, $"cached_Main.db");
            if (File.Exists(mainCache))
            {
                File.Delete(mainCache);
            }

            var logCache = Path.Combine(TestContext.CurrentContext.TestDirectory, $"cached_Log.db");
            if (File.Exists(logCache))
            {
                File.Delete(logCache);
            }
        }
    }
}
