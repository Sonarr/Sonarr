using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProvider
    {
        Type ConfigContract { get; }

        IEnumerable<ProviderDefinition> DefaultDefinitions { get; }
        ProviderDefinition Definition { get; set; }
        IEnumerable<ValidationFailure> Test();
    }
}