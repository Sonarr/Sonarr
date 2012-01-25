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
        private const string DB_TEMPLATE_NAME = "_dbtemplate.sdf";

        internal static string ConnectionString { get; private set; }

        internal static IDatabase GetEmptyDatabase(string fileName = "")
        {
            Console.WriteLine("====================DataBase====================");
            Console.WriteLine("Cloning database from template.");

            if (String.IsNullOrWhiteSpace(fileName))
            {
                fileName = Guid.NewGuid() + ".sdf";
            }

            File.Copy(DB_TEMPLATE_NAME, fileName);

            ConnectionString = Connection.GetConnectionString(fileName);

            var database = Connection.GetPetaPocoDb(ConnectionString);

            Console.WriteLine("====================DataBase====================");
            Console.WriteLine();
            Console.WriteLine();

            return database;
        }

        internal static void CreateDataBaseTemplate()
        {
            Console.WriteLine("Creating an empty PetaPoco database");
            var connectionString = Connection.GetConnectionString(DB_TEMPLATE_NAME);
            var database = Connection.GetPetaPocoDb(connectionString);
            database.Dispose();
        }
    }
}