using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderFactory<TProvider, TProviderDefinition>
        where TProviderDefinition : ProviderDefinition, new()
        where TProvider : IProvider
    {
        List<TProviderDefinition> All();
        IAsyncEnumerable<TProviderDefinition> AllAsync(CancellationToken cancellationToken = default);
        List<TProvider> GetAvailableProviders();
        IAsyncEnumerable<TProvider> GetAvailableProvidersAsync(CancellationToken cancellationToken = default);
        bool Exists(int id);
        Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
        TProviderDefinition Find(int id);
        Task<TProviderDefinition> FindAsync(int id, CancellationToken cancellationToken = default);
        TProviderDefinition Get(int id);
        Task<TProviderDefinition> GetAsync(int id, CancellationToken cancellationToken = default);
        IEnumerable<TProviderDefinition> Get(IEnumerable<int> ids);
        IAsyncEnumerable<TProviderDefinition> GetAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        TProviderDefinition Create(TProviderDefinition definition);
        Task<TProviderDefinition> CreateAsync(TProviderDefinition definition, CancellationToken cancellationToken = default);
        void Update(TProviderDefinition definition);
        Task UpdateAsync(TProviderDefinition definition, CancellationToken cancellationToken = default);
        IEnumerable<TProviderDefinition> Update(IEnumerable<TProviderDefinition> definitions);
        Task<IEnumerable<TProviderDefinition>> UpdateAsync(IEnumerable<TProviderDefinition> definitions, CancellationToken cancellationToken = default);
        void Delete(int id);
        Task DeleteAsync(int id, CancellationToken cancellationToken = default);
        void Delete(IEnumerable<int> ids);
        Task DeleteAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        IEnumerable<TProviderDefinition> GetDefaultDefinitions();
        IEnumerable<TProviderDefinition> GetPresetDefinitions(TProviderDefinition providerDefinition);
        void SetProviderCharacteristics(TProviderDefinition definition);
        void SetProviderCharacteristics(TProvider provider, TProviderDefinition definition);
        TProvider GetInstance(TProviderDefinition definition);
        ValidationResult Test(TProviderDefinition definition);
        object RequestAction(TProviderDefinition definition, string action, IDictionary<string, string> query);
        List<TProviderDefinition> AllForTag(int tagId);
        IAsyncEnumerable<TProviderDefinition> AllForTagAsync(int tagId, CancellationToken cancellationToken = default);
    }
}
