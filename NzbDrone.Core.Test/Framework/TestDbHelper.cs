// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NzbDrone.Common;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using PetaPoco;

namespace NzbDrone.Core.Test.Framework
{
    internal static class TestDbHelper
    {
        private static readonly string dbTemplateName;

        static TestDbHelper()
        {
            dbTemplateName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName()) + ".sdf";
        }

        internal static string ConnectionString { get; private set; }

        internal static IDatabase GetEmptyDatabase(string fileName = "")
        {
            Console.WriteLine("====================DataBase====================");
            Console.WriteLine("Cloning database from template.");

            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = Guid.NewGuid() + ".sdf";
            }

            File.Copy(dbTemplateName, fileName);

            ConnectionString = ConnectionFactory.GetConnectionString(fileName);

            var database = ConnectionFactory.GetPetaPocoDb(ConnectionString);

            Console.WriteLine("====================DataBase====================");
            Console.WriteLine();
            Console.WriteLine();

            return database;
        }

        internal static void CreateDataBaseTemplate()
        {
            Console.WriteLine("Creating an empty PetaPoco database");
            var connectionString = ConnectionFactory.GetConnectionString(dbTemplateName);
            var database = ConnectionFactory.GetPetaPocoDb(connectionString);
            database.Dispose();
        }
    }
}