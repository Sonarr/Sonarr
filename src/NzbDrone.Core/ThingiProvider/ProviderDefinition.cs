using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ThingiProvider
{
    public abstract class ProviderDefinition : ModelBase
    {
        protected ProviderDefinition()
        {
            Tags = new HashSet<int>();
        }

        private IProviderConfig _settings;

        public string Name { get; set; }
        public string ImplementationName { get; set; }
        public string Implementation { get; set; }
        public string ConfigContract { get; set; }
        public virtual bool Enable { get; set; }
        public ProviderMessage Message { get; set; }
        public HashSet<int> Tags { get; set; }

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
