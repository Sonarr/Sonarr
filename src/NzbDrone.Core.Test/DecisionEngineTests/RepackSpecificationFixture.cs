using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class RepackSpecificationFixture : CoreTest<RepackSpecification>
    {
        private ParsedEpisodeInfo _parsedEpisodeInfo;
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _parsedEpisodeInfo = Builder<ParsedEpisodeInfo>.CreateNew()
                                                           .With(p => p.Quality = new QualityModel(Quality.SDTV,
                                                               new Revision(2, 0, false)))
                                                           .With(p => p.ReleaseGroup = "Sonarr")
                                                           .Build();

            _episodes = Builder<Episode>.CreateListOfSize(1)
                                        .All()
                                        .With(e => e.EpisodeFileId = 0)
                                        .BuildList();
        }

        [Test]
        public void should_return_true_if_it_is_not_a_repack()
        {
            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(e => e.ParsedEpisodeInfo = _parsedEpisodeInfo)
                                                      .With(e => e.Episodes = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteEpisode, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_there_are_is_no_episode_file()
        {
            _parsedEpisodeInfo.Quality.Revision.IsRepack = true;

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(e => e.ParsedEpisodeInfo = _parsedEpisodeInfo)
                                                      .With(e => e.Episodes = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteEpisode, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_repack_for_existing_file()
        {
            _parsedEpisodeInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().EpisodeFile = Builder<EpisodeFile>.CreateNew()
                                                                .With(e => e.ReleaseGroup = "Sonarr")
                                                                .Build();

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(e => e.ParsedEpisodeInfo = _parsedEpisodeInfo)
                                                      .With(e => e.Episodes = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteEpisode, null)
                   .Accepted
                   .Should()
                   .BeTrue();
        }

        [Test]
        public void should_return_false_if_is_a_repack_for_existing_file()
        {
            _parsedEpisodeInfo.Quality.Revision.IsRepack = true;
            _episodes.First().EpisodeFileId = 1;
            _episodes.First().EpisodeFile = Builder<EpisodeFile>.CreateNew()
                                                                .With(e => e.ReleaseGroup = "NotSonarr")
                                                                .Build();

            var remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                                                      .With(e => e.ParsedEpisodeInfo = _parsedEpisodeInfo)
                                                      .With(e => e.Episodes = _episodes)
                                                      .Build();

            Subject.IsSatisfiedBy(remoteEpisode, null)
                   .Accepted
                   .Should()
                   .BeFalse();
        }
    }
}
