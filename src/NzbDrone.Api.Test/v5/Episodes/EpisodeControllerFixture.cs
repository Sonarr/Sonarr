using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http.HttpResults;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;
using Sonarr.Api.V5.Episodes;

namespace NzbDrone.Api.Test.v5.Episodes
{
    [TestFixture]
    public class EpisodeControllerFixture : TestBase<EpisodeController>
    {
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            _episode = new Episode
            {
                Id = 1,
                SeriesId = 1,
                EpisodeFileId = 2,
                EpisodeFile = new LazyLoaded<EpisodeFile>(null),
                Series = new Series { Id = 1 }
            };
        }

        [Test]
        public void should_return_episodes_without_file_when_episode_file_was_deleted()
        {
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodeBySeries(1))
                  .Returns(new List<Episode> { _episode });

            var result = Subject.GetEpisodes(1, null, new List<int>(), null, new[] { EpisodeSubresource.EpisodeFile });

            var episodes = result.Result.Should().BeOfType<Ok<List<EpisodeResource>>>().Subject.Value;
            episodes.Should().ContainSingle().Which.EpisodeFile.Should().BeNull();
        }

        [Test]
        public void should_return_episode_without_file_when_episode_file_was_deleted()
        {
            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisode(1))
                  .Returns(_episode);

            var result = Subject.GetResourceByIdWithErrorHandler(1);

            result.Result.Should().BeOfType<Ok<EpisodeResource>>().Subject.Value.EpisodeFile.Should().BeNull();
        }
    }
}
