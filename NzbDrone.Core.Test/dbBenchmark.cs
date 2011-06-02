using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using FizzWare.NBuilder;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class DbBenchmark
    {
        const int COUNT = 10000;
        private List<Episode> episodes;
        private List<EpisodeFile> files;


        [SetUp]
        public void Setup()
        {


            episodes = new List<Episode>(COUNT);
            files = new List<EpisodeFile>(COUNT);

            for (int i = 10; i < COUNT + 10; i++)
            {
                var file = Builder<EpisodeFile>.CreateNew().With(c => c.EpisodeFileId = i).Build();
                files.Add(file);

                var episode = Builder<Episode>.CreateNew()
                    .With(c => c.EpisodeId = i)
                    .And(c => c.EpisodeFileId = i)
                    .And(c => c.Title = DateTime.Now.ToLongTimeString())
                    .And(
                        c =>
                        c.Overview =
                        @"This adds support for silverlight. Both the 3.5 CLR and a Silverlight 3 
                version are included in the zip file. Also includes some other very minor bug fixes.").Build();

                episodes.Add(episode);
            }
        }



        [Test]
        public void Insert_into_episodes()
        {
            var repo = MockLib.GetEmptyRepository();

            Thread.Sleep(1000);
            var sw = Stopwatch.StartNew();
            repo.AddMany(episodes);
            sw.Stop();

            Console.WriteLine("Adding " + COUNT + " items at once took " + sw.Elapsed);
        }


        [Test]
        public void Insert_into_episodes_single()
        {
            var repo = MockLib.GetEmptyRepository();

            Thread.Sleep(1000);
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < 100; i++)
            {
                repo.Add(episodes[i]);
            }

            sw.Stop();

            Console.WriteLine("Adding " + 100 + " single items took " + sw.Elapsed);
        }


        [Test]
        public void get_episode_file()
        {
            var repo = MockLib.GetEmptyRepository();

            repo.AddMany(episodes);
            repo.AddMany(files);

            //var repoEpisodes = repo.All<Episode>().ToList();

            Thread.Sleep(1000);

            var count = 0;

            var random = new Random();

            var sw = Stopwatch.StartNew();



            for (int i = 5000; i < 5000 + 1000; i++)
            {
                count++;
                var file = repo.Single<Episode>(random.Next(10, COUNT - 100)).EpisodeFile;
            }


            sw.Stop();

            Console.WriteLine("Getting " + count + " episode files took " + sw.Elapsed);
        }
    }
}
