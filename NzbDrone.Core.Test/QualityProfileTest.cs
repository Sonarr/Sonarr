// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class QualityProfileTest : TestBase
    {
        ///<summary>
        ///  Test_s the storage.
        ///</summary>
        [Test]
        public void Test_Storage()
        {
            //Arrange
            var database = MockLib.GetEmptyDatabase();
            var testProfile = new QualityProfile
                                  {
                                      Name = Guid.NewGuid().ToString(),
                                      Cutoff = QualityTypes.SDTV,
                                      Allowed = new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.DVD },
                                  };

            //Act
            var id = Convert.ToInt32(database.Insert(testProfile));
            var fetch = database.SingleOrDefault<QualityProfile>(id);

            //Assert
            Assert.AreEqual(id, fetch.QualityProfileId);
            Assert.AreEqual(testProfile.Name, fetch.Name);
            Assert.AreEqual(testProfile.Cutoff, fetch.Cutoff);
            Assert.AreEqual(testProfile.Allowed, fetch.Allowed);
        }


        [Test]
        public void Test_Storage_no_allowed()
        {
            //Arrange
            var database = MockLib.GetEmptyDatabase();
            var testProfile = new QualityProfile
            {
                Name = Guid.NewGuid().ToString(),
                Cutoff = QualityTypes.SDTV
            };

            //Act
            var id = Convert.ToInt32(database.Insert(testProfile));
            var fetch = database.SingleOrDefault<QualityProfile>(id);

            //Assert
            Assert.AreEqual(id, fetch.QualityProfileId);
            Assert.AreEqual(testProfile.Name, fetch.Name);
            Assert.AreEqual(testProfile.Cutoff, fetch.Cutoff);
            fetch.Allowed.Should().HaveCount(0);
        }

        [Test]
        public void Test_Series_Quality()
        {
            //Arrange
            var database = MockLib.GetEmptyDatabase();

            var testProfile = new QualityProfile
                                  {
                                      Name = Guid.NewGuid().ToString(),
                                      Cutoff = QualityTypes.SDTV,
                                      Allowed = new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.DVD },
                                  };


            var profileId = Convert.ToInt32(database.Insert(testProfile));

            var series = Builder<Series>.CreateNew().Build();
            series.QualityProfileId = profileId;

            database.Insert(testProfile);
            database.Insert(series);

            var result = database.Fetch<Series>();

            result.Should().HaveCount(1);
            var profile = database.SingleOrDefault<QualityProfile>(result[0].QualityProfileId);
            Assert.AreEqual(profileId, result[0].QualityProfileId);
            Assert.AreEqual(testProfile.Name, profile.Name);
        }
    }
}