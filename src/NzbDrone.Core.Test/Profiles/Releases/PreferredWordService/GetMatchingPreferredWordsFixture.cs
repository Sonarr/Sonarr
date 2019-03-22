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
    public class GetMatchingPreferredWordsFixture : CoreTest<Core.Profiles.Releases.PreferredWordService>
    {
        private Series _series = null;
        private List<ReleaseProfile> _releaseProfiles = null;
        private string _title = "Series.Title.S01E01.720p.HDTV.x264-Sonarr";

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.Tags = new HashSet<int>(new[] {1, 2}))
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


            Mocker.GetMock<ITermMatcherService>()
                  .Setup(s => s.MatchingTerm(It.IsAny<string>(), _title))
                  .Returns<string, string>((term, title) => title.Contains(term) ? term : null);
        }

            
        private void GivenReleaseProfile()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<int>>()))
                  .Returns(_releaseProfiles);
        }

        [Test]
        public void should_return_empty_list_when_there_are_no_release_profiles()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.AllForTags(It.IsAny<HashSet<int>>()))
                  .Returns(new List<ReleaseProfile>());

            Subject.GetMatchingPreferredWords(_series, _title).Should().BeEmpty();
        }

        [Test]
        public void should_return_empty_list_when_there_are_no_matching_preferred_words()
        {
            _releaseProfiles.First().Preferred.RemoveAt(0);
            GivenReleaseProfile();

            Subject.GetMatchingPreferredWords(_series, _title).Should().BeEmpty();
        }

        [Test]
        public void should_return_list_of_matching_terms()
        {
            GivenReleaseProfile();

            Subject.GetMatchingPreferredWords(_series, _title).Should().Contain(new[] {"x264"});
        }
    }
}
