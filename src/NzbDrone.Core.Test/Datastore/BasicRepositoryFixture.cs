using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class BasicRepositoryFixture : DbTest<BasicRepository<ScheduledTask>, ScheduledTask>
    {
        private readonly TimeSpan _dateTimePrecision = TimeSpan.FromMilliseconds(20);
        private List<ScheduledTask> _basicList;

        [SetUp]
        public void Setup()
        {
            AssertionOptions.AssertEquivalencyUsing(options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation.ToUniversalTime(), _dateTimePrecision)).WhenTypeIs<DateTime>();
                return options;
            });

            _basicList = Builder<ScheduledTask>
                .CreateListOfSize(5)
                .All()
                .With(x => x.Id = 0)
                .BuildList();
        }

        [Test]
        public async Task should_be_able_to_insert()
        {
            await Subject.InsertAsync(_basicList[0]);
            var enumerable = await Subject.AllAsync();
            enumerable.Should().HaveCount(1);
        }

        [Test]
        public async Task should_be_able_to_insert_many()
        {
            await Subject.InsertManyAsync(_basicList);
            var enumerable = await Subject.AllAsync();
            enumerable.Should().HaveCount(5);
        }

        [Test]
        public void insert_many_should_throw_if_id_not_zero()
        {
            _basicList[1].Id = 999;
            Assert.ThrowsAsync<InvalidOperationException>(async () => await Subject.InsertManyAsync(_basicList));
        }

        [Test]
        public async Task should_be_able_to_get_count()
        {
            await Subject.InsertManyAsync(_basicList);
            var counter = await Subject.CountAsync();
            counter.Should().Be(_basicList.Count);
        }

        [Test]
        public async Task should_be_able_to_find_by_id()
        {
            await Subject.InsertManyAsync(_basicList);
            var storeObject = await Subject.GetAsync(_basicList[1].Id);

            storeObject.Should().BeEquivalentTo(_basicList[1], o => o.IncludingAllRuntimeProperties());
        }

        [Test]
        public async Task should_be_able_to_update()
        {
            await Subject.InsertManyAsync(_basicList);

            var item = _basicList[1];
            item.Interval = 999;

            await Subject.UpdateAsync(item);

            var enumerable = await Subject.AllAsync();
            enumerable.Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public async Task should_be_able_to_upsert_new()
        {
            await Subject.UpsertAsync(_basicList[0]);
            var enumerable = await Subject.AllAsync();
            enumerable.Should().HaveCount(1);
        }

        [Test]
        public async Task should_be_able_to_upsert_existing()
        {
            await Subject.InsertManyAsync(_basicList);

            var item = _basicList[1];
            item.Interval = 999;

            await Subject.UpsertAsync(item);

            var enumerable = await Subject.AllAsync();
            enumerable.Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public async Task should_be_able_to_update_single_field()
        {
            await Subject.InsertManyAsync(_basicList);

            var item = _basicList[1];
            var executionBackup = item.LastExecution;
            item.Interval = 999;
            item.LastExecution = DateTime.UtcNow;

            await Subject.SetFieldsAsync(item, default, x => x.Interval);

            item.LastExecution = executionBackup;
            var enumerable = await Subject.AllAsync();
            enumerable.Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public async Task set_fields_should_throw_if_id_zero()
        {
            await Subject.InsertManyAsync(_basicList);
            _basicList[1].Id = 0;
            _basicList[1].LastExecution = DateTime.UtcNow;

            Assert.ThrowsAsync<InvalidOperationException>(async () => await Subject.SetFieldsAsync(_basicList[1], default, x => x.Interval));
        }

        [Test]
        public async Task should_be_able_to_delete_model_by_id()
        {
            await Subject.InsertManyAsync(_basicList);
            var insertEnumerable = await Subject.AllAsync();
            insertEnumerable.Should().HaveCount(5);

            await Subject.DeleteAsync(_basicList[0].Id);
            var deleteEnumerable = await Subject.AllAsync();
            deleteEnumerable.Select(x => x.Id).Should().BeEquivalentTo(_basicList.Skip(1).Select(x => x.Id));
        }

        [Test]
        public async Task should_be_able_to_delete_model_by_object()
        {
            await Subject.InsertManyAsync(_basicList);
            var insertEnumerable = await Subject.AllAsync();
            insertEnumerable.Should().HaveCount(5);

            await Subject.DeleteAsync(_basicList[0]);
            var deleteEnumerable = await Subject.AllAsync();
            deleteEnumerable.Select(x => x.Id).Should().BeEquivalentTo(_basicList.Skip(1).Select(x => x.Id));
        }

        [Test]
        public async Task get_many_should_return_empty_list_if_no_ids()
        {
            var enumerable = await Subject.GetAsync(new List<int>());
            enumerable.Should().BeEquivalentTo(new List<ScheduledTask>());
        }

        [Test]
        public async Task get_many_should_throw_if_not_all_found()
        {
            await Subject.InsertManyAsync(_basicList);
            Assert.ThrowsAsync<ApplicationException>(async () => await Subject.GetAsync(new[] { 999 }));
        }

        [Test]
        public async Task should_be_able_to_find_by_multiple_id()
        {
            await Subject.InsertManyAsync(_basicList);
            var storeObject = await Subject.GetAsync(_basicList.Take(2).Select(x => x.Id));
            storeObject.Select(x => x.Id).Should().BeEquivalentTo(_basicList.Take(2).Select(x => x.Id));
        }

        [Test]
        public async Task should_be_able_to_update_many()
        {
            await Subject.InsertManyAsync(_basicList);
            _basicList.ForEach(x => x.Interval = 999);

            await Subject.UpdateManyAsync(_basicList);
            var enumerable = await Subject.AllAsync();
            enumerable.Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public async Task update_many_should_throw_if_id_zero()
        {
            await Subject.InsertManyAsync(_basicList);
            _basicList[1].Id = 0;
            Assert.ThrowsAsync<InvalidOperationException>(async () => await Subject.UpdateManyAsync(_basicList));
        }

        [Test]
        public async Task should_be_able_to_update_many_single_field()
        {
            await Subject.InsertManyAsync(_basicList);

            var executionBackup = _basicList.Select(x => x.LastExecution).ToList();
            _basicList.ForEach(x => x.Interval = 999);
            _basicList.ForEach(x => x.LastExecution = DateTime.UtcNow);

            await Subject.SetFieldsAsync(_basicList, default, x => x.Interval);

            for (var i = 0; i < _basicList.Count; i++)
            {
                _basicList[i].LastExecution = executionBackup[i];
            }

            var enumerable = await Subject.AllAsync();
            enumerable.Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public async Task set_fields_should_throw_if_any_id_zero()
        {
            await Subject.InsertManyAsync(_basicList);
            _basicList.ForEach(x => x.Interval = 999);
            _basicList[1].Id = 0;

            Assert.ThrowsAsync<InvalidOperationException>(async () => await Subject.SetFieldsAsync(_basicList, default, x => x.Interval));
        }

        [Test]
        public async Task should_be_able_to_delete_many_by_model()
        {
            await Subject.InsertManyAsync(_basicList);
            var insertEnumerable = await Subject.AllAsync();
            insertEnumerable.Should().HaveCount(5);

            await Subject.DeleteManyAsync(_basicList.Take(2).ToList());
            var deleteEnumerable = await Subject.AllAsync();
            deleteEnumerable.Select(x => x.Id).Should().BeEquivalentTo(_basicList.Skip(2).Select(x => x.Id));
        }

        [Test]
        public async Task should_be_able_to_delete_many_by_id()
        {
            await Subject.InsertManyAsync(_basicList);
            var insertEnumerable = await Subject.AllAsync();
            insertEnumerable.Should().HaveCount(5);

            await Subject.DeleteManyAsync(_basicList.Take(2).Select(x => x.Id).ToList());
            var deleteEnumerable = await Subject.AllAsync();
            deleteEnumerable.Select(x => x.Id).Should().BeEquivalentTo(_basicList.Skip(2).Select(x => x.Id));
        }

        [Test]
        public async Task purge_should_delete_all()
        {
            await Subject.InsertManyAsync(_basicList);

            var scheduledTasks1 = await GetAllStoredModelsAsync();
            scheduledTasks1.Should().HaveCount(5);

            await Subject.PurgeAsync();

            var scheduledTasks2 = await GetAllStoredModelsAsync();
            scheduledTasks2.Should().BeEmpty();
        }

        [Test]
        public async Task has_items_should_return_false_with_no_items()
        {
            var hasItems = await Subject.HasItemsAsync();
            hasItems.Should().BeFalse();
        }

        [Test]
        public async Task has_items_should_return_true_with_items()
        {
            await Subject.InsertManyAsync(_basicList);
            var hasiIems = await Subject.HasItemsAsync();
            hasiIems.Should().BeTrue();
        }

        [Test]
        public void single_should_throw_on_empty()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () => await Subject.SingleAsync());
        }

        [Test]
        public async Task should_be_able_to_get_single()
        {
            await Subject.InsertAsync(_basicList[0]);
            var scheduledTask = await Subject.SingleAsync();
            scheduledTask.Should().BeEquivalentTo(_basicList[0]);
        }

        [Test]
        public async Task single_or_default_on_empty_table_should_return_null()
        {
            var scheduledTask = await Subject.SingleOrDefaultAsync();
            scheduledTask.Should().BeNull();
        }

        [Test]
        public void getting_model_with_invalid_id_should_throw()
        {
            Assert.ThrowsAsync<ModelNotFoundException>(async () => await Subject.GetAsync(12));
        }

        [Test]
        public async Task get_all_with_empty_db_should_return_empty_list()
        {
            var enumerable = await Subject.AllAsync();
            enumerable.Should().BeEmpty();
        }

        [Test]
        public async Task should_be_able_to_call_ToList_on_empty_queryable()
        {
            var enumerable = await Subject.AllAsync();
            enumerable.ToList().Should().BeEmpty();
        }

        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 1)]
        public async Task get_paged_should_work(int page, int count)
        {
            await Subject.InsertManyAsync(_basicList);
            var data = await Subject.GetPagedAsync(new PagingSpec<ScheduledTask>() { Page = page, PageSize = 2, SortKey = "LastExecution", SortDirection = SortDirection.Descending });

            data.Page.Should().Be(page);
            data.PageSize.Should().Be(2);
            data.TotalRecords.Should().Be(_basicList.Count);
            data.Records.Should().BeEquivalentTo(_basicList.OrderByDescending(x => x.LastExecution).Skip((page - 1) * 2).Take(2));
        }

        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 1)]
        public async Task get_paged_should_work_with_null_sort_key(int page, int count)
        {
            await Subject.InsertManyAsync(_basicList);
            var data = await Subject.GetPagedAsync(new PagingSpec<ScheduledTask>() { Page = page, PageSize = 2, SortDirection = SortDirection.Descending });

            data.Page.Should().Be(page);
            data.PageSize.Should().Be(2);
            data.TotalRecords.Should().Be(_basicList.Count);
            data.Records.Should().BeEquivalentTo(_basicList.OrderByDescending(x => x.Id).Skip((page - 1) * 2).Take(2));
        }
    }
}
