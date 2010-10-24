using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using Moq;
using NLog;
using NzbDrone.Core.Instrumentation;
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
            return GetEmptyRepository(true);
        }
        public static IRepository GetEmptyRepository(bool enableLogging)
        {
            Console.WriteLine("Creating an empty SQLite database");
            var provider = ProviderFactory.GetProvider("Data Source=" + Guid.NewGuid() + ".db;Version=3;New=True", "System.Data.SQLite");
            if (enableLogging)
            {
                provider.Log = new NlogWriter();
                provider.LogParams = true;
            }
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

        public static IDiskProvider GetStandardDisk(int seasons, int episodes)
        {
            var mock = new Mock<IDiskProvider>();
            mock.Setup(c => c.GetDirectories(It.IsAny<String>())).Returns(StandardSeries);
            mock.Setup(c => c.FolderExists(It.Is<String>(d => StandardSeries.Contains(d)))).Returns(true);


            foreach (var series in StandardSeries)
            {
                var file = new List<String>();
                for (int s = 0; s < seasons; s++)
                {
                    for (int e = 0; e < episodes; e++)
                    {
                        file.Add(String.Format("{0}\\Seasons {1}\\myepname.S{1:00}E{2:00}.avi", series, s, e));
                    }
                }

                string series1 = series;
                mock.Setup(c => c.GetFiles(series1, "*.avi", SearchOption.AllDirectories)).Returns(file.ToArray());
            }

            return mock.Object;
        }
    }
}
