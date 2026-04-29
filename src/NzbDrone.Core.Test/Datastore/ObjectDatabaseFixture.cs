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
    public class ObjectDatabaseFixture : DbTest<BasicRepository<ScheduledTask>, ScheduledTask>
    {
        private ScheduledTask _sampleType;

        [SetUp]
        public void SetUp()
        {
            _sampleType = Builder<ScheduledTask>
                    .CreateNew()
                    .With(s => s.Id = 0)
                    .Build();
        }

        [Test]
        public async Task should_be_able_to_write_to_database()
        {
            await Subject.InsertAsync(_sampleType);
            (await Db.AllAsync<ScheduledTask>()).Should().HaveCount(1);
        }

        [Test]
        public async Task double_insert_should_fail()
        {
            await Subject.InsertAsync(_sampleType);
            Assert.ThrowsAsync<InvalidOperationException>(() => Subject.InsertAsync(_sampleType));
        }

        [Test]
        public void update_item_with_root_index_0_should_faile()
        {
            _sampleType.Id = 0;
            Assert.ThrowsAsync<InvalidOperationException>(() => Subject.UpdateAsync(_sampleType));
        }

        [Test]
        public async Task should_be_able_to_store_empty_list()
        {
            var series = new List<ScheduledTask>();

            await Subject.InsertManyAsync(series);
        }

        [Test]
        public async Task new_objects_should_get_id()
        {
            _sampleType.Id = 0;
            await Subject.InsertAsync(_sampleType);
            _sampleType.Id.Should().NotBe(0);
        }

        [Test]
        public async Task new_object_should_get_new_id()
        {
            _sampleType.Id = 0;
            await Subject.InsertAsync(_sampleType);

            (await Db.AllAsync<ScheduledTask>()).Should().HaveCount(1);
            _sampleType.Id.Should().Be(1);
        }

        [Test]
        public async Task should_read_and_write_in_utc()
        {
            var storedTime = DateTime.UtcNow;

            _sampleType.LastExecution = storedTime;

            await Subject.InsertAsync(_sampleType);

            StoredModel.LastExecution.Kind.Should().Be(DateTimeKind.Utc);
            StoredModel.LastExecution.ToLongTimeString().Should().Be(storedTime.ToLongTimeString());
        }

        [Test]
        public async Task should_convert_all_dates_to_utc()
        {
            var storedTime = DateTime.Now;

            _sampleType.LastExecution = storedTime;

            await Subject.InsertAsync(_sampleType);

            StoredModel.LastExecution.Kind.Should().Be(DateTimeKind.Utc);
            StoredModel.LastExecution.ToLongTimeString().Should().Be(storedTime.ToUniversalTime().ToLongTimeString());
        }

        [Test]
        public async Task should_have_id_when_returned_from_database()
        {
            _sampleType.Id = 0;
            await Subject.InsertAsync(_sampleType);
            var item = await Db.AllAsync<ScheduledTask>();

            item.Should().HaveCount(1);
            item.First().Id.Should().NotBe(0);
            item.First().Id.Should().BeLessThan(100);
            item.First().Id.Should().Be(_sampleType.Id);
        }

        [Test]
        public async Task should_be_able_to_find_object_by_id()
        {
            await Subject.InsertAsync(_sampleType);
            var item = (await Db.AllAsync<ScheduledTask>()).Single(c => c.Id == _sampleType.Id);

            item.Id.Should().NotBe(0);
            item.Id.Should().Be(_sampleType.Id);
        }

        [Test]
        public async Task set_fields_should_only_update_selected_filed()
        {
            var childModel = new ScheduledTask
            {
                TypeName = "Address",
                Interval = 12
            };

            await Subject.InsertAsync(childModel);

            childModel.TypeName = "A";
            childModel.Interval = 0;

            await Subject.SetFieldsAsync(childModel, t => t.TypeName);

            (await Db.AllAsync<ScheduledTask>()).Single().TypeName.Should().Be("A");
            (await Db.AllAsync<ScheduledTask>()).Single().Interval.Should().Be(12);
        }
    }
}
