using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Model;

namespace NzbDrone.Core.DecisionEngine
{
    public class DownloadDecision
    {
        public IndexerParseResult ParseResult { get; private set; }
        public IEnumerable<string> Rejections { get; private set; }

        public bool Approved
        {
            get
            {
                return !Rejections.Any();
            }
        }

        public DownloadDecision(IndexerParseResult parseResult, params string[] rejections)
        {
            ParseResult = parseResult;
            Rejections = rejections.ToList();
        }


        public static IndexerParseResult PickBestReport(IEnumerable<DownloadDecision> downloadDecisions)
        {
            var reports = downloadDecisions
                  .Where(c => c.Approved)
                  .Select(c => c.ParseResult)
                  .OrderByDescending(c => c.Quality)
                  .ThenBy(c => c.EpisodeNumbers.MinOrDefault())
                  .ThenBy(c => c.Age);

            return reports.SingleOrDefault();
        }
    }
}