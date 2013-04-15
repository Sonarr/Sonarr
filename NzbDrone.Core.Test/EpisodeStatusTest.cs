using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;

using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    
    public class EpisodeStatusTest : CoreTest
    {
        [TestCase(1, false, false, EpisodeStatuses.NotAired)]
        [TestCase(-2, false, false, EpisodeStatuses.Missing)]
        [TestCase(0, false, false, EpisodeStatuses.AirsToday)]
        [TestCase(1, true, false, EpisodeStatuses.Ready)]
        public void no_grab_date(int offsetDays, bool hasEpisodes, bool ignored, EpisodeStatuses status)
        {
            Episode episode = Builder<Episode>.CreateNew()
                .With(e => e.AirDate = DateTime.Now.AddDays(offsetDays))
                .With(e => e.Ignored = ignored)
                .With(e => e.GrabDate = null)
                .Build();

            if (hasEpisodes)
            {
                episode.EpisodeFile = new EpisodeFile();
            }

            episode.Status.Should().Be(status);
        }

        [TestCase(1, false, false, EpisodeStatuses.Missing)]
        [TestCase(-2, false, false, EpisodeStatuses.Missing)]
        [TestCase(1, true, false, EpisodeStatuses.Ready)]
        public void old_grab_date(int offsetDays, bool hasEpisodes, bool ignored,
                                                 EpisodeStatuses status)
        {
            Episode episode = Builder<Episode>.CreateNew()
               .With(e => e.Ignored = ignored)
                .With(e => e.GrabDate = DateTime.Now.AddDays(-2).AddHours(-1))
                .With(e => e.AirDate = DateTime.Today.AddDays(-2))
                .Build();

            if (hasEpisodes)
            {
                episode.EpisodeFile = new EpisodeFile();
            }

            episode.Status.Should().Be(status);
        }

        [TestCase(1, false, false, EpisodeStatuses.Downloading)]
        [TestCase(-2, false, false, EpisodeStatuses.Downloading)]
        [TestCase(1, true, false, EpisodeStatuses.Ready)]
        [TestCase(1, true, true, EpisodeStatuses.Ready)]
        [TestCase(1, false, true, EpisodeStatuses.Downloading)]
        public void recent_grab_date(int offsetDays, bool hasEpisodes, bool ignored,
                                                    EpisodeStatuses status)
        {
            Episode episode = Builder<Episode>.CreateNew()
            .With(e => e.AirDate = DateTime.Now.AddDays(offsetDays))
                .With(e => e.Ignored = ignored)
                .With(e => e.GrabDate = DateTime.Now.AddHours(22))
                .Build();

            if (hasEpisodes)
            {
                episode.EpisodeFile = new EpisodeFile();
            }

            episode.Status.Should().Be(status);

        }

        [TestCase(1, true, true, EpisodeStatuses.Ready)]
        public void ignored_episode(int offsetDays, bool ignored, bool hasEpisodes, EpisodeStatuses status)
        {
            Episode episode = Builder<Episode>.CreateNew()
                .With(e => e.AirDate = DateTime.Now.AddDays(offsetDays))
                .With(e => e.Ignored = ignored)
                .With(e => e.GrabDate = null)
                .Build();

            if (hasEpisodes)
            {
                episode.EpisodeFile = new EpisodeFile();
            }

            episode.Status.Should().Be(status);

        }

        [Test]
        public void low_air_date()
        {
            Episode episode = Builder<Episode>.CreateNew()
                .With(e => e.AirDate = DateTime.Now.AddDays(20))
                .With(e => e.Ignored = false)
                .With(e => e.GrabDate = null)
                .Build();


            episode.Status.Should().Be(EpisodeStatuses.NotAired);
        }

        [TestCase(false, false, EpisodeStatuses.Failed, PostDownloadStatusType.Failed)]
        [TestCase(false, false, EpisodeStatuses.Unpacking, PostDownloadStatusType.Unpacking)]
        [TestCase(true, false, EpisodeStatuses.Ready, PostDownloadStatusType.Failed)]
        [TestCase(true, true, EpisodeStatuses.Ready, PostDownloadStatusType.Unpacking)]
        public void episode_downloaded_post_download_status_is_used(bool hasEpisodes, bool ignored,
                                                    EpisodeStatuses status, PostDownloadStatusType postDownloadStatus)
        {
            Episode episode = Builder<Episode>.CreateNew()
                .With(e => e.Ignored = ignored)
                .With(e => e.GrabDate = DateTime.Now.AddHours(22))
                .With(e => e.PostDownloadStatus = postDownloadStatus)
                .Build();

            if (hasEpisodes)
            {
                episode.EpisodeFile = new EpisodeFile();
            }

            episode.Status.Should().Be(status);

        }
    }
}