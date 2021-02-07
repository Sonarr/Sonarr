using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SeriesTitleInfoFixture : CoreTest
    {
        [Test]
        public void should_have_year_zero_when_title_doesnt_have_a_year()
        {
            const string title = "Series.Title.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).SeriesTitleInfo;

            result.Year.Should().Be(0);
        }

        [Test]
        public void should_have_same_title_for_title_and_title_without_year_when_title_doesnt_have_a_year()
        {
            const string title = "Series.Title.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).SeriesTitleInfo;

            result.Title.Should().Be(result.TitleWithoutYear);
        }

        [Test]
        public void should_have_year_when_title_has_a_year()
        {
            const string title = "Series.Title.2004.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).SeriesTitleInfo;

            result.Year.Should().Be(2004);
        }

        [Test]
        public void should_have_year_in_title_when_title_has_a_year()
        {
            const string title = "Series.Title.2004.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).SeriesTitleInfo;

            result.Title.Should().Be("Series Title 2004");
        }

        [Test]
        public void should_title_without_year_should_not_contain_year()
        {
            const string title = "Series.Title.2004.S01E01.pilot.720p.hdtv";

            var result = Parser.Parser.ParseTitle(title).SeriesTitleInfo;

            result.TitleWithoutYear.Should().Be("Series Title");
        }
    }
}
