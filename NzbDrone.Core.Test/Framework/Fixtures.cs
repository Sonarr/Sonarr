using System.IO;
// ReSharper disable CheckNamespace
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

[SetUpFixture]
public class Fixtures : LoggingFixtures
{
    [SetUp]
    public void SetUp()
    {
        var oldDbFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sdf", SearchOption.AllDirectories);
        foreach (var file in oldDbFiles)
        {
            try
            {
                File.Delete(file);
            }
            catch { }
        }

        MockLib.CreateDataBaseTemplate();
    }
}
