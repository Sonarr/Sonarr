using System;
using NUnit.Framework;
using NzbDrone.Api.Config;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Indexers;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.RootFolders;
using NzbDrone.Api.Series;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RootFolders;
using NzbDrone.Test.Common;

namespace NzbDrone.Api.Test.MappingTests
{
    [TestFixture]
    public class ResourceMappingFixture : TestBase
    {
        [TestCase(typeof(Core.Tv.Series), typeof(SeriesResource))]
        [TestCase(typeof(Core.Tv.Episode), typeof(EpisodeResource))]
        [TestCase(typeof(RootFolder), typeof(RootFolderResource))]
        [TestCase(typeof(NamingConfig), typeof(NamingConfigResource))]
        [TestCase(typeof(IndexerDefinition), typeof(IndexerResource))]
        [TestCase(typeof(ReportInfo), typeof(ReleaseResource))]
        [TestCase(typeof(ParsedEpisodeInfo), typeof(ReleaseResource))]
        [TestCase(typeof(DownloadDecision), typeof(ReleaseResource))]
        public void matching_fields(Type modelType, Type resourceType)
        {
            MappingValidation.ValidateMapping(modelType, resourceType);
        }

    }
}