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

        private RemoteEpisode GetRemoteEpisode(List<Episode> episodes, QualityModel quality, int Age = 0, long size = 0)
        {
            var remoteEpisode = new RemoteEpisode();
            remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            remoteEpisode.ParsedEpisodeInfo.Quality = quality;

            remoteEpisode.Episodes = new List<Episode>();
            remoteEpisode.Episodes.AddRange(episodes);

            remoteEpisode.Report = new ReportInfo();
            remoteEpisode.Report.Age = Age;
            remoteEpisode.Report.Size = size;

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
        public void should_order_by_lowest_number_of_episodes()
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
        public void should_order_by_lowest_number_of_episodes_with_multiple_episodes()
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
        public void should_order_by_smallest_rounded_to_200mb_then_age()
        {
            var remoteEpisodeSd = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.SDTV), size: 100.Megabytes(), Age: 1);
            var remoteEpisodeHdSmallOld = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p), size:1200.Megabytes(), Age:1000);
            var remoteEpisodeHdSmallYounge = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p), size:1250.Megabytes(), Age:10);
            var remoteEpisodeHdLargeYounge = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p), size:3000.Megabytes(), Age:1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisodeSd));
            decisions.Add(new DownloadDecision(remoteEpisodeHdSmallOld));
            decisions.Add(new DownloadDecision(remoteEpisodeHdSmallYounge));
            decisions.Add(new DownloadDecision(remoteEpisodeHdLargeYounge));

            var qualifiedReports = Subject.GetQualifiedReports(decisions);
            qualifiedReports.First().RemoteEpisode.Should().Be(remoteEpisodeHdSmallYounge);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteEpisode1 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p), Age: 10);
            var remoteEpisode2 = GetRemoteEpisode(new List<Episode> { GetEpisode(1) }, new QualityModel(Quality.HDTV720p), Age: 5);


            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.GetQualifiedReports(decisions);
            qualifiedReports.First().RemoteEpisode.Should().Be(remoteEpisode2);
        }
    }
}
