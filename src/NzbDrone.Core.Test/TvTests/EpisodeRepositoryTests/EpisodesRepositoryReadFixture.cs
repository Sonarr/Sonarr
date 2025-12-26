using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.EpisodeRepositoryTests
{
    [TestFixture]
    public class EpisodesRepositoryReadFixture : DbTest<EpisodeRepository, Episode>
    {
        private Series _series;

        [SetUp]
        public async Task Setup()
        {
            _series = Builder<Series>.CreateNew()
                                        .With(s => s.Runtime = 30)
                                        .BuildNew();

            await Db.InsertAsync(_series);
        }

        [Test]
        public async Task should_get_episodes_by_file()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            await Db.InsertAsync(episodeFile);

            var episode = Builder<Episode>.CreateListOfSize(2)
                                        .All()
                                        .With(e => e.SeriesId = _series.Id)
                                        .With(e => e.EpisodeFileId = episodeFile.Id)
                                        .BuildListOfNew();

            await Db.InsertManyAsync(episode);

            var episodes = await Subject.GetEpisodeByFileIdAsync(episodeFile.Id);
            episodes.Should().HaveCount(2);
        }
    }
}
