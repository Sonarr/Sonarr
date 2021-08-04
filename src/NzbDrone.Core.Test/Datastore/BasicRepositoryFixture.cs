using System;
using System.Collections.Generic;
using System.Linq;
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
        private List<ScheduledTask> _basicList;

        [SetUp]
        public void Setup()
        {
            AssertionOptions.AssertEquivalencyUsing(options =>
            {
                options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation.ToUniversalTime())).WhenTypeIs<DateTime>();
                return options;
            });

            _basicList = Builder<ScheduledTask>
                .CreateListOfSize(5)
                .All()
                .With(x => x.Id = 0)
                .BuildList();
        }

        [Test]
        public void should_be_able_to_insert()
        {
            Subject.Insert(_basicList[0]);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_be_able_to_insert_many()
        {
            Subject.InsertMany(_basicList);
            Subject.All().Should().HaveCount(5);
        }

        [Test]
        public void insert_many_should_throw_if_id_not_zero()
        {
            _basicList[1].Id = 999;
            Assert.Throws<InvalidOperationException>(() => Subject.InsertMany(_basicList));
        }

        [Test]
        public void should_be_able_to_get_count()
        {
            Subject.InsertMany(_basicList);
            Subject.Count().Should().Be(_basicList.Count);
        }

        [Test]
        public void should_be_able_to_find_by_id()
        {
            Subject.InsertMany(_basicList);
            var storeObject = Subject.Get(_basicList[1].Id);

            storeObject.Should().BeEquivalentTo(_basicList[1], o => o.IncludingAllRuntimeProperties());
        }

        [Test]
        public void should_be_able_to_update()
        {
            Subject.InsertMany(_basicList);

            var item = _basicList[1];
            item.Interval = 999;

            Subject.Update(item);

            Subject.All().Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public void should_be_able_to_upsert_new()
        {
            Subject.Upsert(_basicList[0]);
            Subject.All().Should().HaveCount(1);
        }

        [Test]
        public void should_be_able_to_upsert_existing()
        {
            Subject.InsertMany(_basicList);

            var item = _basicList[1];
            item.Interval = 999;

            Subject.Upsert(item);

            Subject.All().Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public void should_be_able_to_update_single_field()
        {
            Subject.InsertMany(_basicList);

            var item = _basicList[1];
            var executionBackup = item.LastExecution;
            item.Interval = 999;
            item.LastExecution = DateTime.UtcNow;

            Subject.SetFields(item, x => x.Interval);

            item.LastExecution = executionBackup;
            Subject.All().Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public void set_fields_should_throw_if_id_zero()
        {
            Subject.InsertMany(_basicList);
            _basicList[1].Id = 0;
            _basicList[1].LastExecution = DateTime.UtcNow;

            Assert.Throws<InvalidOperationException>(() => Subject.SetFields(_basicList[1], x => x.Interval));
        }

        [Test]
        public void should_be_able_to_delete_model_by_id()
        {
            Subject.InsertMany(_basicList);
            Subject.All().Should().HaveCount(5);

            Subject.Delete(_basicList[0].Id);
            Subject.All().Select(x => x.Id).Should().BeEquivalentTo(_basicList.Skip(1).Select(x => x.Id));
        }

        [Test]
        public void should_be_able_to_delete_model_by_object()
        {
            Subject.InsertMany(_basicList);
            Subject.All().Should().HaveCount(5);

            Subject.Delete(_basicList[0]);
            Subject.All().Select(x => x.Id).Should().BeEquivalentTo(_basicList.Skip(1).Select(x => x.Id));
        }

        [Test]
        public void get_many_should_return_empty_list_if_no_ids()
        {
            Subject.Get(new List<int>()).Should().BeEquivalentTo(new List<ScheduledTask>());
        }

        [Test]
        public void get_many_should_throw_if_not_all_found()
        {
            Subject.InsertMany(_basicList);
            Assert.Throws<ApplicationException>(() => Subject.Get(new[] { 999 }));
        }

        [Test]
        public void should_be_able_to_find_by_multiple_id()
        {
            Subject.InsertMany(_basicList);
            var storeObject = Subject.Get(_basicList.Take(2).Select(x => x.Id));
            storeObject.Select(x => x.Id).Should().BeEquivalentTo(_basicList.Take(2).Select(x => x.Id));
        }

        [Test]
        public void should_be_able_to_update_many()
        {
            Subject.InsertMany(_basicList);
            _basicList.ForEach(x => x.Interval = 999);

            Subject.UpdateMany(_basicList);
            Subject.All().Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public void update_many_should_throw_if_id_zero()
        {
            Subject.InsertMany(_basicList);
            _basicList[1].Id = 0;
            Assert.Throws<InvalidOperationException>(() => Subject.UpdateMany(_basicList));
        }

        [Test]
        public void should_be_able_to_update_many_single_field()
        {
            Subject.InsertMany(_basicList);

            var executionBackup = _basicList.Select(x => x.LastExecution).ToList();
            _basicList.ForEach(x => x.Interval = 999);
            _basicList.ForEach(x => x.LastExecution = DateTime.UtcNow);

            Subject.SetFields(_basicList, x => x.Interval);

            for (int i = 0; i < _basicList.Count; i++)
            {
                _basicList[i].LastExecution = executionBackup[i];
            }

            Subject.All().Should().BeEquivalentTo(_basicList);
        }

        [Test]
        public void set_fields_should_throw_if_any_id_zero()
        {
            Subject.InsertMany(_basicList);
            _basicList.ForEach(x => x.Interval = 999);
            _basicList[1].Id = 0;

            Assert.Throws<InvalidOperationException>(() => Subject.SetFields(_basicList, x => x.Interval));
        }

        [Test]
        public void should_be_able_to_delete_many_by_model()
        {
            Subject.InsertMany(_basicList);
            Subject.All().Should().HaveCount(5);

            Subject.DeleteMany(_basicList.Take(2).ToList());
            Subject.All().Select(x => x.Id).Should().BeEquivalentTo(_basicList.Skip(2).Select(x => x.Id));
        }

        [Test]
        public void should_be_able_to_delete_many_by_id()
        {
            Subject.InsertMany(_basicList);
            Subject.All().Should().HaveCount(5);

            Subject.DeleteMany(_basicList.Take(2).Select(x => x.Id).ToList());
            Subject.All().Select(x => x.Id).Should().BeEquivalentTo(_basicList.Skip(2).Select(x => x.Id));
        }

        [Test]
        public void purge_should_delete_all()
        {
            Subject.InsertMany(_basicList);

            AllStoredModels.Should().HaveCount(5);

            Subject.Purge();

            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void has_items_should_return_false_with_no_items()
        {
            Subject.HasItems().Should().BeFalse();
        }

        [Test]
        public void has_items_should_return_true_with_items()
        {
            Subject.InsertMany(_basicList);
            Subject.HasItems().Should().BeTrue();
        }

        [Test]
        public void single_should_throw_on_empty()
        {
            Assert.Throws<InvalidOperationException>(() => Subject.Single());
        }

        [Test]
        public void should_be_able_to_get_single()
        {
            Subject.Insert(_basicList[0]);
            Subject.Single().Should().BeEquivalentTo(_basicList[0]);
        }

        [Test]
        public void single_or_default_on_empty_table_should_return_null()
        {
            Subject.SingleOrDefault().Should().BeNull();
        }

        [Test]
        public void getting_model_with_invalid_id_should_throw()
        {
            Assert.Throws<ModelNotFoundException>(() => Subject.Get(12));
        }

        [Test]
        public void get_all_with_empty_db_should_return_empty_list()
        {
            Subject.All().Should().BeEmpty();
        }

        [Test]
        public void should_be_able_to_call_ToList_on_empty_queryable()
        {
            Subject.All().ToList().Should().BeEmpty();
        }

        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 1)]
        public void get_paged_should_work(int page, int count)
        {
            Subject.InsertMany(_basicList);
            var data = Subject.GetPaged(new PagingSpec<ScheduledTask>() { Page = page, PageSize = 2, SortKey = "LastExecution", SortDirection = SortDirection.Descending });

            data.Page.Should().Be(page);
            data.PageSize.Should().Be(2);
            data.TotalRecords.Should().Be(_basicList.Count);
            data.Records.Should().BeEquivalentTo(_basicList.OrderByDescending(x => x.LastExecution).Skip((page - 1) * 2).Take(2));
        }

        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(3, 1)]
        public void get_paged_should_work_with_null_sort_key(int page, int count)
        {
            Subject.InsertMany(_basicList);
            var data = Subject.GetPaged(new PagingSpec<ScheduledTask>() { Page = page, PageSize = 2, SortDirection = SortDirection.Descending });

            data.Page.Should().Be(page);
            data.PageSize.Should().Be(2);
            data.TotalRecords.Should().Be(_basicList.Count);
            data.Records.Should().BeEquivalentTo(_basicList.OrderByDescending(x => x.Id).Skip((page - 1) * 2).Take(2));
        }
    }
}
