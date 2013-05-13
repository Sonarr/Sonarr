using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesRepositoryReadFixture : DbTest<EpisodeRepository, Episode>
    {
        private Series series;

        [SetUp]
        public void Setup()
        {
            series = Builder<Series>.CreateNew()
                                        .With(s => s.Runtime = 30)
                                        .BuildNew();

            Db.Insert(series);
        }

        [Test]
        public void should_get_episodes_by_file()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew().BuildNew();

            Db.Insert(episodeFile);

            var episode = Builder<Episode>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.SeriesId = series.Id)
                                        .With(e => e.EpisodeFileId = episodeFile.Id)
                                        .BuildListOfNew();

            Db.InsertMany(episode);

            var episodes = Subject.GetEpisodeByFileId(episodeFile.Id);
            episodes.Should().HaveCount(2);
        }
    }
}
