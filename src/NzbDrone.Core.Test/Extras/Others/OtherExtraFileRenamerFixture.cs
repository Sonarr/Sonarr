using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Extras.Others;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Extras.Others
{
    [TestFixture]
    public class OtherExtraFileRenamerFixture : DbTest<OtherExtraFileRenamer, OtherExtraFile>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.SetConstant<IExtraFileRepository<OtherExtraFile>>(Mocker.Resolve<ExtraFileRepository<OtherExtraFile>>());
            Mocker.SetConstant<IOtherExtraFileService>(Mocker.Resolve<OtherExtraFileService>());
        }

        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(1, false)]
        public void should_replace_all_destination_rows(int destinationRows, bool destinationExists)
        {
            var series = new Series { Id = 1, Path = @"C:\Test\TV\Series".AsOsAgnostic() };
            var relativePath = Path.Combine("Season 1", "Episode.nfo");
            var path = Path.Combine(series.Path, relativePath);
            var source = Db.Insert(new OtherExtraFile { SeriesId = series.Id, EpisodeFileId = 1, SeasonNumber = 1, RelativePath = relativePath, Extension = ".nfo" });

            for (var i = 0; i < destinationRows; i++)
            {
                Db.Insert(new OtherExtraFile { SeriesId = series.Id, EpisodeFileId = 1, SeasonNumber = 1, RelativePath = relativePath + "-orig", Extension = ".nfo-orig" });
            }

            Mocker.GetMock<IDiskProvider>().Setup(d => d.FileExists(path)).Returns(true);
            Mocker.GetMock<IDiskProvider>().Setup(d => d.FileExists(path + "-orig")).Returns(destinationExists);

            Subject.RenameOtherExtraFile(series, path);

            var stored = AllStoredModels.Should().ContainSingle().Which;
            stored.Id.Should().Be(source.Id);
            stored.RelativePath.Should().Be(relativePath + "-orig");
            stored.Extension.Should().Be(".nfo-orig");
            Mocker.GetMock<IRecycleBinProvider>()
                  .Verify(r => r.DeleteFile(path + "-orig", "Season 1"), Times.Exactly(destinationExists ? 1 : 0));
        }
    }
}
