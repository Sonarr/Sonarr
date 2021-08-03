using System;
using System.Linq;
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
                .GetDataMapper().Query<Series>()
                .SingleOrDefault(c => c.CleanTitle == "SomeTitle")
                .Should()
                .BeNull();
        }

        [Test]
        public void vaccume()
        {
            Mocker.Resolve<IDatabase>().Vacuum();
        }

        [Test]
        public void get_version()
        {
            Mocker.Resolve<IDatabase>().Version.Should().BeGreaterThan(new Version("3.0.0"));
        }
    }
}
