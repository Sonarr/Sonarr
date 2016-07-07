using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk.Abstractions;
using NzbDrone.Core.Download.Clients.RssGenerator;
using NzbDrone.Core.Test.Framework;
using FluentAssertions;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.RssGenerator {

    [TestFixture]
    public class TorrentRssGeneratorCachedEpisodeMatcherFixture : CoreTest<TorrentRssGeneratorCachedEpisodeMatcher> {

        protected Mock<IFileSystemInfo> GivenMasterChefS08E16EpisodeFile() {
            var mock = new Mock<IFileSystemInfo>();

            mock.Setup(c => c.FullName)
                .Returns("C:\\foo\\bar\\MasterChef.Au.S08E16.Webrip.x264-MFO.mp4");
            mock.Setup(c => c.Name)
                .Returns("MasterChef.Au.S08E16.Webrip.x264-MFO.mp4");
            mock.Setup(c => c.LogicalName)
                .Returns("MasterChef.Au.S08E16.Webrip.x264-MFO");
            mock.Setup(c => c.Exists)
                .Returns(true);

            return mock;
        }
        protected TorrentRssGeneratorCachedEpisode GivenMasterChefS08E16CachedEpisode() {
            return new TorrentRssGeneratorCachedEpisode {
                Title = "MasterChef Au S08E16 Webrip x264-MFO mp4",
                EpisodeInfo = Parser.Parser.ParseTitle("MasterChef Au S08E16 Webrip x264-MFO mp4")
            };
        }

        [Test]
        public void ShouldMatch_given_filesysteminfo() {
            var fileinfo = this.GivenMasterChefS08E16EpisodeFile().Object;
            var episode = this.GivenMasterChefS08E16CachedEpisode();
        
            this.Subject.Matches(episode, fileinfo).Should().BeTrue();
        }
    }
}
