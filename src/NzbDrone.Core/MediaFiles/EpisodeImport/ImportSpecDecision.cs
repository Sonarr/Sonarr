namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public class ImportSpecDecision
    {
        public bool Accepted { get; private set; }
        public ImportRejectionReason Reason { get; set; }
        public string Message { get; private set; }

        private static readonly ImportSpecDecision AcceptDecision = new () { Accepted = true };
        private ImportSpecDecision()
        {
        }

        public static ImportSpecDecision Accept()
        {
            return AcceptDecision;
        }

        public static ImportSpecDecision Reject(ImportRejectionReason reason, string message, params object[] args)
        {
            return Reject(reason, string.Format(message, args));
        }

        public static ImportSpecDecision Reject(ImportRejectionReason reason, string message)
        {
            return new ImportSpecDecision
            {
                Accepted = false,
                Reason = reason,
                Message = message
            };
        }
    }
}
