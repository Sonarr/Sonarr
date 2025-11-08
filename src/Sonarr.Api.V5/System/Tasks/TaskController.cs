using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.SignalR;
using Sonarr.Http;
using Sonarr.Http.REST;

namespace Sonarr.Api.V5.System.Tasks;

[V5ApiController("system/task")]
public class TaskController : RestControllerWithSignalR<TaskResource, ScheduledTask>, IHandle<CommandExecutedEvent>
{
    private readonly ITaskManager _taskManager;

    public TaskController(ITaskManager taskManager, IBroadcastSignalRMessage broadcastSignalRMessage)
        : base(broadcastSignalRMessage)
    {
        _taskManager = taskManager;
    }

    [HttpGet]
    [Produces("application/json")]
    public ActionResult<List<TaskResource>> GetAll()
    {
        return _taskManager.GetAll()
                               .Select(ConvertToResource)
                               .OrderBy(t => t.Name)
                               .ToList();
    }

    [HttpGet("{id:int}")]
    [Produces("application/json")]
    public ActionResult<TaskResource> GetById(int id)
    {
        var task = _taskManager.GetAll()
                               .SingleOrDefault(t => t.Id == id);

        if (task == null)
        {
            return NotFound();
        }

        return ConvertToResource(task);
    }

    protected override TaskResource? GetResourceById(int id)
    {
        var task = _taskManager.GetAll()
                               .SingleOrDefault(t => t.Id == id);

        return task == null ? null : ConvertToResource(task);
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
            LastStartTime = scheduledTask.LastStartTime,
            NextExecution = scheduledTask.LastExecution.AddMinutes(scheduledTask.Interval),
            LastDuration = scheduledTask.LastExecution - scheduledTask.LastStartTime
        };
    }

    [NonAction]
    public void Handle(CommandExecutedEvent message)
    {
        BroadcastResourceChange(ModelAction.Sync);
    }
}
