using System;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class TestIndexerSettings : IProviderConfig
    {
        public NzbDroneValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
