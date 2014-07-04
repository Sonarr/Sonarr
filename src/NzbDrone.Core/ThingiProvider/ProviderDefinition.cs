using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ThingiProvider
{
    public abstract class ProviderDefinition : ModelBase
    {
        private IProviderConfig _settings;

        public String Name { get; set; }
        public String Implementation { get; set; }
        public String ConfigContract { get; set; }
        public virtual Boolean Enable { get; set; }

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
