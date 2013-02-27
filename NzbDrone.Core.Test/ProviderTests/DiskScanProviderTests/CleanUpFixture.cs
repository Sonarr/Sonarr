using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    // ReSharper disable InconsistentNaming
    public class CleanUpFixture : CoreTest
    {
        [Test]
        public void should_skip_existing_files()
        {
            var episodes = Builder<EpisodeFile>.CreateListOfSize(10).Build();

            Mocker.GetMock<DiskProvider>()
                .Setup(e => e.FileExists(It.IsAny<String>()))
                .Returns(true);


            //Act
            Mocker.Resolve<DiskScanProvider>().CleanUp(episodes);

            //Assert
            Mocker.VerifyAllMocks();
        }

        [Test]
        public void should_delete_none_existing_files()
        {
            var episodes = Builder<EpisodeFile>.CreateListOfSize(10).Build();

            Mocker.GetMock<DiskProvider>()
                .Setup(e => e.FileExists(It.IsAny<String>()))
                .Returns(false);

            Mocker.GetMock<IEpisodeService>()
                .Setup(e => e.GetEpisodesByFileId(It.IsAny<int>()))
                .Returns(new List<Episode>());

            Mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.Delete(It.IsAny<int>()));


            //Act
            Mocker.Resolve<DiskScanProvider>().CleanUp(episodes);

            //Assert
            Mocker.VerifyAllMocks();

            Mocker.GetMock<IEpisodeService>()
               .Verify(e => e.GetEpisodesByFileId(It.IsAny<int>()), Times.Exactly(10));

            Mocker.GetMock<MediaFileProvider>()
                .Verify(e => e.Delete(It.IsAny<int>()), Times.Exactly(10));

        }

        [Test]
        public void should_delete_none_existing_files_remove_links_to_episodes()
        {
            var episodes = Builder<EpisodeFile>.CreateListOfSize(10).Build();

            Mocker.GetMock<DiskProvider>()
                .Setup(e => e.FileExists(It.IsAny<String>()))
                .Returns(false);

            Mocker.GetMock<IEpisodeService>()
                .Setup(e => e.GetEpisodesByFileId(It.IsAny<int>()))
                .Returns(new List<Episode> { new Episode { EpisodeFile = new EpisodeFile { EpisodeFileId = 10 } }, new Episode { EpisodeFile = new EpisodeFile { EpisodeFileId = 10 } } });

            Mocker.GetMock<IEpisodeService>()
                .Setup(e => e.UpdateEpisode(It.IsAny<Episode>()));

            Mocker.GetMock<MediaFileProvider>()
                .Setup(e => e.Delete(It.IsAny<int>()));

            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.AutoIgnorePreviouslyDownloadedEpisodes)
                .Returns(true);

            //Act
            Mocker.Resolve<DiskScanProvider>().CleanUp(episodes);

            //Assert
            Mocker.VerifyAllMocks();

            Mocker.GetMock<IEpisodeService>()
               .Verify(e => e.GetEpisodesByFileId(It.IsAny<int>()), Times.Exactly(10));

            Mocker.GetMock<IEpisodeService>()
                .Verify(e => e.UpdateEpisode(It.Is<Episode>(g => g.EpisodeFileId == 0)), Times.Exactly(20));

            Mocker.GetMock<MediaFileProvider>()
                .Verify(e => e.Delete(It.IsAny<int>()), Times.Exactly(10));

            Mocker.GetMock<MediaFileProvider>()
                .Verify(e => e.Delete(It.IsAny<int>()), Times.Exactly(10));

        }
    }
}
