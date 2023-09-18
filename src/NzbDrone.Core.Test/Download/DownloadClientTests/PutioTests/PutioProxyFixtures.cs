using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download.Clients.Putio;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.PutioTests
{
    public class PutioProxyFixtures : CoreTest<PutioProxy>
    {
        [Test]
        public void test_GetTorrentMetadata_createsNewObject()
        {
            ClientGetWillReturn<PutioConfigResponse>("{\"status\":\"OK\",\"value\":null}");

            var mt = Subject.GetTorrentMetadata(new PutioTorrent { Id = 1 }, new PutioSettings());
            Assert.IsNotNull(mt);
            Assert.AreEqual(1, mt.Id);
            Assert.IsFalse(mt.Downloaded);
        }

        [Test]
        public void test_GetTorrentMetadata_returnsExistingObject()
        {
            ClientGetWillReturn<PutioConfigResponse>("{\"status\":\"OK\",\"value\":{\"id\":4711,\"downloaded\":true}}");

            var mt = Subject.GetTorrentMetadata(new PutioTorrent { Id = 1 }, new PutioSettings());
            Assert.IsNotNull(mt);
            Assert.AreEqual(4711, mt.Id);
            Assert.IsTrue(mt.Downloaded);
        }

        [Test]
        public void test_GetAllTorrentMetadata_filters_properly()
        {
            var json = @"{
                ""config"": {
                    ""sonarr_123"": {
                        ""downloaded"": true,
                        ""id"": 123
                    },
                    ""another_key"": {
                        ""foo"": ""bar""
                    },
                    ""sonarr_456"": {
                        ""downloaded"": true,
                        ""id"": 456
                    }
                },
                ""status"": ""OK""
            }";
            ClientGetWillReturn<PutioAllConfigResponse>(json);

            var list = Subject.GetAllTorrentMetadata(new PutioSettings());
            Assert.IsTrue(list.ContainsKey("123"));
            Assert.IsTrue(list.ContainsKey("456"));
            Assert.AreEqual(list.Count, 2);
            Assert.IsTrue(list["123"].Downloaded);
            Assert.IsTrue(list["456"].Downloaded);
        }

        private void ClientGetWillReturn<TResult>(string obj)
            where TResult : new()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get<TResult>(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(r => new HttpResponse<TResult>(new HttpResponse(r, new HttpHeader(), obj)));
        }
    }
}
