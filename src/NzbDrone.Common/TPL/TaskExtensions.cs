using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Instrumentation;

namespace NzbDrone.Common.TPL
{
    public static class TaskExtensions
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(TaskExtensions));

        public static Task LogExceptions(this Task task)
        {
            task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        var aggregateException = t.Exception.Flatten();
                        foreach (var exception in aggregateException.InnerExceptions)
                        {
                            Logger.Error(exception, "Task Error");
                        }
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }
    }
}
