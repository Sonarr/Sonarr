using System;

namespace NzbDrone.Common.Processes
{
    public class ProcessOutputLine
    {
        public ProcessOutputLevel Level { get; set; }
        public string Content { get; set; }
        public DateTime Time { get; set; }

        public ProcessOutputLine(ProcessOutputLevel level, string content)
        {
            Level = level;
            Content = content;
            Time = DateTime.UtcNow;
        }

        public override string ToString()
        {
            return String.Format("{0} - {1} - {2}", Time, Level, Content);
        }
    }

    public enum ProcessOutputLevel
    {
        Standard = 0,
        Error = 1
    }
}
