namespace NzbDrone.Update
{
    public class UpdateStartupContext
    {
        public int ProcessId { get; set; }
        public string ExecutingApplication { get; set; }
        public string UpdateLocation { get; set; }
    }
}
