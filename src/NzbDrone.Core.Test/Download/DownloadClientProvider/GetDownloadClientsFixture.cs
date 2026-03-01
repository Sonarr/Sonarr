using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class GetDownloadClientsFixture : CoreTest<DownloadClientProvider>
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

        private Mock<IDownloadClient> WithUsenetClient(int priority = 0, HashSet<int> tags = null)
        {
            var mock = new Mock<IDownloadClient>(MockBehavior.Default);
            mock.SetupGet(s => s.Definition)
                .Returns(Builder<DownloadClientDefinition>
                    .CreateNew()
                    .With(v => v.Id = _nextId++)
                    .With(v => v.Priority = priority)
                    .With(v => v.Tags = tags ?? new HashSet<int>())
                    .Build());

            _downloadClients.Add(mock.Object);

            mock.SetupGet(v => v.Protocol).Returns(DownloadProtocol.Usenet);

            return mock;
        }

        private Mock<IDownloadClient> WithTorrentClient(int priority = 0, HashSet<int> tags = null)
        {
            var mock = new Mock<IDownloadClient>(MockBehavior.Default);
            mock.SetupGet(s => s.Definition)
                .Returns(Builder<DownloadClientDefinition>
                    .CreateNew()
                    .With(v => v.Id = _nextId++)
                    .With(v => v.Priority = priority)
                    .With(v => v.Tags = tags ?? new HashSet<int>())
                    .Build());

            _downloadClients.Add(mock.Object);

            mock.SetupGet(v => v.Protocol).Returns(DownloadProtocol.Torrent);

            return mock;
        }

        private void WithTorrentIndexer(int downloadClientId)
        {
            Mocker.GetMock<IIndexerFactory>()
                .Setup(v => v.Find(It.IsAny<int>()))
                .Returns(Builder<IndexerDefinition>
                    .CreateNew()
                    .With(v => v.Id = _nextId++)
                    .With(v => v.DownloadClientId = downloadClientId)
                    .Build());
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
        public void should_return_all_available_clients_for_protocol()
        {
            WithUsenetClient();
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentClient();

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent);

            clients.Should().HaveCount(3);
            clients.Select(c => c.Definition.Id).Should().BeEquivalentTo(new[] { 2, 3, 4 });
        }

        [Test]
        public void should_return_empty_when_no_clients_available_for_protocol()
        {
            WithUsenetClient();
            WithUsenetClient();

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent);

            clients.Should().BeEmpty();
        }

        [Test]
        public void should_return_clients_ordered_by_priority_then_by_last_used()
        {
            WithTorrentClient(priority: 1);
            WithTorrentClient(priority: 0);
            WithTorrentClient(priority: 0);
            WithTorrentClient(priority: 2);

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent);

            clients.Should().HaveCount(4);
            var clientIds = clients.Select(c => c.Definition.Id).ToArray();
            clientIds[0].Should().Be(2);
            clientIds[1].Should().Be(3);
            clientIds[2].Should().Be(1);
            clientIds[3].Should().Be(4);
        }

        [Test]
        public void should_rotate_clients_within_same_priority_based_on_last_used()
        {
            WithTorrentClient(priority: 0);
            WithTorrentClient(priority: 0);
            WithTorrentClient(priority: 0);

            var clients1 = Subject.GetDownloadClients(DownloadProtocol.Torrent);
            clients1.First().Definition.Id.Should().Be(1);

            Subject.ReportSuccessfulDownloadClient(DownloadProtocol.Torrent, 2);

            var clients2 = Subject.GetDownloadClients(DownloadProtocol.Torrent);
            var clientIds = clients2.Select(c => c.Definition.Id).ToArray();
            clientIds[0].Should().Be(3);
            clientIds[1].Should().Be(1);
            clientIds[2].Should().Be(2);
        }

        [Test]
        public void should_filter_clients_by_tags()
        {
            var seriesTags = new HashSet<int> { 1, 2 };

            WithTorrentClient(tags: new HashSet<int> { 1 });
            WithTorrentClient(tags: new HashSet<int> { 3 });
            WithTorrentClient(tags: new HashSet<int> { 2 });
            WithTorrentClient();

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent, tags: seriesTags);

            clients.Should().HaveCount(2);
            clients.Select(c => c.Definition.Id).Should().BeEquivalentTo(new[] { 1, 3 });
        }

        [Test]
        public void should_return_non_tagged_clients_when_no_matching_tags()
        {
            var seriesTags = new HashSet<int> { 5 };

            WithTorrentClient(tags: new HashSet<int> { 1 });
            WithTorrentClient(tags: new HashSet<int> { 2 });
            WithTorrentClient();
            WithTorrentClient();

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent, tags: seriesTags);

            clients.Should().HaveCount(2);
            clients.Select(c => c.Definition.Id).Should().BeEquivalentTo(new[] { 3, 4 });
        }

        [Test]
        public void should_throw_when_all_clients_have_non_matching_tags()
        {
            var seriesTags = new HashSet<int> { 5 };

            WithTorrentClient(tags: new HashSet<int> { 1 });
            WithTorrentClient(tags: new HashSet<int> { 2 });

            Assert.Throws<DownloadClientUnavailableException>(() => Subject.GetDownloadClients(DownloadProtocol.Torrent, tags: seriesTags));
        }

        [Test]
        public void should_return_indexer_specific_client_when_specified()
        {
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentIndexer(2);

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent, indexerId: 1);

            clients.Should().HaveCount(1);
            clients.First().Definition.Id.Should().Be(2);
        }

        [Test]
        public void should_throw_when_indexer_client_does_not_exist()
        {
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentIndexer(5);

            Assert.Throws<DownloadClientUnavailableException>(() => Subject.GetDownloadClients(DownloadProtocol.Torrent, indexerId: 1));
        }

        [Test]
        public void should_filter_blocked_clients_when_requested()
        {
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentClient();

            GivenBlockedClient(2);

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent, filterBlockedClients: true);

            clients.Should().HaveCount(2);
            clients.Select(c => c.Definition.Id).Should().BeEquivalentTo(new[] { 1, 3 });
        }

        [Test]
        public void should_throw_when_all_clients_blocked_and_filter_enabled()
        {
            WithTorrentClient();
            WithTorrentClient();

            GivenBlockedClient(1);
            GivenBlockedClient(2);

            Assert.Throws<DownloadClientUnavailableException>(() => Subject.GetDownloadClients(DownloadProtocol.Torrent, filterBlockedClients: true));
        }

        [Test]
        public void should_return_blocked_clients_when_filter_disabled()
        {
            WithTorrentClient();
            WithTorrentClient();

            GivenBlockedClient(1);
            GivenBlockedClient(2);

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent, filterBlockedClients: false);

            clients.Should().HaveCount(2);
            clients.Select(c => c.Definition.Id).Should().BeEquivalentTo(new[] { 1, 2 });
        }

        [Test]
        public void should_throw_when_indexer_client_is_blocked_and_filter_enabled()
        {
            WithTorrentClient();
            WithTorrentClient();
            WithTorrentIndexer(2);

            GivenBlockedClient(2);

            Assert.Throws<DownloadClientUnavailableException>(() => Subject.GetDownloadClients(DownloadProtocol.Torrent, indexerId: 1, filterBlockedClients: true));
        }

        [Test]
        public void should_combine_tags_and_priority_filtering()
        {
            var seriesTags = new HashSet<int> { 1 };

            WithTorrentClient(priority: 1, tags: new HashSet<int> { 1 });
            WithTorrentClient(priority: 0, tags: new HashSet<int> { 1 });
            WithTorrentClient(priority: 0, tags: new HashSet<int> { 2 });
            WithTorrentClient(priority: 2);

            var clients = Subject.GetDownloadClients(DownloadProtocol.Torrent, tags: seriesTags);

            clients.Should().HaveCount(2);
            var clientIds = clients.Select(c => c.Definition.Id).ToArray();

            clientIds[0].Should().Be(2);
            clientIds[1].Should().Be(1);
        }
    }
}
