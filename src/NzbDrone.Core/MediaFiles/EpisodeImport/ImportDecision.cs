using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public class ImportDecision
    {
        public LocalEpisode LocalEpisode { get; private set; }
        public IEnumerable<ImportRejection> Rejections { get; private set; }

        public bool Approved => Rejections.Empty();

        public ImportDecision(LocalEpisode localEpisode, params ImportRejection[] rejections)
        {
            LocalEpisode = localEpisode;
            Rejections = rejections.ToList();
        }
    }
}
