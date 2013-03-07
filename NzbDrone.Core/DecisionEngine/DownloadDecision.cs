using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecision
    {
        public IEnumerable<string> Rejections { get; private set; }
        public bool Approved
        {
            get
            {
                return !Rejections.Any();
            }
        }

        public DownloadDecision(params string[] rejections)
        {
            Rejections = rejections.ToList();
        }
    }
}