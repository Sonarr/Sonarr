using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles
{
    [TestFixture]
    public class MediaFileRepositoryFixture : DbTest<MediaFileRepository, EpisodeFile>
    {
        [Test]
        public void get_files_by_series()
        {
            var files = Builder<EpisodeFile>.CreateListOfSize(10)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Language = Language.English)
                .With(c => c.Quality = new QualityModel(Quality.Bluray720p))
                .Random(4)
                .With(s => s.SeriesId = 12)
                .BuildListOfNew();

            Db.InsertMany(files);

            var seriesFiles = Subject.GetFilesBySeries(12);

            seriesFiles.Should().HaveCount(4);
            seriesFiles.Should().OnlyContain(c => c.SeriesId == 12);
        }
    }
}
