using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesBetweenDatesFixture : DbTest<EpisodeRepository, Episode>
    {
        [SetUp]
        public void Setup()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.Runtime = 30)
                                        .Build();

            series.Id = Db.Insert(series).Id;

            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.Id = 0)
                                          .With(e => e.SeriesId = series.Id)
                                          .Build();

            Db.Insert(episode);
        }

        [Test]
        public void should_get_episodes()
        {
            var episodes = Subject.EpisodesBetweenDates(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(3));
            episodes.Should().HaveCount(1);
        }
    }
}
