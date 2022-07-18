using System;
using System.Linq;
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore
{
    public class DatabaseFixture : DbTest
    {
        [Test]
        public void SingleOrDefault_should_return_null_on_empty_db()
        {
            Mocker.Resolve<IDatabase>()
                .OpenConnection().Query<Series>("SELECT * FROM \"Series\"")
                .SingleOrDefault()
                .Should()
                .BeNull();
        }

        [Test]
        public void vacuum()
        {
            Mocker.Resolve<IDatabase>().Vacuum();
        }

        [Test]
        public void postgres_should_not_contain_timestamp_without_timezone_columns()
        {
            if (Db.DatabaseType != DatabaseType.PostgreSQL)
            {
                return;
            }

            Mocker.Resolve<IDatabase>()
                .OpenConnection().Query("SELECT table_name, column_name, data_type FROM INFORMATION_SCHEMA.COLUMNS WHERE table_schema = 'public' AND data_type = 'timestamp without time zone'")
                .Should()
                .BeNullOrEmpty();
        }

        [Test]
        public void get_version()
        {
            Mocker.Resolve<IDatabase>().Version.Should().BeGreaterThan(new Version("3.0.0"));
        }
    }
}
