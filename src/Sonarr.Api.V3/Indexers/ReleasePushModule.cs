using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Qualities;

namespace Sonarr.Api.V3.Indexers
{
    public class ReleasePushModule : ReleaseModuleBase
    {
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IProcessDownloadDecisions _downloadDecisionProcessor;
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public ReleasePushModule(IMakeDownloadDecision downloadDecisionMaker,
                                 IProcessDownloadDecisions downloadDecisionProcessor,
                                 IIndexerFactory indexerFactory,
                                 ILanguageProfileService languageProfileService,
                                 IQualityProfileService qualityProfileService,
                                 Logger logger)
            : base(languageProfileService, qualityProfileService)
        {
            _downloadDecisionMaker = downloadDecisionMaker;
            _downloadDecisionProcessor = downloadDecisionProcessor;
            _indexerFactory = indexerFactory;
            _logger = logger;

            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.DownloadUrl).NotEmpty();
            PostValidator.RuleFor(s => s.Protocol).NotEmpty();
            PostValidator.RuleFor(s => s.PublishDate).NotEmpty();

            Post("/push",  x => ProcessRelease(ReadResourceFromRequest()));
        }

        private object ProcessRelease(ReleaseResource release)
        {
            _logger.Info("Release pushed: {0} - {1}", release.Title, release.DownloadUrl);

            var info = release.ToModel();

            info.Guid = "PUSH-" + info.DownloadUrl;

            ResolveIndexer(info);

            var decisions = _downloadDecisionMaker.GetRssDecision(new List<ReleaseInfo> { info });
            _downloadDecisionProcessor.ProcessDecisions(decisions);

            var firstDecision = decisions.FirstOrDefault();

            if (firstDecision?.RemoteEpisode.ParsedEpisodeInfo == null)
            {
                throw new ValidationException(new List<ValidationFailure> { new ValidationFailure("Title", "Unable to parse", release.Title) });
            }

            return MapDecisions(new[] { firstDecision });
        }

        private void ResolveIndexer(ReleaseInfo release)
        {
            if (release.IndexerId == 0 && release.Indexer.IsNotNullOrWhiteSpace())
            {
                var indexer = _indexerFactory.All().FirstOrDefault(v => v.Name == release.Indexer);
                if (indexer != null)
                {
                    release.IndexerId = indexer.Id;
                    _logger.Debug("Push Release {0} associated with indexer {1} - {2}.", release.Title, release.IndexerId, release.Indexer);
                }
                else
                {
                    _logger.Debug("Push Release {0} not associated with known indexer {1}.", release.Title, release.Indexer);
                }
            }
            else if (release.IndexerId != 0 && release.Indexer.IsNullOrWhiteSpace())
            {
                try
                {
                    var indexer = _indexerFactory.Get(release.IndexerId);
                    release.Indexer = indexer.Name;
                    _logger.Debug("Push Release {0} associated with indexer {1} - {2}.", release.Title, release.IndexerId, release.Indexer);
                }
                catch (ModelNotFoundException)
                {
                    _logger.Debug("Push Release {0} not associated with known indexer {0}.", release.Title, release.IndexerId);
                    release.IndexerId = 0;
                }
            }
            else
            {
                _logger.Debug("Push Release {0} not associated with an indexer.", release.Title);
            }
        }
    }
}
