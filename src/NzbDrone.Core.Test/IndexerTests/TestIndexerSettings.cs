using System;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class TestIndexerSettings : IIndexerSettings
    {
        public NzbDroneValidationResult Validate()
        {
            throw new NotImplementedException();
        }

        public string BaseUrl { get; set; }
    }
}
