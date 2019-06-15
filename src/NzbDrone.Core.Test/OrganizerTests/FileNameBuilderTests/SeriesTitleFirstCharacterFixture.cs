using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.OrganizerTests.FileNameBuilderTests
{
    [TestFixture]
    public class SeriesTitleFirstCharacterFixture : CoreTest<FileNameBuilder>
    {
        private Series _series;
        private NamingConfig _namingConfig;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>
                    .CreateNew()
                    .Build();

            _namingConfig = NamingConfig.Default;
            _namingConfig.RenameEpisodes = true;

            Mocker.GetMock<INamingConfigService>()
                  .Setup(c => c.GetConfig()).Returns(_namingConfig);

            Mocker.GetMock<IQualityDefinitionService>()
                .Setup(v => v.Get(Moq.It.IsAny<Quality>()))
                .Returns<Quality>(v => Quality.DefaultQualityDefinitions.First(c => c.Quality == v));
        }

        [TestCase("The Mist", "M\\The Mist")]
        [TestCase("A", "A\\A")]
        [TestCase("30 Rock", "3\\30 Rock")]
        public void should_get_expected_folder_name_back(string title, string expected)
        {
            _series.Title = title;
            _namingConfig.SeriesFolderFormat = "{Series TitleFirstCharacter}\\{Series Title}";

            Subject.GetSeriesFolder(_series).Should().Be(expected);
        }

        [Test]
        public void should_be_able_to_use_lower_case_first_character()
        {
            _series.Title = "Westworld";
            _namingConfig.SeriesFolderFormat = "{series titlefirstcharacter}\\{series title}";

            Subject.GetSeriesFolder(_series).Should().Be("w\\westworld");
        }
    }
}
