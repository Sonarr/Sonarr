using System;
using System.Linq;
using DeskMetrics;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class AnalyticsProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IDeskMetricsClient _deskMetricsClient;
        private readonly ConfigProvider _configProvider;
        public const string DESKMETRICS_TEST_ID = "4ea8d347a14ad71442000002";
        public const string DESKMETRICS_PRODUCTION_ID = "4f20b01ea14ad729b2000000";

        [Inject]
        public AnalyticsProvider(IDeskMetricsClient deskMetricsClient, ConfigProvider configProvider)
        {
            _deskMetricsClient = deskMetricsClient;
            _configProvider = configProvider;
        }

        public AnalyticsProvider()
        {

        }

        public virtual void Checkpoint()
        {
            try
            {
                //Don't report anything unless working from master branch.
                if (!IsOnMasterBranch())
                    return;

                if (EnviromentProvider.IsNewInstall)
                {
                    _deskMetricsClient.RegisterInstall();
                }

                if (_deskMetricsClient.Started)
                {
                    _deskMetricsClient.Stop();
                }

                _deskMetricsClient.Start();
            }
            catch (Exception e)
            {
                if (!EnviromentProvider.IsProduction)
                    throw;

                logger.WarnException("Error while sending analytics data.", e);
            }
        }


        private bool IsOnMasterBranch()
        {
            var defaultUpdateUrl = UpdateProvider.DEFAULT_UPDATE_URL.Trim().Trim('/');
            var currentUpdateUrl = _configProvider.UpdateUrl.Trim().Trim('/');

            return String.Equals(defaultUpdateUrl, currentUpdateUrl, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
