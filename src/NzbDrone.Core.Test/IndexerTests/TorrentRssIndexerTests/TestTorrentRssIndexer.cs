using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Indexers.TorrentRssIndexer;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.IndexerTests.TorrentRssIndexerTests
{
    using NLog.Config;
    using NLog.Targets;

    public class TestTorrentRssIndexer : TorrentRssIndexer
    {
        public TestTorrentRssIndexer(IHttpClient httpClient, IConfigService configService, IParsingService parsingService, Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
        }

        public List<ValidationFailure> TestPublic()
        {
            var result = new List<ValidationFailure>();
            // this.SetupNLog(); // Enable this to enable trace logging with nlog for debugging purposes
            Test(result);
            return result;
        }

        /// <summary>
        /// Code to quickly debug unit tests
        /// </summary>
        private void SetupNLog()
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            fileTarget.FileName = "${basedir}/log.txt";
            fileTarget.Layout = GetStandardLayout();

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(rule1);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }

        private static string GetStandardLayout()
        {
            return @"${date:universalTime=true:format=u}|" + "${processid:fixedLength=True:padding=4:padCharacter= }|"
                   + "${threadid:fixedLength=True:padding=3:padCharacter= }|" + "${level:fixedLength=True:padding=5:padCharacter= :upperCase=True}|"
                // + "${when:when=length('${mdc:Module}') == 0:inner=UNSPECIFIED}${when:when=length('${mdc:Module}') > 0:inner=${mdc:MachineName:lowerCase=True}.${mdc:Module}}|"
                // + "${when:when=length('${mdc:Module}') == 0:inner=UNSPECIFIED}${when:when=length('${mdc:Module}') > 0 and not contains('${mdc:Module}', 'core'):inner=${mdc:MachineName:lowerCase=True}.${mdc:Module}}|"
                // + "${when:when=length('${mdc:Module}') == 0:inner=UNSPECIFIED|}"
                   + "${callsite:fileName=True:className=False:methodName=True:includeSourcePath=False:padding=50:padCharacter= }|" + "${message}"
                   + "  ${exception:maxInnerExceptionLevel=3:format=Method,Message,StackTrace:innerFormat=Method,Message,StackTrace:separator=\r\n:innerExceptionSeparator=\r\n}";
        }
    }
}
