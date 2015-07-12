using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.DecisionEngine;
using NUnit.Framework;
using FluentAssertions;
using FizzWare.NBuilder;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Languages;

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

        private RemoteEpisode GivenRemoteEpisode(List<Episode> episodes, QualityModel quality, Language language, int age = 0, long size = 0, DownloadProtocol downloadProtocol = DownloadProtocol.Usenet)
        {
            var remoteEpisode = new RemoteEpisode();
            remoteEpisode.ParsedEpisodeInfo = new ParsedEpisodeInfo();
            remoteEpisode.ParsedEpisodeInfo.Quality = quality;
            remoteEpisode.ParsedEpisodeInfo.Language = language;

            remoteEpisode.Episodes = new List<Episode>();
            remoteEpisode.Episodes.AddRange(episodes);

            remoteEpisode.Release = new ReleaseInfo();
            remoteEpisode.Release.PublishDate = DateTime.Now.AddDays(-age);
            remoteEpisode.Release.Size = size;
            remoteEpisode.Release.DownloadProtocol = downloadProtocol;

            remoteEpisode.Series = Builder<Series>.CreateNew()
                                                  .With(e => e.Profile = new Profile
                                                  {
                                                      Items = Qualities.QualityFixture.GetDefaultQualities()
                                                  })
                                                  .With(l => l.LanguageProfile = new LanguageProfile
                                                  {
                                                      Languages = LanguageFixture.GetDefaultLanguages(),
                                                      Cutoff = Language.Spanish
                                                  })
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
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p, new Revision(version: 1)), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p, new Revision(version: 2)), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.ParsedEpisodeInfo.Quality.Revision.Version.Should().Be(2);
        }

        [Test]
        public void should_put_higher_quality_before_lower()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.SDTV), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.ParsedEpisodeInfo.Quality.Quality.Should().Be(Quality.HDTV720p);
        }

        [Test]
        public void should_order_by_lowest_number_of_episodes()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(2) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Episodes.First().EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_order_by_lowest_number_of_episodes_with_multiple_episodes()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(2), GivenEpisode(3) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1), GivenEpisode(2) }, new QualityModel(Quality.HDTV720p), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Episodes.First().EpisodeNumber.Should().Be(1);
        }

        [Test]
        public void should_order_by_age_then_largest_rounded_to_200mb()
        {
            var remoteEpisodeSd = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.SDTV), Language.English, size: 100.Megabytes(), age: 1);
            var remoteEpisodeHdSmallOld = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, size: 1200.Megabytes(), age: 1000);
            var remoteEpisodeSmallYoung = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, size: 1250.Megabytes(), age: 10);
            var remoteEpisodeHdLargeYoung = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, size: 3000.Megabytes(), age: 1);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisodeSd));
            decisions.Add(new DownloadDecision(remoteEpisodeHdSmallOld));
            decisions.Add(new DownloadDecision(remoteEpisodeSmallYoung));
            decisions.Add(new DownloadDecision(remoteEpisodeHdLargeYoung));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Should().Be(remoteEpisodeHdLargeYoung);
        }

        [Test]
        public void should_order_by_youngest()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, age: 10);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, age: 5);


            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Should().Be(remoteEpisode2);
        }

        [Test]
        public void should_not_throw_if_no_episodes_are_found()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, size: 500.Megabytes());
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, size: 500.Megabytes());

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

            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, downloadProtocol: DownloadProtocol.Torrent);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Release.DownloadProtocol.Should().Be(DownloadProtocol.Usenet);
        }

        [Test]
        public void should_put_torrent_above_usenet_when_torrent_is_preferred()
        {
            GivenPreferredDownloadProtocol(DownloadProtocol.Torrent);

            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, downloadProtocol: DownloadProtocol.Torrent);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English, downloadProtocol: DownloadProtocol.Usenet);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Release.DownloadProtocol.Should().Be(DownloadProtocol.Torrent);
        }

        [Test]
        public void should_prefer_season_pack_above_single_episode()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1), GivenEpisode(2) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            remoteEpisode1.ParsedEpisodeInfo.FullSeason = true;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.ParsedEpisodeInfo.FullSeason.Should().BeTrue();
        }

        [Test]
        public void should_prefer_multiepisode_over_single_episode_for_anime()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1), GivenEpisode(2) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            remoteEpisode1.Series.SeriesType = SeriesTypes.Anime;
            remoteEpisode2.Series.SeriesType = SeriesTypes.Anime;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Episodes.Count.Should().Be(remoteEpisode1.Episodes.Count);
        }

        [Test]
        public void should_prefer_single_episode_over_multi_episode_for_non_anime()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1), GivenEpisode(2) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Episodes.Count.Should().Be(remoteEpisode2.Episodes.Count);
        }

        [Test]
        public void should_prefer_releases_with_more_seeders()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 100;

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo) qualifiedReports.First().RemoteEpisode.Release).Seeders.Should().Be(torrentInfo2.Seeders);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_given_equal_number_of_seeds()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 10;
            torrentInfo1.Peers = 10;


            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Peers = 100;

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteEpisode.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_releases_with_more_peers_no_seeds()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.Size = 0;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 0;
            torrentInfo1.Peers = 10;


            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 0;
            torrentInfo2.Peers = 100;

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteEpisode.Release).Peers.Should().Be(torrentInfo2.Peers);
        }

        [Test]
        public void should_prefer_first_release_if_peers_and_size_are_too_similar()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 1000;
            torrentInfo1.Peers = 10;
            torrentInfo1.Size = 200.Megabytes();

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 1100;
            torrentInfo2.Peers = 10;
            torrentInfo1.Size = 250.Megabytes();

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo) qualifiedReports.First().RemoteEpisode.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_prefer_first_release_if_age_and_size_are_too_similar()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);

            remoteEpisode1.Release.PublishDate = DateTime.UtcNow.AddDays(-100);
            remoteEpisode1.Release.Size = 200.Megabytes();

            remoteEpisode2.Release.PublishDate = DateTime.UtcNow.AddDays(-150);
            remoteEpisode2.Release.Size = 250.Megabytes();

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.Release.Should().Be(remoteEpisode1.Release);
        }

        [Test]
        public void should_prefer_quality_over_the_number_of_peers()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.Bluray1080p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.SDTV), Language.English);

            var torrentInfo1 = new TorrentInfo();
            torrentInfo1.PublishDate = DateTime.Now;
            torrentInfo1.DownloadProtocol = DownloadProtocol.Torrent;
            torrentInfo1.Seeders = 100;
            torrentInfo1.Peers = 10;
            torrentInfo1.Size = 200.Megabytes();

            var torrentInfo2 = torrentInfo1.JsonClone();
            torrentInfo2.Seeders = 1100;
            torrentInfo2.Peers = 10;
            torrentInfo1.Size = 250.Megabytes();

            remoteEpisode1.Release = torrentInfo1;
            remoteEpisode2.Release = torrentInfo2;

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            ((TorrentInfo)qualifiedReports.First().RemoteEpisode.Release).Should().Be(torrentInfo1);
        }

        [Test]
        public void should_order_by_language()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.English);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.French);
            var remoteEpisode3 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.German);


            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));
            decisions.Add(new DownloadDecision(remoteEpisode3));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.ParsedEpisodeInfo.Language.Should().Be(Language.French);
            qualifiedReports.Last().RemoteEpisode.ParsedEpisodeInfo.Language.Should().Be(Language.German);
        }

        [Test]
        public void should_put_higher_quality_before_lower_allways()
        {
            var remoteEpisode1 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.SDTV), Language.French);
            var remoteEpisode2 = GivenRemoteEpisode(new List<Episode> { GivenEpisode(1) }, new QualityModel(Quality.HDTV720p), Language.German);

            var decisions = new List<DownloadDecision>();
            decisions.Add(new DownloadDecision(remoteEpisode1));
            decisions.Add(new DownloadDecision(remoteEpisode2));

            var qualifiedReports = Subject.PrioritizeDecisions(decisions);
            qualifiedReports.First().RemoteEpisode.ParsedEpisodeInfo.Quality.Quality.Should().Be(Quality.HDTV720p);
        }
    }
}
