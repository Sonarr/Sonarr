// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;

using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QualityProfileTest : SqlCeTest<QualityProvider>
    {
        [SetUp]
        public void SetUp()
        {
            WithRealDb();
        }

        [Test]
        public void Test_Storage()
        {
            //Arrange
            var testProfile = new QualityProfile
                                  {
                                      Name = Guid.NewGuid().ToString(),
                                      Cutoff = QualityTypes.SDTV,
                                      Allowed = new List<QualityTypes> { QualityTypes.HDTV720p, QualityTypes.DVD },
                                  };


            var id = Convert.ToInt32(Db.Insert(testProfile));
            var fetch = Db.SingleOrDefault<QualityProfile>(id);


            Assert.AreEqual(id, fetch.QualityProfileId);
            Assert.AreEqual(testProfile.Name, fetch.Name);
            Assert.AreEqual(testProfile.Cutoff, fetch.Cutoff);
            Assert.AreEqual(testProfile.Allowed, fetch.Allowed);
        }


        [Test]
        public void Test_Storage_no_allowed()
        {
            //Arrange
            var testProfile = new QualityProfile
            {
                Name = Guid.NewGuid().ToString(),
                Cutoff = QualityTypes.SDTV
            };


            var id = Convert.ToInt32(Db.Insert(testProfile));
            var fetch = Db.SingleOrDefault<QualityProfile>(id);


            Assert.AreEqual(id, fetch.QualityProfileId);
            Assert.AreEqual(testProfile.Name, fetch.Name);
            Assert.AreEqual(testProfile.Cutoff, fetch.Cutoff);
            fetch.Allowed.Should().HaveCount(0);
        }


        [Test]
        public void Update_Success()
        {
            var testProfile = new QualityProfile
            {
                Name = Guid.NewGuid().ToString(),
                Cutoff = QualityTypes.SDTV
            };


            var id = Convert.ToInt32(Db.Insert(testProfile));
            var currentProfile = Db.SingleOrDefault<QualityProfile>(id);


            //Update
            currentProfile.Cutoff = QualityTypes.Bluray720p;
            Mocker.Resolve<QualityProvider>().Update(currentProfile);

            var updated = Mocker.Resolve<QualityProvider>().Get(currentProfile.QualityProfileId);


            updated.Name.Should().Be(currentProfile.Name);
            updated.Cutoff.Should().Be(QualityTypes.Bluray720p);
            updated.AllowedString.Should().Be(currentProfile.AllowedString);

        }

        [Test]
        public void Test_Series_Quality()
        {
            var testProfile = new QualityProfile
                                  {
                                      Name = Guid.NewGuid().ToString(),
                                      Cutoff = QualityTypes.SDTV,
                                      Allowed = new List<QualityTypes> { QualityTypes.HDTV720p, QualityTypes.DVD },
                                  };


            var profileId = Convert.ToInt32(Db.Insert(testProfile));

            var series = Builder<Series>.CreateNew().Build();
            series.QualityProfileId = profileId;

            Db.Insert(testProfile);
            Db.Insert(series);

            var result = Db.Fetch<Series>();

            result.Should().HaveCount(1);
            var profile = Db.SingleOrDefault<QualityProfile>(result[0].QualityProfileId);
            Assert.AreEqual(profileId, result[0].QualityProfileId);
            Assert.AreEqual(testProfile.Name, profile.Name);
        }


        [Test]
        public void SetupInitial_should_add_two_profiles()
        {

            Mocker.Resolve<QualityProvider>();


            var profiles = Mocker.Resolve<QualityProvider>().All();


            profiles.Should().HaveCount(2);
            profiles.Should().Contain(e => e.Name == "HD");
            profiles.Should().Contain(e => e.Name == "SD");

        }

        [Test]
        //This confirms that new profiles are added only if no other profiles exists.
        //We don't want to keep adding them back if a user deleted them on purpose.
        public void SetupInitial_should_skip_if_any_profile_exists()
        {
            InitiateSubject();

            var profiles = Subject.All();
            Subject.Delete(profiles[0].QualityProfileId);

            InitiateSubject();

            Subject.All().Should().HaveCount(profiles.Count - 1);
        }
    }
}