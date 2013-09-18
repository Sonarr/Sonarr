using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, History.History>
    {
        [Test]
        public void Trim_Items()
        {
            var historyItem = Builder<History.History>.CreateListOfSize(30)
                .All()
                .With(c=>c.Id = 0)
                .TheFirst(10).With(c => c.Date = DateTime.Now)
                .TheNext(20).With(c => c.Date = DateTime.Now.AddDays(-31))
                .Build();

            Db.InsertMany(historyItem);

            AllStoredModels.Should().HaveCount(30);
            Subject.Trim();

            AllStoredModels.Should().HaveCount(10);
            AllStoredModels.Should().OnlyContain(s => s.Date > DateTime.Now.AddDays(-30));
        }

        [Test]
        public void should_read_write_dictionary()
        {
            var history = Builder<History.History>.CreateNew().BuildNew();

            history.Data.Add("key1","value1");
            history.Data.Add("key2","value2");

            Subject.Insert(history);

            StoredModel.Data.Should().HaveCount(2);
        }

        [Test]
        public void should_delete_orphaned_items_by_series()
        {
            var history = Builder<History.History>.CreateNew().BuildNew();
            Subject.Insert(history);

            Subject.CleanupOrphanedBySeries();
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_delete_orphaned_items_by_episode()
        {
            var history = Builder<History.History>.CreateNew().BuildNew();
            Subject.Insert(history);

            Subject.CleanupOrphanedByEpisode();
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_series()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            Db.Insert(series);

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(h => h.Id = 0)
                                                  .TheFirst(1)
                                                  .With(h => h.SeriesId = series.Id)
                                                  .Build();


            Subject.InsertMany(history);

            Subject.CleanupOrphanedBySeries();
            Subject.All().Should().HaveCount(1);
            Subject.All().Should().Contain(h => h.SeriesId == series.Id);
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_episode()
        {
            var episode = Builder<Episode>.CreateNew()
                                        .BuildNew();

            Db.Insert(episode);

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(h => h.Id = 0)
                                                  .TheFirst(1)
                                                  .With(h => h.EpisodeId = episode.Id)
                                                  .Build();

            
            Subject.InsertMany(history);

            Subject.CleanupOrphanedByEpisode();
            Subject.All().Should().HaveCount(1);
            Subject.All().Should().Contain(h => h.EpisodeId == episode.Id);
        }
    }
}