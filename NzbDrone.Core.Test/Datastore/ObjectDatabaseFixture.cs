using System.Linq;
using Eloquera.Client;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
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

            testSeries = Builder<Series>.CreateNew().Build();
            testEpisode = Builder<Episode>.CreateNew().Build();


        }

        [Test]
        public void should_be_able_to_write_to_database()
        {
            Db.Insert(testSeries);

            Db.AsQueryable<Series>().Should().HaveCount(1);

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
            testEpisode.Series = Builder<Series>.CreateNew().Build();

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
            Db.Insert(testSeries);
            testSeries.Id.Should().NotBe(0);
        }

        [Test]
        public void should_be_able_to_read_unknow_type()
        {
            Db.AsQueryable<UnknownType>().ToList().Should().BeEmpty();
        }
    }

    public class UnknownType
    {
        [ID]
        public string Id;
        public string Field1 { get; set; }
    }
}

