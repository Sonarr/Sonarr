using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class HttpLogFixture : IntegrationTest
    {
        [Test]
        [Retry(5)]
        public void should_log_on_error()
        {
            var config = HostConfig.Get(1);
            config.LogLevel = "Trace";
            HostConfig.Put(config);
            
            var resultGet = Series.All();

            var logFile = Path.Combine(_runner.AppData, "logs", "sonarr.trace.txt");
            var logLines = File.ReadAllLines(logFile);

            var resultPost = Series.InvalidPost(new Api.Series.SeriesResource());

            logLines = File.ReadAllLines(logFile).Skip(logLines.Length).ToArray();

            logLines.Should().Contain(v => v.Contains("|Trace|Http|Req"));
            logLines.Should().Contain(v => v.Contains("|Trace|Http|Res"));
            logLines.Should().Contain(v => v.Contains("|Debug|Api|"));
        }
    }
}
