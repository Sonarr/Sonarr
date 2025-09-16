using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Profiles
{
    [TestFixture]
    public class ReleaseProfileServiceFixture : CoreTest<ReleaseProfileService>
    {
        private List<ReleaseProfile> _releaseProfiles;
        private ReleaseProfile _defaultReleaseProfile;
        private ReleaseProfile _includedReleaseProfile;
        private ReleaseProfile _excludedReleaseProfile;

        [SetUp]
        public void Setup()
        {
            _releaseProfiles = Builder<ReleaseProfile>.CreateListOfSize(4)
                .TheFirst(1)
                .With(r => r.Required = ["required_one"])
                .TheNext(1)
                .With(r => r.Required = ["required_two"])
                .With(r => r.Tags = [1])
                .TheNext(1)
                .With(r => r.Required = ["required_three"])
                .With(r => r.ExcludedTags = [2])
                .TheNext(1)
                .With(r => r.Required = ["required_four"])
                .With(r => r.Tags = [3])
                .With(r => r.ExcludedTags = [4])
                .Build()
                .ToList();

            _defaultReleaseProfile = _releaseProfiles[0];
            _includedReleaseProfile = _releaseProfiles[1];
            _excludedReleaseProfile = _releaseProfiles[2];

            Mocker.GetMock<IRestrictionRepository>()
                  .Setup(s => s.All())
                  .Returns(_releaseProfiles);
        }

        [Test]
        public void all_for_tags_should_return_release_profiles_without_tags_by_default()
        {
            List<ReleaseProfile> releaseProfilesWithoutTags = [_defaultReleaseProfile, _excludedReleaseProfile];
            var releaseProfiles = Subject.AllForTags([]);
            releaseProfiles.Should().Equal(releaseProfilesWithoutTags);
        }

        [Test]
        public void all_for_tags_should_return_release_profiles_with_provided_tag_or_without_tags()
        {
            var providedTag = 1;
            List<ReleaseProfile> releaseProfilesWithProvidedTagOrWithoutTags = [_defaultReleaseProfile, _includedReleaseProfile, _excludedReleaseProfile];
            var releaseProfiles = Subject.AllForTags([providedTag]);
            releaseProfiles.Should().Equal(releaseProfilesWithProvidedTagOrWithoutTags);
        }

        [Test]
        public void all_for_tags_should_not_return_release_profiles_with_provided_tag_excluded()
        {
            var providedTag = 2;
            var releaseProfiles = Subject.AllForTags([providedTag]);
            releaseProfiles.Should().Equal([_defaultReleaseProfile]);
        }

        [Test]
        public void all_for_tag_should_return_release_profiles_with_provided_tag()
        {
            var providedTag = 1;
            var releaseProfiles = Subject.AllForTag(providedTag);
            releaseProfiles.Should().Equal([_includedReleaseProfile]);
        }

        [Test]
        public void all_should_return_all_release_profiles()
        {
            var releaseProfiles = Subject.All();
            releaseProfiles.Should().Equal(_releaseProfiles);
        }

        [Test]
        public void all_for_tags_should_not_return_release_profiles_with_a_provided_tag_both_included_and_excluded()
        {
            HashSet<int> providedTags = [3, 4];
            var releaseProfiles = Subject.AllForTags(providedTags);
            releaseProfiles.Should().Equal([_defaultReleaseProfile, _excludedReleaseProfile]);
        }
    }
}
