using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Organizer
{
    public interface INamingConfigService
    {
        NamingConfig GetConfig();
        void Save(NamingConfig namingConfig);
    }

    public class NamingConfigService : INamingConfigService
    {
        private readonly IBasicRepository<NamingConfig> _repository;

        public NamingConfigService(IBasicRepository<NamingConfig> repository)
        {
            _repository = repository;
        }

        public NamingConfig GetConfig()
        {
            var config = _repository.SingleOrDefault();

            if (config == null)
            {
                _repository.Insert(NamingConfig.Default);
                config = _repository.Single();
            }

            return config;
        }

        public void Save(NamingConfig namingConfig)
        {
            _repository.Upsert(namingConfig);
        }
    }
}