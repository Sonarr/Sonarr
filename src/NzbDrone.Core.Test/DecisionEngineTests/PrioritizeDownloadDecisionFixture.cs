using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.DecisionEngine;
using NUnit.Framework;
using FluentAssertions;
using FizzWare.NBuilder;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class PrioritizeDownloadDecisionFixture : CoreTest<DownloadDecisionPriorizationService>
    {
        [SetUp]
        public void Setup()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);
        }

        private Episode GivenEpisode(int id)
        {
            return Builder<Episode>.CreateNew()
                            .With(e => e.Id = id)
                            .With(e => e.EpisodeNumber = id)
                            .Build();
        }

        private RemoteEpisode GivenRemoteEpisode(List<Episode> episodes, QualityModel quality, int age = 0, long size = 0, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet)
        {
            var remoteEpisode = new RemoteEpisode();
            remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            remoteEpisode.ParsedEpisodeInfo.Quality = quality;

            remoteEpisode.Episodes = new List<Episode>();
            remoteEpisode.Episodes.AddRange(episodes);

            remoteEpisode.Release = new ReleaseInfo();
            remoteEpisode.Release.PublishDate = DateTime.Now.AddDays(-age);
            remoteEpisode.Release.Size = size;
            remoteEpisode.Release.DownloadProtocol = downloadProtocol;

            remoteEpisode.Series = Builder<Series>.CreateNew()
                                                  .With(e => e.Profile = new Profile { Items = Qualities.QualityFixture.GetDefaultQualities() })
                                                  .Build();

            return remoteEpisode;
        }

        private void GivenPreferredDownloadProtocol(DownloadProtocol downloadProtocol)
        {
            Mocker.GetMock<IDelayProfileService>()
                  .Setup(s => s.BestForTags(It.IsAny<HashSet<int>>()))
                  .Returns(new DelayProfile
                  {
                      PreferredProtocol = downloadProtocol
                  });
        }

        [Test]
        public void should_put_propers_before_non_propers()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p, new Revision(version: 1)));
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p, new Revision(version: 2)));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteItem.ParsedInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_put_higher_quality_before_lower()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.SDTV));
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteItem.ParsedInfo.Quality.Quality.Should().Be(Quality.HDTV720p);
        }

        [Test]
        public void should_order_by_lowest_number_of_episodes()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(2) }, new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            (qualifiedReports.First().RemoteItem as RemoteEpisode).Episodes.First().EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_order_by_lowest_number_of_episodes_with_multiple_episodes()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(2), GivenEpisode(3) }, new QualityModel(Quality.HDTV720p));
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1), GivenEpisode(2) }, new QualityModel(Quality.HDTV720p));

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            (qualifiedReports.First().RemoteItem as RemoteEpisode).Episodes.First().EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_order_by_smallest_rounded_to_200mb_then_age()
        {
            var remoteEpisodeSd = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.SDTV), size: 100.Megabytes(), age: 1);
            var remoteEpisodeHdSmallOld = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), size: 1200.Megabytes(), age: 1000);
            var remoteEpisodeSmallYoung = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), size: 1250.Megabytes(), age: 10);
            var remoteEpisodeHdLargeYoung = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), size: 3000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisodeSd));
            decisions.Add(new DownloadDecision(remoteEpisodeHdSmallOld));
            decisions.Add(new DownloadDecision(remoteEpisodeSmallYoung));
            decisions.Add(new DownloadDecision(remoteEpisodeHdLargeYoung));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteItem.Should().Be(remoteEpisodeSmallYoung);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), age: 10);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), age: 5);


            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteItem.Should().Be(remoteEpisode2);
        }

        [Test]
        public void should_not_throw_if_no_episodes_are_found()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), size: 500.Megabytes());
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), size: 500.Megabytes());

            remoteEpisode1.Episodes = new List<Episode>();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            Subject.PrioritizeDecisions(decisions);
        }

        [Test]
        public void should_put_usenet_above_torrent_when_usenet_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Usenet);

            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Torrent);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteItem.Release.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
        }

        [Test]
        public void should_put_torrent_above_usenet_when_torrent_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Torrent);

            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Torrent);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteItem.Release.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
        }
    }
}
