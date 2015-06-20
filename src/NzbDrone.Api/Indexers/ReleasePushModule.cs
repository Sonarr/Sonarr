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
        }

        private Response ProcessRelease(ReleaseResource release)
        {
            _logger.Info("Release pushed: '" + release.Title.ToString() + "'");

            ReleaseInfo info = release.InjectTo<ReleaseInfo>();
            info.Guid = "PUSH-" + info.DownloadUrl;
            if (info.PublishDate == default(DateTime)) info.PublishDate = DateTime.Now;

            var decisions = _downloadDecisionMaker.GetRssDecision(new List<ReleaseInfo> { info });
            var processed = _downloadDecisionProcessor.ProcessDecisions(decisions);

            _logger.Info("Release " + (processed.Grabbed.Any() ? "grabbed" : 
                                       processed.Rejected.Any() ? "rejected" : 
                                       processed.Pending.Any() ? "pending" :
                                       "error") + ": '" + info.Title.ToString() + "'");

            return processed.AsResponse();
        }
    }
}
