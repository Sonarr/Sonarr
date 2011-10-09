using System.Diagnostics;

namespace NzbDrone.Model
{
    public class ProcessInfo
    {
        public int Id { get; set; }
        public ProcessPriorityClass Priority { get; set; }
        public string StartPath { get; set; }
    }
}