using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class IndexProviderFixture : ObjectDbTest<IndexProvider>
    {
        [SetUp]
        public void Setup()
        {
            WithObjectDb();
        }

        [Test]
        public void should_be_able_to_get_sequential_numbers()
        {
            var indexs = new List<int>();


            for (var i = 0; i < 1000; i++)
            {
                indexs.Add(Subject.Next(GetType()));
            }

            indexs.Should().OnlyHaveUniqueItems();
        }



        [Test]
        public void diffrentTypes_should_get_their_own_counter()
        {
            var seriesIndex = new List<int>();
            var episodeIndex = new List<int>();


            for (var i = 0; i < 200; i++)
            {
                seriesIndex.Add(Subject.Next(typeof(Series)));
            }

            for (var i = 0; i < 100; i++)
            {
                episodeIndex.Add(Subject.Next(typeof(Episode)));
            }
            
            seriesIndex.Should().OnlyHaveUniqueItems();
            episodeIndex.Should().OnlyHaveUniqueItems();

            seriesIndex.Min(c => c).Should().Be(1);
            seriesIndex.Max(c => c).Should().Be(200);

            episodeIndex.Min(c => c).Should().Be(1);
            episodeIndex.Max(c => c).Should().Be(100);
        }

    }
}



