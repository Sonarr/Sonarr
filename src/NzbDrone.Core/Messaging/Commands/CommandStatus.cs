namespace NzbDrone.Core.Messaging.Commands
{
    public enum CommandStatus
    {
        Queued = 0,
        Started = 1,
        Completed = 2,
        Failed = 3,
        Aborted = 4,
        Cancelled = 5,
        Orphaned = 6
    }
}
