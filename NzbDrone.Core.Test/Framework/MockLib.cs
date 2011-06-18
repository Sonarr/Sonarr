// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Test.Framework
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

        public static IDatabase GetEmptyDatabase(bool enableLogging = false, string fileName = "")
        {
            Console.WriteLine("Creating an empty PetaPoco database");

            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = Guid.NewGuid() + ".db";
            }

            var connectionString = Connection.GetConnectionString(fileName);

            var database = Connection.GetPetaPocoDb(connectionString);

            return database;
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
    }
}