using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class NormalizeEpisodeTitleFixture : CoreTest
    {
        [TestCase("Episode Title", "episode title")]
        [TestCase("A.B,C;", "a b c")]
        [TestCase("Episode  Title", "episode title")]
        [TestCase("French Title (1)", "french title")]
        [TestCase("Series.Title.S01.Special.Episode.Title.720p.HDTV.x264-Sonarr", "episode title")]
        [TestCase("Series.Title.S01E00.Episode.Title.720p.HDTV.x264-Sonarr", "episode title")]
        public void should_normalize_episode_title(string input, string expected)
        {
            var result = Parser.Parser.NormalizeEpisodeTitle(input);

            result.Should().Be(expected);
        }
    }
}
