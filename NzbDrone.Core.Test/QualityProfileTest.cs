using System;
using System.Collections.Generic;
using System.IO;
using MbUnit.Framework;
using NzbDrone.Core.Repository.Quality;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming

    public class QualityProfileTest
    {
        /// <summary>
        ///   Test_s the storage.
        /// </summary>
        ///
        /// 
        [Test]
        public void Test_Storage()
        {

            //Arrange
            var repo = MockLib.GetEmptyRepository();
            var testProfile = new QualityProfile
                                  {
                                      Name = Guid.NewGuid().ToString(),
                                      Cutoff = QualityTypes.TV,
                                      Allowed = new List<QualityTypes>() { QualityTypes.HDTV, QualityTypes.DVD },
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
    }
}