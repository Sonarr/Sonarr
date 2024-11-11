namespace NzbDrone.Core.DecisionEngine;

public class DownloadRejection : Rejection<DownloadRejectionReason>
{
    public DownloadRejection(DownloadRejectionReason reason, string message, RejectionType type = RejectionType.Permanent)
        : base(reason, message, type)
    {
    }
}
