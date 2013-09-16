using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.Categories;

namespace NzbDrone.Core.Test.MediaFiles.MediaInfo
{
    [TestFixture]
    [DiskAccessTest]
    public class VideoFileInfoReaderFixture : CoreTest<VideoFileInfoReader>
    {
        [Test]
        public void get_runtime()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Media", "H264_sample.mp4");

            Subject.GetRunTime(path).Seconds.Should().Be(10);
        }
    }
}