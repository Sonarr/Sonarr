using NzbDrone.Core.DecisionEngine;

namespace Sonarr.Api.V5.Release;

public class ReleaseDecisionResource
{
    public bool Approved { get; set; }
    public bool TemporarilyRejected { get; set; }
    public bool Rejected { get; set; }
    public IEnumerable<DownloadRejectionResource> Rejections { get; set; } = [];

    public ReleaseDecisionResource(DownloadDecision downloadDecision)
    {
        Approved = downloadDecision.Approved;
        TemporarilyRejected = downloadDecision.TemporarilyRejected;
        Rejected = downloadDecision.Rejected;
        Rejections = downloadDecision.Rejections.Select(r => new DownloadRejectionResource(r)).ToList();
    }
}

public class DownloadRejectionResource
{
    public string Message { get; set; }
    public DownloadRejectionReason Reason { get; set; }
    public RejectionType Type { get; set; }

    public DownloadRejectionResource(DownloadRejection rejection)
    {
        Message = rejection.Message;
        Reason = rejection.Reason;
        Type = rejection.Type;
    }
}
