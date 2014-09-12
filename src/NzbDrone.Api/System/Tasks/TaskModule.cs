using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Core.Jobs;

namespace NzbDrone.Api.System.Tasks
{
    public class TaskModule : NzbDroneRestModule<TaskResource>
    {
        private readonly ITaskManager _taskManager;

        private static readonly Regex NameRegex = new Regex("(?<!^)[A-Z]", RegexOptions.Compiled);

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
    }
}
