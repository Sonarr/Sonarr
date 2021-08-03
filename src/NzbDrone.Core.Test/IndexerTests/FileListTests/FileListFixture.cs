using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.FileList;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.FileListTests
{
    [TestFixture]
    public class FileListFixture : CoreTest<FileList>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
            {
                Name = "FileList",
                Settings = new FileListSettings() { Username = "someuser", Passkey = "somepass" }
            };
        }

        [Test]
        public void should_parse_recent_feed_from_FileList()
        {
            var recentFeed = ReadAllText(@"Files/Indexers/FileList/RecentFeed.json");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(2);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("Mankind.Divided.2019.S01E01.1080p.WEB-DL");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("https://filelist.io/download.php?id=1234&passkey=somepass");
            torrentInfo.InfoUrl.Should().Be("https://filelist.io/details.php?id=1234");
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2019-01-22 22:20:19").ToUniversalTime());
            torrentInfo.Size.Should().Be(830512414);
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(2 + 12);
            torrentInfo.Seeders.Should().Be(12);
        }
    }
}
