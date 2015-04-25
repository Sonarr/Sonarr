using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProvider
    {
        string Name { get; }
        Type ConfigContract { get; }

        IEnumerable<ProviderDefinition> DefaultDefinitions { get; }
        ProviderDefinition Definition { get; set; }
        ValidationResult Test();
    }
}