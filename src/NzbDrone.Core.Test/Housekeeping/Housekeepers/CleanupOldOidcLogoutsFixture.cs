using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOldOidcLogoutsFixture : DbTest<CleanupOldOidcLogouts, OidcLogout>
    {
        [Test]
        public void should_delete_logouts_older_than_seven_days()
        {
            Db.Insert(Builder<OidcLogout>.CreateNew()
                .With(l => l.SessionId = "old")
                .With(l => l.Timestamp = DateTime.UtcNow.AddDays(-8))
                .BuildNew());

            Subject.Clean();

            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_keep_recent_logouts()
        {
            Db.Insert(Builder<OidcLogout>.CreateNew()
                .With(l => l.SessionId = "recent")
                .With(l => l.Timestamp = DateTime.UtcNow.AddDays(-6))
                .BuildNew());

            Subject.Clean();

            AllStoredModels.Should().HaveCount(1);
        }
    }
}
