using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Indexers.Animezb;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Test.IndexerTests.AnimezbTests
{
    [TestFixture]
    public class AnimezbFixture : CoreTest<Animezb>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = new IndexerDefinition()
                {
                    Name = "Animezb",
                    Settings = new NullConfig()
                };

        }

        [Test]
        public void should_parse_recent_feed_from_animezb()
        {
            Assert.Inconclusive("Waiting for animezb to get back up.");

            var recentFeed = ReadAllText(@"Files/RSS/Animezb.xml");

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Get(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));
            
            var releases = Subject.FetchRecent();

            releases.Should().HaveCount(3);

            var releaseInfo = releases.First();

            //releaseInfo.Title.Should().Be("[Vivid] Hanayamata - 10 [A33D6606]");
            releaseInfo.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
            //releaseInfo.DownloadUrl.Should().Be("http://fanzub.com/nzb/296464/Vivid%20Hanayamata%20-%2010.nzb");
            releaseInfo.Indexer.Should().Be(Subject.Definition.Name);
            //releaseInfo.PublishDate.Should().Be(DateTime.Parse("2014/09/13 12:56:53"));
            //releaseInfo.Size.Should().Be(556246858);
        }
    }
}
