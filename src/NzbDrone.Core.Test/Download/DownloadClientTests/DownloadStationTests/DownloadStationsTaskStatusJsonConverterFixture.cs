using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.Download.Clients.DownloadStation;

namespace NzbDrone.Core.Test.Download.DownloadClientTests.DownloadStationTests
{
    [TestFixture]
    public class DownloadStationsTaskStatusJsonConverterFixture
    {
        [TestCase("captcha_needed", DownloadStationTaskStatus.CaptchaNeeded)]
        [TestCase("filehosting_waiting", DownloadStationTaskStatus.FilehostingWaiting)]
        [TestCase("hash_checking", DownloadStationTaskStatus.HashChecking)]
        [TestCase("error", DownloadStationTaskStatus.Error)]
        [TestCase("downloading", DownloadStationTaskStatus.Downloading)]
        public void should_parse_enum_correctly(string value, DownloadStationTaskStatus expected)
        {
            var task = "{\"Status\": \"" + value + "\"}";

            var item = JsonConvert.DeserializeObject<DownloadStationTask>(task);

            item.Status.Should().Be(expected);
        }

        [TestCase("captcha_needed", DownloadStationTaskStatus.CaptchaNeeded)]
        [TestCase("filehosting_waiting", DownloadStationTaskStatus.FilehostingWaiting)]
        [TestCase("hash_checking", DownloadStationTaskStatus.HashChecking)]
        [TestCase("error", DownloadStationTaskStatus.Error)]
        [TestCase("downloading", DownloadStationTaskStatus.Downloading)]
        public void should_serialize_enum_correctly(string expected, DownloadStationTaskStatus value)
        {
            var task = new DownloadStationTask { Status = value };

            var item = JsonConvert.SerializeObject(task);

            item.Should().Contain(expected);
        }

        [Test]
        public void should_return_unknown_if_unknown_enum_value()
        {
            var task = "{\"Status\": \"some_unknown_value\"}";

            var item = JsonConvert.DeserializeObject<DownloadStationTask>(task);

            item.Status.Should().Be(DownloadStationTaskStatus.Unknown);
        }
    }
}
