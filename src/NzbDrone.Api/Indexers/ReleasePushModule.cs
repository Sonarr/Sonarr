using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using Nancy.ModelBinding;
using NLog;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using Sonarr.Http.Extensions;
using Sonarr.Http.Mapping;

namespace NzbDrone.Api.Indexers
{
    class ReleasePushModule : ReleaseModuleBase
    {
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IProcessDownloadDecisions _downloadDecisionProcessor;
        private readonly IIndexerFactory _indexerFactory;
        private readonly Logger _logger;

        public ReleasePushModule(IMakeDownloadDecision downloadDecisionMaker,
                                 IProcessDownloadDecisions downloadDecisionProcessor,
                                 IIndexerFactory indexerFactory,
                                 Logger logger)
        {
            _downloadDecisionMaker = downloadDecisionMaker;
            _downloadDecisionProcessor = downloadDecisionProcessor;
            _indexerFactory = indexerFactory;
            _logger = logger;

            Post["/push"] = x => ProcessRelease(this.Bind<ReleaseResource>());

            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.DownloadUrl).NotEmpty();
            PostValidator.RuleFor(s => s.Protocol).NotEmpty();
            PostValidator.RuleFor(s => s.PublishDate).NotEmpty();
        }

        private Response ProcessRelease(ReleaseResource release)
        {
            _logger.Info("Release pushed: {0} - {1}", release.Title, release.DownloadUrl);

            var info = release.ToModel();

            info.Guid = "PUSH-" + info.DownloadUrl;

            ResolveIndexer(info);

            var decisions = _downloadDecisionMaker.GetRssDecision(new List<ReleaseInfo> { info });
            _downloadDecisionProcessor.ProcessDecisions(decisions);

            return MapDecisions(decisions).First().AsResponse();
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
                    _logger.Debug("Push Release {0} not associated with unknown indexer {1}.", release.Title, release.Indexer);
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
                    _logger.Debug("Push Release {0} not associated with unknown indexer {0}.", release.Title, release.IndexerId);
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
