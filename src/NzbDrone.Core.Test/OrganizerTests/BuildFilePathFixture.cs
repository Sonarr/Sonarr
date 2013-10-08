using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class BuildFilePathFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig namingConfig;

        [SetUp]
        public void Setup()
        {
            namingConfig = new NamingConfig();


            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(namingConfig);
        }

        [Test]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "Season %0s", @"C:\Test\30 Rock\Season 01\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "Season %s", @"C:\Test\30 Rock\Season 1\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, false, "Season %0s", @"C:\Test\30 Rock\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, false, "Season %s", @"C:\Test\30 Rock\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S01E05 - Episode Title", 1, true, "ReallyUglySeasonFolder %s", @"C:\Test\30 Rock\ReallyUglySeasonFolder 1\30 Rock - S01E05 - Episode Title.mkv")]
        [TestCase("30 Rock - S00E05 - Episode Title", 0, true, "Season %s", @"C:\Test\30 Rock\Specials\30 Rock - S00E05 - Episode Title.mkv")]
        public void CalculateFilePath_SeasonFolder_SingleNumber(string filename, int seasonNumber, bool useSeasonFolder, string seasonFolderFormat, string expectedPath)
        {
            var fakeSeries = Builder<Series>.CreateNew()
                .With(s => s.Title = "30 Rock")
                .With(s => s.Path = @"C:\Test\30 Rock".AsOsAgnostic())
                .With(s => s.SeasonFolder = useSeasonFolder)
                .Build();

            Mocker.GetMock<IConfigService>().Setup(e => e.SeasonFolderFormat).Returns(seasonFolderFormat);

            Subject.BuildFilePath(fakeSeries, seasonNumber, filename, ".mkv").Should().Be(expectedPath.AsOsAgnostic());
        }
    }
}