using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using Db4objects.Db4o.Linq;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class ObjectDatabaseFixture : CoreTest
    {
        [SetUp]
        public void SetUp()
        {
            WithObjectDb(false);
        }

        [Test]
        public void should_be_able_to_write_to_database()
        {

            var series = Builder<Series>.CreateNew().Build();

            ObjDb.Create(series);

            ObjDb.Ext().Purge();

            ObjDb.AsQueryable<Series>().Should().HaveCount(1);

        }

        [Test]
        public void should_not_store_dirty_data_in_cache()
        {
            var episode = Builder<Episode>.CreateNew().Build();
            
            //Save series without episode attached
            ObjDb.Create(episode);
            
            ObjDb.AsQueryable<Episode>().Single().Series.Should().BeNull();
            
            episode.Series = Builder<Series>.CreateNew().Build();

            ObjDb.AsQueryable<Episode>().Single().Series.Should().BeNull();
        }


        [Test]
        public void rollback_should_reset_state()
        {
            var episode = Builder<Episode>.CreateNew().Build();
            
            ObjDb.Create(episode);

            ObjDb.AsQueryable<Episode>().Should().HaveCount(1);

            ObjDb.Rollback();

            ObjDb.AsQueryable<Episode>().Should().HaveCount(0);
        }

        [Test]
        public void roolback_should_only_roll_back_what_is_not_commited()
        {
            var episode = Builder<Episode>.CreateNew().Build();
            var series = Builder<Series>.CreateNew().Build();

            ObjDb.Create(episode);

            ObjDb.Commit();

            ObjDb.Create(series);

            ObjDb.Rollback();

            ObjDb.AsQueryable<Episode>().Should().HaveCount(1);
            ObjDb.AsQueryable<Series>().Should().HaveCount(0);
        }


        [Test]
        public void should_store_nested_objects()
        {
            var episode = Builder<Episode>.CreateNew().Build();
            episode.Series = Builder<Series>.CreateNew().Build();

            ObjDb.Create(episode);

            ObjDb.AsQueryable<Episode>().Should().HaveCount(1);
            ObjDb.AsQueryable<Episode>().Single().Series.Should().NotBeNull();
        }

        [Test]
        public void should_update_nested_objects()
        {
            var episode = Builder<Episode>.CreateNew().Build();
            episode.Series = Builder<Series>.CreateNew().Build();

            ObjDb.Create(episode);

            episode.Series.Title = "UpdatedTitle";

            ObjDb.Update(episode,2);

            ObjDb.AsQueryable<Episode>().Should().HaveCount(1);
            ObjDb.AsQueryable<Episode>().Single().Series.Should().NotBeNull();
            ObjDb.AsQueryable<Episode>().Single().Series.Title.Should().Be("UpdatedTitle");
        }
    }
}
