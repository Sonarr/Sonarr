using NLog;
using NzbDrone.Common.Composition;

namespace NzbDrone.Core.Diagnostics
{
    public class ScriptContext
    {
        private readonly IContainer _container;
        private readonly Logger _logger;

        public ScriptContext(IContainer container, Logger logger)
        {
            _container = container;
            _logger = logger;
        }

        public Logger Logger => _logger;

        public T Resolve<T>()
             where T : class
        {
            return _container.Resolve<T>();
        }
    }
}
