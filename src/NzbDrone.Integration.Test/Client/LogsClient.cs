using System;
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
            var request = BuildRequest(filename);
            var content = Execute(request, System.Net.HttpStatusCode.OK);

            var lines = content.Split('\n');
            lines = Array.ConvertAll(lines, s => s.TrimEnd('\r'));
            Array.Resize(ref lines, lines.Length - 1);
            return lines;
        }
    }
}