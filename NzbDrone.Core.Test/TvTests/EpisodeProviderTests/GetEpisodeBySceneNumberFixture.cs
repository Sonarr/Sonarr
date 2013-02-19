// ReSharper disable RedundantUsingDirective

using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.TvTests.EpisodeProviderTests
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class GetEpisodeBySceneNumberFixture : SqlCeTest
    {
        private Series _series;
        private Episode _episode;

        [SetUp]
        public void Setup()
        {
            WithRealDb();

            _series = Builder<Series>
                .CreateNew()
                .Build();

            Db.Insert(_series);
        }

        public void WithNullSceneNumbering()
        {
            _episode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.SeriesId = _series.SeriesId)
                    .Build();

            Db.Insert(_episode);
            Db.Execute("UPDATE Episodes SET SceneSeasonNumber = NULL, SceneEpisodeNumber = NULL");
        }

        public void WithSceneNumbering()
        {
            _episode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.SeriesId = _series.SeriesId)
                    .Build();

            Db.Insert(_episode);
        }

        [Test]
        public void should_return_null_if_no_episodes_in_db()
        {
            Mocker.Resolve<EpisodeProvider>().GetEpisodeBySceneNumbering(_series.SeriesId, 1, 1).Should().BeNull();
        }

        [Test]
        public void should_return_null_if_no_matching_episode_is_found()
        {
            WithNullSceneNumbering();
            Mocker.Resolve<EpisodeProvider>().GetEpisodeBySceneNumbering(_series.SeriesId, 1, 1).Should().BeNull();
        }

        [Test]
        public void should_return_episode_if_matching_episode_is_found()
        {
            WithSceneNumbering();

            var result = Mocker.Resolve<EpisodeProvider>()
                .GetEpisodeBySceneNumbering(_series.SeriesId, _episode.SceneSeasonNumber, _episode.SceneEpisodeNumber);
            
            result.EpisodeId.Should().Be(_episode.EpisodeId);
        }
    }
}
