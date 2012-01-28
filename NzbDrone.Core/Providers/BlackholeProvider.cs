using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using Ninject;
using NzbDrone.Common;
using NzbDrone.Core.Model;
using NzbDrone.Core.Providers.Core;

namespace NzbDrone.Core.Providers
{
    public class BlackholeProvider
    {
        private readonly ConfigProvider _configProvider;
        private readonly HttpProvider _httpProvider;
        private readonly DiskProvider _diskProvider;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        [Inject]
        public BlackholeProvider(ConfigProvider configProvider, HttpProvider httpProvider,
                                    DiskProvider diskProvider)
        {
            _configProvider = configProvider;
            _httpProvider = httpProvider;
            _diskProvider = diskProvider;
        }

        public BlackholeProvider()
        {
        }

        public virtual bool DownloadNzb(EpisodeParseResult parseResult, string title)
        {
            try
            {
                var filename = Path.Combine(_configProvider.BlackholeDirectory, title, ".nzb");

                if(_diskProvider.FileExists(filename))
                {
                    //Return true so a lesser quality is not returned.
                    Logger.Info("NZB already exists on disk: {0)", filename);
                    return true;
                }

                Logger.Trace("Downloading NZB from: {0} to: {1}", parseResult.NzbUrl, filename);
                _httpProvider.DownloadFile(parseResult.NzbUrl, filename);

                Logger.Trace("NZB Download succeeded, saved to: {0}", filename);
                return true;
            }
            catch(Exception ex)
            {
                Logger.WarnException("Failed to download NZB: " + parseResult.NzbUrl, ex);
                return false;
            }
        }
    }
}
