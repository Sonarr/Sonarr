using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.TvTests.SeriesRepositoryTests
{
    [TestFixture]

    public class SeriesRepositoryFixture : DbTest<SeriesRepository, Series>
    {
        [Test]
        public void should_lazyload_quality_profile()
        {
            var profile = new QualityProfile
                {
                    Items = Qualities.QualityFixture.GetDefaultQualities(Quality.Bluray1080p, Quality.DVD, Quality.HDTV720p),

                    Cutoff = Quality.Bluray1080p,
                    Name = "TestProfile"
                };


            Mocker.Resolve<QualityProfileRepository>().Insert(profile);

            var series = Builder<Series>.CreateNew().BuildNew();
            series.QualityProfileId = profile.Id;

            Subject.Insert(series);


            StoredModel.QualityProfile.Should().NotBeNull();


        }
    }
}