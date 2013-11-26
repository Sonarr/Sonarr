using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Test.Framework;
using System.Linq;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class MediaCoverServiceFixture : CoreTest<MediaCoverService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.SetConstant<IAppFolderInfo>(new AppFolderInfo(new DiskProvider(), Mocker.Resolve<IStartupContext>()));
        }

        [Test]
        public void should_convert_trakts_urls_to_local()
        {
            var covers = new List<MediaCover.MediaCover>
                {
                    new MediaCover.MediaCover {CoverType = MediaCoverTypes.Banner}
                };

            Mocker.GetMock<IDiskProvider>().Setup(c => c.GetLastFileWrite(It.IsAny<string>()))
                  .Returns(new DateTime(1234));

            Mocker.GetMock<IDiskProvider>().Setup(c => c.FileExists(It.IsAny<string>()))
                  .Returns(true);

            Subject.ConvertToLocalUrls(12, covers);


            covers.Single().Url.Should().Be("/MediaCover/12/banner.jpg?lastWrite=1234");
        }

        [Test]
        public void should_convert_trakts_urls_to_local_without_time_if_file_doesnt_exist()
        {
            var covers = new List<MediaCover.MediaCover>
                {
                    new MediaCover.MediaCover {CoverType = MediaCoverTypes.Banner}
                };


            Subject.ConvertToLocalUrls(12, covers);


            covers.Single().Url.Should().Be("/MediaCover/12/banner.jpg");
        }

    }
}