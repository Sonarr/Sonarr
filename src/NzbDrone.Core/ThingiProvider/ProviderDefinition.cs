using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ThingiProvider
{
    public abstract class ProviderDefinition : ModelBase
    {
        private IProviderConfig _settings;
        public string Name { get; set; }
        public string Implementation { get; set; }

        public string ConfigContract { get; set; }

        public IProviderConfig Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
                if (value != null)
                {
                    ConfigContract = value.GetType().Name;
                }
            }
        }
    }
}