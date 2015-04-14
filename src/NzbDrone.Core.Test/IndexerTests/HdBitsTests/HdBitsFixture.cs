using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.HDBits;
using NzbDrone.Core.Test.Framework;
using FluentAssertions;
using System.Linq;
using NzbDrone.Core.Parser.Model;
using System;
using System.Text;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.IndexerTests.HdBitsTests
{
    [TestFixture]
    public class HdBitsFixture : CoreTest<HdBits>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
            {
                Name = "HdBits",
                Settings = new HdBitsSettings() { ApiKey = "fakekey" }
            };
        }

        [Test]
        public void TestSimpleResponse()
        {
            var responseJson = ReadAllText(@"Files/Indexers/HdBits/RecentFeed.json");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.POST)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), responseJson));

            var torrents = Subject.FetchRecent();

            torrents.Should().HaveCount(2);
            torrents.First().Should().BeOfType<TorrentInfo>();

            var first = torrents.First() as TorrentInfo;

            first.Guid.Should().Be("HDB-257142");
            first.Title.Should().Be("Supernatural S10E17 1080p WEB-DL DD5.1 H.264-ECI");
            first.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            first.DownloadUrl.Should().Be("https://hdbits.org/download.php?id=257142&passkey=fakekey");
            first.InfoUrl.Should().Be("https://hdbits.org/details.php?id=257142");
            first.PublishDate.Should().Be(DateTime.Parse("2015-04-04T20:30:46+0000"));
            first.Size.Should().Be(1718009717);
            first.MagnetUrl.Should().BeNullOrEmpty();
            first.Peers.Should().Be(47);
            first.Seeders.Should().Be(46);
        }

        [Test]
        public void TestBadPasskey()
        {
            var responseJson = @"
{
    ""status"": 5,
    ""message"": ""Invalid authentication credentials""
}";
            Mocker.GetMock<IHttpClient>()
                .Setup(v => v.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(),
                    Encoding.UTF8.GetBytes(responseJson)));

            var torrents = Subject.FetchRecent();

            torrents.Should().BeEmpty();

            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
