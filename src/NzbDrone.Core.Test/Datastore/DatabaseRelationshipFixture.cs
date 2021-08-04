using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.History;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class DatabaseRelationshipFixture : DbTest
    {
        [Test]
        public void one_to_one()
        {
            var episodeFile = Builder<EpisodeFile>.CreateNew()
                .With(c => c.Language = Language.English)
                .With(c => c.Quality = new QualityModel())
                .With(c => c.Language = Language.English)
                .BuildNew();

            Db.Insert(episodeFile);

            var episode = Builder<Episode>.CreateNew()
                                          .With(c => c.EpisodeFileId = episodeFile.Id)
                                          .BuildNew();

            Db.Insert(episode);

            var loadedEpisodeFile = Db.Single<Episode>().EpisodeFile.Value;

            loadedEpisodeFile.Should().NotBeNull();
            loadedEpisodeFile.Should().BeEquivalentTo(episodeFile,
                options => options
                    .IncludingAllRuntimeProperties()
                    .Excluding(c => c.DateAdded)
                    .Excluding(c => c.Path)
                    .Excluding(c => c.Series)
                    .Excluding(c => c.Episodes));
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
            var quality = new QualityModel { Quality = Quality.Bluray720p, Revision = new Revision(version: 2) };

            var history = Builder<EpisodeHistory>.CreateNew()
                .With(c => c.Language = Language.English)
                .With(c => c.Id = 0)
                .With(c => c.Quality = quality)
                .Build();

            Db.Insert(history);

            var loadedQuality = Db.Single<EpisodeHistory>().Quality;
            loadedQuality.Should().Be(quality);
        }

        [Test]
        public void embedded_list_of_document_with_json()
        {
            var history = Builder<EpisodeHistory>.CreateListOfSize(2)
                .All()
                .With(c => c.Id = 0)
                .With(c => c.Language = Language.English)
                .Build().ToList();

            history[0].Quality = new QualityModel(Quality.HDTV1080p, new Revision(version: 2));
            history[1].Quality = new QualityModel(Quality.Bluray720p, new Revision(version: 2));

            Db.InsertMany(history);

            var returnedHistory = Db.All<EpisodeHistory>();

            returnedHistory[0].Quality.Quality.Should().Be(Quality.HDTV1080p);
        }
    }
}
