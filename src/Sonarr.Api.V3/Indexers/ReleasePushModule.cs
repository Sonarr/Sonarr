using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Nancy;
using Nancy.ModelBinding;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using Sonarr.Http.Extensions;

namespace Sonarr.Api.V3.Indexers
{
    class ReleasePushModule : ReleaseModuleBase
    {
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IProcessDownloadDecisions _downloadDecisionProcessor;
        private readonly Logger _logger;

        public ReleasePushModule(IMakeDownloadDecision downloadDecisionMaker,
                                 IProcessDownloadDecisions downloadDecisionProcessor,
                                 Logger logger)
        {
            _downloadDecisionMaker = downloadDecisionMaker;
            _downloadDecisionProcessor = downloadDecisionProcessor;
            _logger = logger;

            Post["/push"] = x => ProcessRelease(this.Bind<ReleaseResource>());

            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.DownloadUrl).NotEmpty();
            PostValidator.RuleFor(s => s.DownloadProtocol).NotEmpty();
            PostValidator.RuleFor(s => s.PublishDate).NotEmpty();
        }

        private Response ProcessRelease(ReleaseResource release)
        {
            _logger.Info("Release pushed: {0} - {1}", release.Title, release.DownloadUrl);

            var info = release.ToModel();

            info.Guid = "PUSH-" + info.DownloadUrl;

            var decisions = _downloadDecisionMaker.GetRssDecision(new List<ReleaseInfo> { info });
            _downloadDecisionProcessor.ProcessDecisions(decisions);

            return MapDecisions(decisions).First().AsResponse();
        }
    }
}
