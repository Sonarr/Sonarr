using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Sonarr.Http;

namespace NzbDrone.Api.System.Tasks
{
    public class TaskModule : SonarrRestModuleWithSignalR<TaskResource, ScheduledTask>, IHandle<CommandExecutedEvent>
    {
        private readonly ITaskManager _taskManager;

        private static readonly Regex NameRegex = new Regex("(?<!^)[A-Z]", RegexOptions.Compiled);

        public TaskModule(ITaskManager taskManager, IBroadcastSignalRMessage broadcastSignalRMessage)
            : base(broadcastSignalRMessage, "system/task")
        {
            _taskManager = taskManager;
            GetResourceAll = GetAll;
        }

        private List<TaskResource> GetAll()
        {
            return _taskManager.GetAll().Select(ConvertToResource).ToList();
        }

        private static TaskResource ConvertToResource(ScheduledTask scheduledTask)
        {
            var taskName = scheduledTask.TypeName.Split('.').Last().Replace("Command", "");

            return new TaskResource
                   {
                       Id = scheduledTask.Id,
                       Name = NameRegex.Replace(taskName, match => " " + match.Value),
                       TaskName = taskName,
                       Interval = scheduledTask.Interval,
                       LastExecution = scheduledTask.LastExecution,
                       NextExecution = scheduledTask.LastExecution.AddMinutes(scheduledTask.Interval)
                   };
        }

        public void Handle(CommandExecutedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
