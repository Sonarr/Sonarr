using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NzbDrone.Core.Update
{
    public interface IUpdaterConfigProvider
    {

    }

    public class UpdaterConfigProvider : IUpdaterConfigProvider, IHandle<ApplicationStartedEvent>
    {
        private Logger _logger;
        private IConfigFileProvider _configFileProvider;
        private IDeploymentInfoProvider _deploymentInfoProvider;

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
                if (packageUpdateMechanism != externalMechanism && updateMechanism == externalMechanism ||
                    packageUpdateMechanism == externalMechanism && updateMechanism == UpdateMechanism.BuiltIn)
                {
                    _logger.Info("Update mechanism {0} not supported in the current configuration, changing to {1}.", updateMechanism, packageUpdateMechanism);
                    ChangeUpdateMechanism(packageUpdateMechanism);
                    break;
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
    }
}
