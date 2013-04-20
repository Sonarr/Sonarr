using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesWithoutFilesFixture : DbTest<EpisodeRepository, Episode>
    {
        [SetUp]
        public void Setup()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(s => s.Id = 0)
                                        .With(s => s.Runtime = 30)
                                        .Build();

            series.Id = Db.Insert(series).Id;

            var episodes = Builder<Episode>.CreateListOfSize(2)
                                           .All()
                                           .With(e => e.Id = 0)
                                           .With(e => e.SeriesId = series.Id)
                                           .With(e => e.EpisodeFileId = 0)
                                           .TheLast(1)
                                           .With(e => e.SeasonNumber = 0)
                                           .Build();

            Db.InsertMany(episodes);
        }

        [Test]
        public void should_get_episodes()
        {
            var episodes = Subject.EpisodesWithoutFiles(false);
            episodes.Should().HaveCount(1);
        }

        [Test]
        public void should_get_episode_including_specials()
        {
            var episodes = Subject.EpisodesWithoutFiles(true);
            episodes.Should().HaveCount(2);
        }
    }
}
