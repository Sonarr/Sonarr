using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class DownloadClientProviderFixture : CoreTest<DownloadClientProvider>
    {
        private List<IDownloadClient> _downloadClients;
        private List<DownloadClientStatus> _blockedProviders;
        private int _nextId;

        [SetUp]
        public void SetUp()
        {
            _downloadClients = new List<IDownloadClient>();
            _blockedProviders = new List<DownloadClientStatus>();
            _nextId = 1;

            Mocker.GetMock<IDownloadClientFactory>()
                  .Setup(v => v.GetAvailableProviders())
                  .Returns(_downloadClients);

            Mocker.GetMock<IDownloadClientStatusService>()
                  .Setup(v => v.GetBlockedProviders())
                  .Returns(_blockedProviders);
        }

        private Mock<IDownloadClient> WithUsenetClient(int priority = 0)
        {
            var mock = new Mock<IDownloadClient>(MockBehavior.Default);
            mock.SetupGet(s => s.Definition)
                .Returns(Builder<DownloadClientDefinition>
                    .CreateNew()
                    .With(v => v.Id = _nextId++)
                    .With(v => v.Priority = priority)
                    .Build());

            _downloadClients.Add(mock.Object);

            mock.SetupGet(v => v.Protocol).Returns(DownloadProtocol.Usenet);

            return mock;
        }

        private Mock<IDownloadClient> WithTorrentClient(int priority = 0)
        {
            var mock = new Mock<IDownloadClient>(MockBehavior.Default);
            mock.SetupGet(s => s.Definition)
                .Returns(Builder<DownloadClientDefinition>
                    .CreateNew()
                    .With(v => v.Id = _nextId++)
                    .With(v => v.Priority = priority)
                    .Build());

            _downloadClients.Add(mock.Object);

            mock.SetupGet(v => v.Protocol).Returns(DownloadProtocol.Torrent);

            return mock;
        }

        private void GivenBlockedClient(int id)
        {
            _blockedProviders.Add(new DownloadClientStatus
            {
                ProviderId = id,
                DisabledTill = DateTime.UtcNow.AddHours(3)
            });
        }

        [Test]
        public void should_roundrobin_over_usenet_client()
        {
            WithUsenetClient();
            WithUsenetClient();
            WithUsenetClient();
            WithTorrentClient();

            var client1 = Subject.GetDownloadClient(DownloadProtocol.Usenet);
            var client2 = Subject.GetDownloadClient(DownloadProtocol.Usenet);
            var client3 = Subject.GetDownloadClient(DownloadProtocol.Usenet);
            var client4 = Subject.GetDownloadClient(DownloadProtocol.Usenet);
            var client5 = Subject.GetDownloadClient(DownloadProtocol.Usenet);

            client1.Definition.Id.Should().Be(1);
            client2.Definition.Id.Should().Be(2);
            client3.Definition.Id.Should().Be(3);
            client4.Definition.Id.Should().Be(1);
            client5.Definition.Id.Should().Be(2);
        }

        [Test]
        public void should_roundrobin_over_torrent_client()
        {
            WithUsenetClient();
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentClient();

            var client1 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client2 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client3 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client4 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client5 = Subject.GetDownloadClient(DownloadProtocol.Torrent);

            client1.Definition.Id.Should().Be(2);
            client2.Definition.Id.Should().Be(3);
            client3.Definition.Id.Should().Be(4);
            client4.Definition.Id.Should().Be(2);
            client5.Definition.Id.Should().Be(3);
        }

        [Test]
        public void should_roundrobin_over_protocol_separately()
        {
            WithUsenetClient();
            WithTorrentClient();
            WithTorrentClient();

            var client1 = Subject.GetDownloadClient(DownloadProtocol.Usenet);
            var client2 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client3 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client4 = Subject.GetDownloadClient(DownloadProtocol.Torrent);

            client1.Definition.Id.Should().Be(1);
            client2.Definition.Id.Should().Be(2);
            client3.Definition.Id.Should().Be(3);
            client4.Definition.Id.Should().Be(2);
        }

        [Test]
        public void should_skip_blocked_torrent_client()
        {
            WithUsenetClient();
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentClient();

            GivenBlockedClient(3);

            var client1 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client2 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client3 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client4 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client5 = Subject.GetDownloadClient(DownloadProtocol.Torrent);

            client1.Definition.Id.Should().Be(2);
            client2.Definition.Id.Should().Be(4);
            client3.Definition.Id.Should().Be(2);
            client4.Definition.Id.Should().Be(4);
        }

        [Test]
        public void should_not_skip_blocked_torrent_client_if_all_blocked()
        {
            WithUsenetClient();
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentClient();

            GivenBlockedClient(2);
            GivenBlockedClient(3);
            GivenBlockedClient(4);

            var client1 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client2 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client3 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client4 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client5 = Subject.GetDownloadClient(DownloadProtocol.Torrent);

            client1.Definition.Id.Should().Be(2);
            client2.Definition.Id.Should().Be(3);
            client3.Definition.Id.Should().Be(4);
            client4.Definition.Id.Should().Be(2);
        }

        [Test]
        public void should_skip_secondary_prio_torrent_client()
        {
            WithUsenetClient();
            WithTorrentClient(2);
            WithTorrentClient();
            WithTorrentClient();

            var client1 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client2 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client3 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client4 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client5 = Subject.GetDownloadClient(DownloadProtocol.Torrent);

            client1.Definition.Id.Should().Be(3);
            client2.Definition.Id.Should().Be(4);
            client3.Definition.Id.Should().Be(3);
            client4.Definition.Id.Should().Be(4);
        }

        [Test]
        public void should_not_skip_secondary_prio_torrent_client_if_primary_blocked()
        {
            WithUsenetClient();
            WithTorrentClient(2);
            WithTorrentClient(2);
            WithTorrentClient();

            GivenBlockedClient(4);

            var client1 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client2 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client3 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client4 = Subject.GetDownloadClient(DownloadProtocol.Torrent);
            var client5 = Subject.GetDownloadClient(DownloadProtocol.Torrent);

            client1.Definition.Id.Should().Be(2);
            client2.Definition.Id.Should().Be(3);
            client3.Definition.Id.Should().Be(2);
            client4.Definition.Id.Should().Be(3);
        }
    }
}
