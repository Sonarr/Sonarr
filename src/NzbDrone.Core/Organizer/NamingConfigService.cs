namespace NzbDrone.Core.Organizer
{
    public interface INamingConfigService
    {
        NamingConfig GetConfig();
        void Save(NamingConfig namingConfig);
    }

    public class NamingConfigService : INamingConfigService
    {
        private readonly INamingConfigRepository _repository;

        public NamingConfigService(INamingConfigRepository repository)
        {
            _repository = repository;
        }

        public NamingConfig GetConfig()
        {
            var config = _repository.SingleOrDefaultAsync().GetAwaiter().GetResult();

            if (config == null)
            {
                lock (_repository)
                {
                    config = _repository.SingleOrDefaultAsync().GetAwaiter().GetResult();

                    if (config == null)
                    {
                        _repository.InsertAsync(NamingConfig.Default).GetAwaiter().GetResult();
                        config = _repository.SingleAsync().GetAwaiter().GetResult();
                    }
                }
            }

            return config;
        }

        public void Save(NamingConfig namingConfig)
        {
            _repository.UpsertAsync(namingConfig).GetAwaiter().GetResult();
        }
    }
}
