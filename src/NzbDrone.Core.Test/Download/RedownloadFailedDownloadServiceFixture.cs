using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class RedownloadFailedDownloadServiceFixture : CoreTest<RedownloadFailedDownloadService>
    {
        private List<Episode> _episodes;

        [SetUp]
        public void Setup()
        {
            _episodes = Enumerable.Range(1, 3)
                .SelectMany(s => Enumerable.Range(1, 2).Select(e => new Episode { Id = (s * 10) + e, SeriesId = 1, SeasonNumber = s, EpisodeNumber = e }))
                .ToList();

            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoRedownloadFailed)
                  .Returns(true);

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodes(It.IsAny<IEnumerable<int>>()))
                  .Returns<IEnumerable<int>>(ids => _episodes.Where(e => ids.Contains(e.Id)).ToList());

            Mocker.GetMock<IEpisodeService>()
                  .Setup(s => s.GetEpisodesBySeason(1, It.IsAny<int>()))
                  .Returns<int, int>((_, seasonNumber) => _episodes.Where(e => e.SeasonNumber == seasonNumber).ToList());
        }

        private void GivenFailedDownload(IEnumerable<Episode> episodes)
        {
            Subject.Handle(new DownloadFailedEvent
            {
                SeriesId = 1,
                EpisodeIds = episodes.Select(e => e.Id).ToList()
            });
        }

        private void VerifySeasonSearch(int seasonNumber)
        {
            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.Is<SeasonSearchCommand>(c => c.SeriesId == 1 && c.SeasonNumber == seasonNumber), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Once());
        }

        private void VerifyEpisodeSearch(IEnumerable<Episode> episodes)
        {
            var episodeIds = episodes.Select(e => e.Id).ToList();

            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.Is<EpisodeSearchCommand>(c => c.EpisodeIds.SequenceEqual(episodeIds)), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Once());
        }

        private void VerifyNoSeasonSearch()
        {
            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<SeasonSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }

        private void VerifyNoEpisodeSearch()
        {
            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<EpisodeSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), Times.Never());
        }

        [Test]
        public void should_not_search_if_auto_redownload_is_disabled()
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.AutoRedownloadFailed)
                  .Returns(false);

            GivenFailedDownload(_episodes);

            VerifyNoSeasonSearch();
            VerifyNoEpisodeSearch();
        }

        [Test]
        public void should_search_for_episode_if_download_contained_one_episode()
        {
            var episodes = _episodes.Take(1).ToList();

            GivenFailedDownload(episodes);

            VerifyEpisodeSearch(episodes);
            VerifyNoSeasonSearch();
        }

        [Test]
        public void should_search_for_season_if_download_contained_entire_season()
        {
            GivenFailedDownload(_episodes.Where(e => e.SeasonNumber == 2));

            VerifySeasonSearch(2);
            VerifyNoEpisodeSearch();
        }

        [Test]
        public void should_search_for_episodes_if_download_contained_part_of_a_season()
        {
            _episodes.Add(new Episode { Id = 13, SeriesId = 1, SeasonNumber = 1, EpisodeNumber = 3 });

            var episodes = _episodes.Where(e => e.SeasonNumber == 1).Take(2).ToList();

            GivenFailedDownload(episodes);

            VerifyEpisodeSearch(episodes);
            VerifyNoSeasonSearch();
        }

        [Test]
        public void should_search_for_each_season_if_download_contained_multiple_entire_seasons()
        {
            GivenFailedDownload(_episodes);

            VerifySeasonSearch(1);
            VerifySeasonSearch(2);
            VerifySeasonSearch(3);
            VerifyNoEpisodeSearch();
        }

        [Test]
        public void should_search_for_episodes_of_partial_seasons_in_multi_season_download()
        {
            var partialSeasonEpisode = _episodes.Single(e => e.SeasonNumber == 3 && e.EpisodeNumber == 1);
            var episodes = _episodes.Where(e => e.SeasonNumber < 3).Append(partialSeasonEpisode).ToList();

            GivenFailedDownload(episodes);

            VerifySeasonSearch(1);
            VerifySeasonSearch(2);
            VerifyEpisodeSearch([partialSeasonEpisode]);
        }
    }
}
