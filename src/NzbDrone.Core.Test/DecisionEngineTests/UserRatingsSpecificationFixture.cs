using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using System;
using System.Linq;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class UserRatingsSpecificationFixture : CoreTest<UserRatingsSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode
            {
                Release = new ReleaseInfo
                {
                    Title = "Droned",
                    UserRatings = new ReleaseUserRatings()
                }
            };
        }

        [Test]
        public void should_pass_if_no_userratings()
        {
            _remoteEpisode.Release.UserRatings = null;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Should().Be(true);
        }

        [Test]
        public void should_reject_if_spam()
        {
            _remoteEpisode.Release.UserRatings.IsSpamConfirmed = true;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Should().Be(false);
        }

        [Test]
        public void should_reject_if_passworded()
        {
            _remoteEpisode.Release.UserRatings.IsPasswordedConfirmed = true;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Should().Be(false);
        }

        [Test]
        public void should_reject_if_too_many_downvotes()
        {
            _remoteEpisode.Release.UserRatings.DownVotes = 200;
            _remoteEpisode.Release.UserRatings.UpVotes = 50;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Should().Be(false);
        }
    }
}