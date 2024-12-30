using DryIoc;
using NLog;

namespace Workarr.Instrumentation.Instrumentation.Extensions
{
    public static class CompositionExtensions
    {
        public static IContainer AddWorkarrLogger(this IContainer container)
        {
            container.Register(Made.Of<Logger>(() => LogManager.GetLogger(Arg.Index<string>(0)), r => r.Parent.ImplementationType.Name.ToString()), reuse: Reuse.Transient);
            return container;
        }
    }
}
