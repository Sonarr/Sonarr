using System.Data;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Converters;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Converters
{
    [TestFixture]
    public class EmbeddedDocumentConverterFixture : CoreTest
    {
        private static string Serialize(ParsedEpisodeInfo parsedEpisodeInfo)
        {
            var converter = new EmbeddedDocumentConverter<ParsedEpisodeInfo>();
            var parameter = new Mock<IDbDataParameter>();
            parameter.SetupProperty(p => p.Value);

            converter.SetValue(parameter.Object, parsedEpisodeInfo);

            return (string)parameter.Object.Value;
        }

        private static ParsedEpisodeInfo Deserialize(string json)
        {
            return new EmbeddedDocumentConverter<ParsedEpisodeInfo>().Parse(json);
        }

        [Test]
        public void should_round_trip_season_numbers_for_a_multi_season_pack()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                SeriesTitle = "Series Title",
                SeasonNumbers = [1, 2, 3],
                FullSeason = true
            };

            var json = Serialize(parsedEpisodeInfo);
            var result = Deserialize(json);

            json.Should().NotContain("\"seasonNumber\"");

            result.SeasonNumbers.Should().Equal(1, 2, 3);
            result.IsMultiSeason.Should().BeTrue();
        }
    }
}
