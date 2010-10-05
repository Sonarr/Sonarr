using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Moq;
using NzbDrone.Core.Providers;
using SubSonic.DataProviders;
using SubSonic.Repository;
using TvdbLib;

namespace NzbDrone.Core.Test
{
    /// <summary>
    /// Provides the standard Mocks needed for a typical test
    /// </summary>
    static class MockLib
    {
        public static string[] StandardSeries
        {
            get { return new string[] { "c:\\tv\\the simpsons", "c:\\tv\\family guy", "c:\\tv\\southpark", "c:\\tv\\24" }; }
        }

        public static IRepository GetEmptyRepository()
        {
            var provider = ProviderFactory.GetProvider("Data Source=" + Guid.NewGuid() + ".testdb;Version=3;New=True", "System.Data.SQLite");
            return new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations);
        }

        public static IConfigProvider StandardConfig
        {
            get
            {
                var mock = new Mock<IConfigProvider>();
                mock.SetupGet(c => c.SeriesRoot).Returns("C:\\");
                return mock.Object;
            }
        }

        public static IDiskProvider GetStandardDisk()
        {
            var mock = new Mock<IDiskProvider>();
            mock.Setup(c => c.GetDirectories(It.IsAny<String>())).Returns(StandardSeries);
            mock.Setup(c => c.Exists(It.Is<String>(d => StandardSeries.Contains(d)))).Returns(true);
            return mock.Object;
        }
    }
}
