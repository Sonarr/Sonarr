using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ThingiProvider
{
    public interface IProviderRepository<TProvider> : IBasicRepository<TProvider>
        where TProvider : ModelBase, new()
    {
//        void DeleteImplementations(string implementation);
    }
}
