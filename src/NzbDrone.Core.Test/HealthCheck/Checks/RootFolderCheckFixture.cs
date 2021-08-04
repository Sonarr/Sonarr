using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class RootFolderCheckFixture : CoreTest<RootFolderCheck>
    {
        private void GivenMissingRootFolder()
        {
            var series = Builder<Series>.CreateListOfSize(1)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeriesPaths())
                  .Returns(series.ToDictionary(s => s.Id, s => s.Path));

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetParentFolder(series.First().Path))
                  .Returns(@"C:\TV");

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
            GivenMissingRootFolder();

            Subject.Check().ShouldBeError();
        }
    }
}
