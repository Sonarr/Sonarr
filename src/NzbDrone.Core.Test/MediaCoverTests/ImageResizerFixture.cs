using System;
using System.IO;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class ImageResizerFixture : CoreTest<ImageResizer>
    {
        [SetUp]
        public void SetUp()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.OpenReadStream(It.IsAny<string>()))
                  .Returns<string>(s => new FileStream(s, FileMode.Open));

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.OpenWriteStream(It.IsAny<string>()))
                  .Returns<string>(s => new FileStream(s, FileMode.Create));

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.FileExists(It.IsAny<string>()))
                  .Returns<string>(s => File.Exists(s));

            Mocker.GetMock<IDiskProvider>()
                  .Setup(v => v.DeleteFile(It.IsAny<string>()))
                  .Callback<string>(s => File.Delete(s));
        }

        [Test]
        public void should_resize_image()
        {
            var mainFile = Path.Combine(TempFolder, "logo.png");
            var resizedFile = Path.Combine(TempFolder, "logo-170.png");

            File.Copy(GetTestPath("Files/1024.png"), mainFile);

            Subject.Resize(mainFile, resizedFile, 170);

            var fileInfo = new FileInfo(resizedFile);
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeInRange(1000, 30000);

            var image = System.Drawing.Image.FromFile(resizedFile);
            image.Height.Should().Be(170);
            image.Width.Should().Be(170);
        }

        [Test]
        public void should_delete_file_if_failed()
        {
            var mainFile = Path.Combine(TempFolder, "junk.png");
            var resizedFile = Path.Combine(TempFolder, "junk-170.png");

            File.WriteAllText(mainFile, "Just some junk data that should make it throw an Exception.");

            Assert.Throws(Is.InstanceOf<Exception>(), () => Subject.Resize(mainFile, resizedFile, 170));

            File.Exists(resizedFile).Should().BeFalse();
        }
    }
}