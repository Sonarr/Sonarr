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
        private List<ReleaseProfile> _namedReleaseProfiles = null;
        private string _title = "Series.Title.S01E01.extended.720p.HDTV.x264-Sonarr";

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

            _namedReleaseProfiles = new List<ReleaseProfile>();

            _namedReleaseProfiles.Add(new ReleaseProfile
            {
                Name = "CodecProfile",
                Preferred = new List<KeyValuePair<string, int>>
                                                 {
                                                     new KeyValuePair<string, int>("x264", 5),
                                                     new KeyValuePair<string, int>("x265", -10)
                                                 }
            });
            _namedReleaseProfiles.Add(new ReleaseProfile
            {
                Name = "EditionProfile",
                Preferred = new List<KeyValuePair<string, int>>
                                                 {
                                                     new KeyValuePair<string, int>("extended", 5),
                                                     new KeyValuePair<string, int>("uncut", -10)
                                                 }
            });

            Mocker.GetMock<ITermMatcherService>()
                  .Setup(s => s.MatchingTerm(It.IsAny<string>(), _title))
                  .Returns<string, string>((term, title) => title.Contains(term) ? term : null);
        }

        private void GivenReleaseProfile()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                  .Returns(_releaseProfiles);
        }

        private void GivenNamedReleaseProfile()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                  .Returns(_namedReleaseProfiles);
        }

        [Test]
        public void should_return_empty_list_when_there_are_no_release_profiles()
        {
            Mocker.GetMock<IReleaseProfileService>()
                  .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                  .Returns(new List<ReleaseProfile>());

            var matchingResults = Subject.GetMatchingPreferredWords(_series, _title);
            matchingResults.All.Should().BeEmpty();
        }

        [Test]
        public void should_return_empty_list_when_there_are_no_matching_preferred_words_from_unnamedprofile()
        {
            _releaseProfiles.First().Preferred.RemoveAt(0);
            GivenReleaseProfile();

            var matchingResults = Subject.GetMatchingPreferredWords(_series, _title);
            matchingResults.All.Should().BeEmpty();
        }

        [Test]
        public void should_return_list_of_matching_terms_from_unnamedprofile()
        {
            GivenReleaseProfile();

            var matchingResults = Subject.GetMatchingPreferredWords(_series, _title);
            matchingResults.All.Should().Equal(new[] { "x264" });
        }

        [Test]
        public void should_return_empty_list_when_there_are_no_matching_preferred_words_from_namedprofiles()
        {
            _namedReleaseProfiles.First().Preferred.RemoveAt(0);
            _namedReleaseProfiles.Skip(1).First().Preferred.RemoveAt(0);

            GivenNamedReleaseProfile();

            var matchingResults = Subject.GetMatchingPreferredWords(_series, _title);
            matchingResults.All.Should().BeEmpty();
        }

        [Test]
        public void should_return_list_of_matching_terms_from_multiple_namedprofiles()
        {
            GivenNamedReleaseProfile();

            var matchingResults = Subject.GetMatchingPreferredWords(_series, _title);
            matchingResults.ByReleaseProfile.Should().ContainKey("CodecProfile").WhichValue.Should().Equal(new[] { "x264" });
            matchingResults.ByReleaseProfile.Should().ContainKey("EditionProfile").WhichValue.Should().Equal(new[] { "extended" });
        }

        [Test]
        public void should_return_list_of_matching_terms_from_multiple_namedprofiles_all()
        {
            GivenNamedReleaseProfile();

            var matchingResults = Subject.GetMatchingPreferredWords(_series, _title);
            matchingResults.All.Should().Equal(new[] { "x264", "extended" });
        }
    }
}
