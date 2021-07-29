using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sonarr.Api.V3.Series;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class HttpLogFixture : IntegrationTest
    {
        [Test]
        public void should_log_on_error()
        {
            var config = HostConfig.Get(1);
            config.LogLevel = "Trace";
            HostConfig.Put(config);

            var resultGet = Series.All();

            var logFile = "sonarr.trace.txt";
            var logLines = Logs.GetLogFileLines(logFile);

            var resultPost = Series.InvalidPost(new SeriesResource());

            // Skip 2 and 1 to ignore the logs endpoint
            logLines = Logs.GetLogFileLines(logFile).Skip(logLines.Length + 2).ToArray();
            Array.Resize(ref logLines, logLines.Length - 1);

            logLines.Should().Contain(v => v.Contains("|Trace|Http|Req") && v.Contains("/api/series/"));
            logLines.Should().Contain(v => v.Contains("|Trace|Http|Res") && v.Contains("/api/series/: 400.BadRequest"));
            logLines.Should().Contain(v => v.Contains("|Debug|Api|") && v.Contains("/api/series/: 400.BadRequest"));
        }
    }
}
