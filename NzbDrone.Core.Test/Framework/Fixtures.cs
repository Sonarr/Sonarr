using System;
using System.IO;
using NLog;
using NLog.Config;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [SetUpFixture]
    public class Fixtures
    {
        [TearDown]
        public void TearDown()
        {

        }

        [SetUp]
        public void SetUp()
        {
            try
            {
                LogManager.Configuration = new XmlLoggingConfiguration(Path.Combine(CentralDispatch.AppPath, "log.config"), false);
                LogManager.ThrowExceptions = true;

                var exceptionVerification = new ExceptionVerification();
                LogManager.Configuration.AddTarget("ExceptionVerification", exceptionVerification);
                LogManager.Configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Info, exceptionVerification));
                LogManager.Configuration.Reload();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to configure logging. " + e);
            }

            var filesToDelete = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sdf", SearchOption.AllDirectories);
            foreach (var file in filesToDelete)
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
}