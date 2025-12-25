using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Blocklisting
{
    [TestFixture]
    public class BlocklistRepositoryFixture : DbTest<BlocklistRepository, Blocklist>
    {
        private Blocklist _blocklist;
        private Series _series1;
        private Series _series2;

        [SetUp]
        public void Setup()
        {
            _blocklist = new Blocklist
                     {
                         SeriesId = 12345,
                         EpisodeIds = new List<int> { 1 },
                         Quality = new QualityModel(Quality.Bluray720p),
                         Languages = new List<Language> { Language.English },
                         SourceTitle = "series.title.s01e01",
                         Date = DateTime.UtcNow
                     };

            _series1 = Builder<Series>.CreateNew()
                                      .With(s => s.Id = 7)
                                      .Build();

            _series2 = Builder<Series>.CreateNew()
                                      .With(s => s.Id = 8)
                                      .Build();
        }

        [Test]
        public async Task should_be_able_to_write_to_database()
        {
            await Subject.InsertAsync(_blocklist);
            (await Subject.AllAsync()).Should().HaveCount(1);
        }

        [Test]
        public async Task should_should_have_episode_ids()
        {
            await Subject.InsertAsync(_blocklist);

            (await Subject.AllAsync()).First().EpisodeIds.Should().Contain(_blocklist.EpisodeIds);
        }

        [Test]
        public async Task should_check_for_blocklisted_title_case_insensative()
        {
            await Subject.InsertAsync(_blocklist);

            (await Subject.BlocklistedByTitleAsync(_blocklist.SeriesId, _blocklist.SourceTitle.ToUpperInvariant())).Should().HaveCount(1);
        }

        [Test]
        public async Task should_delete_blocklists_by_seriesId()
        {
            var blocklistItems = Builder<Blocklist>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.SeriesId = _series2.Id)
                .TheRest()
                .With(c => c.SeriesId = _series1.Id)
                .All()
                .With(c => c.Quality = new QualityModel())
                .With(c => c.Languages = new List<Language>())
                .With(c => c.EpisodeIds = new List<int> { 1 })
                .BuildListOfNew();

            Db.InsertMany(blocklistItems);

            await Subject.DeleteForSeriesIdsAsync(new List<int> { _series1.Id });

            var removedSeriesBlocklists = await Subject.BlocklistedBySeriesAsync(_series1.Id);
            var nonRemovedSeriesBlocklists = await Subject.BlocklistedBySeriesAsync(_series2.Id);

            removedSeriesBlocklists.Should().HaveCount(0);
            nonRemovedSeriesBlocklists.Should().HaveCount(1);
        }
    }
}
