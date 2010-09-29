using MbUnit.Framework;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
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
            var repo = MockLib.MemoryRepository;
            var testProfile = new QualityProfile
            {
                Cutoff = Quality.SDTV,
                Q = new[] { Quality.HDTV, Quality.DVD }
            };

            //Act
            var id = (int)repo.Add(testProfile);
            var fetch = repo.Single<QualityProfile>(c => c.Id == id);

            //Assert
            Assert.AreEqual(id, fetch.Id);
            Assert.AreEqual(testProfile.Cutoff, fetch.Cutoff);
            Assert.AreEqual(testProfile.Qualitys, fetch.Qualitys);
            Assert.AreElementsEqual(testProfile.Q, fetch.Q);
        }
    }
}