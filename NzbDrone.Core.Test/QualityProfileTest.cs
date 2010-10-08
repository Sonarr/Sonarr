using System.Collections.Generic;
using System.IO;
using MbUnit.Framework;
using NzbDrone.Core.Entities.Quality;

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
                Cutoff = QualityTypes.SDTV,
                Allowed = new List<QualityTypes>() { QualityTypes.HDTV, QualityTypes.DVD },
            };

            //Act
            var id = (int)repo.Add(testProfile);
            var fetch = repo.Single<QualityProfile>(c => c.Id == id);

            //Assert
            Assert.AreEqual(id, fetch.Id);
            Assert.AreEqual(testProfile.Cutoff, fetch.Cutoff);
            Assert.AreEqual(testProfile.Allowed, fetch.Allowed);
        }
    }
}