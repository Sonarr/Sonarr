using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

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
    }
}