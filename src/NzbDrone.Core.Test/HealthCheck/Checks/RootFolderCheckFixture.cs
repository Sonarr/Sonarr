using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class RootFolderCheckFixture : CoreTest<RootFolderCheck>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Some Warning Message");
        }

        private void GivenMissingRootFolder(string rootFolderPath)
        {
            var series = Builder<Series>.CreateListOfSize(1)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeriesPaths())
                  .Returns(series.ToDictionary(s => s.Id, s => s.Path));

            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                  .Returns(rootFolderPath);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);
        }

        [Test]
        public void should_not_return_error_when_no_series()
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeriesPaths())
                  .Returns(new Dictionary<int, string>());

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_error_if_series_parent_is_missing()
        {
            GivenMissingRootFolder(@"C:\TV".AsOsAgnostic());

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_if_series_path_is_for_posix_os()
        {
            WindowsOnly();
            GivenMissingRootFolder("/mnt/tv");

            Subject.Check().ShouldBeError();
        }

        [Test]
        public void should_return_error_if_series_path_is_for_windows()
        {
            PosixOnly();
            GivenMissingRootFolder(@"C:\TV");

            Subject.Check().ShouldBeError();
        }
    }
}
