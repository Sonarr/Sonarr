using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class DownloadDecision
    {
        public RemoteEpisode RemoteEpisode { get; private set; }
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
            RemoteEpisode = episode;
            Rejections = rejections.ToList();
        }


        public override string ToString()
        {
            if (Approved)
            {
                return "[OK] " + RemoteEpisode;
            }

            return "[Rejected " + Rejections.Count() + "]" + RemoteEpisode;
        }
    }
}