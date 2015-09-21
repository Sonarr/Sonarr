using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecision
    {
        public RemoteItem RemoteItem { get; private set; }
        public IEnumerable<Rejection> Rejections { get; private set; }

        public bool Approved
        {
            get
            {
                return !Rejections.Any();
            }
        }

        public bool TemporarilyRejected
        {
            get
            {
                return Rejections.Any() && Rejections.All(r => r.Type == RejectionType.Temporary);
            }
        }

        public bool Rejected
        {
            get
            {
                return Rejections.Any() && Rejections.Any(r => r.Type == RejectionType.Permanent);
            }
        }

        public DownloadDecision(RemoteItem item, params Rejection[] rejections)
        {
            RemoteItem = item;
            Rejections = rejections.ToList();
        }


        public override string ToString()
        {
            if (Approved)
            {
                return "[OK] " + RemoteItem;
            }

            return "[Rejected " + Rejections.Count() + "]" + RemoteItem;
        }
    }
}