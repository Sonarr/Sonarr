using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class delay_profileFixture : MigrationTest<delay_profile>
    {
        [Test]
        public void should_migrate_old_delays()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 1,
                    Name = "OneHour",
                    Cutoff = 0,
                    Items = "[]"
                });

                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 2,
                    Name = "TwoHours",
                    Cutoff = "{}",
                    Items = "[]"
                });
            });

            var allProfiles = db.Query<DelayProfile70>("SELECT * FROM DelayProfiles");

            allProfiles.Should().HaveCount(3);
            allProfiles.Should().OnlyContain(c => c.PreferredProtocol == 1);
            allProfiles.Should().OnlyContain(c => c.TorrentDelay == 0);
            allProfiles.Should().Contain(c => c.UsenetDelay == 60);
            allProfiles.Should().Contain(c => c.UsenetDelay == 120);
        }

        [Test]
        public void should_create_tag_for_delay_profile()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 1,
                    Name = "OneHour",
                    Cutoff = 0,
                    Items = "[]"
                });
            });

            var tags = db.Query<Tag69>("SELECT * FROM Tags");

            tags.Should().HaveCount(1);
            tags.First().Label.Should().Be("delay-60");
        }

        [Test]
        public void should_add_tag_to_series_that_had_a_profile_with_delay_attached()
        {
            var db = WithMigrationTestDb(c =>
            {
                c.Insert.IntoTable("Profiles").Row(new
                                                   {
                                                       GrabDelay = 1,
                                                       Name = "OneHour",
                                                       Cutoff = 0,
                                                       Items = "[]"
                                                   });

                c.Insert.IntoTable("Series").Row(new
                                                 {
                                                     TvdbId = 0,
                                                     TvRageId = 0,
                                                     Title = "Series",
                                                     TitleSlug = "series",
                                                     CleanTitle = "series",
                                                     Status = 0,
                                                     Images = "[]",
                                                     Path = @"C:\Test\Series",
                                                     Monitored = 1,
                                                     SeasonFolder = 1,
                                                     RunTime = 0,
                                                     SeriesType = 0,
                                                     UseSceneNumbering = 0,
                                                     Tags = "[1]"
                                                 });
            });

            var tag = db.Query<Tag69>("SELECT Id, Label FROM Tags").Single();
            var series = db.Query<Series69>("SELECT Tags FROM Series");

            series.Should().HaveCount(1);
            series.First().Tags.Should().BeEquivalentTo(tag.Id);
        }
    }
}
