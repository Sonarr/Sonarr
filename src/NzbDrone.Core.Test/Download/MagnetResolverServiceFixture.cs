using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Download;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class MagnetResolverServiceFixture : CoreTest<MagnetResolverService>
    {
        private byte[] _correctTorrent;

        [SetUp]
        public void SetUp()
        {
            _correctTorrent = System.IO.File.ReadAllBytes(@"Files/Download/640FE84C613C17F663551D218689A64E8AEBEABE.torrent.raw");
        }

        private void WithCorrectTorrent()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(v => v.Get(It.Is<HttpRequest>(r => r.Url.AbsoluteUri == "http://torcache.net/torrent/640FE84C613C17F663551D218689A64E8AEBEABE.torrent")))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader() { ContentType = "application/x-bittorrent" }, (byte[])_correctTorrent.Clone()));
        }

        private void WithWrongContentType()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(v => v.Get(It.Is<HttpRequest>(r => r.Url.AbsoluteUri == "http://torcache.net/torrent/640FE84C613C17F663551D218689A64E8AEBEABE.torrent")))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader() { ContentType = "text/html" }, (byte[])_correctTorrent.Clone()));
        }

        [Test]
        public void should_fetch_magnet_from_torcache()
        {
            WithCorrectTorrent();

            var data = Subject.DownloadTorrentFromMagnet("magnet:?xt=urn:btih:640FE84C613C17F663551D218689A64E8AEBEABE&dn=Test+Name+Torrent");

            data.Length.Should().Be(_correctTorrent.Length);
        }

        [Test]
        public void should_append_announceurls()
        {
            WithCorrectTorrent();

            var data = Subject.DownloadTorrentFromMagnet("magnet:?xt=urn:btih:640FE84C613C17F663551D218689A64E8AEBEABE&dn=Test+Name+Torrent&tr=udp%3A%2F%2Ftracker.publicbt.com%3A80%2Fannounce&tr=udp%3A%2F%2Fglotorrents.pw%3A6969%2Fannounce");

            data.Length.Should().BeGreaterThan(_correctTorrent.Length);
        }

        [Test]
        public void should_return_null_on_wrong_contenttype()
        {
            WithWrongContentType();

            var data = Subject.DownloadTorrentFromMagnet("magnet:?xt=urn:btih:640FE84C613C17F663551D218689A64E8AEBEABE&dn=Test+Name+Torrent");

            data.Should().BeNull();
        }

        [Test]
        public void should_return_null_on_exception()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(v => v.Get(It.Is<HttpRequest>(r => r.Url.AbsoluteUri == "http://torcache.net/torrent/640FE84C613C17F663551D218689A64E8AEBEABE.torrent")))
                .Throws<Exception>();

            var data = Subject.DownloadTorrentFromMagnet("magnet:?xt=urn:btih:640FE84C613C17F663551D218689A64E8AEBEABE&dn=Test+Name+Torrent");

            data.Should().BeNull();
        }
    }
}
