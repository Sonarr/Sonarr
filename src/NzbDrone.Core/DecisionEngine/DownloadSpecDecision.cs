namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadSpecDecision
    {
        public bool Accepted { get; private set; }
        public DownloadRejectionReason Reason { get; set; }
        public string Message { get; private set; }

        private static readonly DownloadSpecDecision AcceptDownloadSpecDecision = new () { Accepted = true };
        private DownloadSpecDecision()
        {
        }

        public static DownloadSpecDecision Accept()
        {
            return AcceptDownloadSpecDecision;
        }

        public static DownloadSpecDecision Reject(DownloadRejectionReason reason, string message, params object[] args)
        {
            return Reject(reason, string.Format(message, args));
        }

        public static DownloadSpecDecision Reject(DownloadRejectionReason reason, string message)
        {
            return new DownloadSpecDecision
            {
                Accepted = false,
                Reason = reason,
                Message = message
            };
        }
    }
}
