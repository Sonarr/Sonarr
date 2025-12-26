using System.Linq;
using System.Threading.Tasks;
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
        public async Task should_lazyload_quality_profile()
        {
            var profile = new QualityProfile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities(Quality.Bluray1080p, Quality.DVD, Quality.HDTV720p),

                    Cutoff = Quality.Bluray1080p.Id,
                    Name = "TestProfile"
                };

            await Mocker.Resolve<QualityProfileRepository>().InsertAsync(profile);

            var series = Builder<Series>.CreateNew().BuildNew();
            series.QualityProfileId = profile.Id;

            await Subject.InsertAsync(series);

            var storedModel = await GetStoredModelAsync();
            storedModel.QualityProfile.Should().NotBeNull();
        }

        private async Task GivenSeries()
        {
            var series = Builder<Series>.CreateListOfSize(2)
                .All()
                .With(a => a.Id = 0)
                .TheFirst(1)
                .With(x => x.CleanTitle = "crown")
                .TheNext(1)
                .With(x => x.CleanTitle = "crownextralong")
                .BuildList();

            await Subject.InsertManyAsync(series);
        }

        [TestCase("crow")]
        [TestCase("rownc")]
        public async Task should_find_no_inexact_matches(string cleanTitle)
        {
            await GivenSeries();

            var found = await Subject.FindByTitleInexactAsync(cleanTitle);
            found.Should().BeEmpty();
        }

        [TestCase("crowna")]
        [TestCase("acrown")]
        [TestCase("acrowna")]
        public async Task should_find_one_inexact_match(string cleanTitle)
        {
            await GivenSeries();

            var found = await Subject.FindByTitleInexactAsync(cleanTitle);
            found.Should().HaveCount(1);
            found.First().CleanTitle.Should().Be("crown");
        }

        [TestCase("crownextralong")]
        [TestCase("crownextralonga")]
        [TestCase("acrownextralong")]
        [TestCase("acrownextralonga")]
        public async Task should_find_two_inexact_matches(string cleanTitle)
        {
            await GivenSeries();

            var found = await Subject.FindByTitleInexactAsync(cleanTitle);
            found.Should().HaveCount(2);
            found.Select(x => x.CleanTitle).Should().BeEquivalentTo(new[] { "crown", "crownextralong" });
        }
    }
}
