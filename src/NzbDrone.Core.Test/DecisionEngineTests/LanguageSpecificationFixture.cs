using FluentAssertions;
using Marr.Data;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class LanguageSpecificationFixture : CoreTest
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    Language = Language.English
                },
                Series = new Series
                         {
                             Profile = new LazyLoaded<Profile>(new Profile
                                                               {
                                                                   Language = Language.English
                                                               })
                         }
            };
        }

        private void WithEnglishRelease()
        {
            _remoteEpisode.ParsedEpisodeInfo.Language = Language.English;
        }

        private void WithGermanRelease()
        {
            _remoteEpisode.ParsedEpisodeInfo.Language = Language.German;            
        }

        [Test]
        public void should_return_true_if_language_is_english()
        {
            WithEnglishRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteEpisode, null).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_language_is_german()
        {
            WithGermanRelease();

            Mocker.Resolve<LanguageSpecification>().IsSatisfiedBy(_remoteEpisode, null).Should().BeFalse();
        }
    }
}