using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]

    public class GetSeriesFolderFixture : CoreTest<FileNameBuilder>
    {
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _namingConfig = NamingConfig.Default;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);
        }

        [TestCase("30 Rock", "{Series Title}", "30 Rock")]
        [TestCase("30 Rock", "{Series.Title}", "30.Rock")]
        [TestCase("24/7 Road to the NHL Winter Classic", "{Series Title}", "24+7 Road to the NHL Winter Classic")]
        [TestCase("Venture Bros.", "{Series.Title}", "Venture.Bros")]
        [TestCase(".hack", "{Series.Title}", "hack")]
        [TestCase("30 Rock", ".{Series.Title}.", "30.Rock")]
        public void should_use_seriesFolderFormat_to_build_folder_name(string seriesTitle, string format, string expected)
        {
            _namingConfig.SeriesFolderFormat = format;

            var series = new Series { Title = seriesTitle };

            Subject.GetSeriesFolder(series).Should().Be(expected);
        }
    }
}
