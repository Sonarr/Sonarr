using NLog;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.EpisodeImport.Specifications
{
    public class NotSampleSpecification : IImportDecisionEngineSpecification
    {
        private readonly IDetectSample _detectSample;
        private readonly Logger _logger;

        public NotSampleSpecification(IDetectSample detectSample,
                                      Logger logger)
        {
            _detectSample = detectSample;
            _logger = logger;
        }

        public ImportSpecDecision IsSatisfiedBy(LocalEpisode localEpisode, DownloadClientItem downloadClientItem)
        {
            if (localEpisode.ExistingFile)
            {
                _logger.Debug("Existing file, skipping sample check");
                return ImportSpecDecision.Accept();
            }

            try
            {
                var sample = _detectSample.IsSample(localEpisode);

                if (sample == DetectSampleResult.Sample)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.Sample, "Sample");
                }
                else if (sample == DetectSampleResult.Indeterminate)
                {
                    return ImportSpecDecision.Reject(ImportRejectionReason.SampleIndeterminate, "Unable to determine if file is a sample");
                }
            }
            catch (InvalidSeasonException e)
            {
                _logger.Warn(e, "Invalid season detected during sample check");
            }

            return ImportSpecDecision.Accept();
        }
    }
}
