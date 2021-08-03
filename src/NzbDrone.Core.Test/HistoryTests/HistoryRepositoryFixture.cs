using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.HistoryTests
{
    [TestFixture]
    public class HistoryRepositoryFixture : DbTest<HistoryRepository, EpisodeHistory>
    {
        [Test]
        public void should_read_write_dictionary()
        {
            var history = Builder<EpisodeHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel())
                .BuildNew();

            history.Data.Add("key1", "value1");
            history.Data.Add("key2", "value2");

            Subject.Insert(history);

            StoredModel.Data.Should().HaveCount(2);
        }

        [Test]
        public void should_get_download_history()
        {
            var historyBluray = Builder<EpisodeHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.Bluray1080p))
                .With(c => c.SeriesId = 12)
                .With(c => c.EventType = EpisodeHistoryEventType.Grabbed)
                .BuildNew();

            var historyDvd = Builder<EpisodeHistory>.CreateNew()
                .With(c => c.Quality = new QualityModel(Quality.DVD))
                .With(c => c.SeriesId = 12)
                .With(c => c.EventType = EpisodeHistoryEventType.Grabbed)
             .BuildNew();

            Subject.Insert(historyBluray);
            Subject.Insert(historyDvd);

            var downloadHistory = Subject.FindDownloadHistory(12, new QualityModel(Quality.Bluray1080p));

            downloadHistory.Should().HaveCount(1);
        }
    }
}
