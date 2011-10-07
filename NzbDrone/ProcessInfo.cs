using System.Diagnostics;

namespace NzbDrone
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public ProcessPriorityClass Priority { get; set; }
        public string StartPath { get; set; }

        public bool HasExited { get; set; }
    }
}