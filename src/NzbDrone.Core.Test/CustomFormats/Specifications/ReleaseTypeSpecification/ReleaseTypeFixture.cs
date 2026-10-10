using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats.Specifications.ReleaseTypeSpecification
{
    [TestFixture]
    public class ReleaseTypeFixture : CoreTest<Core.CustomFormats.ReleaseTypeSpecification>
    {
        private static CustomFormatInput GivenRelease(string title)
        {
            var parsedEpisodeInfo = Parser.Parser.ParseTitle(title);

            return new CustomFormatInput
            {
                EpisodeInfo = parsedEpisodeInfo,
                ReleaseType = parsedEpisodeInfo.ReleaseType
            };
        }

        [Test]
        public void should_match_multi_season_pack()
        {
            Subject.Value = (int)ReleaseType.MultiSeasonPack;

            Subject.IsSatisfiedBy(GivenRelease("Series.Title.S01-S03.1080p.BluRay.x264-RlsGrp")).Should().BeTrue();
        }

        [Test]
        public void should_not_match_single_season_pack_as_multi_season_pack()
        {
            Subject.Value = (int)ReleaseType.MultiSeasonPack;

            Subject.IsSatisfiedBy(GivenRelease("Series.Title.S01.1080p.BluRay.x264-RlsGrp")).Should().BeFalse();
        }

        [Test]
        public void should_not_match_multi_season_pack_as_season_pack()
        {
            Subject.Value = (int)ReleaseType.SeasonPack;

            Subject.IsSatisfiedBy(GivenRelease("Series.Title.S01-S03.1080p.BluRay.x264-RlsGrp")).Should().BeFalse();
        }

        [Test]
        public void should_match_single_season_pack_as_season_pack()
        {
            Subject.Value = (int)ReleaseType.SeasonPack;

            Subject.IsSatisfiedBy(GivenRelease("Series.Title.S01.1080p.BluRay.x264-RlsGrp")).Should().BeTrue();
        }

        [Test]
        public void should_be_valid_for_multi_season_pack()
        {
            Subject.Value = (int)ReleaseType.MultiSeasonPack;

            Subject.Validate().IsValid.Should().BeTrue();
        }
    }
}
