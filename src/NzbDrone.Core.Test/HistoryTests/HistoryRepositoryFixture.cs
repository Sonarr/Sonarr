using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, EpisodeHistory>
    {
        private Series _series1;
        private Series _series2;

        [SetUp]
        public void Setup()
        {
            _series1 = Builder<Series>.CreateNew()
                                      .With(s => s.Id = 7)
                                      .Build();

            _series2 = Builder<Series>.CreateNew()
                                      .With(s => s.Id = 8)
                                      .Build();
        }

        [Test]
        public async Task should_read_write_dictionary()
        {
            var history = Builder<EpisodeHistory>.CreateNew()
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Quality = new QualityModel())
                .BuildNew();

            history.Data.Add("key1", "value1");
            history.Data.Add("key2", "value2");

            await Subject.InsertAsync(history);

            var enumerable = await Subject.AllAsync();
            var storedModel = enumerable.Single();
            storedModel.Data.Should().HaveCount(2);
        }

        [Test]
        public async Task should_get_download_history()
        {
            var historyBluray = Builder<EpisodeHistory>.CreateNew()
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.SeriesId = 12)
                .With(c => c.EventType = EpisodeHistoryEventType.Grabbed)
                .BuildNew();

            var historyDvd = Builder<EpisodeHistory>.CreateNew()
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.Quality = new QualityModel(Quality.DVD))
                .With(c => c.SeriesId = 12)
                .With(c => c.EventType = EpisodeHistoryEventType.Grabbed)
             .BuildNew();

            await Subject.InsertAsync(historyBluray);
            await Subject.InsertAsync(historyDvd);

            var downloadHistory = await Subject.FindDownloadHistoryAsync(12, new QualityModel(Quality.Bluray1080p));

            downloadHistory.Should().HaveCount(1);
        }

        [Test]
        public async Task should_delete_history_items_by_seriesId()
        {
            var items = Builder<EpisodeHistory>.CreateListOfSize(5)
                .TheFirst(1)
                .With(c => c.SeriesId = _series2.Id)
                .TheRest()
                .With(c => c.SeriesId = _series1.Id)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.Languages = new List<Language> { Language.English })
                .With(c => c.EventType = EpisodeHistoryEventType.Grabbed)
                .BuildListOfNew();

            Db.InsertMany(items);

            await Subject.DeleteForSeriesAsync(new List<int> { _series1.Id });

            var dbItems = await Subject.AllAsync();
            var removedItems = dbItems.Where(h => h.SeriesId == _series1.Id);
            var nonRemovedItems = dbItems.Where(h => h.SeriesId == _series2.Id);

            removedItems.Should().HaveCount(0);
            nonRemovedItems.Should().HaveCount(1);
        }
    }
}
