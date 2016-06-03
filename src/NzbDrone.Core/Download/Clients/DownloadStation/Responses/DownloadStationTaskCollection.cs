using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;

namespace NzbDrone.Core.Download.Clients.DownloadStation.Responses
{
    [JsonObject]
    public class DownloadStationTaskCollection : IEnumerable<DownloadStationTask>
    {
        public IEnumerable<DownloadStationTask> Tasks { get; set; }

        public IEnumerator<DownloadStationTask> GetEnumerator()
        {
            return Tasks.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
