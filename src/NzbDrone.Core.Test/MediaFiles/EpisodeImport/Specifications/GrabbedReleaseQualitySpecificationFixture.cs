using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Marr.Data;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.EpisodeImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeImport.Specifications
{
    [TestFixture]
    public class GrabbedReleaseQualitySpecificationFixture : CoreTest<GrabbedReleaseQualitySpecification>
    {
        private LocalEpisode _localEpisode;
        private DownloadClientItem _downloadClientItem;

        [SetUp]
        public void Setup()
        {
            var profile = new Profile
                          {
                              Cutoff = Quality.Bluray1080p.Id,
                              Items = new List<ProfileQualityItem>
                                      {
                                          new ProfileQualityItem
                                          {
                                              Allowed = true,
                                              Quality = Quality.HDTV720p
                                          },
                                          new ProfileQualityItem
                                          {
                                              Id = 1000,
                                              Allowed = true,
                                              Items = new List<ProfileQualityItem>
                                                      {
                                                          new ProfileQualityItem
                                                          {
                                                              Allowed = true,
                                                              Quality = Quality.WEBRip720p
                                                          },
                                                          new ProfileQualityItem
                                                          {
                                                              Allowed = true,
                                                              Quality = Quality.WEBDL720p
                                                          },
                                                      }
                                          },
                                          new ProfileQualityItem
                                          {
                                              Allowed = true,
                                              Quality = Quality.Bluray720p
                                          }
                                      }
                          };

            var series = Builder<Series>.CreateNew()
                                        .With(c => c.Profile = (LazyLoaded<Profile>)profile)
                                        .Build();

            _localEpisode = Builder<LocalEpisode>.CreateNew()
                                                 .With(l => l.Series = series)
                                                 .With(l => l.Quality = new QualityModel(Quality.Bluray720p))
                                                 .Build();

            _downloadClientItem = Builder<DownloadClientItem>.CreateNew()
                                                             .Build();
        }

        private void GivenHistory(List<History.History> history)
        {
            Mocker.GetMock<IHistoryService>()
                  .Setup(s => s.FindByDownloadId(It.IsAny<string>()))
                  .Returns(history);
        }

        [Test]
        public void should_be_accepted_when_downloadClientItem_is_null()
        {
            Subject.IsSatisfiedBy(_localEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_history_for_downloadId()
        {
            GivenHistory(new List<History.History>());

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_no_grabbed_history_for_downloadId()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Unknown)
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_history_is_for_a_season_pack()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Grabbed)
                                                  .With(h => h.Quality = _localEpisode.Quality)
                                                  .With(h => h.SourceTitle = "Series.Title.S01.720p.HDTV.x264-RlsGroup")
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_history_quality_is_unknown()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Grabbed)
                                                  .With(h => h.Quality = new QualityModel(Quality.Unknown))
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_accepted_if_grabbed_history_quality_matches()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Grabbed)
                                                  .With(h => h.Quality = _localEpisode.Quality)
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_rejected_if_grabbed_history_quality_does_not_match()
        {
            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Grabbed)
                                                  .With(h => h.Quality = new QualityModel(Quality.HDTV720p))
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_be_accepted_if_history_quality_and_file_quality_are_different_but_are_in_the_same_group()
        {
            _localEpisode.Quality = new QualityModel(Quality.WEBDL720p);

            var history = Builder<History.History>.CreateListOfSize(1)
                                                  .All()
                                                  .With(h => h.EventType = HistoryEventType.Grabbed)
                                                  .With(h => h.Quality = new QualityModel(Quality.WEBRip720p))
                                                  .BuildList();

            GivenHistory(history);

            Subject.IsSatisfiedBy(_localEpisode, _downloadClientItem).Accepted.Should().BeTrue();
        }
    }
}
