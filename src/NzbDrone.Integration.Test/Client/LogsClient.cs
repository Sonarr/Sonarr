using System;
using System.Threading;
using RestSharp;

namespace NzbDrone.Integration.Test.Client
{
    public class LogsClient : ClientBase
    {
        public LogsClient(IRestClient restClient, string apiKey)
            : base(restClient, apiKey, "log/file")
        {
        }

        public string[] GetLogFileLines(string filename)
        {
            var attempts = 10;
            var attempt = 1;
            while (true)
            {
                try
                {
                    var request = BuildRequest(filename);
                    var content = Execute(request, System.Net.HttpStatusCode.OK);

                    var lines = content.Split('\n');
                    lines = Array.ConvertAll(lines, s => s.TrimEnd('\r'));
                    Array.Resize(ref lines, lines.Length - 1);
                    return lines;
                }
                catch (Exception ex)
                {
                    if (attempt == attempts)
                    {
                        _logger.Error(ex, "Failed to get log lines");
                        throw;
                    }

                    _logger.Info(ex, "Failed to get log lines, attempt {0}/{1}", attempt, attempts);
                    Thread.Sleep(10);
                    attempt++;
                }
            }
        }
    }
}
