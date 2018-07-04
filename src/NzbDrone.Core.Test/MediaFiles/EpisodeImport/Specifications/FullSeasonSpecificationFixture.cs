using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class FullSeasonSpecificationFixture : CoreTest<FullSeasonSpecification>
    {
        private LocalEpisode _localEpisode;

        [SetUp]
        public void Setup()
        {
            _localEpisode = new LocalEpisode
            {
                Path = @"C:\Test\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                Size = 100,
                Series = Builder<Series>.CreateNew().Build(),
                FileEpisodeInfo = new ParsedEpisodeInfo
                                    {
                                        FullSeason = false
                                    }
            };
        }

        [Test]
        public void should_return_true_if_no_fileinfo_available()
        {
            _localEpisode.FileEpisodeInfo = null;
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_file_contains_the_full_season()
        {
            _localEpisode.FileEpisodeInfo.FullSeason = true;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_file_does_not_contain_the_full_season()
        {
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }
    }
}
