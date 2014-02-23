using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            parsedEpisodeInfo.IsPossibleSpecialEpisode().Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_episode_numbers_is_empty()
        {
            var parsedEpisodeInfo = new ParsedEpisodeInfo
            {
                SeasonNumber = 1,
                SeriesTitle = ""
            };

            parsedEpisodeInfo.IsPossibleSpecialEpisode().Should().BeTrue();
        }
    }
}
