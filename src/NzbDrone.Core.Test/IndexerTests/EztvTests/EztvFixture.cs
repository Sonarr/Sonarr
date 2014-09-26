using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Eztv;
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

namespace NzbDrone.Core.Test.IndexerTests.EztvTests
{
    [TestFixture]
    public class EztvFixture : CoreTest<Eztv>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Eztv",
                    Settings = new EztvSettings()
                };
        }

        [Test]
        public void should_parse_recent_feed_from_Eztv()
        {
            var recentFeed = ReadAllText(@"Files/RSS/Eztv.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));

            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(3);
            releases.First().Should().BeOfType<TorrentInfo>();

            var torrentInfo = releases.First() as TorrentInfo;

            torrentInfo.Title.Should().Be("S4C I Grombil Cyfandir Pell American Interior [PDTV - MVGROUP]");
            torrentInfo.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
            torrentInfo.DownloadUrl.Should().Be("http://re.zoink.it/20a4ed4eFC");
            torrentInfo.InfoUrl.Should().Be("http://eztv.it/ep/58439/s4c-i-grombil-cyfandir-pell-american-interior-pdtv-x264-mvgroup/");
            torrentInfo.CommentUrl.Should().Be("http://eztv.it/forum/discuss/58439/");
            torrentInfo.Indexer.Should().Be(Subject.Definition.Name);
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("2014/09/15 18:39:00"));
            torrentInfo.Size.Should().Be(796606175);
            torrentInfo.InfoHash.Should().Be("20FC4FBFA88272274AC671F857CC15144E9AA83E");
            torrentInfo.MagnetUrl.Should().Be("magnet:?xt=urn:btih:ED6E7P5IQJZCOSWGOH4FPTAVCRHJVKB6&dn=S4C.I.Grombil.Cyfandir.Pell.American.Interior.PDTV.x264-MVGroup");
            torrentInfo.Peers.Should().NotHaveValue();
            torrentInfo.Seeds.Should().NotHaveValue();
        }
    }
}
