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
        object ConnectData(string stage, IDictionary<string, object> query);
    }
}