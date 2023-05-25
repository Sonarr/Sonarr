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
        IEnumerable<TProviderDefinition> Get(IEnumerable<int> ids);
        TProviderDefinition Create(TProviderDefinition definition);
        void Update(TProviderDefinition definition);
        IEnumerable<TProviderDefinition> Update(IEnumerable<TProviderDefinition> definitions);
        void Delete(int id);
        void Delete(IEnumerable<int> ids);
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
