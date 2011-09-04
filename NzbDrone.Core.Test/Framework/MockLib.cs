// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
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
                fileName = Guid.NewGuid() + ".sdf";
            }

            var connectionString = Connection.GetConnectionString(fileName);

            var database = Connection.GetPetaPocoDb(connectionString);

            return database;
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