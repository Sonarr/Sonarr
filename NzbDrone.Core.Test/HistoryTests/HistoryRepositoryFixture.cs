using System;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, History.History>
    {
        [Test]
        public void Trim_Items()
        {
            var historyItem = Builder<History.History>.CreateListOfSize(30)
                .All()
                .With(c=>c.Id = 0)
                .TheFirst(10).With(c => c.Date = DateTime.Now)
                .TheNext(20).With(c => c.Date = DateTime.Now.AddDays(-31))
                .Build();

            Db.InsertMany(historyItem);

            AllStoredModels.Should().HaveCount(30);
            Subject.Trim();

            AllStoredModels.Should().HaveCount(10);
            AllStoredModels.Should().OnlyContain(s => s.Date > DateTime.Now.AddDays(-30));
        }


        [Test]
        public void GetBestQualityInHistory_no_result()
        {
            Subject.GetBestQualityInHistory(12).Should().Be(null);
        }

        [Test]
        public void GetBestQualityInHistory_single_result()
        {
            var series = Builder<Series>.CreateNew().Build();
            var episode = Builder<Episode>.CreateNew()
                .With(c => c.Series = series)
                .With(c => c.SeriesId = series.Id)
                .Build();



            var history = Builder<History.History>.CreateNew()
                .With(c => c.Id = 0)
                .With(h => h.Quality = new QualityModel(Quality.Bluray720p, true))
                .With(h => h.EpisodeId = episode.Id)
                .Build();

            Db.Insert(history);

            var result = Subject.GetBestQualityInHistory(episode.Id);

            result.Should().NotBeNull();
            result.Quality.Should().Be(Quality.Bluray720p);
            result.Proper.Should().BeTrue();
        }

        [Test]
        public void GetBestQualityInHistory_should_return_highest_result()
        {

            var series = Builder<Series>.CreateNew().Build();
            var episode = Builder<Episode>.CreateNew()
                .With(c => c.Series = series)
                .With(c => c.SeriesId = series.Id)
                .Build();


            var history = Builder<History.History>
                    .CreateListOfSize(5)
                    .All()
                    .With(c => c.Id = 0)
                    .With(h => h.EpisodeId = episode.Id)
                    .TheFirst(1)
                    .With(h => h.Quality = new QualityModel(Quality.DVD, true))
                    .TheNext(1)
                    .With(h => h.Quality = new QualityModel(Quality.Bluray720p, true))
                    .TheNext(1)
                    .With(h => h.Quality = new QualityModel(Quality.Bluray720p, true))
                    .TheNext(1)
                    .With(h => h.Quality = new QualityModel(Quality.Bluray720p, false))
                    .TheNext(1)
                    .With(h => h.Quality = new QualityModel(Quality.SDTV, true))
                    .Build();

            Db.InsertMany(history);

            var result = Subject.GetBestQualityInHistory(episode.Id);

            result.Should().NotBeNull();
            result.Quality.Should().Be(Quality.Bluray720p);
            result.Proper.Should().BeTrue();
        }

        
     
    }
}