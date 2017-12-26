using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class IsPossibleSpecialEpisodeFixture
    {
        [Test]
        public void should_not_treat_files_without_a_series_title_as_a_special()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
                                    {
                                        EpisodeNumbers = new[]{ 7 },
                                        SeasonNumber = 1,
                                        SeriesTitle = ""
                                    };

            parsedEpisodeInfo.IsPossibleSpecialEpisode.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_episode_numbers_is_empty()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                SeasonNumber = 1,
                SeriesTitle = ""
            };

            parsedEpisodeInfo.IsPossibleSpecialEpisode.Should().BeTrue();
        }

        [TestCase("Under.the.Dome.S02.Special-Inside.Chesters.Mill.HDTV.x264-BAJSKORV")]
        [TestCase("Under.the.Dome.S02.Special-Inside.Chesters.Mill.720p.HDTV.x264-BAJSKORV")]
        [TestCase("Rookie.Blue.Behind.the.Badge.S05.Special.HDTV.x264-2HD")]
        public void IsPossibleSpecialEpisode_should_be_true(string title)
        {
            Parser.Parser.ParseTitle(title).IsPossibleSpecialEpisode.Should().BeTrue();
        }


        [TestCase("Dr.S11E00.A.Chrismas.Carol.Special.720p.HDTV-FieldOfView")]
        public void IsPossibleSpecialEpisode_should_be_true_if_e00_special(string title)
        {
            Parser.Parser.ParseTitle(title).IsPossibleSpecialEpisode.Should().BeTrue();
        }
    }
}
