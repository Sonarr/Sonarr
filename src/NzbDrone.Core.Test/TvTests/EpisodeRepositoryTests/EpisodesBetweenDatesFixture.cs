using System;
using System.Threading.Tasks;
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
        public async Task Setup()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.Runtime = 30)
                                        .With(s => s.Monitored = true)
                                        .Build();

            series.Id = (await Db.InsertAsync(series)).Id;

            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.Id = 0)
                                          .With(e => e.SeriesId = series.Id)
                                          .With(e => e.Monitored = true)
                                          .Build();

            await Db.InsertAsync(episode);
        }

        [Test]
        public async Task should_get_episodes()
        {
            var episodes = await Subject.EpisodesBetweenDatesAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(3), false, true);
            episodes.Should().HaveCount(1);
        }
    }
}
