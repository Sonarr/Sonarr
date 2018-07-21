using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Nyaa;
using NzbDrone.Core.Indexers.YggTorrent;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.IndexerTests.YggTorrentTests
{
    [TestFixture]
    public class YggTorrentFixture : CoreTest<YggTorrent>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
            {
                Name = "YggTorrent",
                Settings = new YggTorrentSettings()
            };
        }

        [Test]
        public void should_parse_recent_feed_from_Nyaa()
        {
            string recentHtmlSearched = ReadAllText(@"Files/Indexers/YggTorrent/YggTorrent.html");
            YggTorrentSettings settings = ((YggTorrentSettings)Subject.Definition.Settings);

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.GET)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentHtmlSearched));

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.Is<HttpRequest>(v => v.Method == HttpMethod.POST)))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), "", HttpStatusCode.OK));

            IList<ReleaseInfo> releases = Subject.FetchRecent();

            releases.Should().NotBeEmpty();
            releases.First().Should().BeOfType<ReleaseInfo>();

            ReleaseInfo torrentInfo = releases.First();

            torrentInfo.Title.Should().Be("Here.Comes the.Boom.2012.TRUEFRENCH.BDRIP.XVID-ATN");
            torrentInfo.DownloadUrl.Should().Be($"{settings.BaseUrl}/{settings.DownloadUrlFormat}283508");
            torrentInfo.PublishDate.Should().Be(DateTime.Parse("21/07/18 15:00:52"));
            torrentInfo.Size.Should().Be(737652244);
            torrentInfo.Guid.Should().Be("TARGET-283508");
            torrentInfo.Source.Should().Be("0");
        }
    }
}
