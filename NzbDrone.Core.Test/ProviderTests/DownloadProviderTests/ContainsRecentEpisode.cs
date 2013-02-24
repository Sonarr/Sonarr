using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Download;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ProviderTests.DownloadProviderTests
{
    [TestFixture]
    public class ContainsRecentEpisode : CoreTest
    {
        private Episode _recentEpisode;
        private Episode _oldEpisode;

        [SetUp]
        public void Setup()
        {
            _recentEpisode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.AirDate = DateTime.Today)
                    .Build();

            _oldEpisode = Builder<Episode>
                    .CreateNew()
                    .With(e => e.AirDate = DateTime.Today.AddDays(-365))
                    .Build();
        }

        [Test]
        public void should_return_true_if_episode_aired_recently()
        {
            var epr = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.Episodes = new List<Episode>
                        {
                                _recentEpisode
                        })
                        .Build();

            Mocker.Resolve<DownloadProvider>().ContainsRecentEpisode(epr).Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_one_episode_aired_recently()
        {
            var epr = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.Episodes = new List<Episode>
                        {
                                _recentEpisode,
                                _oldEpisode
                        })
                        .Build();

            Mocker.Resolve<DownloadProvider>().ContainsRecentEpisode(epr).Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_episode_did_not_air_recently()
        {
            var epr = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.Episodes = new List<Episode>
                        {
                                _oldEpisode
                        })
                        .Build();

            Mocker.Resolve<DownloadProvider>().ContainsRecentEpisode(epr).Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_no_episode_aired_recently()
        {
            var epr = Builder<EpisodeParseResult>
                    .CreateNew()
                    .With(p => p.Episodes = new List<Episode>
                        {
                                _oldEpisode,
                                _oldEpisode
                        })
                        .Build();

            Mocker.Resolve<DownloadProvider>().ContainsRecentEpisode(epr).Should().BeFalse();
        }
    }
}
