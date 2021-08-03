using System;
using System.Collections.Generic;
using FluentValidation.Results;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProvider
    {
        string Name { get; }
        Type ConfigContract { get; }
        ProviderMessage Message { get; }
        IEnumerable<ProviderDefinition> DefaultDefinitions { get; }
        ProviderDefinition Definition { get; set; }
        ValidationResult Test();
        object RequestAction(string stage, IDictionary<string, string> query);
    }
}
