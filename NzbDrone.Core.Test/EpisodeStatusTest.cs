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
    // ReSharper disable InconsistentNaming
    public class EpisodeStatusTest : CoreTest
    {
        [TestCase(1, false, false, EpisodeStatusType.NotAired)]
        [TestCase(-2, false, false, EpisodeStatusType.Missing)]
        [TestCase(0, false, false, EpisodeStatusType.AirsToday)]
        [TestCase(1, true, false, EpisodeStatusType.Ready)]
        public void no_grab_date(int offsetDays, bool hasEpisodes, bool ignored, EpisodeStatusType status)
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

        [TestCase(1, false, false, EpisodeStatusType.Missing)]
        [TestCase(-2, false, false, EpisodeStatusType.Missing)]
        [TestCase(1, true, false, EpisodeStatusType.Ready)]
        public void old_grab_date(int offsetDays, bool hasEpisodes, bool ignored,
                                                 EpisodeStatusType status)
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

        [TestCase(1, false, false, EpisodeStatusType.Downloading)]
        [TestCase(-2, false, false, EpisodeStatusType.Downloading)]
        [TestCase(1, true, false, EpisodeStatusType.Ready)]
        [TestCase(1, true, true, EpisodeStatusType.Ready)]
        [TestCase(1, false, true, EpisodeStatusType.Downloading)]
        public void recent_grab_date(int offsetDays, bool hasEpisodes, bool ignored,
                                                    EpisodeStatusType status)
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

        [TestCase(1, true, true, EpisodeStatusType.Ready)]
        public void ignored_episode(int offsetDays, bool ignored, bool hasEpisodes, EpisodeStatusType status)
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


            episode.Status.Should().Be(EpisodeStatusType.NotAired);
        }

        [TestCase(false, false, EpisodeStatusType.Failed, PostDownloadStatusType.Failed)]
        [TestCase(false, false, EpisodeStatusType.Unpacking, PostDownloadStatusType.Unpacking)]
        [TestCase(true, false, EpisodeStatusType.Ready, PostDownloadStatusType.Failed)]
        [TestCase(true, true, EpisodeStatusType.Ready, PostDownloadStatusType.Unpacking)]
        public void episode_downloaded_post_download_status_is_used(bool hasEpisodes, bool ignored,
                                                    EpisodeStatusType status, PostDownloadStatusType postDownloadStatus)
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