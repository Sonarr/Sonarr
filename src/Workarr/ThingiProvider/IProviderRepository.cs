using Workarr.Datastore;

namespace Workarr.ThingiProvider
{
    public interface IProviderRepository<TProvider> : IBasicRepository<TProvider>
        where TProvider : ModelBase, new()
    {
// void DeleteImplementations(string implementation);
    }
}
