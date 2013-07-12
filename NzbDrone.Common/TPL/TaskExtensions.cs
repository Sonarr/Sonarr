using System.Threading.Tasks;
using NLog;

namespace NzbDrone.Common.TPL
{
    public static class TaskExtensions
    {
        private static readonly Logger Logger = LogManager.GetLogger("TaskExtensions");

        public static Task LogExceptions(this Task task)
        {
            task.ContinueWith(t =>
                {
                    var aggregateException = t.Exception.Flatten();
                    foreach (var exception in aggregateException.InnerExceptions)
                    {
                        Logger.ErrorException("Task Error", exception);
                    }

                }, TaskContinuationOptions.OnlyOnFaulted);

            return task;
        }
    }
}