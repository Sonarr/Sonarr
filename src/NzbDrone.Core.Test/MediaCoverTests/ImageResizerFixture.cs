using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
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
        }

        [Test]
        public void should_resize_image()
        {
            var mainFile = Path.Combine(TempFolder, "logo.png");
            var resizedFile = Path.Combine(TempFolder, "logo-170.png");

            File.Copy(@"Files/1024.png", mainFile);

            Subject.Resize(mainFile, resizedFile, 170);

            var fileInfo = new FileInfo(resizedFile);
            fileInfo.Exists.Should().BeTrue();
            fileInfo.Length.Should().BeInRange(1000, 30000);

            var image = System.Drawing.Image.FromFile(resizedFile);
            image.Height.Should().Be(170);
            image.Width.Should().Be(170);
        }
    }
}