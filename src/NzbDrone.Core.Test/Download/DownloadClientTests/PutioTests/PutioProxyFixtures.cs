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

        [Test]
        public void test_GetFileListingResponse()
        {
            var json = @"{
                ""cursor"": null,
                ""files"": [
                    {
                        ""file_type"": ""VIDEO"",
                        ""id"": 111,
                        ""name"": ""My.download.mkv"",
                        ""parent_id"": 4711
                    },
                    {
                        ""file_type"": ""FOLDER"",
                        ""id"": 222,
                        ""name"": ""Another-folder[dth]"",
                        ""parent_id"": 4711
                    }
                ],
                ""parent"": {
                    ""file_type"": ""FOLDER"",
                    ""id"": 4711,
                    ""name"": ""Incoming"",
                    ""parent_id"": 0
                },
                ""status"": ""OK"",
                ""total"": 2
            }";
            ClientGetWillReturn<PutioFileListingResponse>(json);

            var response = Subject.GetFileListingResponse(4711, new PutioSettings());

            Assert.That(response, Is.Not.Null);
            Assert.AreEqual(response.Files.Count, 2);
            Assert.AreEqual(4711, response.Parent.Id);
            Assert.AreEqual(111, response.Files[0].Id);
            Assert.AreEqual(222, response.Files[1].Id);
        }

        [Test]
        public void test_GetFileListingResponse_empty()
        {
            var json = @"{
                ""cursor"": null,
                ""files"": [],
                ""parent"": {
                    ""file_type"": ""FOLDER"",
                    ""id"": 4711,
                    ""name"": ""Incoming"",
                    ""parent_id"": 0
                },
                ""status"": ""OK"",
                ""total"": 0
            }";
            ClientGetWillReturn<PutioFileListingResponse>(json);

            var response = Subject.GetFileListingResponse(4711, new PutioSettings());

            Assert.That(response, Is.Not.Null);
            Assert.AreEqual(response.Files.Count, 0);
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
