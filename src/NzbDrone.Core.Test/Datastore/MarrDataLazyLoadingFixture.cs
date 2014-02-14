using FizzWare.NBuilder;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Test.Datastore
{

    [TestFixture]
    public class MarrDataLazyLoadingFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
            var qualityProfile = new NzbDrone.Core.Qualities.QualityProfile
                {
                    Name = "Test",
                    Cutoff = Quality.WEBDL720p,
                    Items = NzbDrone.Core.Test.Qualities.QualityFixture.GetDefaultQualities()
                };

            
            qualityProfile = Db.Insert(qualityProfile);

            var series = Builder<Series>.CreateListOfSize(1)
                .All()
                .With(v => v.QualityProfileId = qualityProfile.Id)
                .BuildListOfNew();

            Db.InsertMany(series);

            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(1)
                .All()
                .With(v => v.SeriesId = series[0].Id)
                .BuildListOfNew();

            Db.InsertMany(episodeFiles);

            var episodes = Builder<Episode>.CreateListOfSize(10)
                .All()
                .With(v => v.Monitored = true)
                .With(v => v.EpisodeFileId = episodeFiles[0].Id)
                .With(v => v.SeriesId = series[0].Id)
                .BuildListOfNew();

            Db.InsertMany(episodes);
        }

        [Test]
        public void should_lazy_load_qualityprofile_if_not_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var episodes = DataMapper.Query<Episode>()
                .Join<Episode, Series>(Marr.Data.QGen.JoinType.Inner, v => v.Series, (l, r) => l.SeriesId == r.Id)
                .ToList();

            foreach (var episode in episodes)
            {
                Assert.IsNotNull(episode.Series);
                Assert.IsFalse(episode.Series.QualityProfile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_episodefile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var episodes = DataMapper.Query<Episode>()
                .Join<Episode, EpisodeFile>(Marr.Data.QGen.JoinType.Inner, v => v.EpisodeFile, (l, r) => l.EpisodeFileId == r.Id)
                .ToList();

            foreach (var episode in episodes)
            {
                Assert.IsNull(episode.Series);
                Assert.IsTrue(episode.EpisodeFile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_qualityprofile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var episodes = DataMapper.Query<Episode>()
                .Join<Episode, Series>(Marr.Data.QGen.JoinType.Inner, v => v.Series, (l, r) => l.SeriesId == r.Id)
                .Join<Series, QualityProfile>(Marr.Data.QGen.JoinType.Inner, v => v.QualityProfile, (l, r) => l.QualityProfileId == r.Id)
                .ToList();

            foreach (var episode in episodes)
            {
                Assert.IsNotNull(episode.Series);
                Assert.IsTrue(episode.Series.QualityProfile.IsLoaded);
            }
        }

    }
}