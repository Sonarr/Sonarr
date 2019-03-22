using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.OrganizerTests
{
    [TestFixture]
    public class CleanFilenameFixture : CoreTest
    {
        [TestCase("Law & Order: Criminal Intent - S10E07 - Icarus [HDTV-720p]", "Law & Order - Criminal Intent - S10E07 - Icarus [HDTV-720p]")]
        public void should_replaace_invalid_characters(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }

        [TestCase(".hack s01e01", "hack s01e01")]
        public void should_remove_periods_from_start(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }

        [TestCase(" Series Title - S01E01 - Episode Title", "Series Title - S01E01 - Episode Title")]
        [TestCase("Series Title - S01E01 - Episode Title ", "Series Title - S01E01 - Episode Title")]
        public void should_remove_spaces_from_start_and_end(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }
    }
}
