using System.Linq;
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
        [SetUp]
        public void SetUp()
        {
            WithObjectDb();
        }

        [Test]
        public void should_be_able_to_write_to_database()
        {

            var series = Builder<Series>.CreateNew().Build();

            Db.Create(series);


            Db.AsQueryable<Series>().Should().HaveCount(1);

        }

        [Test]
        public void should_not_store_dirty_data_in_cache()
        {
            var episode = Builder<Episode>.CreateNew().Build();

            //Save series without episode attached
            Db.Create(episode);

            Db.AsQueryable<Episode>().Single().Series.Should().BeNull();

            episode.Series = Builder<Series>.CreateNew().Build();

            Db.AsQueryable<Episode>().Single().Series.Should().BeNull();
        }



        [Test]
        public void should_store_nested_objects()
        {
            var episode = Builder<Episode>.CreateNew().Build();
            episode.Series = Builder<Series>.CreateNew().Build();

            Db.Create(episode);

            Db.AsQueryable<Episode>().Should().HaveCount(1);
            Db.AsQueryable<Episode>().Single().Series.Should().NotBeNull();
        }

        [Test]
        public void should_update_nested_objects()
        {
            var episode = Builder<Episode>.CreateNew().Build();
            episode.Series = Builder<Series>.CreateNew().Build();

            Db.Create(episode);

            episode.Series.Title = "UpdatedTitle";

            Db.Update(episode);

            Db.AsQueryable<Episode>().Should().HaveCount(1);
            Db.AsQueryable<Episode>().Single().Series.Should().NotBeNull();
            Db.AsQueryable<Episode>().Single().Series.Title.Should().Be("UpdatedTitle");
        }
    }
}
