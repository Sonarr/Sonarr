using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecision
    {
        public RemoteEpisode Episode { get; private set; }
        public IEnumerable<string> Rejections { get; private set; }

        public bool Approved
        {
            get
            {
                return !Rejections.Any();
            }
        }

        public DownloadDecision(RemoteEpisode episode, params string[] rejections)
        {
            Episode = episode;
            Rejections = rejections.ToList();
        }
    }
}