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
        [TestCase(1, true, true, EpisodeStatuses.Ready)]
        public void ignored_episode(int offsetDays, bool ignored, bool hasEpisodes, EpisodeStatuses status)
        {
            Episode episode = Builder<Episode>.CreateNew()
                .With(e => e.AirDate = DateTime.Now.AddDays(offsetDays))
                .With(e => e.Ignored = ignored)
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
                .With(e => e.EpisodeFileId = 0)
                .Build();


            episode.Status.Should().Be(EpisodeStatuses.NotAired);
        }
    }
}