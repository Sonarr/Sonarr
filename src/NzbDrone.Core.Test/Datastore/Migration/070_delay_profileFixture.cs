using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Tags;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class delay_profileFixture : MigrationTest<delay_profile>
    {
        [Test]
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

        [Test]
        public void should_create_tag_for_delay_profile()
        {
            WithTestDb(c =>
                c.Insert.IntoTable("Profiles").Row(new
                {
                    GrabDelay = 1,
                    Name = "OneHour",
                    Cutoff = "{}",
                    Items = "{}"
                })
            );

            var tags = Mocker.Resolve<TagRepository>().All().ToList();

            tags.Should().HaveCount(1);
            tags.First().Label.Should().Be("delay-60");
        }

        [Test]
        public void should_add_tag_to_series_that_had_a_profile_with_delay_attached()
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

            var tag = Mocker.Resolve<TagRepository>().All().ToList().First();
            var series = Mocker.Resolve<SeriesRepository>().All().ToList();

            series.Should().HaveCount(1);
            series.First().Tags.Should().HaveCount(1);
            series.First().Tags.First().Should().Be(tag.Id);
        }
    }
}
