using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Sonarr.Http;

namespace Sonarr.Api.V3.System.Tasks
{
    public class TaskModule : SonarrRestModuleWithSignalR<TaskResource, ScheduledTask>, IHandle<CommandExecutedEvent>
    {
        private readonly ITaskManager _taskManager;

        public TaskModule(ITaskManager taskManager, IBroadcastSignalRMessage broadcastSignalRMessage)
            : base(broadcastSignalRMessage, "system/task")
        {
            _taskManager = taskManager;
            GetResourceAll = GetAll;
            GetResourceById = GetTask;
        }

        private List<TaskResource> GetAll()
        {
            return _taskManager.GetAll()
                               .Select(ConvertToResource)
                               .OrderBy(t => t.Name)
                               .ToList();
        }

        private TaskResource GetTask(int id)
        {
            var task = _taskManager.GetAll()
                               .SingleOrDefault(t => t.Id == id);

            if (task == null)
            {
                return null;
            }

            return ConvertToResource(task);
        }

        private static TaskResource ConvertToResource(ScheduledTask scheduledTask)
        {
            var taskName = scheduledTask.TypeName.Split('.').Last().Replace("Command", "");

            return new TaskResource
                   {
                       Id = scheduledTask.Id,
                       Name = taskName.SplitCamelCase(),
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
