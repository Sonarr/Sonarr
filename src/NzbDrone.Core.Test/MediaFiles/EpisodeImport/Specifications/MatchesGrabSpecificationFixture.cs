using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class MatchesGrabSpecificationFixture : CoreTest<MatchesGrabSpecification>
    {
        private Episode _episode1;
        private Episode _episode2;
        private Episode _episode3;
        private LocalEpisode _localEpisode;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _episode1 = Builder<Episode>.CreateNew()
                .With(e => e.Id = 1)
                .Build();

            _episode2 = Builder<Episode>.CreateNew()
                .With(e => e.Id = 2)
                .Build();

            _episode3 = Builder<Episode>.CreateNew()
                .With(e => e.Id = 3)
                .Build();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Path = @"C:\Test\Unsorted\Series.Title.S01E01.720p.HDTV-Sonarr\S01E05.mkv".AsOsAgnostic())
                                                 .With(l => l.Episodes = new List<Episode> { _episode1 })
                                                 .With(l => l.Release = null)
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew().Build();
        }

        private void GivenHistoryForEpisodes(params Episode[] episodes)
        {
            _localEpisode.Release = new GrabbedReleaseInfo();
            _localEpisode.Release.EpisodeIds = episodes.Select(e => e.Id).ToList();
        }

        [Test]
        public void should_be_accepted_for_existing_file()
        {
            _localEpisode.ExistingFile = true;

            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_download_client_item()
        {
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_grabbed_release_info()
        {
            GivenHistoryForEpisodes();

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_episode_matches_single_grabbed_release_info()
        {
            GivenHistoryForEpisodes(_episode1);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_file_episode_is_in_multi_episode_grabbed_release_info()
        {
            GivenHistoryForEpisodes(_episode1, _episode2);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_file_episode_does_not_match_single_grabbed_release_info()
        {
            GivenHistoryForEpisodes(_episode2);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_rejected_if_file_episode_is_not_in_multi_episode_grabbed_release_info()
        {
            GivenHistoryForEpisodes(_episode2, _episode3);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
