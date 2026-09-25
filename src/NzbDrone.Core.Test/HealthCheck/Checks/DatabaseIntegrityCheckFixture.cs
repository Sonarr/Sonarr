using System.Data.SQLite;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class DatabaseIntegrityCheckFixture : CoreTest<DatabaseIntegrityCheck>
    {
        [Test]
        public void should_be_ok_for_postgres_without_running_sqlite_check()
        {
            Mocker.GetMock<IMainDatabase>()
                .SetupGet(s => s.DatabaseType)
                .Returns(DatabaseType.PostgreSQL);

            Subject.Check().ShouldBeOk();

            Mocker.GetMock<IMainDatabase>()
                .Verify(s => s.OpenConnection(), Times.Never);
        }

        [Test]
        public void should_be_ok_when_sqlite_quick_check_returns_ok()
        {
            var connection = new SQLiteConnection("Data Source=:memory:;Version=3;New=True;");
            connection.Open();

            Mocker.GetMock<IMainDatabase>()
                .SetupGet(s => s.DatabaseType)
                .Returns(DatabaseType.SQLite);

            Mocker.GetMock<IMainDatabase>()
                .Setup(s => s.OpenConnection())
                .Returns(connection);

            Subject.Check().ShouldBeOk();
        }
    }
}
