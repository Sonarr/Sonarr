using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Languages;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class LanguageSpecificationFixture : CoreTest
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            LanguageProfile _profile = new LazyLoaded<LanguageProfile> (new LanguageProfile
            {
                Languages = LanguageFixture.GetDefaultLanguages(Language.English, Language.Spanish),
                Cutoff = Language.Spanish
            });

            _remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    Language = Language.English
                },
                Series = new Series
                {
                    LanguageProfile = _profile
                }
            };
        }

        private void WithEnglishRelease()
        {
            _remoteEpisode.ParsedEpisodeInfo.Language = Language.English;
        }

        private void WithSpanishRelease()
        {
            _remoteEpisode.ParsedEpisodeInfo.Language = Language.Spanish;
        }

        private void WithFrenchRelease()
        {
            _remoteEpisode.ParsedEpisodeInfo.Language = Language.French;
        }

        private void WithGermanRelease()
        {
            _remoteEpisode.ParsedEpisodeInfo.Language = Language.German;            
        }

        [Test]
        public void should_return_true_if_language_is_english()
        {
            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_is_german()
        {
            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_language_is_french()
        {
            WithFrenchRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeFalse();
        }


        [Test]
        public void should_return_true_if_language_is_spanish()
        {
            WithSpanishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

    }
}