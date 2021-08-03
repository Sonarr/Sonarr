using System.Collections.Generic;
using FluentValidation.Results;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderFactory<TProvider, TProviderDefinition>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
    {
        List<TProviderDefinition> All();
        List<TProvider> GetAvailableProviders();
        bool Exists(int id);
        TProviderDefinition Find(int id);
        TProviderDefinition Get(int id);
        TProviderDefinition Create(TProviderDefinition definition);
        void Update(TProviderDefinition definition);
        void Delete(int id);
        IEnumerable<TProviderDefinition> GetDefaultDefinitions();
        IEnumerable<TProviderDefinition> GetPresetDefinitions(TProviderDefinition providerDefinition);
        void SetProviderCharacteristics(TProviderDefinition definition);
        void SetProviderCharacteristics(TProvider provider, TProviderDefinition definition);
        TProvider GetInstance(TProviderDefinition definition);
        ValidationResult Test(TProviderDefinition definition);
        object RequestAction(TProviderDefinition definition, string action, IDictionary<string, string> query);
        List<TProviderDefinition> AllForTag(int tagId);
    }
}
