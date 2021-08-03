using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class ProtocolSpecificationFixture : CoreTest<ProtocolSpecification>
    {
        private RemoteEpisode _remoteEpisode;
        private DelayProfile _delayProfile;

        [SetUp]
        public void Setup()
        {
            _remoteEpisode = new RemoteEpisode();
            _remoteEpisode.Release = new ReleaseInfo();
            _remoteEpisode.Series = new Series();

            _delayProfile = new DelayProfile();

            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(_delayProfile);
        }

        private void GivenProtocol(DownloadProtocol downloadProtocol)
        {
            _remoteEpisode.Release.DownloadProtocol = downloadProtocol;
        }

        [Test]
        public void should_be_true_if_usenet_and_usenet_is_enabled()
        {
            GivenProtocol(DownloadProtocol.Usenet);
            _delayProfile.EnableUsenet = true;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().Be(true);
        }

        [Test]
        public void should_be_true_if_torrent_and_torrent_is_enabled()
        {
            GivenProtocol(DownloadProtocol.Torrent);
            _delayProfile.EnableTorrent = true;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().Be(true);
        }

        [Test]
        public void should_be_false_if_usenet_and_usenet_is_disabled()
        {
            GivenProtocol(DownloadProtocol.Usenet);
            _delayProfile.EnableUsenet = false;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().Be(false);
        }

        [Test]
        public void should_be_false_if_torrent_and_torrent_is_disabled()
        {
            GivenProtocol(DownloadProtocol.Torrent);
            _delayProfile.EnableTorrent = false;

            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().Be(false);
        }
    }
}
