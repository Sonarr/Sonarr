namespace NzbDrone.Core.Download
{
    public enum ProcessedDecisionResult
    {
        Grabbed,
        Pending,
        Rejected,
        Failed,
        Skipped
    }
}
