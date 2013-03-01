using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class ObjectDatabaseFixture : ObjectDbTest
    {
        private ChildModel _childModel;
        private ParentModel _parentModel;

        [SetUp]
        public void SetUp()
        {
            _childModel = Builder<ChildModel>
                    .CreateNew()
                    .With(s => s.Id = 0)
                    .Build();

            _parentModel = Builder<ParentModel>
                    .CreateNew()
                    .With(e => e.Id = 0)
                    .Build();

        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Db.Insert(_childModel);
            Db.AsQueryable<ChildModel>().Should().HaveCount(1);
        }

        [Test]
        public void double_insert_should_fail()
        {
            Db.Insert(_childModel);
            Assert.Throws<InvalidOperationException>(() => Db.Insert(_childModel));
        }

        [Test]
        public void update_item_with_root_index_0_should_faile()
        {
            _childModel.Id = 0;
            Assert.Throws<InvalidOperationException>(() => Db.Update(_childModel));
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
            Db.Insert(_parentModel);

            Db.AsQueryable<ParentModel>().Single().Child.Should().BeNull();

            _parentModel.Child = Builder<ChildModel>.CreateNew().Build();

            Db.AsQueryable<ParentModel>().Single().Child.Should().BeNull();
        }

        [Test]
        public void should_store_nested_objects()
        {
            _parentModel.Child = _childModel;

            Db.Insert(_parentModel);

            Db.AsQueryable<ParentModel>().Should().HaveCount(1);
            Db.AsQueryable<ParentModel>().Single().Child.Should().NotBeNull();
        }

        [Test]
        public void should_update_nested_objects()
        {
            _parentModel.Child = Builder<ChildModel>
                                    .CreateNew()
                                    .With(s => s.Id = 0)
                                    .Build();

            Db.Insert(_parentModel);

            _parentModel.Child.A = "UpdatedTitle";

            Db.Update(_parentModel);

            Db.AsQueryable<ParentModel>().Should().HaveCount(1);
            Db.AsQueryable<ParentModel>().Single().Child.Should().NotBeNull();
            Db.AsQueryable<ParentModel>().Single().Child.A.Should().Be("UpdatedTitle");
        }

        [Test]
        public void new_objects_should_get_id()
        {
            _childModel.Id = 0;
            Db.Insert(_childModel);
            _childModel.Id.Should().NotBe(0);
        }

        [Test]
        public void new_object_should_get_new_id()
        {
            _childModel.Id = 0;
            Db.Insert(_childModel);

            Db.AsQueryable<ChildModel>().Should().HaveCount(1);
            _childModel.Id.Should().Be(1);
        }


        [Test]
        public void should_be_able_to_assign_ids_to_nested_objects()
        {
            var nested = new NestedModel();

            nested.List.Add(new NestedModel());

            Db.Insert(nested);

            nested.Id.Should().Be(1);
            nested.List.Should().OnlyContain(c => c.Id > 0);
        }

        [Test]
        public void should_have_id_when_returned_from_database()
        {
            _childModel.Id = 0;
            Db.Insert(_childModel);
            var item = Db.AsQueryable<ChildModel>();

            item.Should().HaveCount(1);
            item.First().Id.Should().NotBe(0);
            item.First().Id.Should().BeLessThan(100);
            item.First().Id.Should().Be(_childModel.Id);
        }

        [Test]
        public void should_be_able_to_find_object_by_id()
        {
            Db.Insert(_childModel);
            var item = Db.AsQueryable<ChildModel>().Single(c => c.Id == _childModel.Id);

            item.Id.Should().NotBe(0);
            item.Id.Should().Be(_childModel.Id);
        }

        [Test]
        public void deleting_child_model_directly_should_set_link_to_null()
        {
            _parentModel.Child = _childModel;

            Db.Insert(_childModel);
            Db.Insert(_parentModel);

            Db.AsQueryable<ParentModel>().Single().Child.Should().NotBeNull();

            Db.Delete(_childModel);

            Db.AsQueryable<ParentModel>().Single().Child.Should().BeNull();
        }

        [Test]
        public void deleting_child_model_directly_should_remove_item_from_child_list()
        {

            var children = Builder<ChildModel>.CreateListOfSize(5)
                                           .All()
                                           .With(c => c.Id = 0)
                                           .Build();

            _parentModel.ChildList = children.ToList();

            Db.Insert(_parentModel);

            Db.AsQueryable<ParentModel>().Single().ChildList.Should().HaveSameCount(children);

            Db.Delete(children[1]);

            Db.AsQueryable<ParentModel>().Single().ChildList.Should().HaveCount(4);
        }

        [Test]
        public void update_field_should_only_update_that_filed()
        {
            var childModel = new ChildModel
                {
                        A = "A_Original",
                        B = 1,
                        C = 1

                };

            Db.Insert(childModel);

            _childModel.A = "A_New";
            _childModel.B = 2;
            _childModel.C = 2;

            Db.UpdateField(childModel, "B");

            Db.AsQueryable<ChildModel>().Single().A.Should().Be("A_Original");
            Db.AsQueryable<ChildModel>().Single().B.Should().Be(2);
            Db.AsQueryable<ChildModel>().Single().C.Should().Be(1);
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
        public List<ChildModel> ChildList { get; set; }
    }

    public class ChildModel : ModelBase
    {
        public String A { get; set; }
        public int B { get; set; }
        public int C { get; set; }
    }
}

