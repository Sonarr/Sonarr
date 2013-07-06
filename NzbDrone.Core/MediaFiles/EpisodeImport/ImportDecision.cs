using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public class ImportDecision
    {
        public LocalEpisode LocalEpisode { get; private set; }
        public IEnumerable<string> Rejections { get; private set; }

        public bool Approved
        {
            get
            {
                return !Rejections.Any();
            }
        }

        public ImportDecision(LocalEpisode localEpisode, params string[] rejections)
        {
            LocalEpisode = localEpisode;
            Rejections = rejections.ToList();
        }
    }
}
