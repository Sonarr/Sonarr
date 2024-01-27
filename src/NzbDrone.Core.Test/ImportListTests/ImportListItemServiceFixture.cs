using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.ImportListItems;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportListTests
{
    public class ImportListItemServiceFixture : CoreTest<ImportListItemService>
    {
        [SetUp]
        public void SetUp()
        {
            var existing = Builder<ImportListItemInfo>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.TvdbId = 6)
                .With(s => s.ImdbId = "6")
                .TheNext(1)
                .With(s => s.TvdbId = 7)
                .With(s => s.ImdbId = "7")
                .TheNext(1)
                .With(s => s.TvdbId = 8)
                .With(s => s.ImdbId = "8")
                .Build().ToList();
            Mocker.GetMock<IImportListItemInfoRepository>()
                .Setup(v => v.GetAllForLists(It.IsAny<List<int>>()))
                .Returns(existing);
        }

        [Test]
        public void should_insert_new_update_existing_and_delete_missing()
        {
            var newItems = Builder<ImportListItemInfo>.CreateListOfSize(3)
                .TheFirst(1)
                .With(s => s.TvdbId = 5)
                .TheNext(1)
                .With(s => s.TvdbId = 6)
                .TheNext(1)
                .With(s => s.TvdbId = 7)
                .Build().ToList();

            var numDeleted = Subject.SyncSeriesForList(newItems, 1);

            numDeleted.Should().Be(1);
            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.InsertMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].TvdbId == 5)), Times.Once());
            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.UpdateMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 2 && s[0].TvdbId == 6 && s[1].TvdbId == 7)), Times.Once());
            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.DeleteMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].TvdbId == 8)), Times.Once());
        }
    }
}
