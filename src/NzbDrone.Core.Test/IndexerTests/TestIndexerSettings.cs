using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentValidation.Results;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Test.IndexerTests
{
    public class TestIndexerSettings : IProviderConfig
    {
        public ValidationResult Validate()
        {
            throw new NotImplementedException();
        }
    }
}
