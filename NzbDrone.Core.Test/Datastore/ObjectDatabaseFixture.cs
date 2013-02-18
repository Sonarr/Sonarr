using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class ObjectDatabaseFixture : ObjectDbTest
    {
        private ChildModel childModel;
        private ParentModel ParentModel;

        [SetUp]
        public void SetUp()
        {
            WithObjectDb(memory:false);

            childModel = Builder<ChildModel>
                    .CreateNew()
                    .With(s => s.OID = 0)
                    .Build();

            ParentModel = Builder<ParentModel>
                    .CreateNew()
                    .With(e => e.OID = 0)
                    .Build();


        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Db.Insert(childModel);
            Db.AsQueryable<ChildModel>().Should().HaveCount(1);
        }

        [Test]
        public void double_insert_should_fail()
        {
            Db.Insert(childModel);
            Assert.Throws<InvalidOperationException>(() => Db.Insert(childModel));
        }

        [Test]
        public void update_item_with_root_index_0_should_faile()
        {
            childModel.OID = 0;
            Assert.Throws<InvalidOperationException>(() => Db.Update(childModel));
        }


        [Test]
        public void should_be_able_to_store_empty_list()
        {
            var series = new List<ParentModel>();

            Db.InsertMany(series);
        }

        [Test]
        public void should_not_store_dirty_data_in_cache()
        {
            Db.Insert(ParentModel);

            Db.AsQueryable<ParentModel>().Single().Child.Should().BeNull();

            ParentModel.Child = Builder<ChildModel>.CreateNew().Build();

            Db.AsQueryable<ParentModel>().Single().Child.Should().BeNull();
        }

        [Test]
        public void should_store_nested_objects()
        {
            ParentModel.Child = childModel;

            Db.Insert(ParentModel);

            Db.AsQueryable<ParentModel>().Should().HaveCount(1);
            Db.AsQueryable<ParentModel>().Single().Child.Should().NotBeNull();
        }

        [Test]
        public void should_update_nested_objects()
        {
            ParentModel.Child = Builder<ChildModel>
                                    .CreateNew()
                                    .With(s => s.OID = 0)
                                    .Build();

            Db.Insert(ParentModel);

            ParentModel.Child.A = "UpdatedTitle";

            Db.Update(ParentModel);

            Db.AsQueryable<ParentModel>().Should().HaveCount(1);
            Db.AsQueryable<ParentModel>().Single().Child.Should().NotBeNull();
            Db.AsQueryable<ParentModel>().Single().Child.A.Should().Be("UpdatedTitle");
        }

        [Test]
        public void new_objects_should_get_id()
        {
            childModel.OID = 0;
            Db.Insert(childModel);
            childModel.OID.Should().NotBe(0);
        }

        [Test]
        public void new_object_should_get_new_id()
        {
            childModel.OID = 0;
            Db.Insert(childModel);

            Db.AsQueryable<ChildModel>().Should().HaveCount(1);
            childModel.OID.Should().Be(1);
        }


        [Test]
        public void should_be_able_to_assign_ids_to_nested_objects()
        {
            var nested = new NestedModel();

            nested.List.Add(new NestedModel());

            Db.Insert(nested);

            nested.OID.Should().Be(1);
            nested.List.Should().OnlyContain(c => c.OID > 0);
        }

        [Test]
        public void should_have_id_when_returned_from_database()
        {
            childModel.OID = 0;
            Db.Insert(childModel);
            var item = Db.AsQueryable<ChildModel>();

            item.Should().HaveCount(1);
            item.First().OID.Should().NotBe(0);
            item.First().OID.Should().BeLessThan(100);
            item.First().OID.Should().Be(childModel.OID);
        }

        [Test]
        public void should_be_able_to_find_object_by_id()
        {
            Db.Insert(childModel);
            var item = Db.AsQueryable<ChildModel>().Single(c => c.OID == childModel.OID);

            item.OID.Should().NotBe(0);
            item.OID.Should().Be(childModel.OID);
        }

        [Test]
        public void should_be_able_to_read_unknown_type()
        {
            Db.AsQueryable<UnknownType>().ToList().Should().BeEmpty();
        }
    }

    public class UnknownType : ModelBase
    {
        public string Field1 { get; set; }
    }

    public class NestedModel : ModelBase
    {
        public NestedModel()
        {
            List = new List<NestedModel> { this };
        }

        public List<NestedModel> List { get; set; }
    }

    public class ParentModel : ModelBase
    {
        public ChildModel Child { get; set; }
    }

    public class ChildModel : ModelBase
    {

        public String A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
    }
}

