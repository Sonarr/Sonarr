using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Profiles.Languages;

namespace NzbDrone.Core.Test.TvTests.SeriesRepositoryTests
{
    [TestFixture]

    public class SeriesRepositoryFixture : DbTest<SeriesRepository, Series>
    {
        [Test]
        public void should_lazyload_quality_profile()
        {
            var profile = new Profile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities(Quality.Bluray1080p, Quality.DVD, Quality.HDTV720p),

                    Cutoff = Quality.Bluray1080p,
                    Name = "TestProfile"
                };

            var langProfile = new LanguageProfile
                {
                    Name = "TestProfile",
                    Languages = Languages.LanguageFixture.GetDefaultLanguages(Language.English),
                    Cutoff = Language.English
                };


            Mocker.Resolve<ProfileRepository>().Insert(profile);
            Mocker.Resolve<LanguageProfileRepository>().Insert(langProfile);

            var series = Builder<Series>.CreateNew().BuildNew();
            series.ProfileId = profile.Id;
            series.LanguageProfileId = langProfile.Id;

            Subject.Insert(series);


            StoredModel.Profile.Should().NotBeNull();
            StoredModel.LanguageProfile.Should().NotBeNull();


        }
    }
}