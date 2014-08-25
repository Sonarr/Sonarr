using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;

namespace NzbDrone.Core.Download
{
    public class ProcessedDecisions
    {
        public List<DownloadDecision> Grabbed { get; set; }
        public List<DownloadDecision> Pending { get; set; }

        public ProcessedDecisions(List<DownloadDecision> grabbed, List<DownloadDecision> pending)
        {
            Grabbed = grabbed;
            Pending = pending;
        }
    }
}
