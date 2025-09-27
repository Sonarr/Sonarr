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
        private ReleaseProfile _includedAndExcludedReleaseProfile;
        private int _providedTag;
        private int _providedTagToExclude;
        private int _notUsedTag;
        private List<ReleaseProfile> _releaseProfilesWithoutTags;
        private List<ReleaseProfile> _releaseProfilesWithProvidedTag;
        private List<ReleaseProfile> _releaseProfilesWithProvidedTagOrWithoutTags;

        [SetUp]
        public void Setup()
        {
            _providedTag = 1;
            _providedTagToExclude = 2;
            _notUsedTag = 3;

            _releaseProfiles = Builder<ReleaseProfile>.CreateListOfSize(5)
                .TheFirst(1)
                .With(r => r.Required = ["required_one"])
                .TheNext(1)
                .With(r => r.Required = ["required_two"])
                .With(r => r.Tags = [_providedTag])
                .TheNext(1)
                .With(r => r.Required = ["required_three"])
                .With(r => r.ExcludedTags = [_providedTagToExclude])
                .TheNext(1)
                .With(r => r.Required = ["required_four"])
                .With(r => r.Tags = [_providedTag])
                .With(r => r.ExcludedTags = [_providedTagToExclude])
                .TheNext(1)
                .With(r => r.Required = ["required_five"])
                .With(r => r.Tags = [_notUsedTag])
                .Build()
                .ToList();

            _defaultReleaseProfile = _releaseProfiles[0];
            _includedReleaseProfile = _releaseProfiles[1];
            _excludedReleaseProfile = _releaseProfiles[2];
            _includedAndExcludedReleaseProfile = _releaseProfiles[3];

            _releaseProfilesWithoutTags = [_defaultReleaseProfile, _excludedReleaseProfile];
            _releaseProfilesWithProvidedTag = [_includedReleaseProfile, _includedAndExcludedReleaseProfile];
            _releaseProfilesWithProvidedTagOrWithoutTags = [_defaultReleaseProfile, _includedReleaseProfile, _excludedReleaseProfile, _includedAndExcludedReleaseProfile];

            Mocker.GetMock<IRestrictionRepository>()
                  .Setup(s => s.All())
                  .Returns(_releaseProfiles);
        }

        [Test]
        public void all_for_tags_should_return_release_profiles_without_tags_by_default()
        {
            var releaseProfiles = Subject.AllForTags([]);
            releaseProfiles.Should().Equal(_releaseProfilesWithoutTags);
        }

        [Test]
        public void all_for_tags_should_return_release_profiles_with_provided_tag_or_without_tags()
        {
            var releaseProfiles = Subject.AllForTags([_providedTag]);
            releaseProfiles.Should().Equal(_releaseProfilesWithProvidedTagOrWithoutTags);
        }

        [Test]
        public void all_for_tags_should_not_return_release_profiles_with_provided_tag_excluded()
        {
            var releaseProfiles = Subject.AllForTags([_providedTagToExclude]);
            releaseProfiles.Should().NotContain(_excludedReleaseProfile);
            releaseProfiles.Should().NotContain(_includedAndExcludedReleaseProfile);
        }

        [Test]
        public void all_for_tag_should_return_release_profiles_with_provided_tag()
        {
            var releaseProfiles = Subject.AllForTag(_providedTag);
            releaseProfiles.Should().Equal(_releaseProfilesWithProvidedTag);
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
            var releaseProfiles = Subject.AllForTags([_providedTag, _providedTagToExclude]);
            releaseProfiles.Should().Equal([_defaultReleaseProfile, _includedReleaseProfile]);
        }

        [Test]
        public void all_for_tags_should_return_matching_tags_that_are_not_excluded_tags()
        {
            var releaseProfiles = Subject.AllForTags([_providedTag]);
            releaseProfiles.Should().Equal([_defaultReleaseProfile, _includedReleaseProfile, _excludedReleaseProfile, _includedAndExcludedReleaseProfile]);
        }
    }
}
