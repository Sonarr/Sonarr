using System;
using System.Collections.Generic;
using System.Data;
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
    public class ObjectDatabaseFixture : DbTest<BasicRepository<JobDefinition>, JobDefinition>
    {
        private JobDefinition _sampleType;

        [SetUp]
        public void SetUp()
        {
            _sampleType = Builder<JobDefinition>
                    .CreateNew()
                    .With(s => s.Id = 0)
                    .Build();

        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Subject.Insert(_sampleType);
            Db.All<JobDefinition>().Should().HaveCount(1);
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
            var series = new List<JobDefinition>();

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

            Db.All<JobDefinition>().Should().HaveCount(1);
            _sampleType.Id.Should().Be(1);
        }




        [Test]
        public void should_have_id_when_returned_from_database()
        {
            _sampleType.Id = 0;
            Subject.Insert(_sampleType);
            var item = Db.All<JobDefinition>();

            item.Should().HaveCount(1);
            item.First().Id.Should().NotBe(0);
            item.First().Id.Should().BeLessThan(100);
            item.First().Id.Should().Be(_sampleType.Id);
        }

        [Test]
        public void should_be_able_to_find_object_by_id()
        {
            Subject.Insert(_sampleType);
            var item = Db.All<JobDefinition>().Single(c => c.Id == _sampleType.Id);

            item.Id.Should().NotBe(0);
            item.Id.Should().Be(_sampleType.Id);
        }


        [Test]
        public void set_fields_should_only_update_selected_filed()
        {
            var childModel = new JobDefinition
                {
                    Type = "Address",
                    Name = "Name",
                    Interval = 12

                };

            Subject.Insert(childModel);

            childModel.Type = "A";
            childModel.Name = "B";
            childModel.Interval = 0;

            Subject.SetFields(childModel, t => t.Name);

            Db.All<JobDefinition>().Single().Type.Should().Be("Address");
            Db.All<JobDefinition>().Single().Name.Should().Be("B");
            Db.All<JobDefinition>().Single().Interval.Should().Be(12);
        }
    }

}

