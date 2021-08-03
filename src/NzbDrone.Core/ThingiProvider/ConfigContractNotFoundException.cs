using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.ThingiProvider
{
    public class ConfigContractNotFoundException : NzbDroneException
    {
        public ConfigContractNotFoundException(string contract)
            : base("Couldn't find config contract " + contract)
        {
        }
    }
}
