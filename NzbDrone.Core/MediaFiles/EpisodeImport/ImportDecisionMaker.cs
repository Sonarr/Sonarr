using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;


namespace NzbDrone.Core.MediaFiles.EpisodeImport
{
    public interface IMakeImportDecision
    {
        List<ImportDecision> GetImportDecisions(IEnumerable<String> videoFiles, Series series);
    }

    public class ImportDecisionMaker : IMakeImportDecision
    {
        private readonly IEnumerable<IRejectWithReason> _specifications;
        private readonly IParsingService _parsingService;
        private readonly Logger _logger;

        public ImportDecisionMaker(IEnumerable<IRejectWithReason> specifications, IParsingService parsingService, Logger logger)
        {
            _specifications = specifications;
            _parsingService = parsingService;
            _logger = logger;
        }

        public List<ImportDecision> GetImportDecisions(IEnumerable<String> videoFiles, Series series)
        {
            return GetDecisions(videoFiles, series).ToList();
        }

        private IEnumerable<ImportDecision> GetDecisions(IEnumerable<String> videoFiles, Series series)
        {
            foreach (var file in videoFiles)
            {
                ImportDecision decision = null;

                try
                {
                    var parsedEpisode = _parsingService.GetEpisodes(file, series);

                    if (parsedEpisode != null)
                    {
                        decision = GetDecision(parsedEpisode);
                    }

                    else
                    {
                        parsedEpisode = new LocalEpisode();
                        parsedEpisode.Path = file;

                        decision = new ImportDecision(parsedEpisode, "Unable to parse file");
                    }
                }
                catch (Exception e)
                {
                    _logger.ErrorException("Couldn't process report.", e);
                }

                if (decision != null)
                {
                    yield return decision;
                }
            }
        }

        private ImportDecision GetDecision(LocalEpisode localEpisode)
        {
            var reasons = _specifications.Select(c => EvaluateSpec(c, localEpisode))
                .Where(c => !string.IsNullOrWhiteSpace(c));

            return new ImportDecision(localEpisode, reasons.ToArray());
        }

        private string EvaluateSpec(IRejectWithReason spec, LocalEpisode localEpisode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(spec.RejectionReason))
                {
                    throw new InvalidOperationException("[Need Rejection Text]");
                }

                var generalSpecification = spec as IImportDecisionEngineSpecification;
                if (generalSpecification != null && !generalSpecification.IsSatisfiedBy(localEpisode))
                {
                    return spec.RejectionReason;
                }
            }
            catch (Exception e)
            {
                //e.Data.Add("report", remoteEpisode.Report.ToJson());
                //e.Data.Add("parsed", remoteEpisode.ParsedEpisodeInfo.ToJson());
                _logger.ErrorException("Couldn't evaluate decision on " + localEpisode.Path, e);
                return string.Format("{0}: {1}", spec.GetType().Name, e.Message);
            }

            return null;
        }
    }
}
