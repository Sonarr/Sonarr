using System.Linq;
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
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
            WithObjectDb();
        }

        [Test]
        public void should_be_able_to_write_to_database()
        {

            var series = Builder<Series>.CreateNew().Build();

            ObjDb.Save(series);

            ObjDb.Ext().Purge();

            ObjDb.AsQueryable<Series>().Should().HaveCount(1);

        }

        [Test]
        public void should_not_store_dirty_data_in_cache()
        {
            var episode = Builder<Episode>.CreateNew().Build();
            
            //Save series without episode attached
            ObjDb.Save(episode);
            
            ObjDb.AsQueryable<Episode>().Single().Series.Should().BeNull();
            
            episode.Series = Builder<Series>.CreateNew().Build();

            ObjDb.AsQueryable<Episode>().Single().Series.Should().BeNull();

        }
    }
}
