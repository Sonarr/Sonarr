using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Subtitles;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedSubtitleFilesFixture : DbTest<CleanupOrphanedSubtitleFiles, SubtitleFile>
    {
        [Test]
        public async Task should_delete_subtitle_files_that_dont_have_a_coresponding_series()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            await Db.InsertAsync(episodeFile);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                    .With(m => m.EpisodeFileId = episodeFile.Id)
                                                    .With(m => m.Language = Language.English)
                                                    .BuildNew();

            await Db.InsertAsync(subtitleFile);
            Subject.Clean();
            var subtitleFiles = await GetAllStoredModelsAsync();
            subtitleFiles.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_subtitle_files_that_have_a_coresponding_series()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            await Db.InsertAsync(series);
            await Db.InsertAsync(episodeFile);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                    .With(m => m.SeriesId = series.Id)
                                                    .With(m => m.EpisodeFileId = episodeFile.Id)
                                                    .With(m => m.Language = Language.English)
                                                    .BuildNew();

            await Db.InsertAsync(subtitleFile);
            Subject.Clean();
            var subtitleFiles = await GetAllStoredModelsAsync();
            subtitleFiles.Should().HaveCount(1);
        }

        [Test]
        public async Task should_delete_subtitle_files_that_dont_have_a_coresponding_episode_file()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            await Db.InsertAsync(series);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                    .With(m => m.SeriesId = series.Id)
                                                    .With(m => m.EpisodeFileId = 10)
                                                    .With(m => m.Language = Language.English)
                                                    .BuildNew();

            await Db.InsertAsync(subtitleFile);
            Subject.Clean();
            var subtitleFiles = await GetAllStoredModelsAsync();
            subtitleFiles.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_subtitle_files_that_have_a_coresponding_episode_file()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            await Db.InsertAsync(series);
            await Db.InsertAsync(episodeFile);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                    .With(m => m.SeriesId = series.Id)
                                                    .With(m => m.EpisodeFileId = episodeFile.Id)
                                                    .With(m => m.Language = Language.English)
                                                    .BuildNew();

            await Db.InsertAsync(subtitleFile);
            Subject.Clean();
            var subtitleFiles = await GetAllStoredModelsAsync();
            subtitleFiles.Should().HaveCount(1);
        }

        [Test]
        public async Task should_delete_subtitle_files_that_have_episodefileid_of_zero()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            await Db.InsertAsync(series);

            var subtitleFile = Builder<SubtitleFile>.CreateNew()
                                                 .With(m => m.SeriesId = series.Id)
                                                 .With(m => m.EpisodeFileId = 0)
                                                 .With(m => m.Language = Language.English)
                                                 .BuildNew();

            await Db.InsertAsync(subtitleFile);
            Subject.Clean();
            var subtitleFiles = await GetAllStoredModelsAsync();
            subtitleFiles.Should().HaveCount(0);
        }
    }
}
