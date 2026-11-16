using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupDuplicateExtraFilesFixture : DbTest<CleanupDuplicateExtraFiles, OtherExtraFile>
    {
        [Test]
        public void should_not_delete_extra_files_with_different_paths()
        {
            var files = Builder<OtherExtraFile>.CreateListOfSize(2)
                                               .All()
                                               .With(m => m.SeriesId = 1)
                                               .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_not_delete_extra_files_with_the_same_path_for_different_series()
        {
            var files = Builder<OtherExtraFile>.CreateListOfSize(2)
                                               .All()
                                               .With(m => m.RelativePath = "Season 1/Series Title - S01E01.nfo-orig")
                                               .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_keep_only_the_newest_extra_file_with_the_same_series_and_path()
        {
            var files = Builder<OtherExtraFile>.CreateListOfSize(3)
                                               .All()
                                               .With(m => m.SeriesId = 1)
                                               .With(m => m.RelativePath = "Season 1/Series Title - S01E01.nfo-orig")
                                               .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Single().Id.Should().Be(files.Max(f => f.Id));
        }
    }
}
