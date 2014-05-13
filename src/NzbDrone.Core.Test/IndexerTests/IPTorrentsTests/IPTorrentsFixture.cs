using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.IPTorrents;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Test.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluentAssertions;

namespace NzbDrone.Core.Test.IndexerTests.IPTorrentsTests
{
    [TestFixture]
    public class IPTorrentsFixture : CoreTest<IPTorrents>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                                    {
                                        Name = "IPTorrents",
                                        Settings = new IPTorrentsSettings() {  Url = "http://fake.com/" }
                                    };
        }

        [Test]
        public void should_parse_recent_feed_from_IPTorrents()
        {
            var recentFeed = ReadAllText(@"Files/RSS/IPTorrents.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(5);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("24 S03E12 720p WEBRip h264-DRAWER");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://iptorrents.com/download.php/1234/24.S03E12.720p.WEBRip.h264-DRAWER.torrent?torrent_pass=abcd");
            torrentInfo.InfoUrl.Should().BeNullOrEmpty();
            torrentInfo.CommentUrl.Should().BeNullOrEmpty();
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/05/12 19:06:34"));
            torrentInfo.Size.Should().Be(1471026299);
            torrentInfo.InfoHash.Should().Be(null);
            torrentInfo.MagnetUrl.Should().Be(null);
            torrentInfo.Peers.Should().Be(null);
            torrentInfo.Seeds.Should().Be(null);
        }
    }
}
