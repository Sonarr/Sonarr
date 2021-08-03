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
        public void should_be_able_to_write_to_database()
        {
            Subject.Insert(_sampleType);
            Db.All<ScheduledTask>().Should().HaveCount(1);
        }

        [Test]
        public void double_insert_should_fail()
        {
            Subject.Insert(_sampleType);
            Assert.Throws<InvalidOperationException>(() => Subject.Insert(_sampleType));
        }

        [Test]
        public void update_item_with_root_index_0_should_faile()
        {
            _sampleType.Id = 0;
            Assert.Throws<InvalidOperationException>(() => Subject.Update(_sampleType));
        }

        [Test]
        public void should_be_able_to_store_empty_list()
        {
            var series = new List<ScheduledTask>();

            Subject.InsertMany(series);
        }

        [Test]
        public void new_objects_should_get_id()
        {
            _sampleType.Id = 0;
            Subject.Insert(_sampleType);
            _sampleType.Id.Should().NotBe(0);
        }

        [Test]
        public void new_object_should_get_new_id()
        {
            _sampleType.Id = 0;
            Subject.Insert(_sampleType);

            Db.All<ScheduledTask>().Should().HaveCount(1);
            _sampleType.Id.Should().Be(1);
        }

        [Test]
        public void should_read_and_write_in_utc()
        {
            var storedTime = DateTime.UtcNow;

            _sampleType.LastExecution = storedTime;

            Subject.Insert(_sampleType);

            StoredModel.LastExecution.Kind.Should().Be(DateTimeKind.Utc);
            StoredModel.LastExecution.ToLongTimeString().Should().Be(storedTime.ToLongTimeString());
        }

        [Test]
        public void should_convert_all_dates_to_utc()
        {
            var storedTime = DateTime.Now;

            _sampleType.LastExecution = storedTime;

            Subject.Insert(_sampleType);

            StoredModel.LastExecution.Kind.Should().Be(DateTimeKind.Utc);
            StoredModel.LastExecution.ToLongTimeString().Should().Be(storedTime.ToUniversalTime().ToLongTimeString());
        }

        [Test]
        public void should_have_id_when_returned_from_database()
        {
            _sampleType.Id = 0;
            Subject.Insert(_sampleType);
            var item = Db.All<ScheduledTask>();

            item.Should().HaveCount(1);
            item.First().Id.Should().NotBe(0);
            item.First().Id.Should().BeLessThan(100);
            item.First().Id.Should().Be(_sampleType.Id);
        }

        [Test]
        public void should_be_able_to_find_object_by_id()
        {
            Subject.Insert(_sampleType);
            var item = Db.All<ScheduledTask>().Single(c => c.Id == _sampleType.Id);

            item.Id.Should().NotBe(0);
            item.Id.Should().Be(_sampleType.Id);
        }

        [Test]
        public void set_fields_should_only_update_selected_filed()
        {
            var childModel = new ScheduledTask
                {
                    TypeName = "Address",
                    Interval = 12
                };

            Subject.Insert(childModel);

            childModel.TypeName = "A";
            childModel.Interval = 0;

            Subject.SetFields(childModel, t => t.TypeName);

            Db.All<ScheduledTask>().Single().TypeName.Should().Be("A");
            Db.All<ScheduledTask>().Single().Interval.Should().Be(12);
        }
    }
}
