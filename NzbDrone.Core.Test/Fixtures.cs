using System;
using System.IO;
using MbUnit.Framework;

namespace NzbDrone.Core.Test
{
    [AssemblyFixture]
    public class Fixtures
    {
        [TearDown]
        public void TearDown()
        {
            var dbFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.testdb");

            foreach (var dbFile in dbFiles)
            {
                try
                {
                    File.Delete(dbFile);
                }
                catch
                { }

            }
        }

        [SetUp]
        public void Setup()
        {
            Main.ConfigureNlog();
        }
    }
}