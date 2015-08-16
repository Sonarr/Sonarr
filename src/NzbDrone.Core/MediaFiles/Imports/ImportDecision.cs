using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Imports
{
    public class ImportDecision
    {
        public LocalItem LocalItem { get; private set; }
        public IEnumerable<Rejection> Rejections { get; private set; }

        public bool Approved
        {
            get
            {
                return Rejections.Empty();
            }
        }

        public ImportDecision(LocalItem localItem, params Rejection[] rejections)
        {
            LocalItem = localItem;
            Rejections = rejections.ToList();
        }
    }
}
