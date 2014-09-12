using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Jobs;

namespace NzbDrone.Api.System.Tasks
{
    public class TaskModule : NzbDroneRestModule<TaskResource>
    {
        private readonly ITaskManager _taskManager;

        public TaskModule(ITaskManager taskManager)
            : base("system/task")
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
            return new TaskResource
                   {
                       Id = scheduledTask.Id,
                       Name = scheduledTask.TypeName.Split('.').Last().Replace("Command", ""),
                       CommandName = scheduledTask.TypeName.Split('.').Last(),
                       Interval = scheduledTask.Interval,
                       LastExecution = scheduledTask.LastExecution,
                       NextExecution = scheduledTask.LastExecution.AddMinutes(scheduledTask.Interval)
                   };
        }
    }
}
