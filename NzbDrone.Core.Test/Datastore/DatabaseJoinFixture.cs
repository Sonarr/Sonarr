using System;
using System.Diagnostics;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class DatabaseJoinFixture : DbTest<BasicRepository<Series>, Series>
    {
        [Test]
        [Explicit]
        public void benchmark()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(c => c.Id = 0)
                                        .Build();

            Marr.Data.MapRepository.Instance.EnableTraceLogging = false;

            Subject.Insert(series);

            var covers = Builder<MediaCover.MediaCover>.CreateListOfSize(5)
                .All()
                .With(c => c.SeriesId = series.Id)
                .With(c => c.Id = 0)
                .Build()
                .ToList();

            Db.InsertMany(covers);

            var loadedSeries = Subject.SingleOrDefault();

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                loadedSeries = Subject.SingleOrDefault();
                var list = loadedSeries.Covers.Value;
            }

            sw.Stop();

            Console.WriteLine(sw.Elapsed);

            loadedSeries.Covers.Value.Should().HaveSameCount(covers);
        }

        [Test]
        public void one_to_many()
        {
            var series = Builder<Series>.CreateNew()
                                        .With(c => c.Id = 0)
                                        .Build();

            Subject.Insert(series);

            var covers = Builder<MediaCover.MediaCover>.CreateListOfSize(5)
                .All()
                .With(c => c.SeriesId = series.Id)
                .With(c => c.Id = 0)
                .Build()
                .ToList();

            Db.InsertMany(covers);

            var loadedSeries = Subject.SingleOrDefault();
            loadedSeries = Subject.SingleOrDefault();

            loadedSeries.Covers.Value.Should().HaveSameCount(covers);
        }

        [Test]
        public void embeded_document_as_json()
        {
            var episode = Builder<Episode>.CreateNew()
                                                       .With(c => c.Id = 0)
                                                       .Build();

            Db.Insert(episode);


            var history = Builder<History.History>.CreateNew()
                            .With(c => c.Id = 0)
                            .With(c => c.EpisodeId = episode.Id)
                            .With(c => c.Quality = new QualityModel())
                            .Build();

            Db.Insert(history);
            Db.Single<History.History>().Episode.Value.Should().NotBeNull();
        }
    }
}
