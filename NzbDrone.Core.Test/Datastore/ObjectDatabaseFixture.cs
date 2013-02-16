using System;
using System.Collections.Generic;
using System.Linq;
using Eloquera.Client;
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
        private Series testSeries;
        private Episode testEpisode;

        [SetUp]
        public void SetUp()
        {
            WithObjectDb();

            testSeries = Builder<Series>
                    .CreateNew()
                    .With(s => s.Id = 0)
                    .Build();

            testEpisode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.Id = 0)
                    .Build();


        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Db.Insert(testSeries);
            Db.AsQueryable<Series>().Should().HaveCount(1);
        }

        [Test]
        public void double_insert_should_fail()
        {
            Db.Insert(testSeries);
            Assert.Throws<InvalidOperationException>(() => Db.Insert(testSeries));
        }

        [Test]
        public void update_item_with_root_index_0_should_faile()
        {
            testSeries.Id = 0;
            Assert.Throws<InvalidOperationException>(() => Db.Update(testSeries));
        }


        [Test]
        public void should_be_able_to_store_empty_list()
        {
            var series = new List<Series>();

            Db.InsertMany(series);
        }

        [Test]
        public void should_not_store_dirty_data_in_cache()
        {
            Db.Insert(testEpisode);

            Db.AsQueryable<Episode>().Single().Series.Should().BeNull();

            testEpisode.Series = Builder<Series>.CreateNew().Build();

            Db.AsQueryable<Episode>().Single().Series.Should().BeNull();
        }

        [Test]
        public void should_store_nested_objects()
        {
            testEpisode.Series = testSeries;

            Db.Insert(testEpisode);

            Db.AsQueryable<Episode>().Should().HaveCount(1);
            Db.AsQueryable<Episode>().Single().Series.Should().NotBeNull();
        }

        [Test]
        public void should_update_nested_objects()
        {
            testEpisode.Series = Builder<Series>
                                    .CreateNew()
                                    .With(s => s.Id = 0)
                                    .Build();

            Db.Insert(testEpisode);

            testEpisode.Series.Title = "UpdatedTitle";

            Db.Update(testEpisode);

            Db.AsQueryable<Episode>().Should().HaveCount(1);
            Db.AsQueryable<Episode>().Single().Series.Should().NotBeNull();
            Db.AsQueryable<Episode>().Single().Series.Title.Should().Be("UpdatedTitle");
        }

        [Test]
        public void new_objects_should_get_id()
        {
            testSeries.Id = 0;
            Db.Insert(testSeries);
            testSeries.Id.Should().NotBe(0);
        }

        [Test]
        public void new_object_should_get_new_id()
        {
            testSeries.Id = 0;
            Db.Insert(testSeries);

            Db.AsQueryable<Series>().Should().HaveCount(1);
            testSeries.Id.Should().Be(1);
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
            testSeries.Id = 0;
            Db.Insert(testSeries);
            var item = Db.AsQueryable<Series>();

            item.Should().HaveCount(1);
            item.First().Id.Should().NotBe(0);
            item.First().Id.Should().BeLessThan(100);
            item.First().Id.Should().Be(testSeries.Id);
        }

        [Test]
        public void should_be_able_to_find_object_by_id()
        {
            Db.Insert(testSeries);
            var item = Db.AsQueryable<Series>().Single(c => c.Id == testSeries.Id);

            item.Id.Should().NotBe(0);
            item.Id.Should().Be(testSeries.Id);
        }

        [Test]
        public void should_be_able_to_read_unknown_type()
        {
            Db.AsQueryable<UnknownType>().ToList().Should().BeEmpty();
        }
    }

    public class UnknownType : BaseRepositoryModel
    {
        public string Field1 { get; set; }
    }

    public class NestedModel : BaseRepositoryModel
    {
        public NestedModel()
        {
            List = new List<NestedModel> { this };
        }

        public IList<NestedModel> List { get; set; }
    }
}

