using System.Collections.Generic;
using System.Threading.Tasks;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedExtraFilesFixture : DbTest<CleanupOrphanedExtraFiles, OtherExtraFile>
    {
        [Test]
        public async Task should_delete_extra_files_that_dont_have_a_coresponding_series()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            await Db.InsertAsync(episodeFile);

            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                    .With(m => m.EpisodeFileId = episodeFile.Id)
                                                    .BuildNew();

            await Db.InsertAsync(extraFile);
            Subject.Clean();
            var otherExtraFiles = await GetAllStoredModelsAsync();
            otherExtraFiles.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_extra_files_that_have_a_coresponding_series()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            await Db.InsertAsync(series);
            await Db.InsertAsync(episodeFile);

            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                    .With(m => m.SeriesId = series.Id)
                                                    .With(m => m.EpisodeFileId = episodeFile.Id)
                                                    .BuildNew();

            await Db.InsertAsync(extraFile);
            Subject.Clean();
            var otherExtraFiles = await GetAllStoredModelsAsync();
            otherExtraFiles.Should().HaveCount(1);
        }

        [Test]
        public async Task should_delete_extra_files_that_dont_have_a_coresponding_episode_file()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            await Db.InsertAsync(series);

            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                    .With(m => m.SeriesId = series.Id)
                                                    .With(m => m.EpisodeFileId = 10)
                                                    .BuildNew();

            await Db.InsertAsync(extraFile);
            Subject.Clean();
            var otherExtraFiles = await GetAllStoredModelsAsync();
            otherExtraFiles.Should().BeEmpty();
        }

        [Test]
        public async Task should_not_delete_extra_files_that_have_a_coresponding_episode_file()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(h => h.Quality = new QualityModel())
                .With(h => h.Languages = new List<Language> { Language.English })
                .BuildNew();

            await Db.InsertAsync(series);
            await Db.InsertAsync(episodeFile);

            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                    .With(m => m.SeriesId = series.Id)
                                                    .With(m => m.EpisodeFileId = episodeFile.Id)
                                                    .BuildNew();

            await Db.InsertAsync(extraFile);
            Subject.Clean();
            var otherExtraFiles = await GetAllStoredModelsAsync();
            otherExtraFiles.Should().HaveCount(1);
        }

        [Test]
        public async Task should_delete_extra_files_that_have_episodefileid_of_zero()
        {
            var series = Builder<Series>.CreateNew()
                                        .BuildNew();

            await Db.InsertAsync(series);

            var extraFile = Builder<OtherExtraFile>.CreateNew()
                                                 .With(m => m.SeriesId = series.Id)
                                                 .With(m => m.EpisodeFileId = 0)
                                                 .BuildNew();

            await Db.InsertAsync(extraFile);
            Subject.Clean();
            var otherExtraFiles = await GetAllStoredModelsAsync();
            otherExtraFiles.Should().HaveCount(0);
        }
    }
}
