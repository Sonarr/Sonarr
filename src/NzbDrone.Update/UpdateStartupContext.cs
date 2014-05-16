using System;

namespace NzbDrone.Update
{
    public class UpdateStartupContext
    {
        public Int32 ProcessId { get; set; }
        public String ExecutingApplication { get; set; }
        public String UpdateLocation { get; set; }
    }
}
