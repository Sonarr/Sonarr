using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Qualities;

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
                .With(c => c.Id = 0)
                .With(c => c.Quality = new QualityModel())
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
            var history = Builder<History.History>.CreateNew()
                .With(c => c.Quality = new QualityModel())
                .BuildNew();

            history.Data.Add("key1","value1");
            history.Data.Add("key2","value2");

            Subject.Insert(history);

            StoredModel.Data.Should().HaveCount(2);
        }

        [Test]
        public void grabbed_should_return_grabbed_items()
        {
            var history = Builder<History.History>
                .CreateListOfSize(5)
                .All()
                .With(c => c.Quality = new QualityModel())
                .With(c => c.EventType = HistoryEventType.Unknown)
                .Random(3)
                .With(c => c.EventType = HistoryEventType.Grabbed)
                .BuildListOfNew();

            Subject.InsertMany(history);

            Subject.Grabbed().Should().HaveCount(3);
        }
    }
}