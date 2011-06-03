// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
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
            var repo = MockLib.GetEmptyRepository();
            var testProfile = new QualityProfile
                                  {
                                      Name = Guid.NewGuid().ToString(),
                                      Cutoff = QualityTypes.SDTV,
                                      Allowed = new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.DVD },
                                  };

            //Act
            var id = (int)repo.Add(testProfile);
            var fetch = repo.Single<QualityProfile>(c => c.QualityProfileId == id);

            //Assert
            Assert.AreEqual(id, fetch.QualityProfileId);
            Assert.AreEqual(testProfile.Name, fetch.Name);
            Assert.AreEqual(testProfile.Cutoff, fetch.Cutoff);
            Assert.AreEqual(testProfile.Allowed, fetch.Allowed);
        }

        [Test]
        public void Test_Series_Quality()
        {
            //Arrange
            var repo = MockLib.GetEmptyRepository();

            var testProfile = new QualityProfile
                                  {
                                      Name = Guid.NewGuid().ToString(),
                                      Cutoff = QualityTypes.SDTV,
                                      Allowed = new List<QualityTypes> { QualityTypes.HDTV, QualityTypes.DVD },
                                  };


            var profileId = (int)repo.Add(testProfile);

            var series = Builder<Series>.CreateNew().Build();
            series.QualityProfileId = profileId;

            repo.Add(testProfile);
            repo.Add(series);

            var result = repo.All<Series>();


            result.Should().HaveCount(1);
            Assert.AreEqual(result.ToList()[0].QualityProfile.Name, testProfile.Name);

            //Act
        }
    }
}