using FizzWare.NBuilder;
using NUnit.Framework;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Test.Languages;

namespace NzbDrone.Core.Test.Datastore
{

    [TestFixture]
    public class MarrDataLazyLoadingFixture : DbTest
    {
        [SetUp]
        public void Setup()
        {
            var profile = new Profile
                {
                    Name = "Test",
                    Cutoff = Quality.WEBDL720p,
                    Items = Qualities.QualityFixture.GetDefaultQualities(),
                };

            var languageProfile = new LanguageProfile
                {
                    Name = "Test",
                    Languages = LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                };

            profile = Db.Insert(profile);
            languageProfile = Db.Insert(languageProfile);

            var series = Builder<Series>.CreateListOfSize(1)
                .All()
                .With(v => v.ProfileId = profile.Id)
                .With(v => v.LanguageProfileId = languageProfile.Id)
                .BuildListOfNew();

            Db.InsertMany(series);

            var episodeFiles = Builder<EpisodeFile>.CreateListOfSize(1)
                .All()
                .With(v => v.SeriesId = series[0].Id)
                .With(v => v.Quality = new QualityModel())
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
        public void should_lazy_load_profile_if_not_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var episodes = DataMapper.Query<Episode>()
                .Join<Episode, Series>(Marr.Data.QGen.JoinType.Inner, v => v.Series, (l, r) => l.SeriesId == r.Id)
                .ToList();

            foreach (var episode in episodes)
            {
                Assert.IsNotNull(episode.Series);
                Assert.IsFalse(episode.Series.Profile.IsLoaded);
                Assert.IsFalse(episode.Series.LanguageProfile.IsLoaded);
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
        public void should_explicit_load_profile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var episodes = DataMapper.Query<Episode>()
                .Join<Episode, Series>(Marr.Data.QGen.JoinType.Inner, v => v.Series, (l, r) => l.SeriesId == r.Id)
                .Join<Series, Profile>(Marr.Data.QGen.JoinType.Inner, v => v.Profile, (l, r) => l.ProfileId == r.Id)
                .ToList();

            foreach (var episode in episodes)
            {
                Assert.IsNotNull(episode.Series);
                Assert.IsTrue(episode.Series.Profile.IsLoaded);
                Assert.IsFalse(episode.Series.LanguageProfile.IsLoaded);
            }
        }

        [Test]
        public void should_explicit_load_languageprofile_if_joined()
        {
            var db = Mocker.Resolve<IDatabase>();
            var DataMapper = db.GetDataMapper();

            var episodes = DataMapper.Query<Episode>()
                                     .Join<Episode, Series>(Marr.Data.QGen.JoinType.Inner, v => v.Series, (l, r) => l.SeriesId == r.Id)
                                     .Join<Series, LanguageProfile>(Marr.Data.QGen.JoinType.Inner, v => v.LanguageProfile, (l, r) => l.ProfileId == r.Id)
                                     .ToList();

            foreach (var episode in episodes)
            {
                Assert.IsNotNull(episode.Series);
                Assert.IsFalse(episode.Series.Profile.IsLoaded);
                Assert.IsTrue(episode.Series.LanguageProfile.IsLoaded);
            }
        }
    }
}