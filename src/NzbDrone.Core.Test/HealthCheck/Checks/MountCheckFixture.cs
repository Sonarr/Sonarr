using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.HealthCheck.Checks;
using NzbDrone.Core.Localization;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.HealthCheck.Checks
{
    [TestFixture]
    public class MountCheckFixture : CoreTest<MountCheck>
    {
        private readonly string _seriesPath = @"C:\TV\Series Title (2025)".AsOsAgnostic();
        private readonly string _otherSeriesPath = @"C:\TV\Other Series Title (2025)".AsOsAgnostic();
        private readonly string _readOnlySeriesPath = @"D:\TV\Series Title (2025)".AsOsAgnostic();
        private readonly string _otherReadOnlySeriesPath = @"D:\TV\Other Series Title (2025)".AsOsAgnostic();

        private IMount _writableMount;
        private IMount _readOnlyMount;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<ILocalizationService>()
                  .Setup(s => s.GetLocalizedString(It.IsAny<string>()))
                  .Returns("Mount containing a series path is mounted read-only: ");

            _writableMount = CreateMount("/dev/sda1", @"C:\".AsOsAgnostic(), false);
            _readOnlyMount = CreateMount("/dev/sdb1", @"D:\".AsOsAgnostic(), true);

            GivenSeries();
        }

        private static IMount CreateMount(string name, string rootDirectory, bool? readOnly)
        {
            var options = readOnly.HasValue
                ? new MountOptions(new Dictionary<string, string> { { readOnly.Value ? "ro" : "rw", string.Empty } })
                : null;

            var mount = new Mock<IMount>();
            mount.SetupGet(s => s.Name).Returns(name);
            mount.SetupGet(s => s.RootDirectory).Returns(rootDirectory);
            mount.SetupGet(s => s.MountOptions).Returns(options);

            return mount.Object;
        }

        private void GivenSeries(params (string Path, IMount Mount)[] series)
        {
            Mocker.GetMock<ISeriesService>()
                  .Setup(s => s.GetAllSeriesPaths())
                  .Returns(series.Select((s, i) => (Id: i + 1, s.Path)).ToDictionary(s => s.Id, s => s.Path));

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.GetMountsForPaths(It.IsAny<HashSet<string>>()))
                  .Returns(series.ToDictionary(s => s.Path, s => s.Mount));
        }

        [Test]
        public void should_return_ok_when_there_are_no_series()
        {
            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_series_are_on_writable_mounts()
        {
            GivenSeries((_seriesPath, _writableMount), (_otherSeriesPath, _writableMount));

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mount_of_series_is_unknown()
        {
            GivenSeries((_seriesPath, null));

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_ok_when_mount_options_of_series_are_unknown()
        {
            GivenSeries((_seriesPath, CreateMount("/dev/sda1", @"C:\".AsOsAgnostic(), null)));

            Subject.Check().ShouldBeOk();
        }

        [Test]
        public void should_return_error_when_series_is_on_read_only_mount()
        {
            GivenSeries((_seriesPath, _writableMount), (_readOnlySeriesPath, _readOnlyMount));

            Subject.Check().ShouldBeError($"/dev/sdb1 ({_readOnlySeriesPath})", "series-mount-ro");
        }

        [Test]
        public void should_list_read_only_mount_once_with_the_path_of_its_first_series()
        {
            GivenSeries((_readOnlySeriesPath, _readOnlyMount), (_otherReadOnlySeriesPath, _readOnlyMount));

            var result = Subject.Check();

            result.ShouldBeError($"read-only: /dev/sdb1 ({_readOnlySeriesPath})");
            result.Message.Should().NotContain(_otherReadOnlySeriesPath);
        }

        [Test]
        public void should_get_the_mounts_of_all_series_paths_at_once()
        {
            GivenSeries((_seriesPath, _writableMount), (_otherSeriesPath, _writableMount), (_readOnlySeriesPath, _readOnlyMount));

            Subject.Check();

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetMountsForPaths(It.Is<HashSet<string>>(p => p.SetEquals(new[] { _seriesPath, _otherSeriesPath, _readOnlySeriesPath }))), Times.Once());

            Mocker.GetMock<IDiskProvider>()
                  .Verify(v => v.GetMount(It.IsAny<string>()), Times.Never());
        }
    }
}
