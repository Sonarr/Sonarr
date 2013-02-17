using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers;
using NzbDrone.Core.Providers.Core;
using NzbDrone.Core.Repository;
using NzbDrone.Core.Repository.Quality;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using NzbDrone.Test.Common.AutoMoq;

namespace NzbDrone.Core.Test.ProviderTests.DiskScanProviderTests
{
    // ReSharper disable InconsistentNaming
    public class ScanFixture : CoreTest
    {
        [Test]
        public void series_should_update_the_last_scan_date()
        {

            
            Mocker.GetMock<SeriesProvider>()
                .Setup(c => c.UpdateSeries(It.Is<Series>(s => s.LastDiskSync != null))).Verifiable();

            Mocker.GetMock<EpisodeProvider>()
                .Setup(c => c.GetEpisodeBySeries(It.IsAny<long>()))
                .Returns(new List<Episode> { new Episode() });

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(It.IsAny<string>()))
                .Returns(true);

            Mocker.GetMock<MediaFileProvider>()
                .Setup(c => c.GetSeriesFiles(It.IsAny<int>()))
                .Returns(new List<EpisodeFile>());

            Mocker.Resolve<DiskScanProvider>().Scan(new Series());

            Mocker.VerifyAllMocks();

        }

        [Test]
        public void series_should_log_warning_if_path_doesnt_exist_on_disk()
        {
            //Setup
            WithStrictMocker();

            var series = Builder<Series>.CreateNew()
                .With(s => s.Path = @"C:\Test\TV\SeriesName\")
                .Build();


            Mocker.GetMock<MediaFileProvider>()
                    .Setup(c => c.CleanUpDatabase());
   

            Mocker.GetMock<DiskProvider>()
                .Setup(c => c.FolderExists(series.Path))
                .Returns(false);

            //Act
            Mocker.Resolve<DiskScanProvider>().Scan(series, series.Path);

            //Assert
            Mocker.VerifyAllMocks();
            ExceptionVerification.ExpectedWarns(1);
        }
    }
}
