using System.Text;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.TorrentInfo;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.TorrentInfo
{
    [TestFixture]
    public class TorrentFileInfoReaderFixture : CoreTest<TorrentFileInfoReader>
    {
        private static byte[] MakeSingleFileTorrent(string name)
        {
            return Encoding.UTF8.GetBytes($"d4:infod6:lengthi100e4:name{name.Length}:{name}12:piece lengthi16384e6:pieces20:{new string('a', 20)}ee");
        }

        private static byte[] MakeMultiFileTorrent(params string[] fileNames)
        {
            var builder = new StringBuilder("d4:infod5:filesl");

            foreach (var fileName in fileNames)
            {
                builder.Append($"d6:lengthi100e4:pathl{fileName.Length}:{fileName}ee");
            }

            builder.Append($"e4:name4:Show12:piece lengthi16384e6:pieces20:{new string('a', 20)}ee");

            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        [Test]
        public void should_get_file_name_from_single_file_torrent()
        {
            var fileNames = Subject.GetFileNamesFromTorrentFile(MakeSingleFileTorrent("Show.S01E01.1080p.WEB.mkv"));

            fileNames.Should().ContainSingle(f => f.EndsWith("Show.S01E01.1080p.WEB.mkv"));
        }

        [Test]
        public void should_get_file_names_from_multi_file_torrent()
        {
            var fileNames = Subject.GetFileNamesFromTorrentFile(MakeMultiFileTorrent("Show.S01E01.1080p.WEB.mkv", "Setup.exe"));

            fileNames.Should().HaveCount(2);
            fileNames.Should().Contain(f => f.EndsWith("Setup.exe"));
        }

        [Test]
        public void should_get_hash_from_torrent_file()
        {
            var hash = Subject.GetHashFromTorrentFile(MakeSingleFileTorrent("Show.S01E01.1080p.WEB.mkv"));

            hash.Should().HaveLength(40);
        }

        [Test]
        public void should_throw_when_torrent_is_invalid()
        {
            Assert.Throws<MonoTorrent.TorrentException>(() => Subject.GetFileNamesFromTorrentFile(Encoding.UTF8.GetBytes("not a torrent")));
        }
    }
}
