using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.RssSync;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Test.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistorySpecificationFixture : CoreTest<HistorySpecification>
    {
        private RemoteEpisode _remoteEpisode;
        private Series _series;
        private Episode _episode;
        private QualityModel _quality;
        private QualityProfile _profile;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                .With(s => s.Id = 1)
                .Build();

            _episode = Builder<Episode>.CreateNew()
                .With(e => e.Id = 1)
                .With(e => e.SeriesId = _series.Id)
                .Build();

            _quality = new QualityModel(Quality.HDTV720p);
            _series.QualityProfile = _profile;

            _remoteEpisode = Builder<RemoteEpisode>.CreateNew()
                .With(r => r.Series = _series)
                .With(r => r.Episodes = new List<Episode> { _episode })
                .With(r => r.ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = _quality })
                .With(r => r.CustomFormats = new List<CustomFormat>())
                .With(r => r.CustomFormatScore = 0)
                .Build();

            _profile = new QualityProfile
            {
                Cutoff = Quality.HDTV720p.Id,
                Items = QualityFixture.GetDefaultQualities(),
                Name = "Test"
            };
        }

        private void SetupHistoryServiceMock(List<EpisodeHistory> history)
        {
            Mocker.GetMock<IHistoryService>()
                .Setup(s => s.FindByEpisodeId(It.IsAny<int>()))
                .Returns(history);
        }

        private void SetupCdh(bool cdhEnabled)
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.EnableCompletedDownloadHandling)
                .Returns(cdhEnabled);
        }

        private List<EpisodeHistory> GivenFileHistory(DateTime date, QualityModel quality, EpisodeHistoryEventType eventType = EpisodeHistoryEventType.SeriesFolderImported)
        {
            return new List<EpisodeHistory>
            {
                new()
                {
                    Date = date,
                    Quality = quality,
                    EventType = eventType
                }
            };
        }

        private RemoteEpisode MockUpOrDownGrade(int customFormatScoreHistory, int customFormatScoreNew, QualityModel newQuality, int cutoff, bool hasFile = false)
        {
            _profile.FormatItems = new List<ProfileFormatItem>
            {
                new()
                {
                    Format = new CustomFormat(),
                    Score = customFormatScoreHistory
                }
            };
            _profile.Cutoff = cutoff;

            var episode = Builder<Episode>.CreateNew()
                .With(e => e.Id = 1)
                .With(e => e.EpisodeFileId = hasFile ? 1 : 0)
                .Build();

            var remoteEpisode = new RemoteEpisode
            {
                Episodes = new List<Episode> { episode },
                Series = new Series
                {
                    QualityProfile = _profile
                },
                CustomFormatScore = customFormatScoreNew,
                ParsedEpisodeInfo = new ParsedEpisodeInfo { Quality = newQuality },
            };

            Mocker.GetMock<ICustomFormatCalculationService>()
                .Setup(s => s.ParseCustomFormat(It.IsAny<EpisodeHistory>(), It.IsAny<Series>()))
                .Returns(new List<CustomFormat>()
                {
                    _profile.FormatItems.First().Format
                });
            return remoteEpisode;
        }

        [Test]
        public void should_accept_if_no_history()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_grabbed_history_is_old_and_cdh_enabled_when_no_quality_update()
        {
            var newQuality = new QualityModel(Quality.HDTV720p);
            var remoteEpisode = MockUpOrDownGrade(3000, 3000, newQuality, Quality.HDTV720p.Id, true);
            var history = GivenFileHistory(DateTime.UtcNow.AddHours(-13), _quality, EpisodeHistoryEventType.Grabbed);
            history.AddRange(GivenFileHistory(DateTime.UtcNow.AddHours(-13), _quality));
            SetupHistoryServiceMock(history);
            SetupCdh(true);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_when_grabbed_history_is_old_and_cdh_enabled_when_quality_update()
        {
            var newQuality = new QualityModel(Quality.Bluray1080p);
            var remoteEpisode = MockUpOrDownGrade(3000, 3000, newQuality, Quality.WEBRip2160p.Id);
            var history = GivenFileHistory(DateTime.UtcNow.AddHours(-13), _quality, EpisodeHistoryEventType.Grabbed);
            SetupHistoryServiceMock(history);
            SetupCdh(true);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeTrue();
        }

        [Test]
        public void should_accept_when_grabbed_history_is_old_and_cdh_enabled_when_custom_format_score_update()
        {
            var newQuality = new QualityModel(Quality.Bluray1080p);
            var remoteEpisode = MockUpOrDownGrade(3000, 8000, newQuality, Quality.WEBRip1080p.Id);
            var history = GivenFileHistory(DateTime.UtcNow.AddHours(-13), _quality, EpisodeHistoryEventType.Grabbed);
            history.AddRange(GivenFileHistory(DateTime.UtcNow.AddHours(-13), _quality));
            SetupHistoryServiceMock(history);
            SetupCdh(true);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_grabbed_history_meets_cutoff_and_is_recent()
        {
            var betterQuality = new QualityModel(Quality.Bluray1080p);
            var history = GivenFileHistory(DateTime.UtcNow.AddHours(-1), betterQuality, EpisodeHistoryEventType.Grabbed);
            var newQuality = new QualityModel(Quality.Bluray1080p);
            var remoteEpisode = MockUpOrDownGrade(3000, 8000, newQuality, Quality.Bluray1080p.Id);
            SetupHistoryServiceMock(history);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.HistoryRecentCutoffMet);
        }

        [Test]
        public void should_reject_when_grabbed_history_meets_cutoff_and_cdh_disabled()
        {
            var newQuality = new QualityModel(Quality.WEBDL720p);
            var remoteEpisode = MockUpOrDownGrade(3000, 8000, newQuality, Quality.Bluray1080p.Id);
            var betterQuality = new QualityModel(Quality.Bluray1080p);
            var history = GivenFileHistory(DateTime.UtcNow.AddHours(-13), betterQuality, EpisodeHistoryEventType.Grabbed);
            SetupHistoryServiceMock(history);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.HistoryCdhDisabledCutoffMet);
        }

        [Test]
        public void should_accept_when_file_history_has_lower_quality_in_custom_format_score()
        {
            var newQuality = new QualityModel(Quality.SDTV);
            var remoteEpisode = MockUpOrDownGrade(3000, 8000, newQuality, Quality.WEBDL720p.Id, true);
            var history = GivenFileHistory(DateTime.UtcNow.AddDays(-5), new QualityModel(Quality.SDTV));
            SetupHistoryServiceMock(history);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_file_history_has_higher_quality_in_custom_format_score()
        {
            var newQuality = new QualityModel(Quality.SDTV);
            var remoteEpisode = MockUpOrDownGrade(8000, 3000, newQuality, Quality.WEBDL720p.Id, true);
            var history = GivenFileHistory(DateTime.UtcNow.AddDays(-5), new QualityModel(Quality.SDTV));
            SetupHistoryServiceMock(history);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeFalse();
        }

        [Test]
        public void should_accept_when_file_history_has_lower_quality_in_quality_profile()
        {
            var newQuality = new QualityModel(Quality.WEBDL720p);
            var remoteEpisode = MockUpOrDownGrade(3000, 3000, newQuality, Quality.WEBDL720p.Id, true);
            var history = GivenFileHistory(DateTime.UtcNow.AddDays(-5), new QualityModel(Quality.SDTV));
            SetupHistoryServiceMock(history);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeTrue();
        }

        [Test]
        public void should_reject_when_file_history_has_higher_quality_in_quality_profile()
        {
            var newQuality = new QualityModel(Quality.SDTV);
            var remoteEpisode = MockUpOrDownGrade(3000, 3000, newQuality, Quality.WEBDL720p.Id, true);
            var history = GivenFileHistory(DateTime.UtcNow.AddDays(-5), new QualityModel(Quality.WEBDL720p));
            SetupHistoryServiceMock(history);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeFalse();
        }

        [Test]
        public void should_reject_when_grabbed_history_has_better_custom_format()
        {
            var date = DateTime.UtcNow.AddMinutes(-10);
            var grabHistory = GivenFileHistory(date, new QualityModel(Quality.HDTV720p), EpisodeHistoryEventType.Grabbed);
            var remoteEpisode = MockUpOrDownGrade(5, 2, new QualityModel(Quality.HDTV720p), 0);
            SetupHistoryServiceMock(grabHistory);

            var decision = Subject.IsSatisfiedBy(remoteEpisode, null);

            decision.Accepted.Should().BeFalse();
            decision.Reason.Should().Be(DownloadRejectionReason.HistoryRecentCutoffMet);
        }
    }
}
