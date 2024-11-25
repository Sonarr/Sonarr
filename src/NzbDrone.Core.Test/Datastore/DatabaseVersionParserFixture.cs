using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Test.Datastore;

[TestFixture]
public class DatabaseVersionParserFixture
{
    [TestCase("3.44.2", 3, 44, 2)]
    public void should_parse_sqlite_database_version(string serverVersion, int majorVersion, int minorVersion, int buildVersion)
    {
        var version = DatabaseVersionParser.ParseServerVersion(serverVersion);

        version.Should().NotBeNull();
        version.Major.Should().Be(majorVersion);
        version.Minor.Should().Be(minorVersion);
        version.Build.Should().Be(buildVersion);
    }

    [TestCase("14.8 (Debian 14.8-1.pgdg110+1)", 14, 8, null)]
    [TestCase("16.3 (Debian 16.3-1.pgdg110+1)", 16, 3, null)]
    [TestCase("16.3 - Percona Distribution", 16, 3, null)]
    [TestCase("17.0 - Percona Server", 17, 0, null)]
    public void should_parse_postgres_database_version(string serverVersion, int majorVersion, int minorVersion, int? buildVersion)
    {
        var version = DatabaseVersionParser.ParseServerVersion(serverVersion);

        version.Should().NotBeNull();
        version.Major.Should().Be(majorVersion);
        version.Minor.Should().Be(minorVersion);

        if (buildVersion.HasValue)
        {
            version.Build.Should().Be(buildVersion.Value);
        }
    }
}
