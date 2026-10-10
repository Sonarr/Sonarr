using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser.Model;
using Sonarr.Api.V3.Parse;

namespace NzbDrone.Api.Test.v3.Parse
{
    [TestFixture]
    public class ParsedEpisodeInfoResourceFixture
    {
        [Test]
        public void should_return_first_season_as_season_number()
        {
            var resource = new ParsedEpisodeInfo { SeasonNumbers = [2, 3, 4] }.ToResource();

            resource.SeasonNumber.Should().Be(2);
            resource.SeasonNumbers.Should().Equal(2, 3, 4);
            resource.IsMultiSeason.Should().BeTrue();
        }

        [Test]
        public void should_return_negative_season_number_when_there_are_no_seasons()
        {
            var resource = new ParsedEpisodeInfo { SeasonNumbers = [] }.ToResource();

            resource.SeasonNumber.Should().Be(-1);
        }
    }
}
