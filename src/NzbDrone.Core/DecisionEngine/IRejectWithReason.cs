namespace NzbDrone.Core.DecisionEngine
{
    public interface IRejectWithReason
    {
        string RejectionReason { get; }
    }
}
