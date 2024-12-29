using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using Workarr.Languages;
using Workarr.MediaFiles;
using Workarr.Qualities;
using Workarr.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesRepositoryReadFixture : DbTest<EpisodeRepository, Episode>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                        .With(s => s.Runtime = 30)
                                        .BuildNew();

            Db.Insert(_series);
        }

        [Test]
        public void should_get_episodes_by_file()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            Db.Insert(episodeFile);

            var episode = Builder<Episode>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.SeriesId = _series.Id)
                                        .With(e => e.EpisodeFileId = episodeFile.Id)
                                        .BuildListOfNew();

            Db.InsertMany(episode);

            var episodes = Subject.GetEpisodeByFileId(episodeFile.Id);
            episodes.Should().HaveCount(2);
        }
    }
}
