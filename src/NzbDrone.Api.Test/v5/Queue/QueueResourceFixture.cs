using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using Sonarr.Api.V5.Queue;

namespace NzbDrone.Api.Test.v5.Queue
{
    [TestFixture]
    public class QueueResourceFixture
    {
        [Test]
        public void should_group_episodes_by_season()
        {
            var queue = new NzbDrone.Core.Queue.Queue
            {
                Episodes =
                [
                    new Episode { Id = 1, SeasonNumber = 1, EpisodeFileId = 1 },
                    new Episode { Id = 2, SeasonNumber = 1 },
                    new Episode { Id = 3, SeasonNumber = 2 }
                ]
            };

            var resource = queue.ToResource(false, false);

            resource.Seasons.Should().BeEquivalentTo(new List<QueueSeasonResource>
            {
                new() { SeasonNumber = 1, EpisodeCount = 2, EpisodesWithFilesCount = 1 },
                new() { SeasonNumber = 2, EpisodeCount = 1, EpisodesWithFilesCount = 0 }
            });
        }

        [Test]
        public void should_return_empty_seasons_when_queue_item_has_no_episodes()
        {
            var queue = new NzbDrone.Core.Queue.Queue
            {
                Episodes = []
            };

            var resource = queue.ToResource(false, false);

            resource.Seasons.Should().BeEmpty();
        }
    }
}
