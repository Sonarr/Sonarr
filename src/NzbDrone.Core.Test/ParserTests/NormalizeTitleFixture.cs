using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class NormalizeTitleFixture : CoreTest
    {
        [TestCase("Series", "series")]
        [TestCase("Series (2009)", "series2009")]
        [TestCase("Series.2010", "series2010")]
        [TestCase("Series_and_Title_Sonarr", "seriestitlesonarr")]
        public void should_normalize_series_title(string parsedSeriesName, string seriesName)
        {
            var result = parsedSeriesName.CleanSeriesTitle();
            result.Should().Be(seriesName);
        }

        [TestCase("CaPitAl", "capital")]
        [TestCase("peri.od", "period")]
        [TestCase("this.^&%^**$%@#$!That", "thisthat")]
        [TestCase("test/test", "testtest")]
        [TestCase("90210", "90210")]
        [TestCase("24", "24")]
        public void should_remove_special_characters_and_casing(string dirty, string clean)
        {
            var result = dirty.CleanSeriesTitle();
            result.Should().Be(clean);
        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("an")]
        [TestCase("of")]
        public void should_remove_common_words_from_middle_of_title(string word)
        {
            var dirtyFormat = new[]
                            {
                                "word.{0}.word",
                                "word {0} word",
                                "word-{0}-word"
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, word);
                dirty.CleanSeriesTitle().Should().Be("wordword");
            }
        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("an")]
        [TestCase("of")]
        public void should_not_remove_common_words_from_end_of_title(string word)
        {
            var dirtyFormat = new[]
                              {
                                  "word.word.{0}",
                                  "word-word-{0}",
                                  "word-word {0}"
                              };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, word);
                dirty.CleanSeriesTitle().Should().Be("wordword" + word.ToLower());
            }
        }

        [Test]
        public void should_remove_a_from_middle_of_title()
        {
            var dirtyFormat = new[]
                            {
                                "word.{0}.word",
                                "word {0} word",
                                "word-{0}-word",
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, "a");
                dirty.CleanSeriesTitle().Should().Be("wordword");
            }
        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("a")]
        [TestCase("an")]
        [TestCase("of")]
        public void should_not_remove_common_words_in_the_middle_of_word(string word)
        {
            var dirtyFormat = new[]
                            {
                                "word.{0}word",
                                "word {0}word",
                                "word-{0}word",
                                "word{0}.word",
                                "word{0}-word",
                                "word{0}-word",
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, word);
                dirty.CleanSeriesTitle().Should().Be("word" + word.ToLower() + "word");
            }
        }

        [TestCase("The Series", "theseries")]
        [TestCase("The Series Show With Sonarr Dev", "theseriesshowwithsonarrdev")]
        [TestCase("The.Series.Show", "theseriesshow")]
        public void should_not_remove_from_the_beginning_of_the_title(string parsedSeriesName, string seriesName)
        {
            var result = parsedSeriesName.CleanSeriesTitle();
            result.Should().Be(seriesName);
        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("a")]
        [TestCase("an")]
        [TestCase("of")]
        public void should_not_clean_word_from_beginning_of_string(string word)
        {
            var dirtyFormat = new[]
                            {
                                "{0}.word.word",
                                "{0}-word-word",
                                "{0} word word"
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, word);
                dirty.CleanSeriesTitle().Should().Be(word + "wordword");
            }
        }

        [Test]
        public void should_not_clean_trailing_a()
        {
            "Series Title A".CleanSeriesTitle().Should().Be("seriestitlea");
        }

        [TestCase("3%", "3percent")]
        [TestCase("Series Top & 100% Coding Developers", "seriestop100percentcodingdevelopers")]
        [TestCase("Series Title What's Your F@%king Deal?!", "seriestitlewhatsyourfkingdeal")]
        public void should_replace_percent_sign_with_percent_following_numbers(string input, string expected)
        {
            input.CleanSeriesTitle().Should().Be(expected);
        }
    }
}
