using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.SeriesRepositoryTests
{
    [TestFixture]

    public class SeriesRepositoryFixture : DbTest<SeriesRepository, Series>
    {
        [Test]
        public void should_persist_retention_defaults_and_policy_updates()
        {
            var series = new Series
            {
                TvdbId = 123,
                Title = "Retention Test",
                CleanTitle = "retentiontest",
                SortTitle = "retention test",
                TitleSlug = "retention-test",
                Path = "/tv/Retention Test"
            };
            Subject.Insert(series);

            var stored = Subject.Get(series.Id);
            stored.RetentionEnabled.Should().BeFalse();
            stored.RetentionKeepEpisodes.Should().Be(5);
            stored.RetentionIncludeSpecials.Should().BeFalse();

            stored.RetentionEnabled = true;
            stored.RetentionKeepEpisodes = 7;
            stored.RetentionIncludeSpecials = true;
            Subject.Update(stored);

            var updated = Subject.Get(series.Id);
            updated.RetentionEnabled.Should().BeTrue();
            updated.RetentionKeepEpisodes.Should().Be(7);
            updated.RetentionIncludeSpecials.Should().BeTrue();
        }

        [Test]
        public void should_lazyload_quality_profile()
        {
            var profile = new QualityProfile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities(Quality.Bluray1080p, Quality.DVD, Quality.HDTV720p),

                    Cutoff = Quality.Bluray1080p.Id,
                    Name = "TestProfile"
                };

            Mocker.Resolve<QualityProfileRepository>().Insert(profile);

            var series = Builder<Series>.CreateNew().BuildNew();
            series.QualityProfileId = profile.Id;

            Subject.Insert(series);

            StoredModel.QualityProfile.Should().NotBeNull();
        }

        private void GivenSeries()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .All()
                .With(a => a.Id = 0)
                .TheFirst(1)
                .With(x => x.CleanTitle = "crown")
                .TheNext(1)
                .With(x => x.CleanTitle = "crownextralong")
                .BuildList();

            Subject.InsertMany(series);
        }

        [TestCase("crow")]
        [TestCase("rownc")]
        public void should_find_no_inexact_matches(string cleanTitle)
        {
            GivenSeries();

            var found = Subject.FindByTitleInexact(cleanTitle);
            found.Should().BeEmpty();
        }

        [TestCase("crowna")]
        [TestCase("acrown")]
        [TestCase("acrowna")]
        public void should_find_one_inexact_match(string cleanTitle)
        {
            GivenSeries();

            var found = Subject.FindByTitleInexact(cleanTitle);
            found.Should().HaveCount(1);
            found.First().CleanTitle.Should().Be("crown");
        }

        [TestCase("crownextralong")]
        [TestCase("crownextralonga")]
        [TestCase("acrownextralong")]
        [TestCase("acrownextralonga")]
        public void should_find_two_inexact_matches(string cleanTitle)
        {
            GivenSeries();

            var found = Subject.FindByTitleInexact(cleanTitle);
            found.Should().HaveCount(2);
            found.Select(x => x.CleanTitle).Should().BeEquivalentTo(new[] { "crown", "crownextralong" });
        }
    }
}
