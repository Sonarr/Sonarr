using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class AlreadyImportedSpecificationFixture : CoreTest<AlreadyImportedSpecification>
    {
        private Series _series;
        private Episode _episode;
        private LocalEpisode _localEpisode;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.SeriesType = SeriesTypes.Standard)
                                     .With(s => s.Path = @"C:\Test\TV\30 Rock".AsOsAgnostic())
                                     .Build();

            _episode = Builder<Episode>.CreateNew()
                .With(e => e.SeasonNumber = 1)
                .With(e => e.AirDateUtc = DateTime.UtcNow)
                .Build();

            _localEpisode = new LocalEpisode
                                {
                                    Path = @"C:\Test\Unsorted\30 Rock\30.rock.s01e01.avi".AsOsAgnostic(),
                                    Episodes = new List<Episode> { _episode },
                                    Series = _series
                                };

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                .Build();
        }

        private void GivenHistory(List<EpisodeHistory> history)
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByEpisodeId(It.IsAny<int>()))
                .Returns(history);
        }

        [Test]
        public void should_accepted_if_download_client_item_is_null()
        {
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_does_not_have_file()
        {
            _episode.EpisodeFileId = 0;

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_has_not_been_imported()
        {
            var history = Builder<EpisodeHistory>.CreateListOfSize(1)
                .All()
                .With(h => h.EpisodeId = _episode.Id)
                .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_if_episode_was_grabbed_after_being_imported()
        {
            var history = Builder<EpisodeHistory>.CreateListOfSize(3)
                .All()
                .With(h => h.EpisodeId = _episode.Id)
                .TheFirst(1)
                .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow)
                .TheNext(1)
                .With(h => h.EventType = EpisodeHistoryEventType.DownloadFolderImported)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-2))
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_if_episode_imported_after_being_grabbed()
        {
            var history = Builder<EpisodeHistory>.CreateListOfSize(2)
                .All()
                .With(h => h.EpisodeId = _episode.Id)
                .TheFirst(1)
                .With(h => h.EventType = EpisodeHistoryEventType.DownloadFolderImported)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-1))
                .TheNext(1)
                .With(h => h.EventType = EpisodeHistoryEventType.Grabbed)
                .With(h => h.Date = DateTime.UtcNow.AddDays(-2))
                .Build()
                .ToList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeFalse();
        }
    }
}
