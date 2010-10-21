using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Gallio.Framework;
using MbUnit.Framework;
using MbUnit.Framework.ContractVerifiers;
using Moq;
using Ninject;
using Ninject.Moq;
using NzbDrone.Core.Entities;
using NzbDrone.Core.Entities.Episode;
using NzbDrone.Core.Entities.Quality;
using NzbDrone.Core.Providers;
using SubSonic.Repository;

namespace NzbDrone.Core.Test
{
    [TestFixture]
    // ReSharper disable InconsistentNaming
    public class MediaFileProviderTests
    {
        [Test]
        public void scan_test()
        {
            //Arrange
            var repository = new Mock<IRepository>();
            repository.Setup(c => c.Update(It.IsAny<EpisodeInfo>())).Verifiable();

            var diskProvider = MockLib.GetStandardDisk(1, 2);

            var kernel = new MockingKernel();
            kernel.Bind<IDiskProvider>().ToConstant(diskProvider);
            kernel.Bind<IRepository>().ToConstant(repository.Object);
            kernel.Bind<IMediaFileProvider>().To<MediaFileProvider>();

            var fakeSeries = new Series()
            {
                Path = MockLib.StandardSeries[0]
            };

            //Act
            kernel.Get<IMediaFileProvider>().Scan(fakeSeries);

            //Assert
            repository.Verify(c => c.Update(It.IsAny<EpisodeInfo>()), Times.Exactly(1 * 2));


        }

        [Test]
        [Row("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", 3, 1)]
        [Row("Two.and.a.Half.Me.103.720p.HDTV.X264-DIMENSION", 1, 3)]
        [Row("Chuck.4x05.HDTV.XviD-LOL", 4, 5)]
        [Row("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", 3, 6)]
        [Row("Degrassi.S10E27.WS.DSR.XviD-2HD", 10, 27)]
        public void episode_parse(string path, int season, int episode)
        {
            var result = Parser.ParseBasicEpisode(path);
            Assert.AreEqual(season, result.SeasonNumber);
            Assert.AreEqual(episode, result.EpisodeNumber);
        }

        [Test]
        [Row("WEEDS.S03E01-06.DUAL.BDRip.XviD.AC3.-HELLYWOOD", QualityTypes.DVD)]
        [Row("WEEDS.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", QualityTypes.Bluray)]
        [Row("Two.and.a.Half.Men.S08E05.720p.HDTV.X264-DIMENSION", QualityTypes.HDTV)]
        [Row("Chuck.S04E05.HDTV.XviD-LOL", QualityTypes.TV)]
        [Row("The.Girls.Next.Door.S03E06.DVDRip.XviD-WiDE", QualityTypes.DVD)]
        [Row("Degrassi.S10E27.WS.DSR.XviD-2HD", QualityTypes.TV)]
        [Row("Sonny.With.a.Chance.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", QualityTypes.WEBDL)]
        [Row("Sonny.With.a.Chance.S02E15.720p", QualityTypes.HDTV)]
        [Row("Sonny.With.a.Chance.S02E15.mkv", QualityTypes.HDTV)]
        [Row("Sonny.With.a.Chance.S02E15.avi", QualityTypes.TV)]
        [Row("Sonny.With.a.Chance.S02E15.xvid", QualityTypes.TV)]
        [Row("Sonny.With.a.Chance.S02E15.divx", QualityTypes.TV)]
        [Row("Sonny.With.a.Chance.S02E15", QualityTypes.Unknown)]
        public void quality_parse(string path, object quality)
        {
            var result = Parser.ParseQuality(path);
            Assert.AreEqual(quality, result);
        }

        [Test]
        [Timeout(2)]
        public void quality_parse()
        {
            var sw = Stopwatch.StartNew();
            var name = "WEEDSawdawdadawdawd\\awdawdawdadadad.mkv";
            var quality = QualityTypes.HDTV;

            for (int i = 0; i < 100000; i++)
            {
                Assert.AreEqual(quality, Parser.ParseQuality(name));
            }

            Console.WriteLine(sw.Elapsed.ToString());


        }

    }


}
