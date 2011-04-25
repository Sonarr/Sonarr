using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NzbDrone.Core.Instrumentation;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.DataProviders;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    /// <summary>
    ///   Provides the standard Mocks needed for a typical test
    /// </summary>
    internal static class MockLib
    {
        public static string[] StandardSeries
        {
            get { return new[] { "c:\\tv\\the simpsons", "c:\\tv\\family guy", "c:\\tv\\southpark", "c:\\tv\\24" }; }
        }

        public static ConfigProvider StandardConfig
        {
            get
            {
                var mock = new Mock<ConfigProvider>();
                mock.SetupGet(c => c.SeriesRoot).Returns("C:\\");
                return mock.Object;
            }
        }

        public static IRepository GetEmptyRepository()
        {
            return GetEmptyRepository(true);
        }

        public static IRepository GetEmptyRepository(bool enableLogging)
        {
            Console.WriteLine("Creating an empty SQLite database");
            var provider = ProviderFactory.GetProvider("Data Source=" + Guid.NewGuid() + ".db;Version=3;New=True",
                                                       "System.Data.SQLite");
            if (enableLogging)
            {
                provider.Log = new NlogWriter();
            }
            var repo = new SimpleRepository(provider, SimpleRepositoryOptions.RunMigrations);
            ForceMigration(repo);
            return repo;
        }

        public static DiskProvider GetStandardDisk(int seasons, int episodes)
        {
            var mock = new Mock<DiskProvider>();
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

        public static Series GetFakeSeries(int id, string title)
        {
            return Builder<Series>.CreateNew()
                .With(c => c.SeriesId = id)
                .With(c => c.Title = title)
                .With(c => c.CleanTitle = Parser.NormalizeTitle(title))
                .Build();
        }

        public static IList<Episode> GetFakeEpisodes(int seriesId)
        {
            var epNumber = new SequentialGenerator<int>();
            return Builder<Episode>.CreateListOfSize(10)
                .WhereAll().Have(c => c.SeriesId = seriesId)
                .WhereAll().Have(c => c.EpisodeNumber = epNumber.Generate())
                .Build();
        }

        private static void ForceMigration(IRepository repository)
        {
            repository.All<Series>().Count();
            repository.All<Season>().Count();
            repository.All<Episode>().Count();
            repository.All<EpisodeFile>().Count();
            repository.All<QualityProfile>().Count();
            repository.All<History>().Count();
        }
    }
}