using System;
using System.Linq;
using FluentAssertions;
using FluentMigrator;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class delay_profileFixture : MigrationTest<delay_profile>
    {
        [TestCase]
        public void should_migrate_old_delays()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 1,
                    Name = "OneHour",
                    Cutoff = "{}",
                    Items = "{}"
                });
                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 2,
                    Name = "TwoHours",
                    Cutoff = "{}",
                    Items = "[]"
                });
            });


            var allProfiles = Mocker.Resolve<DelayProfileRepository>().All().ToList();


            allProfiles.Should().HaveCount(3);
            allProfiles.Should().OnlyContain(c => c.PreferredProtocol == DownloadProtocol.Usenet);
            allProfiles.Should().OnlyContain(c => c.TorrentDelay == 0);
            allProfiles.Should().Contain(c => c.UsenetDelay == 60);
            allProfiles.Should().Contain(c => c.UsenetDelay == 120);

        }
    }

}