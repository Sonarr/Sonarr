using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.TrackedDownloads
{
    [TestFixture]
    public class TrackedDownloadServiceFixture : CoreTest<TrackedDownloadService>
    {
        private void GivenDownloadHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<History.History>(){
                 new History.History(){
                     DownloadId = "35238",
                     SourceTitle = "TV Series S01",
                     SeriesId = 5,
                     EpisodeId = 4
                 }
                });
        }

        private void GivenMovieDownloadHistory()
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByDownloadId(It.Is<string>(sr => sr == "35238")))
                .Returns(new List<History.History>(){
                 new History.History(){
                     DownloadId = "35238",
                     SourceTitle = "Movie Title 2015",
                     MovieId = 5
                 }
                });
        }

        [Test]
        public void should_track_downloads_using_the_source_title_if_it_cannot_be_found_using_the_download_title()
        {
            GivenDownloadHistory();

            var remoteEpisode = new RemoteEpisode
            {
                Series = new Series() { Id = 5 },
                Episodes = new List<Episode> { new Episode { Id = 4 } },
                ParsedEpisodeInfo = new ParsedEpisodeInfo()
                {
                    Title = "TV Series",
                    SeasonNumber = 1
                }
            };

            Mocker.GetMock<IParsingService>()
               .Setup(s => s.Map(It.Is<ParsedEpisodeInfo>(i => i.SeasonNumber == 1 && i.Title == "TV Series"), It.IsAny<int>(), It.IsAny<IEnumerable<int>>()))
               .Returns(remoteEpisode);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "The torrent release folder",
                DownloadId = "35238",
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteItem.Should().NotBeNull();
            trackedDownload.RemoteItem.Media.Should().NotBeNull();
            trackedDownload.RemoteItem.Media.Id.Should().Be(5);
            (trackedDownload.RemoteItem as RemoteEpisode).Episodes.First().Id.Should().Be(4);
            (trackedDownload.RemoteItem.ParsedInfo as ParsedEpisodeInfo).SeasonNumber.Should().Be(1);
        }

        [Test]
        public void should_track_downloads_using_the_movie_source_title_if_it_cannot_be_found_using_the_download_title()
        {
            GivenMovieDownloadHistory();

            var remoteMovie = new RemoteMovie
            {
                Movie = new Movie() { Id = 5 },
                ParsedMovieInfo = new ParsedMovieInfo()
                {
                    Title = "Movie Title"
                }
            };

            Mocker.GetMock<IParsingService>()
               .Setup(s => s.Map(It.Is<ParsedMovieInfo>(i => i.Title == "Movie Title"), It.IsAny<int>()))
               .Returns(remoteMovie);

            var client = new DownloadClientDefinition()
            {
                Id = 1,
                Protocol = DownloadProtocol.Torrent
            };

            var item = new DownloadClientItem()
            {
                Title = "The torrent release folder",
                DownloadId = "35238",
            };

            var trackedDownload = Subject.TrackDownload(client, item);

            trackedDownload.Should().NotBeNull();
            trackedDownload.RemoteItem.Should().NotBeNull();
            trackedDownload.RemoteItem.Media.Should().NotBeNull();
            trackedDownload.RemoteItem.Media.Id.Should().Be(5);
        }

    }
}
