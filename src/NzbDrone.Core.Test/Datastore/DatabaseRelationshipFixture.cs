using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class DatabaseRelationshipFixture : DbTest
    {
        /*        [Test]
                [Explicit]
                public void benchmark()
                {
                    var series = Builder<Series>.CreateNew()
                                                .With(c => c.Id = 0)
                                                .Build();

                    Marr.Data.MapRepository.Instance.EnableTraceLogging = false;

                    Db.Insert(series);

                    var covers = Builder<MediaCover.MediaCover>.CreateListOfSize(5)
                        .All()
                        .With(c => c.SeriesId = series.Id)
                        .With(c => c.Id = 0)
                        .Build()
                        .ToList();

                    Db.InsertMany(covers);

                    var loadedSeries = Db.Single<Series>();

                    var sw = Stopwatch.StartNew();
                    for (int i = 0; i < 10000; i++)
                    {
                        loadedSeries = Db.Single<Series>();
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

                    Db.Insert(series);

                    var covers = Builder<MediaCover.MediaCover>.CreateListOfSize(5)
                        .All()
                        .With(c => c.SeriesId = series.Id)
                        .With(c => c.Id = 0)
                        .Build()
                        .ToList();

                    Db.InsertMany(covers);

                    var loadedSeries = Db.Single<Series>();
                    loadedSeries.Covers.Value.Should().HaveSameCount(covers);
                }*/

        [Test]
        public void one_to_one()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                           .With(c => c.Quality = new QualityModel())
                           .BuildNew();

            Db.Insert(episodeFile);

            var episode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeFileId = episodeFile.Id)
                .BuildNew();

            Db.Insert(episode);





            var loadedEpisodeFile = Db.Single<Episode>().EpisodeFile.Value;

            loadedEpisodeFile.Should().NotBeNull();
            loadedEpisodeFile.ShouldHave().AllProperties().But(c => c.DateAdded).EqualTo(episodeFile);
        }

        [Test]
        public void one_to_one_should_not_query_db_if_foreign_key_is_zero()
        {
            var episode = Builder<Episode>.CreateNew()
                .With(c => c.EpisodeFileId = 0)
                .BuildNew();

            Db.Insert(episode);

            Db.Single<Episode>().EpisodeFile.Value.Should().BeNull();
        }


        [Test]
        public void embedded_document_as_json()
        {
            var quality = new QualityModel { Quality = Quality.Bluray720p, Proper = true };

            var history = Builder<History.History>.CreateNew()
                            .With(c => c.Id = 0)
                            .With(c => c.Quality = quality)
                            .Build();

            Db.Insert(history);

            var loadedQuality = Db.Single<History.History>().Quality;
            loadedQuality.Should().Be(quality);
        }

        [Test]
        public void embedded_list_of_document_with_json()
        {
            var quality = new QualityModel { Quality = Quality.Bluray720p, Proper = true };

            var history = Builder<History.History>.CreateListOfSize(2)
                            .All().With(c => c.Id = 0)
                            .Build().ToList();

            history[0].Quality = new QualityModel(Quality.HDTV1080p, true);
            history[1].Quality = new QualityModel(Quality.Bluray720p, true);


            Db.InsertMany(history);

            var returnedHistory = Db.All<History.History>();

            returnedHistory[0].Quality.Quality.Should().Be(Quality.HDTV1080p);
        }
    }
}
