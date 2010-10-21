using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FizzWare.NBuilder;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using NzbDrone.Core.Model;
using NzbDrone.Core.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class RepoTest
    {
        [Test]
        public void to_many__series_to_episode()
        {
            //Arrange
            var fakeSeries = Builder<Series>.CreateNew().With(s => s.SeriesId = 69).Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = 69).Build();

            //Act
            var repo = MockLib.GetEmptyRepository();
            repo.Add(fakeSeries);
            repo.Add(fakeEpisode);
            var fetchedSeries = repo.Single<Series>(fakeSeries.SeriesId);

            //Assert
            Assert.AreEqual(fakeSeries.SeriesId, fetchedSeries.SeriesId);
            Assert.AreEqual(fakeSeries.Title, fetchedSeries.Title);

            Assert.IsNotEmpty(fetchedSeries.Episodes);
            Assert.AreEqual(fetchedSeries.Episodes[0].EpisodeId, fakeEpisode.EpisodeId);
            Assert.AreEqual(fetchedSeries.Episodes[0].SeriesId, fakeEpisode.SeriesId);
            Assert.AreEqual(fetchedSeries.Episodes[0].Title, fakeEpisode.Title);
        }

        [Test]
        [Description("This test confirms that the tvdb id stored in the db is preserved rather than being replaced by an auto incrementing value")]
        public void tvdbid_is_preserved([RandomNumbers(Minimum = 100, Maximum = 999, Count = 1)] int tvdbId)
        {
            //Arrange
            var sonicRepo = MockLib.GetEmptyRepository();
            var series = Builder<Series>.CreateNew().With(c => c.SeriesId = tvdbId).Build();

            //Act
            var addId = sonicRepo.Add(series);

            //Assert
            Assert.AreEqual(tvdbId, addId);
            var allSeries = sonicRepo.All<Series>();
            Assert.IsNotEmpty(allSeries);
            Assert.AreEqual(tvdbId, allSeries.First().SeriesId);
        }

        [Test]
        public void enteties_toString()
        {
            Console.WriteLine(new Episode().ToString());
            Console.WriteLine(new EpisodeModel().ToString());
        }
    }
}
