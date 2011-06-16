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
    [Ignore]
    public class DbBenchmark : TestBase
    {
        const int Episodes_Per_Season = 20;
        private readonly List<int> seasonsNumbers = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private readonly List<int> seriesIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };
        private readonly List<Episode> episodes = new List<Episode>();
        private readonly List<EpisodeFile> files = new List<EpisodeFile>();
        private readonly IRepository repo = MockLib.GetEmptyRepository();

        [TestFixtureSetUp]
        public new void Setup()
        {


            base.Setup();
            int currentFileId = 0;

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
                        var epFileId = 0;

                        if (i < 10)
                        {
                            var epFile = Builder<EpisodeFile>.CreateNew()
                               .With(e => e.SeriesId = seriesId)
                               .With(e => e.SeriesId = seriesId)
                               .And(e => e.SeasonNumber = _seasonNumber)
                               .And(e => e.Path = Guid.NewGuid().ToString())
                               .Build();

                            files.Add(epFile);

                            currentFileId++;
                            epFileId = currentFileId;

                        }


                        var episode = Builder<Episode>.CreateNew()
                            .With(e => e.SeriesId = seriesId)
                            .And(e => e.SeasonNumber = _seasonNumber)
                            .And(e => e.EpisodeNumber = i)
                            .And(e => e.Ignored = false)
                            .And(e => e.TvDbEpisodeId = episodes.Count + 1)
                            .And(e => e.EpisodeFileId = epFileId)
                            .And(e => e.AirDate = DateTime.Today.AddDays(-20))
                            .Build();

                        episodes.Add(episode);


                    }
                }

            }

            repo.AddMany(episodes);
            repo.AddMany(files);
        }


        [Test]
        public void get_episode_by_series_seasons_episode_x5000()
        {
            var epProvider = new EpisodeProvider(null, null);


            Thread.Sleep(1000);


            var random = new Random();
            Console.WriteLine("Starting Test");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 5000; i++)
            {
                epProvider.GetEpisode(6, random.Next(2, 5), random.Next(2, Episodes_Per_Season - 10)).Should().NotBeNull();
            }


            sw.Stop();

            Console.WriteLine("Took " + sw.Elapsed);
        }

        [Test]
        public void get_episode_by_series_seasons_x1000()
        {
            var epProvider = new EpisodeProvider( null, null);


            Thread.Sleep(1000);


            var random = new Random();
            Console.WriteLine("Starting Test");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                epProvider.GetEpisodesBySeason(6, random.Next(2, 5)).Should().NotBeNull();
            }


            sw.Stop();

            Console.WriteLine("Took " + sw.Elapsed);
        }

        [Test]
        public void get_episode_file_count_x100()
        {
            var mocker = new AutoMoq.AutoMoqer();
            mocker.SetConstant(repo);
            mocker.SetConstant(mocker.Resolve<EpisodeProvider>());
            var mediaProvider = mocker.Resolve<MediaFileProvider>();


            Thread.Sleep(1000);


            var random = new Random();
            Console.WriteLine("Starting Test");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                mediaProvider.GetEpisodeFilesCount(random.Next(1, 5)).Should().NotBeNull();
            }


            sw.Stop();

            Console.WriteLine("Took " + sw.Elapsed);
        }


        [Test]
        public void get_season_count_x5000()
        {
            var mocker = new AutoMoq.AutoMoqer();
            mocker.SetConstant(repo);
            var provider = mocker.Resolve<EpisodeProvider>();


            Thread.Sleep(1000);


            var random = new Random();
            Console.WriteLine("Starting Test");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 5000; i++)
            {
                provider.GetSeasons(random.Next(1, 10)).Should().HaveSameCount(seasonsNumbers);
            }


            sw.Stop();

            Console.WriteLine("Took " + sw.Elapsed);
        }


        [Test]
        public void get_episode_file_count_x10()
        {
            var mocker = new AutoMoq.AutoMoqer();
            mocker.SetConstant(repo);
            mocker.SetConstant(mocker.Resolve<EpisodeProvider>());
            var provider = mocker.Resolve<MediaFileProvider>();


            Thread.Sleep(1000);


            var random = new Random();
            Console.WriteLine("Starting Test");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                var result = provider.GetEpisodeFilesCount(random.Next(1, 10));
                result.Item1.Should().NotBe(0);
                result.Item2.Should().NotBe(0);
            }


            sw.Stop();

            Console.WriteLine("Took " + sw.Elapsed);
        }

    }
}
