using System.Collections.Generic;
using System.Linq;
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
    public class EpisodesWithFilesFixture : DbTest<EpisodeRepository, Episode>
    {
        private const int SERIES_ID = 1;
        private List<Episode> _episodes;
        private List<EpisodeFile> _episodeFiles;

        [SetUp]
        public void Setup()
        {
            _episodeFiles = Builder<EpisodeFile>.CreateListOfSize(5)
                                                .All()
                                                .With(c => c.Quality = new QualityModel())
                                                .With(c => c.Language = Language.English)
                                                .BuildListOfNew();

            Db.InsertMany(_episodeFiles);

            _episodes = Builder<Episode>.CreateListOfSize(10)
                                        .All()
                                        .With(e => e.EpisodeFileId = 0)
                                        .With(e => e.SeriesId = SERIES_ID)
                                        .BuildListOfNew()
                                        .ToList();

            for (int i = 0; i < _episodeFiles.Count; i++)
            {
                _episodes[i].EpisodeFileId = _episodeFiles[i].Id;
            }

            Db.InsertMany(_episodes);
        }

        [Test]
        public void should_only_get_files_that_have_episode_files()
        {
            var result = Subject.EpisodesWithFiles(SERIES_ID);

            result.Should().OnlyContain(e => e.EpisodeFileId > 0);
            result.Should().HaveCount(_episodeFiles.Count);
        }

        [Test]
        public void should_only_contain_episodes_for_the_given_series()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                                                  .With(f => f.RelativePath = "another path")
                                                  .With(c => c.Quality = new QualityModel())
                                                  .With(c => c.Language = Language.English)
                                                  .BuildNew();

            Db.Insert(episodeFile);

            var episode = Builder<Episode>.CreateNew()
                                          .With(e => e.SeriesId = SERIES_ID + 10)
                                          .With(e => e.EpisodeFileId = episodeFile.Id)
                                          .BuildNew();

            Db.Insert(episode);

            Subject.EpisodesWithFiles(episode.SeriesId).Should().OnlyContain(e => e.SeriesId == episode.SeriesId);
        }

        [Test]
        public void should_have_episode_file_loaded()
        {
            Subject.EpisodesWithFiles(SERIES_ID).Should().OnlyContain(e => e.EpisodeFile.IsLoaded);
        }
    }
}
