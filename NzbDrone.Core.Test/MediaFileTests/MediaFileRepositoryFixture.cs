using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFileTests
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
                .Random(4)
                .With(s => s.SeriesId = 12)
                .Build();


            Db.InsertMany(files);

            var seriesFiles = Subject.GetFilesBySeries(12);

            seriesFiles.Should().HaveCount(4);
            seriesFiles.Should().OnlyContain(c => c.SeriesId == 12);

        }

        [Test]
        public void get_files_by_season()
        {
            var files = Builder<EpisodeFile>.CreateListOfSize(20)
                   .All()
                   .With(c => c.Id = 0)
                   .With(s => s.SeasonNumber = 10)
                   .TheFirst(10)
                   .With(c => c.SeriesId = 1)
                   .TheNext(10)
                   .With(c => c.SeriesId = 2)
                   .Random(10)
                   .With(s => s.SeasonNumber = 20)
                   .Build();


            Db.InsertMany(files);


            Subject.GetFilesBySeason(1, 20).Should().OnlyContain(c => c.SeriesId == 1 && c.SeasonNumber == 20);
        }


        [Test]
        public void GetFileByPath_should_return_null_if_file_does_not_exist_in_database()
        {
            Subject.GetFileByPath(@"C:\Test\EpisodeFile.avi").Should().BeNull();
        }

        [Test]
        public void GetFileByPath_should_return_EpisodeFile_if_file_exists_in_database()
        {
            var path = @"C:\Test\EpisodeFile.avi";

            var episodeFile = Builder<EpisodeFile>.CreateNew()
                    .With(f => f.Id = 0)
                    .With(f => f.Path = path.NormalizePath())
                    .Build();

           Subject.Insert(episodeFile);

            var file = Subject.GetFileByPath(path);

            //Resolve
            file.Should().NotBeNull();
            file.Path.Should().Be(path.NormalizePath());
        }
    }
}