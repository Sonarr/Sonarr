using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sonarr.Http.REST;
using NzbDrone.Core.Messaging.Commands;

namespace NzbDrone.Api.Commands
{
    public class CommandResource : RestResource
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public object Body { get; set; }
        public CommandPriority Priority { get; set; }
        public CommandStatus Status { get; set; }
        public DateTime Queued { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Ended { get; set; }
        public TimeSpan? Duration { get; set; }
        public string Exception { get; set; }
        public CommandTrigger Trigger { get; set; }

        [JsonIgnore]
        public string CompletionMessage { get; set; }

        //Legacy
        public CommandStatus State
        {
            get
            {
                return Status;
            }

            set { }
        }

        public bool Manual
        {
            get
            {
                return Trigger == CommandTrigger.Manual;
            }

            set { }
        }

        public DateTime StartedOn
        {
            get
            {
                return Queued;
            }

            set { }
        }

        public DateTime? StateChangeTime
        {
            get
            {

                if (Started.HasValue) return Started.Value;

                return Ended;
            }

            set { }
        }

        public bool SendUpdatesToClient
        {
            get
            {
                if (Body != null) return (Body as Command).SendUpdatesToClient;

                return false;
            }

            set { }
        }

        public bool UpdateScheduledTask
        {
            get
            {
                if (Body != null) return (Body as Command).UpdateScheduledTask;

                return false;
            }

            set { }
        }

        public DateTime? LastExecutionTime { get; set; }
    }

    public static class CommandResourceMapper
    {
        public static CommandResource ToResource(this CommandModel model)
        {
            if (model == null) return null;

            return new CommandResource
            {
                Id = model.Id,

                Name = model.Name,
                Message = model.Message,
                Body = model.Body,
                Priority = model.Priority,
                Status = model.Status,
                Queued = model.QueuedAt,
                Started = model.StartedAt,
                Ended = model.EndedAt,
                Duration = model.Duration,
                Exception = model.Exception,
                Trigger = model.Trigger,

                CompletionMessage = model.Body.CompletionMessage,
                LastExecutionTime = model.Body.LastExecutionTime
            };
        }

        public static List<CommandResource> ToResource(this IEnumerable<CommandModel> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
