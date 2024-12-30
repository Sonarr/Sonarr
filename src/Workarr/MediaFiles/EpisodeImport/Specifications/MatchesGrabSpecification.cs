using NLog;
using Workarr.Download;
using Workarr.Extensions;
using Workarr.Parser.Model;
using Workarr.Tv;

namespace Workarr.MediaFiles.EpisodeImport.Specifications
{
    public class MatchesGrabSpecification : IImportDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MatchesGrabSpecification(Logger logger)
        {
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                return ImportSpecDecision.Accept();
            }

            var releaseInfo = localEpisode.Release;

            if (releaseInfo == null || EnumerableExtensions.Empty<int>(releaseInfo.EpisodeIds))
            {
                return ImportSpecDecision.Accept();
            }

            var unexpected = localEpisode.Episodes.Where(e => Enumerable.All<int>(releaseInfo.EpisodeIds, o => o != e.Id)).ToList();

            if (unexpected.Any())
            {
                _logger.Debug("Unexpected episode(s) in file: {0}", FormatEpisode(unexpected));

                if (unexpected.Count == 1)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeNotFoundInRelease, "Episode {0} was not found in the grabbed release: {1}", FormatEpisode(unexpected), releaseInfo.Title);
                }

                return ImportSpecDecision.Reject(ImportRejectionReason.EpisodeNotFoundInRelease, "Episodes {0} were not found in the grabbed release: {1}", FormatEpisode(unexpected), releaseInfo.Title);
            }

            return ImportSpecDecision.Accept();
        }

        private string FormatEpisode(List<Episode> episodes)
        {
            return string.Join(", ", episodes.Select(e => $"{e.SeasonNumber}x{e.EpisodeNumber:00}"));
        }
    }
}
