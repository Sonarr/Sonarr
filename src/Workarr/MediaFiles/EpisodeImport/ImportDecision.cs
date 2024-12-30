using Workarr.Extensions;
using Workarr.Parser.Model;

namespace Workarr.MediaFiles.EpisodeImport
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
