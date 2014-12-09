using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class unknown_quality_in_profileFixture : MigrationTest<unknown_quality_in_profile>
    {
        [Test]
        public void should_add_unknown_to_old_profile()
        {
            WithTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    Name = "SDTV",
                    Cutoff = 1,
                    Items = "[ { \"quality\": 1, \"allowed\": true } ]",
                    Language = 1
                });
            });

            var allProfiles = Mocker.Resolve<ProfileRepository>().All().ToList();

            allProfiles.Should().HaveCount(1);
            allProfiles.First().Items.Should().HaveCount(2);
            allProfiles.First().Items.Should().Contain(i => i.Quality.Id == 0 && i.Allowed == false);
        }
    }
}
