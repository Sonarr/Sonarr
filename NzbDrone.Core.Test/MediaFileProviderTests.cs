using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using FizzWare.NBuilder;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.Moq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileProviderTests
    {

        [Test]
        [Description("Verifies that a new file imported properly")]
        public void import_new_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = @"WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int seasonNumber = 3;
            const int episodeNumner = 01;
            const int size = 12345;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var repository = new Mock<IRepository>();
            repository.Setup(r => r.Exists<EpisodeFile>(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(false).Verifiable();

            var episodeProvider = new Mock<IEpisodeProvider>();
            episodeProvider.Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns(fakeEpisode).Verifiable();

            var diskProvider = new Mock<IDiskProvider>();
            diskProvider.Setup(e => e.GetSize(fileName)).Returns(12345).Verifiable();

            var kernel = new MockingKernel();
            kernel.Bind<IRepository>().ToConstant(repository.Object);

            kernel.Bind<IEpisodeProvider>().ToConstant(episodeProvider.Object);
            kernel.Bind<IDiskProvider>().ToConstant(diskProvider.Object);
            kernel.Bind<IMediaFileProvider>().To<MediaFileProvider>();

            //Act
            var result = kernel.Get<IMediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            repository.VerifyAll();
            episodeProvider.VerifyAll();
            diskProvider.VerifyAll();
            Assert.IsNotNull(result);
            repository.Verify(r => r.Add<EpisodeFile>(result), Times.Once());

            Assert.AreEqual(fakeEpisode.EpisodeId, result.EpisodeId);
            Assert.AreEqual(fakeEpisode.SeriesId, result.SeriesId);
            Assert.AreEqual(QualityTypes.DVD, result.Quality);
            Assert.AreEqual(Parser.NormalizePath(fileName), result.Path);
            Assert.AreEqual(size, result.Size);
            Assert.AreEqual(false, result.Proper);
            Assert.AreNotEqual(new DateTime(), result.DateAdded);
        }


        [Test]
        [Description("Verifies that an existing file will skip import")]
        public void import_existing_file()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();
            var fakeEpisode = Builder<Episode>.CreateNew().With(c => c.SeriesId = fakeSeries.SeriesId).Build();

            //Mocks
            var repository = new Mock<IRepository>(MockBehavior.Strict);
            repository.Setup(r => r.Exists<EpisodeFile>(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(true).Verifiable();

            var episodeProvider = new Mock<IEpisodeProvider>(MockBehavior.Strict);
            var diskProvider = new Mock<IDiskProvider>(MockBehavior.Strict);

            var kernel = new MockingKernel();
            kernel.Bind<IRepository>().ToConstant(repository.Object);

            kernel.Bind<IEpisodeProvider>().ToConstant(episodeProvider.Object);
            kernel.Bind<IDiskProvider>().ToConstant(diskProvider.Object);
            kernel.Bind<IMediaFileProvider>().To<MediaFileProvider>();

            //Act
            var result = kernel.Get<IMediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            repository.VerifyAll();
            episodeProvider.VerifyAll();
            diskProvider.VerifyAll();
            Assert.IsNull(result);
            repository.Verify(r => r.Add<EpisodeFile>(result), Times.Never());
        }

        [Test]
        [Description("Verifies that a file with no episode is skipped")]
        public void import_file_with_no_episode()
        {
            //Arrange
            /////////////////////////////////////////

            //Constants
            const string fileName = "WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD.avi";
            const int seasonNumber = 3;
            const int episodeNumner = 01;

            //Fakes
            var fakeSeries = Builder<Series>.CreateNew().Build();

            //Mocks
            var repository = new Mock<IRepository>(MockBehavior.Strict);
            repository.Setup(r => r.Exists<EpisodeFile>(It.IsAny<Expression<Func<EpisodeFile, Boolean>>>())).Returns(false).Verifiable();

            var episodeProvider = new Mock<IEpisodeProvider>(MockBehavior.Strict);
            episodeProvider.Setup(e => e.GetEpisode(fakeSeries.SeriesId, seasonNumber, episodeNumner)).Returns<Episode>(null).Verifiable();

            var diskProvider = new Mock<IDiskProvider>(MockBehavior.Strict);


            var kernel = new MockingKernel();
            kernel.Bind<IRepository>().ToConstant(repository.Object);
            kernel.Bind<IEpisodeProvider>().ToConstant(episodeProvider.Object);
            kernel.Bind<IDiskProvider>().ToConstant(diskProvider.Object);
            kernel.Bind<IMediaFileProvider>().To<MediaFileProvider>();

            //Act
            var result = kernel.Get<IMediaFileProvider>().ImportFile(fakeSeries, fileName);

            //Assert
            repository.VerifyAll();
            episodeProvider.VerifyAll();
            diskProvider.VerifyAll();
            Assert.IsNull(result);
            repository.Verify(r => r.Add<EpisodeFile>(result), Times.Never());
        }


        [Test]
        [Row("Season {season}\\S{season:00}E{episode:00} - {title} - {quality}", "Season 6\\S06E08 - Lethal Inspection - hdtv")]
        [Row("Season {season}\\{series} - {season:##}{episode:00} - {title} - {quality}", "Season 6\\Futurama - 608 - Lethal Inspection - hdtv")]
        [Row("Season {season}\\{series} - {season:##}{episode:00} - {title}", "Season 6\\Futurama - 608 - Lethal Inspection")]
        public void test_file_path_generation(string patern, string path)
        {
            var fakeConfig = new Mock<IConfigProvider>();
            fakeConfig.Setup(c => c.EpisodeNameFormat).Returns(patern);

            var kernel = new MockingKernel();
            kernel.Bind<IConfigProvider>().ToConstant(fakeConfig.Object);
            kernel.Bind<IMediaFileProvider>().To<MediaFileProvider>();

            var fakeEpisode = new EpisodeModel
            {
                SeasonNumber = 6,
                EpisodeNumber = 8,
                EpisodeTitle = "Lethal Inspection",
                Quality = QualityTypes.HDTV,
                SeriesTitle = "Futurama"
            };

            //Act
            var result = kernel.Get<IMediaFileProvider>().GenerateEpisodePath(fakeEpisode);

            //Assert
            Assert.AreEqual(path.ToLowerInvariant(), result.ToLowerInvariant());
        }

    }


}
