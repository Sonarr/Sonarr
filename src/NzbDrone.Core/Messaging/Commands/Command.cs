using System;
using System.Text.Json.Serialization;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Messaging.Commands
{
    [JsonConverter(typeof(PolymorphicWriteOnlyJsonConverter<Command>))]
    public abstract class Command
    {
        private bool _sendUpdatesToClient;

        public virtual bool SendUpdatesToClient
        {
            get
            {
                return _sendUpdatesToClient;
            }

            set
            {
                _sendUpdatesToClient = value;
            }
        }

        public virtual bool UpdateScheduledTask => true;
        public virtual string CompletionMessage => "Completed";
        public virtual bool RequiresDiskAccess => false;
        public virtual bool IsExclusive => false;

        public string Name { get; private set; }
        public DateTime? LastExecutionTime { get; set; }
        public CommandTrigger Trigger { get; set; }
        public bool SuppressMessages { get; set; }

        public string ClientUserAgent { get; set; }

        public Command()
        {
            Name = GetType().Name.Replace("Command", "");
        }
    }
}
