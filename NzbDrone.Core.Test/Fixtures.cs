using System;
using System.IO;
using MbUnit.Framework;
using NLog;
using NLog.Config;
using System.Linq;

namespace NzbDrone.Core.Test
{
    [AssemblyFixture]
    public class Fixtures
    {
        [TearDown]
        public void TearDown()
        {
            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.db", SearchOption.AllDirectories))
            {
                try
                {
                    File.Delete(file);
                }
                catch
                { }
            }
        }

        [FixtureTearDown]
        public void FixtureTearDown()
        {
            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories))
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }
        }

        [SetUp]
        public void SetUp()
        {
            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(CentralDispatch.AppPath, "log.config"), false);
                LogManager.ThrowExceptions = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to configure logging. " + e);
            }
        }


    }
}