using Nancy;
using Nancy.ModelBinding;
using FluentValidation;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Extensions;
using NLog;

namespace NzbDrone.Api.Indexers
{
    class ReleasePushModule : NzbDroneRestModule<ReleaseResource>
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
            _logger.Info("Release pushed: {0}", release.Title);

            ReleaseInfo info = release.InjectTo<ReleaseInfo>();
            info.Guid = "PUSH-" + info.DownloadUrl;

            var decisions = _downloadDecisionMaker.GetRssDecision(new List<ReleaseInfo> { info });
            var processed = _downloadDecisionProcessor.ProcessDecisions(decisions);

            string status = processed.Grabbed.Any()  ? "grabbed"  :
                            processed.Rejected.Any() ? "rejected" :
                            processed.Pending.Any()  ? "pending"  :
                                                       "error"    ;

            _logger.Info("Release {0}: {1}", status, info.Title);

            return processed.AsResponse();
        }
    }
}
