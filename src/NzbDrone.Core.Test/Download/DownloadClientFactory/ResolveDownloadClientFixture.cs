using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class ResolveDownloadClientFixture : CoreTest<DownloadClientFactory>
    {
        private List<DownloadClientDefinition> _downloadClients;

        [SetUp]
        public void SetUp()
        {
            _downloadClients = Builder<DownloadClientDefinition>.CreateListOfSize(3)
                .TheFirst(2)
                .With(v => v.Enable = true)
                .TheNext(1)
                .With(v => v.Enable = false)
                .Build()
                .ToList();

            Mocker.GetMock<IDownloadClientRepository>()
                .Setup(v => v.All())
                .Returns(_downloadClients);
        }

        [Test]
        public void should_throw_if_download_client_with_id_cannot_be_found()
        {
            Assert.Throws<ResolveDownloadClientException>(() => Subject.ResolveDownloadClient(10, null));
        }

        [Test]
        public void should_throw_if_download_client_with_name_cannot_be_found()
        {
            Assert.Throws<ResolveDownloadClientException>(() => Subject.ResolveDownloadClient(null, "Not a Real Client"));
        }

        [Test]
        public void should_throw_if_download_client_with_id_does_not_match_download_client_with_name()
        {
            Assert.Throws<ResolveDownloadClientException>(() => Subject.ResolveDownloadClient(_downloadClients[0].Id, _downloadClients[1].Name));
        }

        [Test]
        public void should_throw_if_download_client_is_not_enabled()
        {
            Assert.Throws<ResolveDownloadClientException>(() => Subject.ResolveDownloadClient(_downloadClients[2].Id, null));
        }

        [Test]
        public void should_return_download_client_when_only_id_is_provided()
        {
            var result = Subject.ResolveDownloadClient(_downloadClients[0].Id, null);

            result.Should().NotBeNull();
            result.Should().Be(_downloadClients[0]);
        }

        [Test]
        public void should_return_download_client_when_only_name_is_provided()
        {
            var result = Subject.ResolveDownloadClient(null, _downloadClients[0].Name);

            result.Should().NotBeNull();
            result.Should().Be(_downloadClients[0]);
        }

        [Test]
        public void should_return_download_client_when_id_and_name_provided_for_the_same_download_client()
        {
            var result = Subject.ResolveDownloadClient(_downloadClients[0].Id, _downloadClients[0].Name);

            result.Should().NotBeNull();
            result.Should().Be(_downloadClients[0]);
        }

        [Test]
        public void should_return_null_if_both_id_and_name_are_not_provided()
        {
            var result = Subject.ResolveDownloadClient(null, null);

            result.Should().BeNull();
        }
    }
}
