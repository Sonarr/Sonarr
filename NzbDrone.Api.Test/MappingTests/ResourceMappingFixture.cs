using System;
using NUnit.Framework;
using NzbDrone.Api.Episodes;
using NzbDrone.Api.Mapping;
using NzbDrone.Api.Series;
using NzbDrone.Test.Common;

namespace NzbDrone.Api.Test.MappingTests
{
    [TestFixture]
    public class ResourceMappingFixture : TestBase
    {
        [TestCase(typeof(Core.Tv.Series), typeof(SeriesResource))]
        [TestCase(typeof(Core.Tv.Episode), typeof(EpisodeResource))]
        public void matching_fields(Type modelType, Type resourceType)
        {
            MappingValidation.ValidateMapping(modelType, resourceType);
        }

    }
}