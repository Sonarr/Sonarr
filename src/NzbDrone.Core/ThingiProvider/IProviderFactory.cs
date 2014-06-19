using System;
using System.Collections.Generic;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderFactory<TProvider, TProviderDefinition>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
    {
        List<TProviderDefinition> All();
        List<TProvider> GetAvailableProviders();
        TProviderDefinition Get(int id);
        TProviderDefinition Create(TProviderDefinition indexer);
        void Update(TProviderDefinition indexer);
        void Delete(int id);
        IEnumerable<TProviderDefinition> GetDefaultDefinitions();
        IEnumerable<TProviderDefinition> GetPresetDefinitions(TProviderDefinition providerDefinition);
    }
}