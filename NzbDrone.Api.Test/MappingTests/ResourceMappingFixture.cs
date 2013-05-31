using System;
using NUnit.Framework;
using NzbDrone.Api.Config;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.History;
using NzbDrone.Api.Indexers;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Qualities;
using NzbDrone.Api.RootFolders;
using NzbDrone.Api.Series;
using NzbDrone.Api.Update;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Update;
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
        [TestCase(typeof(Indexer), typeof(IndexerResource))]
        [TestCase(typeof(ReportInfo), typeof(ReleaseResource))]
        [TestCase(typeof(ParsedEpisodeInfo), typeof(ReleaseResource))]
        [TestCase(typeof(DownloadDecision), typeof(ReleaseResource))]
        [TestCase(typeof(Core.History.History), typeof(HistoryResource))]
        [TestCase(typeof(UpdatePackage), typeof(UpdateResource))]
        [TestCase(typeof(QualityProfile), typeof(QualityProfileResource))]
        [TestCase(typeof(Quality), typeof(QualityResource))]
        public void matching_fields(Type modelType, Type resourceType)
        {
            MappingValidation.ValidateMapping(modelType, resourceType);
        }

    }
}