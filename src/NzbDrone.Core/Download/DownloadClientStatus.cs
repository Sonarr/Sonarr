using System;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.Download
{
    public class DownloadClientStatus : ProviderStatusBase
    {
        public int SubmissionFailureCount { get; set; }
        public DateTime? MostRecentSubmissionFailure { get; set; }

        public bool IsSubmissionPaused()
        {
            return SubmissionFailureCount >= 3;
        }

        public override bool IsDisabled()
        {
            return IsSubmissionPaused() ||
                   (MostRecentSubmissionFailure.HasValue && MostRecentSubmissionFailure.Value.AddMinutes(5) > DateTime.UtcNow) ||
                   base.IsDisabled();
        }
    }
}
