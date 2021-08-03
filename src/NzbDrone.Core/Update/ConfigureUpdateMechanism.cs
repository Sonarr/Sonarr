using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Update
{
    public interface IUpdaterConfigProvider
    {
    }

    public class UpdaterConfigProvider : IUpdaterConfigProvider, IHandle<ApplicationStartedEvent>
    {
        private readonly Logger _logger;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IDeploymentInfoProvider _deploymentInfoProvider;

        public UpdaterConfigProvider(IDeploymentInfoProvider deploymentInfoProvider, IConfigFileProvider configFileProvider, Logger logger)
        {
            _deploymentInfoProvider = deploymentInfoProvider;
            _configFileProvider = configFileProvider;
            _logger = logger;
        }

        public void Handle(ApplicationStartedEvent message)
        {
            var updateMechanism = _configFileProvider.UpdateMechanism;
            var packageUpdateMechanism = _deploymentInfoProvider.PackageUpdateMechanism;

            var externalMechanisms = Enum.GetValues(typeof(UpdateMechanism))
                                         .Cast<UpdateMechanism>()
                                         .Where(v => v >= UpdateMechanism.External)
                                         .ToArray();

            foreach (var externalMechanism in externalMechanisms)
            {
                if ((packageUpdateMechanism != externalMechanism && updateMechanism == externalMechanism) ||
                    (packageUpdateMechanism == externalMechanism && updateMechanism == UpdateMechanism.BuiltIn))
                {
                    _logger.Info("Update mechanism {0} not supported in the current configuration, changing to {1}.", updateMechanism, packageUpdateMechanism);
                    ChangeUpdateMechanism(packageUpdateMechanism);
                    break;
                }
            }

            if (_deploymentInfoProvider.IsExternalUpdateMechanism)
            {
                var currentBranch = _configFileProvider.Branch;
                var packageBranch = _deploymentInfoProvider.PackageBranch;
                if (packageBranch.IsNotNullOrWhiteSpace() && packageBranch != currentBranch)
                {
                    _logger.Info("External updater uses branch {0} instead of the currently selected {1}, changing to {0}.", packageBranch, currentBranch);
                    ChangeBranch(packageBranch);
                }
            }
        }

        private void ChangeUpdateMechanism(UpdateMechanism updateMechanism)
        {
            var config = new Dictionary<string, object>
            {
                [nameof(_configFileProvider.UpdateMechanism)] = updateMechanism
            };
            _configFileProvider.SaveConfigDictionary(config);
        }

        private void ChangeBranch(string branch)
        {
            var config = new Dictionary<string, object>
            {
                [nameof(_configFileProvider.Branch)] = branch
            };
            _configFileProvider.SaveConfigDictionary(config);
        }
    }
}
