using Workarr.Exceptions;

namespace Workarr.ThingiProvider
{
    public class ConfigContractNotFoundException : WorkarrException
    {
        public ConfigContractNotFoundException(string contract)
            : base("Couldn't find config contract " + contract)
        {
        }
    }
}
