using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.Download.DownloadApprovedReportsTests
{
    [TestFixture]
    public class DownloadApprovedFixture : CoreTest<DownloadApprovedReports>
    {
        private Episode GetEpisode(int id)
        {
            return Builder<Episode>.CreateNew()
                            .With(e => e.Id = id)
                            .With(e => e.EpisodeNumber = id)
                            .Build();
        }

        private RemoteEpisode GetRemoteEpisode(List<Episode> episodes, QualityModel quality)
        {
            var remoteEpisode = new RemoteEpisode();
            remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            remoteEpisode.ParsedEpisodeInfo.Quality = quality;

            remoteEpisode.Episodes = new List<Episode>();
            remoteEpisode.Episodes.AddRange(episodes);

            remoteEpisode.Release = new ReleaseInfo();
            remoteEpisode.Release.PublishDate = DateTime.UtcNow;

            remoteEpisode.Series = Builder<Series>.CreateNew()
                .With(e => e.QualityProfile = new QualityProfile { Allowed = Qualities.QualityFixture.GetDefaultQualities() })
                .Build();

            return remoteEpisode;
        }

        [Test]
        public void should_download_report_if_epsiode_was_not_already_downloaded()
        {
            var episodes = new List<Episode> { GetEpisode(1) };
            var remoteEpisode = GetRemoteEpisode(episodes, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));

            Subject.DownloadApproved(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteEpisode>()), Times.Once());
        }

        [Test]
        public void should_only_download_episode_once()
        {
            var episodes = new List<Episode> { GetEpisode(1) };
            var remoteEpisode = GetRemoteEpisode(episodes, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));
            decisions.Add(new DownloadDecision(remoteEpisode));

            Subject.DownloadApproved(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteEpisode>()), Times.Once());
        }

        [Test]
        public void should_not_download_if_any_episode_was_already_downloaded()
        {
            var remoteEpisode1 = GetRemoteEpisode(
                                                    new List<Episode> { GetEpisode(1) },
                                                    new QualityModel(Quality.HDTV720p)
                                                 );

            var remoteEpisode2 = GetRemoteEpisode(
                                                    new List<Episode> { GetEpisode(1), GetEpisode(2) },
                                                    new QualityModel(Quality.HDTV720p)
                                                 );

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            Subject.DownloadApproved(decisions);
            Mocker.GetMock<IDownloadService>().Verify(v => v.DownloadReport(It.IsAny<RemoteEpisode>()), Times.Once());
        }

        [Test]
        public void should_return_downloaded_reports()
        {
            var episodes = new List<Episode> { GetEpisode(1) };
            var remoteEpisode = GetRemoteEpisode(episodes, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));

            Subject.DownloadApproved(decisions).Should().HaveCount(1);
        }

        [Test]
        public void should_return_all_downloaded_reports()
        {
            var remoteEpisode1 = GetRemoteEpisode(
                                                    new List<Episode> { GetEpisode(1) },
                                                    new QualityModel(Quality.HDTV720p)
                                                 );

            var remoteEpisode2 = GetRemoteEpisode(
                                                    new List<Episode> { GetEpisode(2) },
                                                    new QualityModel(Quality.HDTV720p)
                                                 );

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            Subject.DownloadApproved(decisions).Should().HaveCount(2);
        }

        [Test]
        public void should_only_return_downloaded_reports()
        {
            var remoteEpisode1 = GetRemoteEpisode(
                                                    new List<Episode> { GetEpisode(1) },
                                                    new QualityModel(Quality.HDTV720p)
                                                 );

            var remoteEpisode2 = GetRemoteEpisode(
                                                    new List<Episode> { GetEpisode(2) },
                                                    new QualityModel(Quality.HDTV720p)
                                                 );

            var remoteEpisode3 = GetRemoteEpisode(
                                                    new List<Episode> { GetEpisode(2) },
                                                    new QualityModel(Quality.HDTV720p)
                                                 );

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));
            decisions.Add(new DownloadDecision(remoteEpisode3));

            Subject.DownloadApproved(decisions).Should().HaveCount(2);
        }

        [Test]
        public void should_not_add_to_downloaded_list_when_download_fails()
        {
            var episodes = new List<Episode> { GetEpisode(1) };
            var remoteEpisode = GetRemoteEpisode(episodes, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode));

            Mocker.GetMock<IDownloadService>().Setup(s => s.DownloadReport(It.IsAny<RemoteEpisode>())).Throws(new Exception());
            Subject.DownloadApproved(decisions).Should().BeEmpty();
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
