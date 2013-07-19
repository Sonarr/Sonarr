using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Download.DownloadApprovedReportsTests
{
    [TestFixture]
    public class DownloadApprovedReportsFixture : CoreTest<DownloadApprovedReports>
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

            remoteEpisode.Report = new ReportInfo();
            remoteEpisode.Report.Age = 0;

            return remoteEpisode;
        }

        [Test]
        public void should_return_an_empty_list_when_none_are_appproved()
        {
            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(null, "Failure!"));
            decisions.Add(new DownloadDecision(null, "Failure!"));

            Subject.GetQualifiedReports(decisions).Should().BeEmpty();
        }

        [Test]
        public void should_put_propers_before_non_propers()
        {
            var remoteEpisode1 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p, false));
            var remoteEpisode2 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p, true));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.GetQualifiedReports(decisions);
            qualifiedReports.First().RemoteEpisode.ParsedEpisodeInfo.Quality.Proper.Should().BeTrue();
        }

        [Test]
        public void should_put_higher_quality_before_lower()
        {
            var remoteEpisode1 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.SDTV));
            var remoteEpisode2 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.GetQualifiedReports(decisions);
            qualifiedReports.First().RemoteEpisode.ParsedEpisodeInfo.Quality.Quality.Should().Be(Quality.HDTV720p);
        }

        [Test]
        public void should_order_by_lowest_episode_number()
        {
            var remoteEpisode1 = GetRemoteEpisode(new List<Episode> { GetEpisode(2) }, new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.GetQualifiedReports(decisions);
            qualifiedReports.First().RemoteEpisode.Episodes.First().EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_order_by_lowest_episode_number_with_multiple_episodes()
        {
            var remoteEpisode1 = GetRemoteEpisode(new List<Episode> { GetEpisode(2), GetEpisode(3) }, new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GetRemoteEpisode(new List<Episode> { GetEpisode(1), GetEpisode(2) }, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.GetQualifiedReports(decisions);
            qualifiedReports.First().RemoteEpisode.Episodes.First().EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteEpisode1 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p));

            remoteEpisode1.Report.Age = 10;
            remoteEpisode2.Report.Age = 5;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.GetQualifiedReports(decisions);
            qualifiedReports.First().RemoteEpisode.Report.Age.Should().Be(5);
        }
    }
}
