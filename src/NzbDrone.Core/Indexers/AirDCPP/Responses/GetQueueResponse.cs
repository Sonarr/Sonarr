namespace NzbDrone.Core.Indexers.AirDCPP.Responses
{
    public class GetQueueResponse
    {
        public QueueResult[] Property1 { get; set; }
    }

    public class QueueResult
    {
        public float downloaded_bytes { get; set; }
        public long id { get; set; }
        public string name { get; set; }
        public Priority priority { get; set; }
        public float seconds_left { get; set; }
        public float size { get; set; }
        public Sources sources { get; set; }
        public float speed { get; set; }
        public Status status { get; set; }
        public string target { get; set; }
        public float time_added { get; set; }
        public float time_finished { get; set; }
        public Type type { get; set; }
    }

    public class Priority
    {
        public bool auto { get; set; }
        public long id { get; set; }
        public string str { get; set; }
    }

    public class Sources
    {
        public int online { get; set; }
        public string str { get; set; }
        public int total { get; set; }
    }

    public class Status
    {
        public bool completed { get; set; }
        public bool downloaded { get; set; }
        public bool failed { get; set; }
        public object hook_error { get; set; }
        public string id { get; set; }
        public string str { get; set; }
    }
}
