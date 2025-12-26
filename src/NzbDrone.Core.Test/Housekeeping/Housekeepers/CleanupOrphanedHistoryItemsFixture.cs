using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedHistoryItemsFixture : DbTest<CleanupOrphanedHistoryItems, EpisodeHistory>
    {
        private Series _series;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .BuildNew();

            _episode = Builder<Episode>.CreateNew()
                                       .BuildNew();
        }

        private async Task GivenSeries()
        {
            await Db.InsertAsync(_series);
        }

        private async Task GivenEpisode()
        {
            await Db.InsertAsync(_episode);
        }

        [Test]
        public async Task should_delete_orphaned_items_by_series()
        {
            await GivenEpisode();

            var history = Builder<EpisodeHistory>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .With(h => h.EpisodeId = _episode.Id)
                .BuildNew();
            await Db.InsertAsync(history);

            Subject.Clean();
            var episodeHistories = await GetAllStoredModelsAsync();
            episodeHistories.Should().BeEmpty();
        }

        [Test]
        public async Task should_delete_orphaned_items_by_episode()
        {
            await GivenSeries();

            var history = Builder<EpisodeHistory>.CreateNew()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .With(h => h.SeriesId = _series.Id)
                .BuildNew();
            await Db.InsertAsync(history);

            Subject.Clean();
            var episodeHistories = await GetAllStoredModelsAsync();
            episodeHistories.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_unorphaned_data_by_series()
        {
            await GivenSeries();
            await GivenEpisode();

            var history = Builder<EpisodeHistory>.CreateListOfSize(2)
                .All()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .With(h => h.EpisodeId = _episode.Id)
                .TheFirst(1)
                .With(h => h.SeriesId = _series.Id)
                .BuildListOfNew();

            await Db.InsertManyAsync(history);

            Subject.Clean();
            var allModels = await GetAllStoredModelsAsync();
            allModels.Should().HaveCount(1);
            allModels.Should().Contain(h => h.SeriesId == _series.Id);
        }

        [Test]
        public async Task should_not_delete_unorphaned_data_by_episode()
        {
            await GivenSeries();
            await GivenEpisode();

            var history = Builder<EpisodeHistory>.CreateListOfSize(2)
                .All()
                .With(h => h.Languages = new List<Language> { Language.English })
                .With(h => h.Quality = new QualityModel())
                .With(h => h.SeriesId = _series.Id)
                .TheFirst(1)
                .With(h => h.EpisodeId = _episode.Id)
                .BuildListOfNew();

            await Db.InsertManyAsync(history);

            Subject.Clean();
            var allModels = await GetAllStoredModelsAsync();
            allModels.Should().HaveCount(1);
            allModels.Should().Contain(h => h.EpisodeId == _episode.Id);
        }
    }
}
