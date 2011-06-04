using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class DbBenchmark : TestBase
    {
        const int Episodes_Per_Season = 20;
        private readonly List<int> seasonsNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        private readonly List<int> seriesIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        private readonly List<Episode> episodes = new List<Episode>();
        private readonly List<EpisodeFile> files = new List<EpisodeFile>();
        private readonly IRepository repo = MockLib.GetEmptyRepository();

        [TestFixtureSetUp]
        public new void Setup()
        {


            base.Setup();

            foreach (var _seriesId in seriesIds)
            {
                int seriesId = _seriesId;
                var series = Builder<Series>.CreateNew()
                    .With(s => s.SeriesId = seriesId)
                    .With(s => s.Monitored = true)
                    .Build();

                repo.Add(series);

                foreach (var _seasonNumber in seasonsNumbers)
                {
                    for (int i = 1; i <= Episodes_Per_Season; i++)
                    {
                        var episode = Builder<Episode>.CreateNew()
                            .With(e => e.SeriesId = seriesId)
                            .And(e => e.SeasonNumber = _seasonNumber)
                            .And(e => e.EpisodeNumber = i)
                            .And(e => e.Ignored = false)
                            .And(e => e.TvDbEpisodeId = episodes.Count + 1)
                            .And(e => e.AirDate = DateTime.Today.AddDays(-20))
                            .Build();

                        episodes.Add(episode);

                        if (i < 10)
                        {
                            var epFile = Builder<EpisodeFile>.CreateNew()
                               .With(e => e.SeriesId = seriesId)
                               .And(e => e.SeasonNumber = _seasonNumber)
                               .And(e => e.Path = Guid.NewGuid().ToString())
                               .Build();

                            files.Add(epFile);
                        }
                    }
                }

                repo.AddMany(episodes);
                repo.AddMany(files);

            }
        }


        [Test]
        public void get_episode_by_series_seasons_episode_x1000()
        {
            var epProvider = new EpisodeProvider(repo, null);


            Thread.Sleep(1000);


            var random = new Random();
            Console.WriteLine("Starting Test");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                epProvider.GetEpisode(6, random.Next(2, 5), random.Next(2, Episodes_Per_Season - 10)).Should().NotBeNull();
            }


            sw.Stop();

            Console.WriteLine("Took " + sw.Elapsed);
        }

        [Test]
        public void get_episode_by_series_seasons_x500()
        {
            var epProvider = new EpisodeProvider(repo, null);


            Thread.Sleep(1000);


            var random = new Random();
            Console.WriteLine("Starting Test");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 500; i++)
            {
                epProvider.GetEpisodesBySeason(6, random.Next(2, 5)).Should().NotBeNull();
            }


            sw.Stop();

            Console.WriteLine("Took " + sw.Elapsed);
        }

        [Test]
        public void get_episode_file_count_x50()
        {
            var mocker = new AutoMoq.AutoMoqer();
            mocker.SetConstant(repo);
            mocker.SetConstant(mocker.Resolve<EpisodeProvider>());
            var mediaProvider = mocker.Resolve<MediaFileProvider>();


            Thread.Sleep(1000);


            var random = new Random();
            Console.WriteLine("Starting Test");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 50; i++)
            {
                mediaProvider.GetEpisodeFilesCount(random.Next(1, 5)).Should().NotBeNull();
            }


            sw.Stop();

            Console.WriteLine("Took " + sw.Elapsed);
        }
    }
}
