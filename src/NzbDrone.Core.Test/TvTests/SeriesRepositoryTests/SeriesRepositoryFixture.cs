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

        private Series BuildSeries(int tvdbId, string seriesEdition, string titleSlug)
        {
            return Builder<Series>.CreateNew()
                .With(s => s.Id = 0)
                .With(s => s.TvdbId = tvdbId)
                .With(s => s.SeriesEdition = seriesEdition)
                .With(s => s.TitleSlug = titleSlug)
                .With(s => s.Path = $@"C:\Test\{titleSlug}")
                .BuildNew();
        }

        [Test]
        public void should_allow_same_tvdbid_with_different_series_edition()
        {
            Subject.Insert(BuildSeries(305089, SeriesEditions.Standard, "rezero"));
            Subject.Insert(BuildSeries(305089, SeriesEditions.DirectorsCut, "rezero-directors-cut"));

            Subject.FindAllByTvdbId(305089).Should().HaveCount(2);
        }

        [Test]
        public void should_find_standard_series_by_legacy_tvdbid_lookup()
        {
            Subject.Insert(BuildSeries(305089, SeriesEditions.Standard, "rezero"));
            Subject.Insert(BuildSeries(305089, SeriesEditions.DirectorsCut, "rezero-directors-cut"));

            Subject.FindByTvdbId(305089).SeriesEdition.Should().Be(SeriesEditions.Standard);
        }

        [Test]
        public void should_not_allow_duplicate_tvdbid_and_series_edition()
        {
            Subject.Insert(BuildSeries(305089, SeriesEditions.DirectorsCut, "rezero-directors-cut"));

            System.Action duplicate = () => Subject.Insert(BuildSeries(305089, SeriesEditions.DirectorsCut, "rezero-directors-cut-2"));

            duplicate.Should().Throw<System.Exception>();
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
