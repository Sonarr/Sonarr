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
        private void GivenExisting(List<ImportListItemInfo> existing)
        {
            Mocker.GetMock<IImportListItemInfoRepository>()
                .Setup(v => v.GetAllForLists(It.IsAny<List<int>>()))
                .Returns(existing);
        }

        [Test]
        public void should_insert_new_update_existing_and_delete_missing_based_on_tvdb_id()
        {
            var existing = Builder<ImportListItemInfo>.CreateListOfSize(2)
                .All()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .TheFirst(1)
                .With(s => s.TvdbId = 6)
                .TheNext(1)
                .With(s => s.TvdbId = 7)
                .Build().ToList();

            var newItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 5)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .Build();

            var updatedItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 6)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .Build();

            GivenExisting(existing);
            var newItems = new List<ImportListItemInfo> { newItem, updatedItem };

            var numDeleted = Subject.SyncSeriesForList(newItems, 1);

            numDeleted.Should().Be(1);

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.InsertMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].TvdbId == newItem.TvdbId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.UpdateMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].TvdbId == updatedItem.TvdbId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.DeleteMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].TvdbId != newItem.TvdbId && s[0].TvdbId != updatedItem.TvdbId)), Times.Once());
        }

        [Test]
        public void should_insert_new_update_existing_and_delete_missing_based_on_imdb_id()
        {
            var existing = Builder<ImportListItemInfo>.CreateListOfSize(2)
                .All()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .TheFirst(1)
                .With(s => s.ImdbId = "6")
                .TheNext(1)
                .With(s => s.ImdbId = "7")
                .Build().ToList();

            var newItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = "5")
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .Build();

            var updatedItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = "6")
                .With(s => s.TmdbId = 6)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .Build();

            GivenExisting(existing);
            var newItems = new List<ImportListItemInfo> { newItem, updatedItem };

            var numDeleted = Subject.SyncSeriesForList(newItems, 1);

            numDeleted.Should().Be(1);

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.InsertMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].ImdbId == newItem.ImdbId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.UpdateMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].ImdbId == updatedItem.ImdbId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.DeleteMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].ImdbId != newItem.ImdbId && s[0].ImdbId != updatedItem.ImdbId)), Times.Once());
        }

        [Test]
        public void should_insert_new_update_existing_and_delete_missing_based_on_tmdb_id()
        {
            var existing = Builder<ImportListItemInfo>.CreateListOfSize(2)
                .All()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .TheFirst(1)
                .With(s => s.TmdbId = 6)
                .TheNext(1)
                .With(s => s.TmdbId = 7)
                .Build().ToList();

            var newItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 5)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .Build();

            var updatedItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 6)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .Build();

            GivenExisting(existing);
            var newItems = new List<ImportListItemInfo> { newItem, updatedItem };

            var numDeleted = Subject.SyncSeriesForList(newItems, 1);

            numDeleted.Should().Be(1);

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.InsertMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].TmdbId == newItem.TmdbId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.UpdateMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].TmdbId == updatedItem.TmdbId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.DeleteMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].TmdbId != newItem.TmdbId && s[0].TmdbId != updatedItem.TmdbId)), Times.Once());
        }

        [Test]
        public void should_insert_new_update_existing_and_delete_missing_based_on_mal_id()
        {
            var existing = Builder<ImportListItemInfo>.CreateListOfSize(2)
                .All()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .TheFirst(1)
                .With(s => s.MalId = 6)
                .TheNext(1)
                .With(s => s.MalId = 7)
                .Build().ToList();

            var newItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 5)
                .With(s => s.AniListId = 0)
                .Build();

            var updatedItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 6)
                .With(s => s.AniListId = 0)
                .Build();

            GivenExisting(existing);
            var newItems = new List<ImportListItemInfo> { newItem, updatedItem };

            var numDeleted = Subject.SyncSeriesForList(newItems, 1);

            numDeleted.Should().Be(1);

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.InsertMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].MalId == newItem.MalId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.UpdateMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].MalId == updatedItem.MalId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.DeleteMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].MalId != newItem.MalId && s[0].MalId != updatedItem.MalId)), Times.Once());
        }

        [Test]
        public void should_insert_new_update_existing_and_delete_missing_based_on_anilist_id()
        {
            var existing = Builder<ImportListItemInfo>.CreateListOfSize(2)
                .All()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 0)
                .TheFirst(1)
                .With(s => s.AniListId = 6)
                .TheNext(1)
                .With(s => s.AniListId = 7)
                .Build().ToList();

            var newItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 5)
                .Build();

            var updatedItem = Builder<ImportListItemInfo>.CreateNew()
                .With(s => s.TvdbId = 0)
                .With(s => s.ImdbId = null)
                .With(s => s.TmdbId = 0)
                .With(s => s.MalId = 0)
                .With(s => s.AniListId = 6)
                .Build();

            GivenExisting(existing);
            var newItems = new List<ImportListItemInfo> { newItem, updatedItem };

            var numDeleted = Subject.SyncSeriesForList(newItems, 1);

            numDeleted.Should().Be(1);

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.InsertMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].AniListId == newItem.AniListId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.UpdateMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].AniListId == updatedItem.AniListId)), Times.Once());

            Mocker.GetMock<IImportListItemInfoRepository>()
                .Verify(v => v.DeleteMany(It.Is<List<ImportListItemInfo>>(s => s.Count == 1 && s[0].AniListId != newItem.AniListId && s[0].AniListId != updatedItem.AniListId)), Times.Once());
        }
    }
}
