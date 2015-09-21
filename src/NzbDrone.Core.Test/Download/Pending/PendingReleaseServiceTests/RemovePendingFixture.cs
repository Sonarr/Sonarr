using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.Pending.PendingReleaseServiceTests
{
    [TestFixture]
    public class RemovePendingFixture : CoreTest<PendingReleaseService>
    {
        private List<PendingRelease> _pending;
        private Episode _episode;
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _pending = new List<PendingRelease>();

            _episode = Builder<Episode>.CreateNew()
                                       .Build();

            _movie = Builder<Movie>.CreateNew()
                                   .Build();

            Mocker.GetMock<IPendingReleaseRepository>()
                 .Setup(s => s.AllBySeriesId(It.IsAny<int>()))
                 .Returns(_pending);

            Mocker.GetMock<IPendingReleaseRepository>()
                  .Setup(s => s.All())
                  .Returns(_pending);

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetSeries(It.IsAny<int>()))
                  .Returns(new Series());

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_movie);

            Mocker.GetMock<IParsingService>()
                  .Setup(s => s.GetEpisodes(It.IsAny<ParsedEpisodeInfo>(), It.IsAny<Series>(), It.IsAny<bool>(), null))
                  .Returns(new List<Episode> { _episode });
        }

        private void AddPending(int id, int serieId, int seasonNumber, int[] episodes)
        {
            _pending.Add(new PendingRelease
             {
                 Id = id,
                 SeriesId = serieId,
                 ParsedInfo = new ParsedEpisodeInfo { SeasonNumber = seasonNumber, EpisodeNumbers = episodes }
             });
        }

        private void AddPending(int id, int movieId)
        {
            _pending.Add(new PendingRelease
            {
                Id = id,
                MovieId = movieId,
                ParsedInfo = new ParsedMovieInfo()
            });
        }

        [Test]
        public void should_remove_same_release()
        {
            AddPending(id: 1, serieId: 1, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(String.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_remove_same_release_movie()
        {
            AddPending(id: 1, movieId: 1);

            var queueId = HashConverter.GetHashInt31(String.Format("pending-{0}-ep0", 1));

            Subject.RemovePendingQueueItems(queueId);

            AssertMovieRemoved(1);
        }

        [Test]
        public void should_remove_multiple_releases_release()
        {
            AddPending(id: 1, serieId: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, serieId: 1, seasonNumber: 2, episodes: new[] { 2 });
            AddPending(id: 3, serieId: 1, seasonNumber: 2, episodes: new[] { 3 });
            AddPending(id: 4, serieId: 1, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(String.Format("pending-{0}-ep{1}", 3, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(3, 4);
        }

        [Test]
        public void should_remove_multiple_releases_release_movie()
        {
            AddPending(id: 1, movieId: 1);
            AddPending(id: 2, movieId: 2);
            AddPending(id: 3, movieId: 3);
            AddPending(id: 4, movieId: 3);

            var queueId = HashConverter.GetHashInt31(String.Format("pending-{0}-ep0", 3));

            Subject.RemovePendingQueueItems(queueId);

            AssertMovieRemoved(3);
        }

        [Test]
        public void should_not_remove_diffrent_season()
        {
            AddPending(id: 1, serieId: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, serieId: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 3, serieId: 1, seasonNumber: 3, episodes: new[] { 1 });
            AddPending(id: 4, serieId: 1, seasonNumber: 3, episodes: new[] { 1 });

            var queueId = HashConverter.GetHashInt31(String.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_diffrent_episodes()
        {
            AddPending(id: 1, serieId: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, serieId: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 3, serieId: 1, seasonNumber: 2, episodes: new[] { 2 });
            AddPending(id: 4, serieId: 1, seasonNumber: 2, episodes: new[] { 3 });

            var queueId = HashConverter.GetHashInt31(String.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1, 2);
        }

        [Test]
        public void should_not_remove_multiepisodes()
        {
            AddPending(id: 1, serieId: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, serieId: 1, seasonNumber: 2, episodes: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31(String.Format("pending-{0}-ep{1}", 1, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(1);
        }

        [Test]
        public void should_not_remove_singleepisodes()
        {
            AddPending(id: 1, serieId: 1, seasonNumber: 2, episodes: new[] { 1 });
            AddPending(id: 2, serieId: 1, seasonNumber: 2, episodes: new[] { 1, 2 });

            var queueId = HashConverter.GetHashInt31(String.Format("pending-{0}-ep{1}", 2, _episode.Id));

            Subject.RemovePendingQueueItems(queueId);

            AssertRemoved(2);
        }

        private void AssertRemoved(params int[] ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteMany(It.Is<IEnumerable<int>>(s => s.SequenceEqual(ids))));
        }

        private void AssertMovieRemoved(int ids)
        {
            Mocker.GetMock<IPendingReleaseRepository>().Verify(c => c.DeleteByMovieId(It.Is<int>(s => s == ids)));
        }
    }
}
