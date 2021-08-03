using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Profiles.Releases.PreferredWordService
{
    [TestFixture]
    public class CalculateFixture : CoreTest<Core.Profiles.Releases.PreferredWordService>
    {
        private Series _series = null;
        private List<ReleaseProfile> _releaseProfiles = null;
        private string _title = "Series.Title.S01E01.720p.HDTV.x264-Sonarr";

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Tags = new HashSet<int>(new[] { 1, 2 }))
                                     .Build();

            _releaseProfiles = new List<ReleaseProfile>();

            _releaseProfiles.Add(new ReleaseProfile
                                 {
                                     Preferred = new List<KeyValuePair<string, int>>
                                                 {
                                                     new KeyValuePair<string, int>("x264", 5),
                                                     new KeyValuePair<string, int>("x265", -10)
                                                 }
                                 });

            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                  .Returns(_releaseProfiles);
        }

        private void GivenMatchingTerms(params string[] terms)
        {
            Mocker.GetMock<ITermMatcherService>()
                  .Setup(s => s.IsMatch(It.IsAny<string>(), _title))
                  .Returns<string, string>((term, title) => terms.Contains(term));
        }

        [Test]
        public void should_return_0_when_there_are_no_release_profiles()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                  .Returns(new List<ReleaseProfile>());

            Subject.Calculate(_series, _title, 0).Should().Be(0);
        }

        [Test]
        public void should_return_0_when_there_are_no_matching_preferred_words()
        {
            GivenMatchingTerms();

            Subject.Calculate(_series, _title, 0).Should().Be(0);
        }

        [Test]
        public void should_calculate_positive_score()
        {
            GivenMatchingTerms("x264");

            Subject.Calculate(_series, _title, 0).Should().Be(5);
        }

        [Test]
        public void should_calculate_negative_score()
        {
            GivenMatchingTerms("x265");

            Subject.Calculate(_series, _title, 0).Should().Be(-10);
        }

        [Test]
        public void should_calculate_using_multiple_profiles()
        {
            _releaseProfiles.Add(_releaseProfiles.First());

            GivenMatchingTerms("x264");

            Subject.Calculate(_series, _title, 0).Should().Be(10);
        }
    }
}
