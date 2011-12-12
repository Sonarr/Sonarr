using System.Diagnostics;

namespace NzbDrone.Common.Model
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartPath { get; set; }
        public ProcessPriorityClass Priority { get; set; }

        public override string ToString()
        {
            return string.Format("{0}:{1} [{2}] [{3}]", Id, Name ?? "Unknown", StartPath ?? "Unknown", Priority);
        }
    }
}