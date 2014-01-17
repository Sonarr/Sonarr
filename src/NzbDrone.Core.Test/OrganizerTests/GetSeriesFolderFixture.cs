using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetSeriesFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig namingConfig;

        [SetUp]
        public void Setup()
        {
            namingConfig = new NamingConfig();

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(namingConfig);
        }

        [TestCase("30 Rock", "{Series Title}", "30 Rock")]
        [TestCase("30 Rock", "{Series.Title}", "30.Rock")]
        [TestCase("24/7 Road to the NHL Winter Classic", "{Series Title}", "24+7 Road to the NHL Winter Classic")]
        public void should_use_seriesFolderFormat_to_build_folder_name(string seriesTitle, string format, string expected)
        {
            namingConfig.SeriesFolderFormat = format;

            Subject.GetSeriesFolder(seriesTitle).Should().Be(expected);
        }
    }
}